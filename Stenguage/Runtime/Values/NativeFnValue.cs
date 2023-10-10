namespace Stenguage.Runtime.Values
{
    public class NativeFnValue : FunctionBase
    {
        public Func<List<RuntimeValue>, Context, RuntimeResult> Func { get; set; }

        public NativeFnValue(Func<List<RuntimeValue>, Context, RuntimeResult> func)
        {
            Func = func;
        }

        public override string ValueString()
        {
            return ToString();
        }

        public override string ToString()
        {
            return "NativeFn";
        }

        public override RuntimeResult Call(List<RuntimeValue> args, Context ctx)
        {
            return Func(args, ctx);
        }
    }
}
