namespace Stenguage
{
    public class Position
    {
        public int Index { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }

        public Position(int index, int line, int column)
        {
            Index = index;
            Line = line;
            Column = column;
        }

        public Position Copy()
        {
            return new Position(Index, Line, Column);
        }
    }
}
