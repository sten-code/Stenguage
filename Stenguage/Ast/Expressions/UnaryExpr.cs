using Stenguage.Errors;
using Stenguage.Runtime;
using Stenguage.Runtime.Values;

namespace Stenguage.Ast.Expressions
{
    public class UnaryExpr : Expr
    {
        public Expr Expr { get; set; }
        public string Operator { get; set; }

        public UnaryExpr(Expr expr, string op, Position start, Position end) : base(NodeType.UnaryExpr, start, end)
        {
            Expr = expr;
            Operator = op;
        }

        public override RuntimeResult Evaluate(Runtime.Environment env)
        {
            RuntimeResult res = new RuntimeResult();
            RuntimeValue value = res.Register(Expr.Evaluate(env));
            if (res.ShouldReturn()) return res;
            Context ctx = new Context(Start, End, env);
            switch (Operator)
            {
                case "-":
                    return value.Min(ctx);
                case "!":
                    return value.Not(ctx);
                default:
                    return res.Failure(new Error($"'{Operator}' isn't a valid unary operator.", env.SourceCode, Start, End));
            }
        }
    }

}
