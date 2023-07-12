using Stenguage.Errors;

namespace Stenguage.Runtime.Values
{
    public class BooleanValue : RuntimeValue
    {
        public bool Value { get; set; }

        public BooleanValue(bool value, string sourceCode, Position start, Position end) : base(RuntimeValueType.Boolean, sourceCode, start, end)
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

        public override RuntimeResult CompareEE(RuntimeValue right, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.Boolean)
                return res.Success(new BooleanValue(false, SourceCode, start, end));
            return res.Success(new BooleanValue(Value == ((BooleanValue)right).Value, SourceCode, start, end));
        }
        public override RuntimeResult CompareNE(RuntimeValue right, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.Boolean)
                return res.Success(new BooleanValue(true, SourceCode, start, end));
            return res.Success(new BooleanValue(Value != ((BooleanValue)right).Value, SourceCode, start, end));
        }
        public override RuntimeResult CompareLT(RuntimeValue right, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.Boolean)
                return res.Success(new BooleanValue(false, SourceCode, start, end));
            return res.Success(new BooleanValue(!Value && ((BooleanValue)right).Value, SourceCode, start, end));
        }
        public override RuntimeResult CompareLTE(RuntimeValue right, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.Boolean)
                return res.Success(new BooleanValue(false, SourceCode, start, end));
            return res.Success(new BooleanValue(!(Value && !((BooleanValue)right).Value), SourceCode, start, end));
        }
        public override RuntimeResult CompareGT(RuntimeValue right, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.Boolean)
                return res.Success(new BooleanValue(false, SourceCode, start, end));
            return res.Success(new BooleanValue(Value && !((BooleanValue)right).Value, SourceCode, start, end));
        }
        public override RuntimeResult CompareGTE(RuntimeValue right, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.Boolean)
                return res.Success(new BooleanValue(false, SourceCode, start, end));
            return res.Success(new BooleanValue(!(!Value && ((BooleanValue)right).Value), SourceCode, start, end));
        }
        public override RuntimeResult And(RuntimeValue right, Position start, Position end)
        {
            return new RuntimeResult().Success(right.Type == RuntimeValueType.Boolean ? new BooleanValue(Value && ((BooleanValue)right).Value, SourceCode, start, end) : right);
        }
        public override RuntimeResult Or(RuntimeValue right, Position start, Position end)
        {
            return new RuntimeResult().Success(right.Type == RuntimeValueType.Boolean ? new BooleanValue(Value || ((BooleanValue)right).Value, SourceCode, start, end) : this);
        }

        public override RuntimeResult Add(RuntimeValue right, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            switch (right.Type)
            {
                case RuntimeValueType.Number:
                    return res.Success(new NumberValue((Value ? 1 : 0) + ((NumberValue)right).Value, SourceCode, start, end));
                case RuntimeValueType.Boolean:
                    BooleanValue booleanValue = (BooleanValue)right;
                    return res.Success(new NumberValue((Value ? 1 : 0) + (booleanValue.Value ? 1 : 0), SourceCode, start, end));
                case RuntimeValueType.String:
                    StringValue stringValue = (StringValue)right;
                    return res.Success(new StringValue(Value.ToString() + stringValue.Value, SourceCode, start, end));
                default:
                    return new RuntimeResult().Failure(new OperationError("+", Type, right.Type, SourceCode, start, end));
            }
        }
        public override RuntimeResult Sub(RuntimeValue right, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            switch (right.Type)
            {
                case RuntimeValueType.Number:
                    return res.Success(new NumberValue((Value ? 1 : 0) - ((NumberValue)right).Value, SourceCode, start, end));
                case RuntimeValueType.Boolean:
                    return res.Success(new NumberValue((Value ? 1 : 0) - (((BooleanValue)right).Value ? 1 : 0), SourceCode, start, end));
                default:
                    return new RuntimeResult().Failure(new OperationError("-", Type, right.Type, SourceCode, start, end));
            }
        }
        public override RuntimeResult Mul(RuntimeValue right, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            switch (right.Type)
            {
                case RuntimeValueType.Number:
                    return res.Success(new NumberValue((Value ? 1 : 0) * ((NumberValue)right).Value, SourceCode, start, end));
                case RuntimeValueType.Boolean:
                    return res.Success(new NumberValue((Value ? 1 : 0) * (((BooleanValue)right).Value ? 1 : 0), SourceCode, start, end));
                default:
                    return new RuntimeResult().Failure(new OperationError("*", Type, right.Type, SourceCode, start, end));
            }
        }
        public override RuntimeResult Div(RuntimeValue right, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            switch (right.Type)
            {
                case RuntimeValueType.Number:
                    return res.Success(new NumberValue((Value ? 1 : 0) / ((NumberValue)right).Value, SourceCode, start, end));
                case RuntimeValueType.Boolean:
                    return res.Success(new NumberValue((Value ? 1 : 0) / (((BooleanValue)right).Value ? 1 : 0), SourceCode, start, end));
                default:
                    return new RuntimeResult().Failure(new OperationError("/", Type, right.Type, SourceCode, start, end));
            }
        }

        public override RuntimeResult Not(Position start, Position end)
        {
            return new RuntimeResult().Success(new BooleanValue(!Value, SourceCode, start, end));
        }
        public override RuntimeResult Min(Position start, Position end)
        {
            return new RuntimeResult().Success(new NumberValue(Value ? -1 : 0, SourceCode, start, end));
        }
    }
}
