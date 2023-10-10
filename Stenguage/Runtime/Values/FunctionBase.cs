namespace Stenguage.Runtime.Values
{
    public abstract class FunctionBase : RuntimeValue
    {
        public FunctionBase() : base(RuntimeValueType.Function)
        {

        }

        public abstract RuntimeResult Call(List<RuntimeValue> args, Context ctx);

        public override string ValueString()
        {
            return ToString();
        }

    }
}
