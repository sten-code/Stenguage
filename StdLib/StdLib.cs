using Stenguage.Runtime;
using Stenguage.Runtime.Values;

namespace StdLib
{
    public class StdLib
    {
        public static RuntimeResult index(Context ctx,
                                          StringValue source, StringValue value)
        {
            return new RuntimeResult().Success(new NumberValue(source.Value.IndexOf(value.Value)));
        }

        public static RuntimeResult contains(Context ctx,
                                             StringValue source, StringValue value)
        {
            return new RuntimeResult().Success(new BooleanValue(source.Value.Contains(value.Value)));
        }

        public static RuntimeResult contains(Context ctx,
                                             ListValue source, RuntimeValue value)
        {
            return new RuntimeResult().Success(new BooleanValue(source.Items.Contains(value)));
        }

        public static RuntimeResult substring(Context ctx,
                                              StringValue source, NumberValue startIndex, NumberValue length)
        {
            return new RuntimeResult().Success(new StringValue(source.Value.Substring((int)startIndex.Value, (int)length.Value)));
        }

        public static RuntimeResult cls(Context ctx)
        {
            Console.Clear();
            return RuntimeResult.Null();
        }

    }
}