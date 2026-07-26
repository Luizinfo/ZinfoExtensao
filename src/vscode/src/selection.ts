export interface NormalizedLineRange {
  startLine: number;
  endLine: number;
}

export function normalizeSelectionLines(
  startLine: number,
  endLine: number,
  endCharacter: number,
  isEmpty: boolean
): NormalizedLineRange {
  if (isEmpty) {
    return { startLine, endLine: startLine };
  }

  const normalizedEnd = endCharacter === 0 && endLine > startLine
    ? endLine - 1
    : endLine;

  return { startLine, endLine: Math.max(startLine, normalizedEnd) };
}

