using Stenguage;
using Stenguage.Runtime;
using Stenguage.Runtime.Values;

namespace StdLib
{
    public class StdLib
    {
        public static RuntimeResult index(Stenguage.Runtime.Environment scope, Position start, Position end,
                                         StringValue source, StringValue value)
        {
            return new RuntimeResult().Success(new NumberValue(source.Value.IndexOf(value.Value), scope.SourceCode, start, end));
        }

        public static RuntimeResult contains(Stenguage.Runtime.Environment scope, Position start, Position end, 
                                            StringValue source, StringValue value)
        {
            return new RuntimeResult().Success(new BooleanValue(source.Value.Contains(value.Value), scope.SourceCode, start, end));
        }

        public static RuntimeResult contains(Stenguage.Runtime.Environment scope, Position start, Position end,
                                            ListValue source, RuntimeValue value)
        {
            return new RuntimeResult().Success(new BooleanValue(source.Items.Contains(value), scope.SourceCode, start, end));
        }

        public static RuntimeResult substring(Stenguage.Runtime.Environment scope, Position start, Position end,
                                            StringValue source, NumberValue startIndex, NumberValue length)
        {
            return new RuntimeResult().Success(new StringValue(source.Value.Substring((int)startIndex.Value, (int)length.Value), scope.SourceCode, start, end));
        }

        public static RuntimeResult cls(Stenguage.Runtime.Environment scope, Position start, Position end)
        {
            Console.Clear();
            return RuntimeResult.Null(scope.SourceCode, start, end);
        }

    }
}