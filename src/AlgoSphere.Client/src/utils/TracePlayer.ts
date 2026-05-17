export interface TraceStep {
  s: number;           // step index
  l?: number;          // source code line number
  a: 'cmp' | 'swp' | 'vis' | 'fnd' | 'done' | 'node' | 'link' | 'err' | 'push' | 'pop' | 'enq' | 'deq' | 'hset' | 'hget' | 'hdel' | 'ptr' | 'dpset' | 'gedge' | 'gvis' | 'call' | 'ret'; // action type
  t: number[];         // target indices
  v?: Record<string, any>; // variable snapshot
  msg?: string;        // human-readable message
}

export interface TraceLog {
  initialState: any[];
  trace: TraceStep[];
  algorithm?: string;
}

type StateChangeCallback = (
  state: any[],
  step: TraceStep | null,
  index: number,
  total: number,
  dsState?: Record<string, any>,
  nodeState?: any
) => void;

/**
 * TracePlayer — Snapshot-based bidirectional stepper.
 * Each step stores a full state snapshot so stepBackward is O(1).
 */
export class TracePlayer {
  private snapshots: any[][];    // snapshots[i] = array state AFTER step i
  private dsSnapshots: Record<string, any>[]; // snapshots[i] = DS state AFTER step i
  private nodeSnapshots: any[]; // snapshots[i] = Node/Link state AFTER step i
  private trace: TraceStep[];
  private currentIndex: number = -1;
  private onStateChange: StateChangeCallback;

  // Playback internals
  private playTimer: ReturnType<typeof setTimeout> | null = null;
  private _isPlaying = false;
  private _speed = 1.0; // multiplier (0.25, 0.5, 1, 2, 4)

  constructor(log: TraceLog, onStateChange: StateChangeCallback) {
    this.trace = log.trace;
    this.onStateChange = onStateChange;
    const { arraySnapshots, dsSnapshots, nodeSnapshots } = this.buildSnapshots(log.initialState, log.trace);
    this.snapshots = arraySnapshots;
    this.dsSnapshots = dsSnapshots;
    this.nodeSnapshots = nodeSnapshots;
  }

