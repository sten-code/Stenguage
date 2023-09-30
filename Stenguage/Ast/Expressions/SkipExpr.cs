using Stenguage.Errors;
using Stenguage.Runtime;
using Stenguage.Runtime.Values;

namespace Stenguage.Ast.Expressions
{
    public class SkipExpr : Expr
    {
        public Expr SkipAmount { get; set; }

        public SkipExpr(Expr skipAmount, Position start, Position end) : base(NodeType.Skip, start, end)
        {
            SkipAmount = skipAmount;
        }

        public override RuntimeResult Evaluate(Runtime.Environment env)
        {
            RuntimeResult res = new RuntimeResult();
            RuntimeValue amount = res.Register(SkipAmount.Evaluate(env));
            if (res.ShouldReturn()) return res;
            if (amount.Type != RuntimeValueType.Number)
            {
                return res.Failure(new Error("Can only skip on a number.", env.SourceCode, Start, End));
            }
            if (((NumberValue)amount).Value % 1 != 0)
            {
                return res.Failure(new Error("Can only skip on an integer.", env.SourceCode, Start, End));
            }
            return res.SuccessSkip((NumberValue)amount);
        }
    }

}
