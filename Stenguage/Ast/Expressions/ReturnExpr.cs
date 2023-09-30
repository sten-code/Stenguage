using Stenguage.Runtime;
using Stenguage.Runtime.Values;

namespace Stenguage.Ast.Expressions
{
    public class ReturnExpr : Expr
    {
        public Expr Return { get; set; }

        public ReturnExpr(Expr retExpr, Position start, Position end) : base(NodeType.Return, start, end)
        {
            Return = retExpr;
        }

        public override RuntimeResult Evaluate(Runtime.Environment env)
        {
            RuntimeResult res = new RuntimeResult();
            RuntimeValue val = res.Register(Return.Evaluate(env));
            if (res.ShouldReturn()) return res;
            return res.SuccessReturn(val);
        }
    }

}
