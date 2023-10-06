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
            // Split the source code into lines
            var lines = SourceCode.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            // Create a StringBuilder to build the error message
            var errorMessage = new StringBuilder();

            // Add the error type and message
            errorMessage.AppendLine($"{ErrorType}: {Message}");

            // Add the source code lines with error indicators
            for (int i = Start.Line - 1; i <= End.Line - 1; i++)
            {
                var line = lines[i];

                // Add the line number
                errorMessage.Append($"Line {Start.Line + i}: ");

                // Add the source code line
                errorMessage.AppendLine(line);

                // Add arrows to indicate the error location
                if (Start.Line == End.Line)
                {
                    // If the error is in a single line
                    int arrowStart = Start.Column - 1;
                    int arrowEnd = End.Column - 1;

                    // Add spaces before the arrows
                    errorMessage.Append(' ', Math.Max(arrowStart, 0));

                    // Add arrows
                    errorMessage.Append('^', Math.Max(arrowEnd - arrowStart, 1));
                }
                else if (i == Start.Line - 1)
                {
                    // If the error starts on this line
                    errorMessage.Append(' ', Math.Max(Start.Column - 1, 0));
                    errorMessage.AppendLine("^");
                }
                else if (i == End.Line - 1)
                {
                    // If the error ends on this line
                    errorMessage.AppendLine("^");
                }
                else
                {
                    // If the error is in a middle line, just add a caret under the line
                    errorMessage.AppendLine("^");
                }
            }

            return errorMessage.ToString();
        }
    }
}
