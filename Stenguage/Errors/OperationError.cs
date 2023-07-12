namespace Stenguage.Errors
{
    public class OperationError : Error
    {
        public OperationError(string op, Runtime.Values.RuntimeValueType left, Runtime.Values.RuntimeValueType right, string sourceCode, Position start, Position end) 
            : base($"Cannot do a '{op}' operation on a '{left}' type with a '{right}' type.", sourceCode, start, end, "OperationError")
        { }
    }
}
