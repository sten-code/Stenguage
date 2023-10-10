using Stenguage.Errors;

namespace Stenguage.Runtime.Values
{
    public class StringValue : RuntimeValue
    {
        public string Value { get; set; }

        public StringValue(string value) : base(RuntimeValueType.String)
        {
            Value = value;
        }

        public override string ValueString()
        {
            return $"\"{Value}\"";
        }

        public override string ToString()
        {
            return Value;
        }

        public override RuntimeResult CompareEE(RuntimeValue right, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.String)
                return res.Success(new BooleanValue(false));
            return res.Success(new BooleanValue(Value == ((StringValue)right).Value));
        }
        public override RuntimeResult CompareNE(RuntimeValue right, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.String)
                return res.Success(new BooleanValue(true));
            return res.Success(new BooleanValue(Value != ((StringValue)right).Value));
        }

        public override RuntimeResult Add(RuntimeValue right, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            switch (right.Type)
            {
                case RuntimeValueType.Number:
                    NumberValue numberValue = (NumberValue)right;
                    return res.Success(new StringValue(Value + numberValue.Value));
                case RuntimeValueType.String:
                    StringValue stringValue = (StringValue)right;
                    return res.Success(new StringValue(Value + stringValue.Value));
                case RuntimeValueType.Boolean:
                    BooleanValue booleanValue = (BooleanValue)right;
                    return res.Success(new StringValue(Value + booleanValue.Value.ToString()));
                default:
                    return new RuntimeResult().Failure(new OperationError("+", Type, right.Type, ctx.Env.SourceCode, ctx.Start, ctx.End));
            }
        }
        public override RuntimeResult Mul(RuntimeValue right, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            switch (right.Type)
            {
                case RuntimeValueType.Number:
                    NumberValue numberValue = (NumberValue)right;
                    if (numberValue.Value % 1 != 0)
                    {
                        return new RuntimeResult().Failure(new Error("Can only multiply a string by an integer.", ctx.Env.SourceCode, ctx.Start, ctx.End));
                    }

                    return res.Success(new StringValue(string.Concat(Enumerable.Repeat(Value, (int)numberValue.Value))));
                case RuntimeValueType.Boolean:
                    BooleanValue booleanValue = (BooleanValue)right;
                    return res.Success(new StringValue(string.Concat(Enumerable.Repeat(Value, booleanValue.Value ? 1 : 0))));
                default:
                    return new RuntimeResult().Failure(new OperationError("*", Type, right.Type, ctx.Env.SourceCode, ctx.Start, ctx.End));
            }
        }

        public override RuntimeResult Not(Context ctx)
        {
            return new RuntimeResult().Success(new BooleanValue(Value.Length == 0));
        }

        public override RuntimeResult GetIndex(RuntimeValue index, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            if (index.Type != RuntimeValueType.Number)
            {
                return res.Failure(new Error("Can only get the index of a string with a number.", ctx.Env.SourceCode, ctx.Start, ctx.End));
            }

            NumberValue num = (NumberValue)index;
            if (num.Value % 1 != 0)
            {
                return res.Failure(new Error("Can only get the index of a string with an integer.", ctx.Env.SourceCode, ctx.Start, ctx.End));
            }

            if ((int)num.Value < 0 || Value.Length <= (int)num.Value)
            {
                return res.Failure(new Error("Index out of bounds.", ctx.Env.SourceCode, ctx.Start, ctx.End));
            }

            return res.Success(new StringValue(Value[(int)num.Value].ToString()));
        }

        public override (RuntimeResult, List<RuntimeValue>) Iterate(Context ctx)
        {
            List<RuntimeValue> list = new List<RuntimeValue>();
            foreach (char c in Value)
            {
                list.Add(new StringValue(c.ToString()));
            }
            return (RuntimeResult.Null(), list);
        }
    }
}
