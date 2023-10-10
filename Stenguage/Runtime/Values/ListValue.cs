using Stenguage.Errors;
using Stenguage.Utils;
using System.Collections.Generic;

namespace Stenguage.Runtime.Values
{
    public class ListValue : RuntimeValue
    {
        public List<RuntimeValue> Items { get; set; }

        public ListValue(List<RuntimeValue> items) : base(RuntimeValueType.List)
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

        public override RuntimeResult Add(RuntimeValue right, Context ctx)
        {
            return new RuntimeResult().Success(new ListValue(new List<RuntimeValue>(Items) { right }));
        }
        public override RuntimeResult Sub(RuntimeValue right, Context ctx)
        {
            if (right.Type != RuntimeValueType.Number)
            {
                return new RuntimeResult().Failure(new OperationError("-", Type, right.Type, ctx.Env.SourceCode, ctx.Start, ctx.End));
            }

            List<RuntimeValue> newList = new List<RuntimeValue>(Items);
            newList.RemoveAt((int)((NumberValue)right).Value);
            return new RuntimeResult().Success(new ListValue(newList));
        }
        public override RuntimeResult Mul(RuntimeValue right, Context ctx)
        {
            if (right.Type != RuntimeValueType.Number)
            {
                return new RuntimeResult().Failure(new OperationError("*", Type, right.Type, ctx.Env.SourceCode, ctx.Start, ctx.End));
            }
            NumberValue mul = (NumberValue)right;
            List<RuntimeValue> values = new List<RuntimeValue>();
            for (int i = 0; i < mul.Value; i++)
            {
                values.AddRange(Items.Copy());
            }
            return new RuntimeResult().Success(new ListValue(values));
        }

        public override RuntimeResult Not(Context ctx)
        {
            return new RuntimeResult().Success(new BooleanValue(Items.Count == 0));
        }

        public override RuntimeResult GetIndex(RuntimeValue index, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            if (index.Type != RuntimeValueType.Number)
            {
                return res.Failure(new Error("Can only get the index of a list with a number.", ctx.Env.SourceCode, ctx.Start, ctx.End));
            }

            NumberValue num = (NumberValue)index;
            if (num.Value % 1 != 0)
            {
                return res.Failure(new Error("Can only get the index of a list with an integer.", ctx.Env.SourceCode, ctx.Start, ctx.End));
            }

            if ((int)num.Value < 0 || Items.Count <= (int)num.Value)
            {
                return res.Failure(new Error("Index out of bounds.", ctx.Env.SourceCode, ctx.Start, ctx.End));
            }

            return res.Success(Items[(int)num.Value]);
        }

        public override RuntimeResult SetIndex(RuntimeValue index, RuntimeValue value, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            if (index.Type != RuntimeValueType.Number)
            {
                return res.Failure(new Error($"Expected a number as an index, got '{index.Type}'.", ctx.Env.SourceCode, ctx.Start, ctx.End));
            }

            NumberValue num = (NumberValue)index;
            if (num.Value % 1 != 0)
            {
                return res.Failure(new Error("Cannot use a double as an index in a list.", ctx.Env.SourceCode, ctx.Start, ctx.End));
            }
            if (Items.Count <= (int)num.Value || (int)num.Value < 0)
                return res.Failure(new Error("Index out of bounds.", ctx.Env.SourceCode, ctx.Start, ctx.End));

            Items[(int)num.Value] = value;
            return res.Success(value);
        }

        public override (RuntimeResult, List<RuntimeValue>) Iterate(Context ctx)
        {
            return (RuntimeResult.Null(), Items);
        }
    }
}
