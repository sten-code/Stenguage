using Stenguage.Runtime;

namespace Stenguage.Ast.Expressions
{
    public class Identifier : Expr
    {
        public string Symbol { get; set; }

        public Identifier(string symbol, Position start, Position end) : base(NodeType.Identifier, start, end)
        {
            Symbol = symbol;
        }

        public override RuntimeResult Evaluate(Runtime.Environment env)
        {
            return new RuntimeResult().Success(env.LookupVar(Symbol));
        }
    }

}
