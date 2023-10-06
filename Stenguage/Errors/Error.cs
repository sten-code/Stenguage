using System.Text;

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
            StringBuilder errorMessage = new StringBuilder();

            errorMessage.AppendLine($"{ErrorType}: {Message}");
            errorMessage.AppendLine($"At Line {Start.Line}, Column {Start.Column}:");

            string[] sourceLines = SourceCode.Split('\n');

            int longestNum = End.Line.ToString().Length;

            for (int i = Math.Max(Start.Line - 3, 0); i < Start.Line; i++) 
            {
                errorMessage.AppendLine((i + 1).ToString().PadLeft(longestNum) + " | " + sourceLines[i]);
            }
            errorMessage.AppendLine((Start.Line + 1).ToString().PadLeft(longestNum) + " | " + sourceLines[Start.Line]);
            errorMessage.AppendLine(new string(' ', longestNum) + " | " + new string(' ', Start.Column) + new string('^', End.Column - Start.Column + 1));
            for (int i = End.Line + 1; i < Math.Min(End.Line + 5, sourceLines.Length - 1); i++)
            {
                errorMessage.AppendLine((i + 1).ToString().PadLeft(longestNum) + " | " + sourceLines[i]);
            }

            return errorMessage.ToString();
        }
    }
}
