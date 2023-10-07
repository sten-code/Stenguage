using Stenguage.Ast;
using Stenguage.Runtime;
using Stenguage.Runtime.Values;

namespace Stenguage.Tests
{
    [TestClass]
    public class VariableTests
    {
        [TestMethod]
        public void VariableString()
        {
            string code = @"let testvar = ""Hello, World!""";
            ParseResult parseResult = new Parser(code).ProduceAST();
            Assert.IsFalse(parseResult.ShouldReturn());
            RuntimeResult result = parseResult.Expr.Evaluate(new Runtime.Environment(code));
            Assert.IsNull(result.Error);
            Assert.IsTrue(result.Value.Type == RuntimeValueType.String);
            Assert.IsTrue(((StringValue)result.Value).Value == "Hello, World!");
        }

        [TestMethod]
        public void VariableInteger()
        {
            string code = @"let testvar = 69420";
            ParseResult parseResult = new Parser(code).ProduceAST();
            Assert.IsFalse(parseResult.ShouldReturn());
            RuntimeResult result = parseResult.Expr.Evaluate(new Runtime.Environment(code));
            Assert.IsNull(result.Error);
            Assert.IsTrue(result.Value.Type == RuntimeValueType.Number);
            Assert.IsTrue(((NumberValue)result.Value).Value == 69420);
        }

        [TestMethod]
        public void VariableDouble()
        {
            string code = @"let testvar = 69.420";
            ParseResult parseResult = new Parser(code).ProduceAST();
            Assert.IsFalse(parseResult.ShouldReturn());
            RuntimeResult result = parseResult.Expr.Evaluate(new Runtime.Environment(code));
            Assert.IsNull(result.Error);
            Assert.IsTrue(result.Value.Type == RuntimeValueType.Number);
            Assert.IsTrue(((NumberValue)result.Value).Value == 69.42);
        }

        [TestMethod]
        public void VariableBooleanTrue()
        {
            string code = @"let testvar = true";
            ParseResult parseResult = new Parser(code).ProduceAST();
            Assert.IsFalse(parseResult.ShouldReturn());
            RuntimeResult result = parseResult.Expr.Evaluate(new Runtime.Environment(code));
            Assert.IsNull(result.Error);
            Assert.IsTrue(result.Value.Type == RuntimeValueType.Boolean);
            Assert.IsTrue(((BooleanValue)result.Value).Value);
        }

        [TestMethod]
        public void VariableBooleanFalse()
        {
            string code = @"let testvar = false";
            ParseResult parseResult = new Parser(code).ProduceAST();
            Assert.IsFalse(parseResult.ShouldReturn());
            RuntimeResult result = parseResult.Expr.Evaluate(new Runtime.Environment(code));
            Assert.IsNull(result.Error);
            Assert.IsTrue(result.Value.Type == RuntimeValueType.Boolean);
            Assert.IsFalse(((BooleanValue)result.Value).Value);
        }
    }
}