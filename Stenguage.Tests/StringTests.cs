using Stenguage.Runtime.Values;
using static Stenguage.Tests.Test;

namespace Stenguage.Tests
{
    [TestClass]
    public class StringTests
    {
        [TestMethod]
        public void HelloWorld()
        {
            StringValue value = (StringValue)TestCode(@"""Hello, World!""", false, RuntimeValueType.String, null);
            Assert.IsTrue(value.Value == "Hello, World!");
        }

        [TestMethod]
        public void Escaping()
        {
            StringValue value = (StringValue)TestCode(@"""\""\\\/\b\f\n\r\t""", false, RuntimeValueType.String, null);
            Assert.IsTrue(value.Value == "\"\\/\b\f\n\r\t");
        }

        [TestMethod]
        public void Symbols()
        {
            StringValue value = (StringValue)TestCode(@"""`-=[]\\;',./~!@#$%^&*()_+{}|:\""<>?""", false, RuntimeValueType.String, null);
            Assert.IsTrue(value.Value == "`-=[]\\;',./~!@#$%^&*()_+{}|:\"<>?");
        }
    }
}