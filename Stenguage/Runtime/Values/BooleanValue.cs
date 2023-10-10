using Stenguage.Errors;

namespace Stenguage.Runtime.Values
{
    public class BooleanValue : RuntimeValue
    {
        public bool Value { get; set; }

        public BooleanValue(bool value) : base(RuntimeValueType.Boolean)
        {
            Value = value;
        }

        public override string ValueString()
        {
            return ToString();
        }

        public override string ToString()
        {
            return Value ? "true" : "false";
        }

        public override RuntimeResult CompareEE(RuntimeValue right, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.Boolean)
                return res.Success(new BooleanValue(false));
            return res.Success(new BooleanValue(Value == ((BooleanValue)right).Value));
        }
        public override RuntimeResult CompareNE(RuntimeValue right, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.Boolean)
                return res.Success(new BooleanValue(true));
            return res.Success(new BooleanValue(Value != ((BooleanValue)right).Value));
        }
        public override RuntimeResult CompareLT(RuntimeValue right, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.Boolean)
                return res.Success(new BooleanValue(false));
            return res.Success(new BooleanValue(!Value && ((BooleanValue)right).Value));
        }
        public override RuntimeResult CompareLTE(RuntimeValue right, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.Boolean)
                return res.Success(new BooleanValue(false));
            return res.Success(new BooleanValue(!(Value && !((BooleanValue)right).Value)));
        }
        public override RuntimeResult CompareGT(RuntimeValue right, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.Boolean)
                return res.Success(new BooleanValue(false));
            return res.Success(new BooleanValue(Value && !((BooleanValue)right).Value));
        }
        public override RuntimeResult CompareGTE(RuntimeValue right, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.Boolean)
                return res.Success(new BooleanValue(false));
            return res.Success(new BooleanValue(!(!Value && ((BooleanValue)right).Value)));
        }
        public override RuntimeResult And(RuntimeValue right, Context ctx)
        {
            return new RuntimeResult().Success(right.Type == RuntimeValueType.Boolean ? new BooleanValue(Value && ((BooleanValue)right).Value) : right);
        }
        public override RuntimeResult Or(RuntimeValue right, Context ctx)
        {
            return new RuntimeResult().Success(right.Type == RuntimeValueType.Boolean ? new BooleanValue(Value || ((BooleanValue)right).Value) : this);
        }

        public override RuntimeResult Add(RuntimeValue right, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            switch (right.Type)
            {
                case RuntimeValueType.Number:
                    return res.Success(new NumberValue((Value ? 1 : 0) + ((NumberValue)right).Value));
                case RuntimeValueType.Boolean:
                    BooleanValue booleanValue = (BooleanValue)right;
                    return res.Success(new NumberValue((Value ? 1 : 0) + (booleanValue.Value ? 1 : 0)));
                case RuntimeValueType.String:
                    StringValue stringValue = (StringValue)right;
                    return res.Success(new StringValue(Value.ToString() + stringValue.Value));
                default:
                    return new RuntimeResult().Failure(new OperationError("+", Type, right.Type, ctx.Env.SourceCode, ctx.Start, ctx.End));
            }
        }
        public override RuntimeResult Sub(RuntimeValue right, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            switch (right.Type)
            {
                case RuntimeValueType.Number:
                    return res.Success(new NumberValue((Value ? 1 : 0) - ((NumberValue)right).Value));
                case RuntimeValueType.Boolean:
                    return res.Success(new NumberValue((Value ? 1 : 0) - (((BooleanValue)right).Value ? 1 : 0)));
                default:
                    return new RuntimeResult().Failure(new OperationError("-", Type, right.Type, ctx.Env.SourceCode, ctx.Start, ctx.End));
            }
        }
        public override RuntimeResult Mul(RuntimeValue right, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            switch (right.Type)
            {
                case RuntimeValueType.Number:
                    return res.Success(new NumberValue((Value ? 1 : 0) * ((NumberValue)right).Value));
                case RuntimeValueType.Boolean:
                    return res.Success(new NumberValue((Value ? 1 : 0) * (((BooleanValue)right).Value ? 1 : 0)));
                default:
                    return new RuntimeResult().Failure(new OperationError("*", Type, right.Type, ctx.Env.SourceCode, ctx.Start, ctx.End));
            }
        }
        public override RuntimeResult Div(RuntimeValue right, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            switch (right.Type)
            {
                case RuntimeValueType.Number:
                    return res.Success(new NumberValue((Value ? 1 : 0) / ((NumberValue)right).Value));
                case RuntimeValueType.Boolean:
                    return res.Success(new NumberValue((Value ? 1 : 0) / (((BooleanValue)right).Value ? 1 : 0)));
                default:
                    return new RuntimeResult().Failure(new OperationError("/", Type, right.Type, ctx.Env.SourceCode, ctx.Start, ctx.End));
            }
        }

        public override RuntimeResult Not(Context ctx)
        {
            return new RuntimeResult().Success(new BooleanValue(!Value));
        }
        public override RuntimeResult Min(Context ctx)
        {
            return new RuntimeResult().Success(new NumberValue(Value ? -1 : 0));
        }
    }
}
