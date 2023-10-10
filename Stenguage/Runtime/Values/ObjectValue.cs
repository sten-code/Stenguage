using Stenguage.Errors;
using System.Reflection;

namespace Stenguage.Runtime.Values
{
    public class ObjectValue : RuntimeValue
    {
        public Dictionary<string, RuntimeValue> Properties { get; set; }

        public ObjectValue(Dictionary<string, RuntimeValue> properties = null) : base(RuntimeValueType.Object)
        {
            Properties = properties == null ? new Dictionary<string, RuntimeValue>() : properties;
        }

        public void Init()
        {
            Type type = GetType();
            foreach (MethodInfo method in type.GetMethods())
            {
                if (!method.IsPublic)
                    continue;

                if (method.ReturnType != typeof(RuntimeResult))
                    continue;

                Properties[method.Name] = new NativeFnValue((args, ctx) =>
                {
                    return (RuntimeResult)method.Invoke(this, args.Concat(new object[] { ctx }).ToArray());
                });
            }

            foreach (PropertyInfo property in type.GetProperties())
            {
                if (!property.CanRead)
                    continue;

                if (!property.CanWrite)
                    continue;

                if (!property.PropertyType.IsSubclassOf(typeof(RuntimeValue)))
                    continue;

                Properties[property.Name] = (RuntimeValue)property.GetValue(this, null);
            }

            foreach (FieldInfo field in type.GetFields())
            {
                if (!field.IsPublic)
                    continue;

                if (!field.FieldType.IsSubclassOf(typeof(RuntimeValue)))
                    continue;

                Properties[field.Name] = (RuntimeValue)field.GetValue(this);
            }
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
                if (prop.Value == null)
                {
                    output += prop.Key + ": null, ";
                    continue;
                }
                output += prop.Key + ": " + prop.Value.ValueString() + ", ";
            }
            if (Properties.Count > 0)
                output = output.Substring(0, output.Length - 2);
            return output + "}";
        }

        public override RuntimeResult GetIndex(RuntimeValue index, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            if (index.Type != RuntimeValueType.String)
            {
                return res.Failure(new Error("Can only get the index of an object with a string.", ctx.Env.SourceCode, ctx.Start, ctx.End));
            }

            StringValue value = (StringValue)index;
            if (!Properties.ContainsKey(value.Value))
                return res.Failure(new Error($"Property '{value.Value}' doesn't exist in object.", ctx.Env.SourceCode, ctx.Start, ctx.End));

            return res.Success(Properties[value.Value]);
        }

        public override RuntimeResult SetIndex(RuntimeValue index, RuntimeValue value, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            if (index.Type != RuntimeValueType.String)
            {
                return res.Failure(new Error($"Cannot set index of an object with '{index.Type}'.", ctx.Env.SourceCode, ctx.Start, ctx.End));
            }

            Properties[((StringValue)index).Value] = value;
            return res.Success(value);
        }

        public override (RuntimeResult, List<RuntimeValue>) Iterate(Context ctx)
        {
            List<RuntimeValue> values = new List<RuntimeValue>();
            foreach (KeyValuePair<string, RuntimeValue> prop in Properties)
            {
                values.Add(new ListValue(new List<RuntimeValue>
                {
                    new StringValue(prop.Key),
                    prop.Value
                }));
            }
            return (RuntimeResult.Null(), values);
        }

    }
}
