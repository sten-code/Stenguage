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
        NativeFn,
        Function,
    }

    public abstract class RuntimeValue
    {
        public RuntimeValueType Type { get; set; }
        public string SourceCode { get; set; }

        public RuntimeValue(RuntimeValueType type, string sourceCode)
        {
            Type = type;
            SourceCode = sourceCode;
        }

        public abstract string ValueString();

        // Compare Operations
        public virtual RuntimeResult CompareEE(RuntimeValue right, Position start, Position end)
        {
            return new RuntimeResult().Success(new BooleanValue(this == right, SourceCode));
        }
        public virtual RuntimeResult CompareNE(RuntimeValue right, Position start, Position end)
        {
            return new RuntimeResult().Success(new BooleanValue(this != right, SourceCode));
        }
        public virtual RuntimeResult CompareLT(RuntimeValue right, Position start, Position end)
        {
            return new RuntimeResult().Failure(new OperationError("<", Type, right.Type, SourceCode, start, end));
        }
        public virtual RuntimeResult CompareLTE(RuntimeValue right, Position start, Position end)
        {
            return new RuntimeResult().Failure(new OperationError("<=", Type, right.Type, SourceCode, start, end));
        }
        public virtual RuntimeResult CompareGT(RuntimeValue right, Position start, Position end)
        {
            return new RuntimeResult().Failure(new OperationError(">", Type, right.Type, SourceCode, start, end));
        }
        public virtual RuntimeResult CompareGTE(RuntimeValue right, Position start, Position end)
        {
            return new RuntimeResult().Failure(new OperationError(">=", Type, right.Type, SourceCode, start, end));
        }
        public virtual RuntimeResult And(RuntimeValue right, Position start, Position end)
        {
            return new RuntimeResult().Success(right);
        }
        public virtual RuntimeResult Or(RuntimeValue right, Position start, Position end)
        {
            return new RuntimeResult().Success(this);
        }

        // Arithmetic Operations
        public virtual RuntimeResult Add(RuntimeValue right, Position start, Position end)
        {
            return new RuntimeResult().Failure(new OperationError("+", Type, right.Type, SourceCode, start, end));
        }
        public virtual RuntimeResult Sub(RuntimeValue right, Position start, Position end)
        {
            return new RuntimeResult().Failure(new OperationError("-", Type, right.Type, SourceCode, start, end));
        }
        public virtual RuntimeResult Mul(RuntimeValue right, Position start, Position end)
        {
            return new RuntimeResult().Failure(new OperationError("*", Type, right.Type, SourceCode, start, end));
        }
        public virtual RuntimeResult Div(RuntimeValue right, Position start, Position end)
        {
            return new RuntimeResult().Failure(new OperationError("/", Type, right.Type, SourceCode, start, end));
        }
        public virtual RuntimeResult Mod(RuntimeValue right, Position start, Position end)
        {
            return new RuntimeResult().Failure(new OperationError("%", Type, right.Type, SourceCode, start, end));
        }
        public virtual RuntimeResult Pow(RuntimeValue right, Position start, Position end)
        {
            return new RuntimeResult().Failure(new OperationError("^", Type, right.Type, SourceCode, start, end));
        }

        // Unary Operations
        public virtual RuntimeResult Not(Position start, Position end)
        {
            return new RuntimeResult().Success(new BooleanValue(false, SourceCode));
        }
        public virtual RuntimeResult Min(Position start, Position end)
        {
            return new RuntimeResult().Failure(new Error($"Cannot do a '-' unary operator on a {Type} type.", SourceCode, start, end));
        }

        // Indexing Operations
        public virtual RuntimeResult SetIndex(RuntimeValue index, RuntimeValue value, Position start, Position end)
        {
            return new RuntimeResult().Failure(new Error($"Cannot set the index on a '{Type}' type.", SourceCode, start, end));
        }

        public virtual RuntimeResult GetIndex(RuntimeValue index, Position start, Position end)
        {
            return new RuntimeResult().Failure(new Error($"Cannot get the index of a '{Type}' type.", SourceCode, start, end));
        }

        // Misc
        public virtual (RuntimeResult, List<RuntimeValue>) Iterate(Position start, Position end)
        {
            return (new RuntimeResult().Failure(new Error($"Cannot iterate over a '{Type}' type.", SourceCode, start, end)), new List<RuntimeValue>());
        }
    }
}
