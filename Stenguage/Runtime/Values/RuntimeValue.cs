using Stenguage.Errors;

namespace Stenguage.Runtime.Values
{
    public enum RuntimeValueType
    {
        Null,
        Any,
        Number,
        String,
        Boolean,
        Object,
        List,
        Function,
    }

    public abstract class RuntimeValue
    {
        public RuntimeValueType Type { get; set; }

        public RuntimeValue(RuntimeValueType type)
        {
            Type = type;
        }

        public abstract string ValueString();

        // Compare Operations
        public virtual RuntimeResult CompareEE(RuntimeValue right, Context ctx)
        {
            return new RuntimeResult().Success(new BooleanValue(this == right));
        }
        public virtual RuntimeResult CompareNE(RuntimeValue right, Context ctx)
        {
            return new RuntimeResult().Success(new BooleanValue(this != right));
        }
        public virtual RuntimeResult CompareLT(RuntimeValue right, Context ctx)
        {
            return new RuntimeResult().Failure(new OperationError("<", Type, right.Type, ctx.Env.SourceCode, ctx.Start, ctx.End));
        }
        public virtual RuntimeResult CompareLTE(RuntimeValue right, Context ctx)
        {
            return new RuntimeResult().Failure(new OperationError("<=", Type, right.Type, ctx.Env.SourceCode, ctx.Start, ctx.End));
        }
        public virtual RuntimeResult CompareGT(RuntimeValue right, Context ctx)
        {
            return new RuntimeResult().Failure(new OperationError(">", Type, right.Type, ctx.Env.SourceCode, ctx.Start, ctx.End));
        }
        public virtual RuntimeResult CompareGTE(RuntimeValue right, Context ctx)
        {
            return new RuntimeResult().Failure(new OperationError(">=", Type, right.Type, ctx.Env.SourceCode, ctx.Start, ctx.End));
        }
        public virtual RuntimeResult And(RuntimeValue right, Context ctx)
        {
            return new RuntimeResult().Success(right);
        }
        public virtual RuntimeResult Or(RuntimeValue right, Context ctx)
        {
            return new RuntimeResult().Success(this);
        }

        // Arithmetic Operations
        public virtual RuntimeResult Add(RuntimeValue right, Context ctx)
        {
            return new RuntimeResult().Failure(new OperationError("+", Type, right.Type, ctx.Env.SourceCode, ctx.Start, ctx.End));
        }
        public virtual RuntimeResult Sub(RuntimeValue right, Context ctx)
        {
            return new RuntimeResult().Failure(new OperationError("-", Type, right.Type, ctx.Env.SourceCode, ctx.Start, ctx.End));
        }
        public virtual RuntimeResult Mul(RuntimeValue right, Context ctx)
        {
            return new RuntimeResult().Failure(new OperationError("*", Type, right.Type, ctx.Env.SourceCode, ctx.Start, ctx.End));
        }
        public virtual RuntimeResult Div(RuntimeValue right, Context ctx)
        {
            return new RuntimeResult().Failure(new OperationError("/", Type, right.Type, ctx.Env.SourceCode, ctx.Start, ctx.End));
        }
        public virtual RuntimeResult Mod(RuntimeValue right, Context ctx)
        {
            return new RuntimeResult().Failure(new OperationError("%", Type, right.Type, ctx.Env.SourceCode, ctx.Start, ctx.End));
        }
        public virtual RuntimeResult Pow(RuntimeValue right, Context ctx)
        {
            return new RuntimeResult().Failure(new OperationError("^", Type, right.Type, ctx.Env.SourceCode, ctx.Start, ctx.End));
        }

        // Unary Operations
        public virtual RuntimeResult Not(Context ctx)
        {
            return new RuntimeResult().Success(new BooleanValue(false));
        }
        public virtual RuntimeResult Min(Context ctx)
        {
            return new RuntimeResult().Failure(new Error($"Cannot do a '-' unary operator on a {Type} type.", ctx.Env.SourceCode, ctx.Start, ctx.End));
        }

        // Indexing Operations
        public virtual RuntimeResult SetIndex(RuntimeValue index, RuntimeValue value, Context ctx)
        {
            return new RuntimeResult().Failure(new Error($"Cannot set the index on a '{Type}' type.", ctx.Env.SourceCode, ctx.Start, ctx.End));
        }

        public virtual RuntimeResult GetIndex(RuntimeValue index, Context ctx)
        {
            return new RuntimeResult().Failure(new Error($"Cannot get the index of a '{Type}' type.", ctx.Env.SourceCode, ctx.Start, ctx.End));
        }

        // Misc
        public virtual (RuntimeResult, List<RuntimeValue>) Iterate(Context ctx)
        {
            return (new RuntimeResult().Failure(new Error($"Cannot iterate over a '{Type}' type.", ctx.Env.SourceCode, ctx.Start, ctx.End)), new List<RuntimeValue>());
        }
    }
}
