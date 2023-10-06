using Stenguage;
using Stenguage.Runtime;
using Stenguage.Runtime.Values;

namespace StdLib
{
    public class ObjectTest : ObjectValue
    {
        public StringValue TestValue { get; set; }

        public ObjectTest(string sourceCode, Position start, Position end, StringValue testValue) : base(sourceCode, start, end)
        {
            TestValue = testValue;
        }

        public RuntimeResult TestFunction()
        {
            return new RuntimeResult().Success(TestValue);
        }

        public override string ValueString()
        {
            return "Object Test";
        }
    }
}
