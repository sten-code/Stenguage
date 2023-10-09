using Stenguage.Ast;
using Stenguage.Runtime.Values;
using Stenguage.Runtime;
using Stenguage.Errors;

namespace Stenguage.Tests
{
    public class Test
    {
        public static RuntimeValue TestCode(string code, bool shouldReturn, RuntimeValueType returnType, Error expectedError)
        {
            // Generate AST
            ParseResult parseResult = new Parser(code).ProduceAST();

            // Did an error occur during parsing (syntax error)
            Assert.IsTrue(parseResult.ShouldReturn() == shouldReturn);

            // Run the code
            RuntimeResult result = parseResult.Expr.Evaluate(new Runtime.Environment(code));

            // Should the code have resulted in an error?
            if (expectedError != null)
                Assert.IsTrue(result.Error.Equals(expectedError));

            // What should have been returned
            Assert.IsTrue(result.Value.Type == returnType);

            return result.Value;
        }
    }
}
