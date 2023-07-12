using Stenguage.Runtime;
using Stenguage.Runtime.Values;
using System.Xml.Linq;

namespace Stenguage.Ast.Expressions
{
    public class NumericLiteral : Expr
    {
        public double Value { get; set; }

        public NumericLiteral(double value, Position start, Position end) : base(NodeType.NumericLiteral, start, end)
        {
            Value = value;
        }

        public override RuntimeResult Evaluate(Runtime.Environment env)
        {
            return new RuntimeResult().Success(new NumberValue(Value, env.SourceCode, Start, End));
        }
    }

}