  // Pre-compute all state snapshots upfront — O(n) one-time cost
  private buildSnapshots(initial: any[], trace: TraceStep[]): { arraySnapshots: any[][], dsSnapshots: any[], nodeSnapshots: any[] } {
    const arraySnapshots: any[][] = [];
    const dsSnapshots: any[] = [];
    const nodeSnapshots: any[] = [];
    
    let arrayState = [...initial];
    let dsState: Record<string, any> = {};
    let nodeState: { nodes: Record<number, any>, links: any[] } = { nodes: {}, links: [] };
    let recursionId = 0;

    for (const step of trace) {
      // Handle array swaps
      if (step.a === 'swp' && step.t.length >= 2) {
        arrayState = [...arrayState];
        const [i, j] = step.t;
        [arrayState[i], arrayState[j]] = [arrayState[j], arrayState[i]];
      }

      // Handle data structure visualization (vis action)
      if (step.a === 'vis' && step.v?.ds) {
        const { name, op, val } = step.v.ds;
        if (!dsState[name]) dsState[name] = { type: 'Set', items: [] };
        
        if (op === 'add') {
          if (!dsState[name].items.includes(val)) {
            dsState[name] = { ...dsState[name], items: [...dsState[name].items, val] };
          }
        }
      }

      // Handle custom stack/queue/map actions
      if (step.a === 'push') {
        const name = step.v?.name || 'stack';
        const val = step.v?.val;
        if (!dsState[name]) dsState[name] = { type: 'Stack', items: [] };
        dsState[name] = { ...dsState[name], items: [...dsState[name].items, val] };
      }
      if (step.a === 'pop') {
        const name = step.v?.name || 'stack';
        if (!dsState[name]) dsState[name] = { type: 'Stack', items: [] };
        const newItems = [...dsState[name].items];
        newItems.pop();
        dsState[name] = { ...dsState[name], items: newItems };
      }
      if (step.a === 'enq') {
        const name = step.v?.name || 'queue';
        const val = step.v?.val;
        if (!dsState[name]) dsState[name] = { type: 'Queue', items: [] };
        dsState[name] = { ...dsState[name], items: [...dsState[name].items, val] };
      }
      if (step.a === 'deq') {
        const name = step.v?.name || 'queue';
        if (!dsState[name]) dsState[name] = { type: 'Queue', items: [] };
        const newItems = [...dsState[name].items];
        newItems.shift();
        dsState[name] = { ...dsState[name], items: newItems };
      }
      if (step.a === 'hset') {
        const name = step.v?.name || 'Map';
        const { key, val } = step.v || {};
        if (!dsState[name]) dsState[name] = { type: 'Map', items: {} };
        dsState[name] = { ...dsState[name], items: { ...dsState[name].items, [key]: val } };
      }
      if (step.a === 'hdel') {
        const name = step.v?.name || 'Map';
        const { key } = step.v || {};
        if (!dsState[name]) dsState[name] = { type: 'Map', items: {} };
        const newItems = { ...dsState[name].items };
        delete newItems[key];
        dsState[name] = { ...dsState[name], items: newItems };
      }
      if (step.a === 'ptr') {
        const { name, val } = step.v || {};
        if (!dsState['pointers']) dsState['pointers'] = {};
        dsState['pointers'] = { ...dsState['pointers'], [name]: { pos: step.t[0], val } };
      }
      if (step.a === 'dpset') {
        const [r, c] = step.t;
        const { val, formula } = step.v || {};
        if (!dsState['dpTable']) dsState['dpTable'] = { grid: {}, maxRow: 0, maxCol: 0 };
        const grid = { ...dsState['dpTable'].grid };
        const key = `${r},${c}`;
        grid[key] = { val, formula };
        const maxRow = Math.max(dsState['dpTable'].maxRow, r);
        const maxCol = Math.max(dsState['dpTable'].maxCol, c || 0);
        dsState['dpTable'] = { grid, maxRow, maxCol };
      }
      if (step.a === 'gedge') {
        const [u, v] = step.t;
        const { weight, directed } = step.v || {};
        if (!dsState['graph']) dsState['graph'] = { edges: [], nodeStates: {} };
        const filteredEdges = dsState['graph'].edges.filter((e: any) => !(e.u === u && e.v === v));
        dsState['graph'] = {
          ...dsState['graph'],
          edges: [...filteredEdges, { u, v, weight, directed }]
        };
      }
      if (step.a === 'gvis') {
        const [nodeId] = step.t;
        const { state } = step.v || {};
        if (!dsState['graph']) dsState['graph'] = { edges: [], nodeStates: {} };
        dsState['graph'] = {
          ...dsState['graph'],
          nodeStates: { ...dsState['graph'].nodeStates, [nodeId]: state }
        };
      }
      if (step.a === 'call') {
        const [depth] = step.t;
        const { fn, args } = step.v || {};
        if (!dsState['recursion']) dsState['recursion'] = { calls: [] };
        dsState['recursion'] = {
          calls: [...dsState['recursion'].calls, { depth, fn, args, returned: false, retVal: null, id: ++recursionId }]
        };
      }
      if (step.a === 'ret') {
        const [depth] = step.t;
        const { val } = step.v || {};
        if (!dsState['recursion']) dsState['recursion'] = { calls: [] };
        const calls = [...dsState['recursion'].calls];
        for (let i = calls.length - 1; i >= 0; i--) {
          if (calls[i].depth === depth && !calls[i].returned) {
            calls[i] = { ...calls[i], returned: true, retVal: val };
            break;
          }
        }
        dsState['recursion'] = { calls };
      }

      // Handle Node creation
      if (step.a === 'node') {
        const [id] = step.t;
        nodeState = { ...nodeState, nodes: { ...nodeState.nodes, [id]: { id, val: step.v?.val } } };
      }

      // Handle Link creation/update
      if (step.a === 'link') {
        const [source, target] = step.t;
        const prop = step.v?.prop;
        // Remove old link from this source/prop if exists
        const filteredLinks = nodeState.links.filter(l => !(l.source === source && l.prop === prop));
        if (target !== -1) {
          nodeState = { ...nodeState, links: [...filteredLinks, { source, target, prop }] };
        } else {
          nodeState = { ...nodeState, links: filteredLinks };
        }
      }

      arraySnapshots.push([...arrayState]);
      dsSnapshots.push(JSON.parse(JSON.stringify(dsState)));
      // Deep copy nodeState so each snapshot is independent
      nodeSnapshots.push({
        nodes: { ...nodeState.nodes },
        links: [...nodeState.links],
      });
    }

    return { arraySnapshots, dsSnapshots, nodeSnapshots };
  }

