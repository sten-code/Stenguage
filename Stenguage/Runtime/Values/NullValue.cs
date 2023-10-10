namespace Stenguage.Runtime.Values
{
    public class NullValue : RuntimeValue
    {
        public NullValue()
            : base(RuntimeValueType.Null)
        { }

        public override string ValueString()
        {
            return ToString();
        }

        public override string ToString()
        {
            return "null";
        }

        public override RuntimeResult CompareEE(RuntimeValue right, Context ctx)
        {
            return new RuntimeResult().Success(new BooleanValue(right.Type == RuntimeValueType.Null));
        }
        public override RuntimeResult CompareNE(RuntimeValue right, Context ctx)
        {
            return new RuntimeResult().Success(new BooleanValue(right.Type != RuntimeValueType.Null));
        }
    }
}
