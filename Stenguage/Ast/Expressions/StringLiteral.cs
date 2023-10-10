using Stenguage.Runtime;
using Stenguage.Runtime.Values;
using System.Xml.Linq;

namespace Stenguage.Ast.Expressions
{
    public class StringLiteral : Expr
    {
        public string Value { get; set; }

        public StringLiteral(string value, Position start, Position end) : base(NodeType.StringLiteral, start, end)
        {
            Value = value;
        }

        public override RuntimeResult Evaluate(Runtime.Environment env)
        {
            return new RuntimeResult().Success(new StringValue(Value));
        }
    }

}
