using Stenguage.Runtime;
using Stenguage.Runtime.Values;

namespace StdLib
{
    public class ObjectTest : ObjectValue
    {
        public StringValue TestValue { get; set; }

        public ObjectTest(string sourceCode, StringValue testValue) : base(sourceCode)
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
