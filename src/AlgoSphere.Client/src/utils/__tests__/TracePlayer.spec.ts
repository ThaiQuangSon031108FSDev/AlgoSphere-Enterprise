import { describe, it, expect } from 'vitest';
import { TracePlayer, type TraceLog } from '../TracePlayer';

describe('TracePlayer Visualizer State Engine', () => {
  it('should build correct snapshots for Two-Pointer algorithm events', () => {
    const log: TraceLog = {
      initialState: [1, 2, 3],
      trace: [
        { s: 0, l: 2, a: 'ptr', t: [0], v: { name: 'left', val: 1 } },
        { s: 1, l: 3, a: 'ptr', t: [2], v: { name: 'right', val: 3 } }
      ]
    };

    const player = new TracePlayer(log, () => {});
    const dsSnapshots = (player as any).dsSnapshots;

    expect(dsSnapshots).toHaveLength(2);
    expect(dsSnapshots[0].pointers.left.pos).toBe(0);
    expect(dsSnapshots[0].pointers.left.val).toBe(1);

    expect(dsSnapshots[1].pointers.right.pos).toBe(2);
    expect(dsSnapshots[1].pointers.right.val).toBe(3);
  });

  it('should build correct snapshots for Stack operations', () => {
    const log: TraceLog = {
      initialState: [],
      trace: [
        { s: 0, l: 2, a: 'push', t: [], v: { name: 'myStack', val: 10 } },
        { s: 1, l: 3, a: 'push', t: [], v: { name: 'myStack', val: 20 } }
      ]
    };

    const player = new TracePlayer(log, () => {});
    const dsSnapshots = (player as any).dsSnapshots;

    expect(dsSnapshots).toHaveLength(2);
    expect(dsSnapshots[0].myStack.type).toBe('Stack');
    expect(dsSnapshots[0].myStack.items).toEqual([10]);
    expect(dsSnapshots[1].myStack.items).toEqual([10, 20]);
  });

  it('should build correct snapshots for HashMap băm operations', () => {
    const log: TraceLog = {
      initialState: [],
      trace: [
        { s: 0, l: 2, a: 'hset', t: [], v: { name: 'myMap', key: 'a', val: 100 } },
        { s: 1, l: 3, a: 'hset', t: [], v: { name: 'myMap', key: 'b', val: 200 } }
      ]
    };

    const player = new TracePlayer(log, () => {});
    const dsSnapshots = (player as any).dsSnapshots;

    expect(dsSnapshots).toHaveLength(2);
    expect(dsSnapshots[0].myMap.items.a).toBe(100);
    expect(dsSnapshots[1].myMap.items.b).toBe(200);
  });

  it('should build correct snapshots for Recursion call stack events', () => {
    const log: TraceLog = {
      initialState: [],
      trace: [
        { s: 0, l: 2, a: 'call', t: [0], v: { fn: 'fib', args: [2] } },
        { s: 1, l: 3, a: 'ret', t: [0], v: { val: 1 } }
      ]
    };

    const player = new TracePlayer(log, () => {});
    const dsSnapshots = (player as any).dsSnapshots;

    expect(dsSnapshots).toHaveLength(2);
    expect(dsSnapshots[0].recursion.calls).toHaveLength(1);
    expect(dsSnapshots[0].recursion.calls[0].fn).toBe('fib');
    expect(dsSnapshots[0].recursion.calls[0].returned).toBe(false);

    expect(dsSnapshots[1].recursion.calls[0].returned).toBe(true);
    expect(dsSnapshots[1].recursion.calls[0].retVal).toBe(1);
  });

  it('should build correct snapshots for 2D DP Table grids', () => {
    const log: TraceLog = {
      initialState: [],
      trace: [
        { s: 0, l: 2, a: 'dpset', t: [0, 0], v: { val: 1, formula: 'dp[0][0]=1' } },
        { s: 1, l: 3, a: 'dpset', t: [0, 1], v: { val: 2, formula: 'dp[0][1]=2' } }
      ]
    };

    const player = new TracePlayer(log, () => {});
    const dsSnapshots = (player as any).dsSnapshots;

    expect(dsSnapshots).toHaveLength(2);
    expect(dsSnapshots[0].dpTable.grid['0,0'].val).toBe(1);
    expect(dsSnapshots[0].dpTable.grid['0,0'].formula).toBe('dp[0][0]=1');
    expect(dsSnapshots[1].dpTable.grid['0,1'].val).toBe(2);
  });
});
