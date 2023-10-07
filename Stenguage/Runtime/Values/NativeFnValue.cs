namespace Stenguage.Runtime.Values
{
    public class NativeFnValue : RuntimeValue
    {
        public Func<List<RuntimeValue>, Environment, Position, Position, RuntimeResult> Call { get; set; }

        public NativeFnValue(Func<List<RuntimeValue>, Environment, Position, Position, RuntimeResult> call) : base(RuntimeValueType.NativeFn, "")
        {
            Call = call;
        }

        public override string ValueString()
        {
            return ToString();
        }

        public override string ToString()
        {
            return "NativeFn";
        }
    }
}
