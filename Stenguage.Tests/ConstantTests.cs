using Stenguage.Runtime.Values;
using static Stenguage.Tests.Test;

namespace Stenguage.Tests
{
    [TestClass]
    public class ConstantTests
    {
        [TestMethod]
        public void ConstantString()
        {
            StringValue value = (StringValue)TestCode(@"const testvar = ""Hello, World!""", false, RuntimeValueType.String, null);
            Assert.IsTrue(value.Value == "Hello, World!");
        }

        [TestMethod]
        public void ConstantInteger()
        {
            NumberValue value = (NumberValue)TestCode(@"const testvar = 69420", false, RuntimeValueType.Number, null);
            Assert.IsTrue(value.Value == 69420);
        }

        [TestMethod]
        public void ConstantDouble()
        {
            NumberValue value = (NumberValue)TestCode(@"const testvar = 69.420", false, RuntimeValueType.Number, null);
            Assert.IsTrue(value.Value == 69.42);
        }

        [TestMethod]
        public void ConstantBooleanTrue()
        {
            BooleanValue value = (BooleanValue)TestCode(@"const testvar = true", false, RuntimeValueType.Boolean, null);
            Assert.IsTrue(value.Value);
        }

        [TestMethod]
        public void ConstantBooleanFalse()
        {
            BooleanValue value = (BooleanValue)TestCode(@"const testvar = false", false, RuntimeValueType.Boolean, null);
            Assert.IsFalse(value.Value);
        }
    }
}