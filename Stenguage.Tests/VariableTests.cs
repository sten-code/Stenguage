using Stenguage.Runtime.Values;
using static Stenguage.Tests.Test;

namespace Stenguage.Tests
{
    [TestClass]
    public class VariableTests
    {
        [TestMethod]
        public void VariableString()
        {
            StringValue value = (StringValue)TestCode(@"let testvar = ""Hello, World!""", false, RuntimeValueType.String, null);
            Assert.IsTrue(value.Value == "Hello, World!");
        }

        [TestMethod]
        public void VariableInteger()
        {
            NumberValue value = (NumberValue)TestCode(@"let testvar = 69420", false, RuntimeValueType.Number, null);
            Assert.IsTrue(value.Value == 69420);
        }

        [TestMethod]
        public void VariableDouble()
        {
            NumberValue value = (NumberValue)TestCode(@"let testvar = 69.420", false, RuntimeValueType.Number, null);
            Assert.IsTrue(value.Value == 69.42);
        }

        [TestMethod]
        public void VariableBooleanTrue()
        {
            BooleanValue value = (BooleanValue)TestCode(@"let testvar = true", false, RuntimeValueType.Boolean, null);
            Assert.IsTrue(value.Value);
        }

        [TestMethod]
        public void VariableBooleanFalse()
        {
            BooleanValue value = (BooleanValue)TestCode(@"let testvar = false", false, RuntimeValueType.Boolean, null);
            Assert.IsFalse(value.Value);
        }
    }
}