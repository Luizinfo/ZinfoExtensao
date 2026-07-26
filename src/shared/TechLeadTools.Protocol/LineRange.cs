namespace TechLeadTools.Protocol
{
    public readonly struct LineRange
    {
        public LineRange(int startLine, int endLine)
        {
            StartLine = startLine;
            EndLine = endLine;
        }

        public int StartLine { get; }

        public int EndLine { get; }

        public static LineRange FromSelection(
            int startLine,
            int endLine,
            int endColumn,
            bool isEmpty)
        {
            if (isEmpty)
            {
                return new LineRange(startLine, startLine);
            }

            var normalizedEnd = endColumn == 1 && endLine > startLine
                ? endLine - 1
                : endLine;

            return new LineRange(startLine, normalizedEnd < startLine ? startLine : normalizedEnd);
        }
    }
}