  get isPlaying() { return this._isPlaying; }
  get speed() { return this._speed; }
  get currentStep() { return this.currentIndex; }
  get totalSteps() { return this.trace.length; }
  get progress() {
    return this.trace.length === 0 ? 0 : (this.currentIndex + 1) / this.trace.length;
  }

  public stepForward() {
    if (this.currentIndex >= this.trace.length - 1) {
      this.pause();
      return;
    }
    this.currentIndex++;
    this.emit();
  }

  public stepBackward() {
    if (this.currentIndex < 0) return;
    this.currentIndex--;
    this.emit();
  }

  public seekTo(index: number) {
    const clamped = Math.max(-1, Math.min(index, this.trace.length - 1));
    this.currentIndex = clamped;
    this.emit();
  }

  public play() {
    if (this._isPlaying) return;
    this._isPlaying = true;
    this.scheduleNext();
  }

  public pause() {
    this._isPlaying = false;
    if (this.playTimer !== null) {
      clearTimeout(this.playTimer);
      this.playTimer = null;
    }
  }

  public togglePlay() {
    this._isPlaying ? this.pause() : this.play();
  }

  public setSpeed(multiplier: number) {
    this._speed = multiplier;
  }

  public reset() {
    this.pause();
    this.currentIndex = -1;
    this.emitInitial();
  }

  private scheduleNext() {
    if (!this._isPlaying) return;
    const baseDelay = 700; // ms at 1x speed
    const delay = baseDelay / this._speed;

    this.playTimer = setTimeout(() => {
      if (this.currentIndex >= this.trace.length - 1) {
        this.pause();
        return;
      }
      this.stepForward();
      this.scheduleNext();
    }, delay);
  }

  private emit() {
    if (this.currentIndex < 0) {
      this.emitInitial();
      return;
    }
    const state = this.snapshots[this.currentIndex];
    const dsState = this.dsSnapshots[this.currentIndex];
    const nodeState = this.nodeSnapshots[this.currentIndex];
    const step = this.trace[this.currentIndex];
    this.onStateChange([...state], step, this.currentIndex, this.trace.length, dsState, nodeState);
  }

  private emitInitial() {
    const initialState = this.snapshots.length > 0
      ? this.buildInitialState()
      : [];
    this.onStateChange(initialState, null, -1, this.trace.length);
  }

  // Recover initial state by reverse-applying snapshots (or just use first snapshot before any swap)
  private buildInitialState(): any[] {
    // Rebuild from scratch — snapshot[0] is state AFTER step 0, we need BEFORE step 0
    // We stored a pre-step copy in buildSnapshots as the initial
    return this._initialCache;
  }

  private _initialCache: any[] = [];

  static create(log: TraceLog, onStateChange: StateChangeCallback): TracePlayer {
    const player = new TracePlayer(log, onStateChange);
    player._initialCache = [...log.initialState];
    // Auto-play immediately so the visualization starts right away
    if (player.trace.length > 0) {
      player.play();
    }
    return player;
  }

  public getCurrentState(): any[] {
    if (this.currentIndex < 0) return [...this._initialCache];
    return [...this.snapshots[this.currentIndex]];
  }
}
