using Stenguage.Errors;

namespace Stenguage.Runtime.Values
{
    public class NumberValue : RuntimeValue
    {
        public double Value { get; set; }

        public NumberValue(double value) : base(RuntimeValueType.Number)
        {
            Value = value;
        }

        public override string ValueString()
        {
            return ToString();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public override RuntimeResult CompareEE(RuntimeValue right, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.Number)
                return res.Success(new BooleanValue(false));
            return res.Success(new BooleanValue(Value == ((NumberValue)right).Value));
        }
        public override RuntimeResult CompareNE(RuntimeValue right, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.Number)
                return res.Success(new BooleanValue(true));
            return res.Success(new BooleanValue(Value != ((NumberValue)right).Value));
        }
        public override RuntimeResult CompareLT(RuntimeValue right, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.Number)
                return res.Success(new BooleanValue(false));
            return res.Success(new BooleanValue(Value < ((NumberValue)right).Value));
        }
        public override RuntimeResult CompareLTE(RuntimeValue right, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.Number)
                return res.Success(new BooleanValue(false));
            return res.Success(new BooleanValue(Value <= ((NumberValue)right).Value));
        }
        public override RuntimeResult CompareGT(RuntimeValue right, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.Number)
                return res.Success(new BooleanValue(false));
            return res.Success(new BooleanValue(Value > ((NumberValue)right).Value));
        }
        public override RuntimeResult CompareGTE(RuntimeValue right, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.Number)
                return res.Success(new BooleanValue(false));
            return res.Success(new BooleanValue(Value >= ((NumberValue)right).Value));
        }

        public override RuntimeResult Add(RuntimeValue right, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            switch (right.Type)
            {
                case RuntimeValueType.Number:
                    return res.Success(new NumberValue(Value + ((NumberValue)right).Value));
                case RuntimeValueType.String:
                    return res.Success(new StringValue(Value + ((StringValue)right).Value));
                case RuntimeValueType.Boolean:
                    return res.Success(new NumberValue(Value + (((BooleanValue)right).Value ? 1 : 0)));
                default:
                    return res.Failure(new OperationError("+", Type, right.Type, ctx.Env.SourceCode, ctx.Start, ctx.End));
            }
        }
        public override RuntimeResult Sub(RuntimeValue right, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            switch (right.Type)
            {
                case RuntimeValueType.Number:
                    return res.Success(new NumberValue(Value - ((NumberValue)right).Value));
                case RuntimeValueType.Boolean:
                    return res.Success(new NumberValue(Value - (((BooleanValue)right).Value ? 1 : 0)));
                default:
                    return res.Failure(new OperationError("-", Type, right.Type, ctx.Env.SourceCode, ctx.Start, ctx.End));
            }
        }
        public override RuntimeResult Mul(RuntimeValue right, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            switch (right.Type)
            {
                case RuntimeValueType.Number:
                    return res.Success(new NumberValue(Value * ((NumberValue)right).Value));
                case RuntimeValueType.Boolean:
                    return res.Success(new NumberValue(Value * (((BooleanValue)right).Value ? 1 : 0)));
                case RuntimeValueType.String:
                    if (Value % 1 != 0)
                    {
                        return res.Failure(new Error("Can only multiply a string by an integer", ctx.Env.SourceCode, ctx.Start, ctx.End));
                    }

                    return res.Success(new StringValue(string.Concat(Enumerable.Repeat(((StringValue)right).Value, (int)Value))));
                default:
                    return res.Failure(new OperationError("*", Type, right.Type, ctx.Env.SourceCode, ctx.Start, ctx.End));
            }
        }
        public override RuntimeResult Div(RuntimeValue right, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            switch (right.Type)
            {
                case RuntimeValueType.Number:
                    return res.Success(new NumberValue(Value / ((NumberValue)right).Value));
                case RuntimeValueType.Boolean:
                    BooleanValue booleanValue = (BooleanValue)right;
                    if (!booleanValue.Value)
                    {
                        return res.Failure(new Error("Cannot divide by zero.", ctx.Env.SourceCode, ctx.Start, ctx.End));
                    }
                    return res.Success(new NumberValue(Value));
                default:
                    return res.Failure(new OperationError("/", Type, right.Type, ctx.Env.SourceCode, ctx.Start, ctx.End));
            }
        }
        public override RuntimeResult Mod(RuntimeValue right, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            switch (right.Type)
            {
                case RuntimeValueType.Number:
                    return res.Success(new NumberValue(Value % ((NumberValue)right).Value));
                default:
                    return res.Failure(new OperationError("%", Type, right.Type, ctx.Env.SourceCode, ctx.Start, ctx.End));
            }
        }
        public override RuntimeResult Pow(RuntimeValue right, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            switch (right.Type)
            {
                case RuntimeValueType.Number:
                    return res.Success(new NumberValue(Math.Pow(Value, ((NumberValue)right).Value)));
                default:
                    return res.Failure(new OperationError("^", Type, right.Type, ctx.Env.SourceCode, ctx.Start, ctx.End));
            }
        }

        public override RuntimeResult Not(Context ctx)
        {
            return new RuntimeResult().Success(new BooleanValue(Value == 0));
        }
        public override RuntimeResult Min(Context ctx)
        {
            return new RuntimeResult().Success(new NumberValue(-Value));
        }
    }
}
