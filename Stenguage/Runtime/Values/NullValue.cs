namespace Stenguage.Runtime.Values
{
    public class NullValue : RuntimeValue
    {
        public NullValue(string sourceCode)
            : base(RuntimeValueType.Null, sourceCode)
        { }

        public override string ValueString()
        {
            return ToString();
        }

        public override string ToString()
        {
            return "null";
        }

        public override RuntimeResult CompareEE(RuntimeValue right, Position start, Position end)
        {
            return new RuntimeResult().Success(new BooleanValue(right.Type == RuntimeValueType.Null, SourceCode));
        }
        public override RuntimeResult CompareNE(RuntimeValue right, Position start, Position end)
        {
            return new RuntimeResult().Success(new BooleanValue(right.Type != RuntimeValueType.Null, SourceCode));
        }
    }
}
