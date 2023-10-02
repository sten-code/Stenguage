using Stenguage;
using Stenguage.Runtime.Values;

namespace StdLib
{
    public class ObjectTest : ObjectValue
    {
        public StringValue TestValue { get; set; }

        public ObjectTest(string sourceCode, Position start, Position end) : base(new Dictionary<string, RuntimeValue>(), sourceCode, start, end)
        {
            TestValue = new StringValue("hello", SourceCode, Start, End);
            Properties["TestValue"] = TestValue;
        }

        public override string ValueString()
        {
            return "Object Test";
        }
    }
}
