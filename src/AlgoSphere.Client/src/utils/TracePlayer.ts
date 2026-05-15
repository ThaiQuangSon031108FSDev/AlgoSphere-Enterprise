export interface TraceStep {
  s: number;           // step index
  l?: number;          // source code line number
  a: 'cmp' | 'swp' | 'vis' | 'fnd' | 'done'; // action type
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
  total: number
) => void;

/**
 * TracePlayer — Snapshot-based bidirectional stepper.
 * Each step stores a full state snapshot so stepBackward is O(1).
 */
export class TracePlayer {
  private snapshots: any[][];    // snapshots[i] = state AFTER step i
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
    this.snapshots = this.buildSnapshots(log.initialState, log.trace);
  }

  // Pre-compute all state snapshots upfront — O(n) one-time cost
  private buildSnapshots(initial: any[], trace: TraceStep[]): any[][] {
    const snapshots: any[][] = [];
    let state = [...initial];

    for (const step of trace) {
      if (step.a === 'swp' && step.t.length >= 2) {
        state = [...state];
        const [i, j] = step.t;
        [state[i], state[j]] = [state[j], state[i]];
      }
      snapshots.push([...state]);
    }

    return snapshots;
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
    const step = this.trace[this.currentIndex];
    this.onStateChange([...state], step, this.currentIndex, this.trace.length);
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
    return player;
  }

  public getCurrentState(): any[] {
    if (this.currentIndex < 0) return [...this._initialCache];
    return [...this.snapshots[this.currentIndex]];
  }
}
