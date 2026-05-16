export const calculateLevel = (xp: number): number => {
  return Math.max(1, Math.floor(Math.sqrt(xp / 50)) + 1);
};

export const getXpRangeForLevel = (level: number) => {
  const floor = Math.pow(level - 1, 2) * 50;
  const ceil = Math.pow(level, 2) * 50;
  return { floor, ceil };
};
