using Stenguage.Runtime;

namespace Stenguage.Ast.Expressions
{
    public class ContinueExpr : Expr
    {
        public ContinueExpr(Position start, Position end) : base(NodeType.Continue, start, end)
        {
        }

        public override RuntimeResult Evaluate(Runtime.Environment env)
        {
            return new RuntimeResult().SuccessContinue();
        }
    }

}
