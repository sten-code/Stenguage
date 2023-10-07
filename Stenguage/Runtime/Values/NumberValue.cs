﻿using Stenguage.Errors;

namespace Stenguage.Runtime.Values
{
    public class NumberValue : RuntimeValue
    {
        public double Value { get; set; }

        public NumberValue(double value, string sourceCode) : base(RuntimeValueType.Number, sourceCode)
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

        public override RuntimeResult CompareEE(RuntimeValue right, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.Number)
                return res.Success(new BooleanValue(false, SourceCode));
            return res.Success(new BooleanValue(Value == ((NumberValue)right).Value, SourceCode));
        }
        public override RuntimeResult CompareNE(RuntimeValue right, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.Number)
                return res.Success(new BooleanValue(true, SourceCode));
            return res.Success(new BooleanValue(Value != ((NumberValue)right).Value, SourceCode));
        }
        public override RuntimeResult CompareLT(RuntimeValue right, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.Number)
                return res.Success(new BooleanValue(false, SourceCode));
            return res.Success(new BooleanValue(Value < ((NumberValue)right).Value, SourceCode));
        }
        public override RuntimeResult CompareLTE(RuntimeValue right, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.Number)
                return res.Success(new BooleanValue(false, SourceCode));
            return res.Success(new BooleanValue(Value <= ((NumberValue)right).Value, SourceCode));
        }
        public override RuntimeResult CompareGT(RuntimeValue right, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.Number)
                return res.Success(new BooleanValue(false, SourceCode));
            return res.Success(new BooleanValue(Value > ((NumberValue)right).Value, SourceCode));
        }
        public override RuntimeResult CompareGTE(RuntimeValue right, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            if (right.Type != RuntimeValueType.Number)
                return res.Success(new BooleanValue(false, SourceCode));
            return res.Success(new BooleanValue(Value >= ((NumberValue)right).Value, SourceCode));
        }

        public override RuntimeResult Add(RuntimeValue right, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            switch (right.Type)
            {
                case RuntimeValueType.Number:
                    return res.Success(new NumberValue(Value + ((NumberValue)right).Value, SourceCode));
                case RuntimeValueType.String:
                    return res.Success(new StringValue(Value + ((StringValue)right).Value, SourceCode));
                case RuntimeValueType.Boolean:
                    return res.Success(new NumberValue(Value + (((BooleanValue)right).Value ? 1 : 0), SourceCode));
                default:
                    return res.Failure(new OperationError("+", Type, right.Type, SourceCode, start, end));
            }
        }
        public override RuntimeResult Sub(RuntimeValue right, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            switch (right.Type)
            {
                case RuntimeValueType.Number:
                    return res.Success(new NumberValue(Value - ((NumberValue)right).Value, SourceCode));
                case RuntimeValueType.Boolean:
                    return res.Success(new NumberValue(Value - (((BooleanValue)right).Value ? 1 : 0), SourceCode));
                default:
                    return res.Failure(new OperationError("-", Type, right.Type, SourceCode, start, end));
            }
        }
        public override RuntimeResult Mul(RuntimeValue right, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            switch (right.Type)
            {
                case RuntimeValueType.Number:
                    return res.Success(new NumberValue(Value * ((NumberValue)right).Value, SourceCode));
                case RuntimeValueType.Boolean:
                    return res.Success(new NumberValue(Value * (((BooleanValue)right).Value ? 1 : 0), SourceCode));
                case RuntimeValueType.String:
                    if (Value % 1 != 0)
                    {
                        return res.Failure(new Error("Can only multiply a string by an integer", SourceCode, start, end));
                    }

                    return res.Success(new StringValue(string.Concat(Enumerable.Repeat(((StringValue)right).Value, (int)Value)), SourceCode));
                default:
                    return res.Failure(new OperationError("*", Type, right.Type, SourceCode, start, end));
            }
        }
        public override RuntimeResult Div(RuntimeValue right, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            switch (right.Type)
            {
                case RuntimeValueType.Number:
                    return res.Success(new NumberValue(Value / ((NumberValue)right).Value, SourceCode));
                case RuntimeValueType.Boolean:
                    BooleanValue booleanValue = (BooleanValue)right;
                    if (!booleanValue.Value)
                    {
                        return res.Failure(new Error("Cannot divide by zero.", SourceCode, start, end));
                    }
                    return res.Success(new NumberValue(Value, SourceCode));
                default:
                    return res.Failure(new OperationError("/", Type, right.Type, SourceCode, start, end));
            }
        }
        public override RuntimeResult Mod(RuntimeValue right, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            switch (right.Type)
            {
                case RuntimeValueType.Number:
                    return res.Success(new NumberValue(Value % ((NumberValue)right).Value, SourceCode));
                default:
                    return res.Failure(new OperationError("%", Type, right.Type, SourceCode, start, end));
            }
        }
        public override RuntimeResult Pow(RuntimeValue right, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            switch (right.Type)
            {
                case RuntimeValueType.Number:
                    return res.Success(new NumberValue(Math.Pow(Value, ((NumberValue)right).Value), SourceCode));
                default:
                    return res.Failure(new OperationError("^", Type, right.Type, SourceCode, start, end));
            }
        }

        public override RuntimeResult Not(Position start, Position end)
        {
            return new RuntimeResult().Success(new BooleanValue(Value == 0, SourceCode));
        }
        public override RuntimeResult Min(Position start, Position end)
        {
            return new RuntimeResult().Success(new NumberValue(-Value, SourceCode));
        }
    }
}
