using Stenguage.Errors;

namespace Stenguage.Runtime.Values
{
    public class StringValue : RuntimeValue
    {
        public string Value { get; set; }

        public StringValue(string value, string sourceCode) : base(RuntimeValueType.String, sourceCode)
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

        public override RuntimeResult CompareEE(RuntimeValue right, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.String)
                return res.Success(new BooleanValue(false, SourceCode));
            return res.Success(new BooleanValue(Value == ((StringValue)right).Value, SourceCode));
        }
        public override RuntimeResult CompareNE(RuntimeValue right, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.String)
                return res.Success(new BooleanValue(true, SourceCode));
            return res.Success(new BooleanValue(Value != ((StringValue)right).Value, SourceCode));
        }

        public override RuntimeResult Add(RuntimeValue right, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            switch (right.Type)
            {
                case RuntimeValueType.Number:
                    NumberValue numberValue = (NumberValue)right;
                    return res.Success(new StringValue(Value + numberValue.Value, SourceCode));
                case RuntimeValueType.String:
                    StringValue stringValue = (StringValue)right;
                    return res.Success(new StringValue(Value + stringValue.Value, SourceCode));
                case RuntimeValueType.Boolean:
                    BooleanValue booleanValue = (BooleanValue)right;
                    return res.Success(new StringValue(Value + booleanValue.Value.ToString(), SourceCode));
                default:
                    return new RuntimeResult().Failure(new OperationError("+", Type, right.Type, SourceCode, start, end));
            }
        }
        public override RuntimeResult Mul(RuntimeValue right, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            switch (right.Type)
            {
                case RuntimeValueType.Number:
                    NumberValue numberValue = (NumberValue)right;
                    if (numberValue.Value % 1 != 0)
                    {
                        return new RuntimeResult().Failure(new Error("Can only multiply a string by an integer.", SourceCode, start, end));
                    }

                    return res.Success(new StringValue(string.Concat(Enumerable.Repeat(Value, (int)numberValue.Value)), SourceCode));
                case RuntimeValueType.Boolean:
                    BooleanValue booleanValue = (BooleanValue)right;
                    return res.Success(new StringValue(string.Concat(Enumerable.Repeat(Value, booleanValue.Value ? 1 : 0)), SourceCode));
                default:
                    return new RuntimeResult().Failure(new OperationError("*", Type, right.Type, SourceCode, start, end));
            }
        }

        public override RuntimeResult Not(Position start, Position end)
        {
            return new RuntimeResult().Success(new BooleanValue(Value.Length == 0, SourceCode));
        }

        public override RuntimeResult GetIndex(RuntimeValue index, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            if (index.Type != RuntimeValueType.Number)
            {
                return res.Failure(new Error("Can only get the index of a string with a number.", SourceCode, start, end));
            }

            NumberValue num = (NumberValue)index;
            if (num.Value % 1 != 0)
            {
                return res.Failure(new Error("Can only get the index of a string with an integer.", SourceCode, start, end));
            }

            if ((int)num.Value < 0 || Value.Length <= (int)num.Value)
            {
                return res.Failure(new Error("Index out of bounds.", SourceCode, start, end));
            }

            return res.Success(new StringValue(Value[(int)num.Value].ToString(), SourceCode));
        }

        public override (RuntimeResult, List<RuntimeValue>) Iterate(Position start, Position end)
        {
            List<RuntimeValue> list = new List<RuntimeValue>();
            foreach (char c in Value)
            {
                list.Add(new StringValue(c.ToString(), SourceCode));
            }
            return (RuntimeResult.Null(SourceCode), list);
        }
    }
}
