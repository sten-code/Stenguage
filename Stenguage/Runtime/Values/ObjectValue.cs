using Stenguage.Errors;

namespace Stenguage.Runtime.Values
{
    public class ObjectValue : RuntimeValue
    {
        public Dictionary<string, RuntimeValue> Properties { get; set; }

        public ObjectValue(Dictionary<string, RuntimeValue> properties, string sourceCode, Position start, Position end) : base(RuntimeValueType.Object, sourceCode, start, end)
        {
            Properties = properties;
        }

        public override string ValueString()
        {
            return ToString();
        }

        public override string ToString()
        {
            string output = "{";
            foreach (KeyValuePair<string, RuntimeValue> prop in Properties)
            {
                output += prop.Key + ": " + prop.Value.ValueString() + ", ";
            }
            if (Properties.Count > 0)
                output = output.Substring(0, output.Length - 2);
            return output + "}";
        }

        public override RuntimeResult GetIndex(RuntimeValue index, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            if (index.Type != RuntimeValueType.String)
            {
                return res.Failure(new Error("Can only get the index of an object with a string.", SourceCode, start, end));
            }

            StringValue value = (StringValue)index;
            if (!Properties.ContainsKey(value.Value))
                return res.Failure(new Error($"Property '{value.Value}' doesn't exist in object.", SourceCode, start, end));

            return res.Success(Properties[value.Value]);
        }

        public override RuntimeResult SetIndex(RuntimeValue index, RuntimeValue value, Position start, Position end)
        {
            RuntimeResult res = new RuntimeResult();
            if (index.Type != RuntimeValueType.String)
            {
                return res.Failure(new Error($"Cannot set index of an object with '{index.Type}'.", SourceCode, start, end));
            }

            Properties[((StringValue)index).Value] = value;
            return res.Success(value);
        }

        public override (RuntimeResult, List<RuntimeValue>) Iterate(Position start, Position end)
        {
            List<RuntimeValue> values = new List<RuntimeValue>();
            foreach (KeyValuePair<string, RuntimeValue> prop in Properties)
            {
                values.Add(new ListValue(new List<RuntimeValue>
                {
                    new StringValue(prop.Key, SourceCode, start, end),
                    prop.Value
                }, SourceCode, start, end));
            }
            return (new RuntimeResult().Success(new NullValue(SourceCode, new Position(0, 0, 0), new Position(0, 0, 0))), values);
        }

    }
}
