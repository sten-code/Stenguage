using Stenguage.Runtime;

namespace Stenguage.Ast.Expressions
{
    public class BreakExpr : Expr
    {
        public BreakExpr(Position start, Position end) : base(NodeType.Break, start, end)
        { }

        public override RuntimeResult Evaluate(Runtime.Environment env)
        {
            return new RuntimeResult().SuccessBreak();
        }
    }

}
