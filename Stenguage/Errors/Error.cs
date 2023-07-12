namespace Stenguage.Errors
{
    public class Error
    {
        public string ErrorType { get; set; }
        public string Message { get; set; }
        public string SourceCode { get; set; }
        public Position Start { get; set; }
        public Position End { get; set; }

        public Error(string message, string sourceCode, Position start, Position end, string errorType = "Error")
        {
            ErrorType = errorType;
            Message = message;
            SourceCode = sourceCode;
            Start = start;
            End = end;
        }

        public override string ToString()
        {
            string output = "";

            string[] lines = SourceCode.Split('\n');
            int lineCount = End.Line - Start.Line + 1;
            for (int i = 0; i < lineCount; i++)
            {
                output += lines[i + Start.Line] + "\n" + new string(' ', Start.Column) + new string('^', End.Column - Start.Column + 1) + "\n";
            }

            return $"{output}\nLine: {Start.Line + 1}\n{ErrorType}: {Message}";
        }
    }
}
