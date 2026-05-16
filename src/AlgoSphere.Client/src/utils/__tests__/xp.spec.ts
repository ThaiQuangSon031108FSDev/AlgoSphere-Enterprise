import { describe, it, expect } from 'vitest';
import { calculateLevel, getXpRangeForLevel } from '../xp';

describe('XP Utilities', () => {
  it('should calculate level 1 for 0 XP', () => {
    expect(calculateLevel(0)).toBe(1);
  });

  it('should calculate level 2 for 50 XP', () => {
    expect(calculateLevel(50)).toBe(2);
  });

  it('should calculate level 3 for 200 XP', () => {
    expect(calculateLevel(200)).toBe(3);
  });

  it('should return correct range for level 2', () => {
    const range = getXpRangeForLevel(2);
    expect(range.floor).toBe(50);
    expect(range.ceil).toBe(200);
  });
});
