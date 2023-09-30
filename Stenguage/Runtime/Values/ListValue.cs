using Stenguage.Errors;
using Stenguage.Utils;

namespace Stenguage.Runtime.Values
{
    public class ListValue : RuntimeValue
    {
        public List<RuntimeValue> Items { get; set; }

        public ListValue(List<RuntimeValue> items, string sourceCode, Position start, Position end) : base(RuntimeValueType.List, sourceCode, start, end)
        {
            Items = items;
        }

        public override string ValueString()
        {
            return ToString();
        }

        public override string ToString()
        {
            string output = "[";
            foreach (RuntimeValue item in Items)
            {
                output += item.ValueString() + ", ";
            }
            if (Items.Count > 0)
                output = output.Substring(0, output.Length - 2);
            return output + "]";
        }

        public override RuntimeResult Add(RuntimeValue right, Position start, Position end)
        {
            return new RuntimeResult().Success(new ListValue(new List<RuntimeValue>(Items) { right }, SourceCode, start, end));
        }
        public override RuntimeResult Mul(RuntimeValue right, Position start, Position end)
        {
            if (right.Type != RuntimeValueType.Number)
            {
                return new RuntimeResult().Failure(new OperationError("*", Type, right.Type, SourceCode, start, end));
            }
            NumberValue mul = (NumberValue)right;
            List<RuntimeValue> values = new List<RuntimeValue>();
            for (int i = 0; i < mul.Value; i++)
            {
                values.AddRange(Items.Copy());
            }
            return new RuntimeResult().Success(new ListValue(values, SourceCode, start, end));
        }

        public override RuntimeResult Not(Position start, Position end)
        {
            return new RuntimeResult().Success(new BooleanValue(Items.Count == 0, SourceCode, start, end));
        }

        public override RuntimeResult GetIndex(RuntimeValue index, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            if (index.Type != RuntimeValueType.Number)
            {
                return res.Failure(new Error("Can only get the index of a list with a number.", SourceCode, start, end));
            }

            NumberValue num = (NumberValue)index;
            if (num.Value % 1 != 0)
            {
                return res.Failure(new Error("Can only get the index of a list with an integer.", SourceCode, start, end));
            }

            if ((int)num.Value < 0 || Items.Count <= (int)num.Value)
            {
                return res.Failure(new Error("Index out of bounds.", SourceCode, start, end));
            }

            return res.Success(Items[(int)num.Value]);
        }

        public override RuntimeResult SetIndex(RuntimeValue index, RuntimeValue value, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            if (index.Type != RuntimeValueType.Number)
            {
                return res.Failure(new Error($"Expected a number as an index, got '{index.Type}'.", SourceCode, start, end));
            }

            NumberValue num = (NumberValue)index;
            if (num.Value % 1 != 0)
            {
                return res.Failure(new Error("Cannot use a double as an index in a list.", SourceCode, start, end));
            }
            if (Items.Count <= (int)num.Value || (int)num.Value < 0)
                return res.Failure(new Error("Index out of bounds.", SourceCode, start, end));

            Items[(int)num.Value] = value;
            return res.Success(value);
        }

        public override (RuntimeResult, List<RuntimeValue>) Iterate(Position start, Position end)
        {
            return (new RuntimeResult().Success(new NullValue(SourceCode, new Position(0, 0, 0), new Position(0, 0, 0))), Items);
        }
    }
}
