using Stenguage.Errors;
using Stenguage.Runtime;
using Stenguage.Runtime.Values;

namespace Stenguage.Ast.Expressions
{
    public class BinaryExpr : Expr
    {
        public Expr Left { get; set; }
        public Expr Right { get; set; }
        public string Operator { get; set; }

        public BinaryExpr(Expr left, Expr right, string op, Position start, Position end) : base(NodeType.BinaryExpr, start, end)
        {
            Left = left;
            Right = right;
            Operator = op;
        }

        public override RuntimeResult Evaluate(Runtime.Environment env)
        {
            RuntimeResult res = new RuntimeResult();
            RuntimeValue left = res.Register(Left.Evaluate(env));
            if (res.ShouldReturn()) return res;
            RuntimeValue right = res.Register(Right.Evaluate(env));
            if (res.ShouldReturn()) return res;

            Context ctx = new Context(Start, End, env);

            switch (Operator)
            {
                // Arithmetic
                case "+":
                    return left.Add(right, ctx);
                case "-":
                    return left.Sub(right, ctx);
                case "*":
                    return left.Mul(right, ctx);
                case "/":
                    return left.Div(right, ctx);
                case "%":
                    return left.Mod(right, ctx);
                case "^":
                    return left.Pow(right, ctx);

                // Comparisons
                case "==":
                    return left.CompareEE(right, ctx);
                case "!=":
                    return left.CompareNE(right, ctx);
                case "<":
                    return left.CompareLT(right, ctx);
                case "<=":
                    return left.CompareLTE(right, ctx);
                case ">":
                    return left.CompareGT(right, ctx);
                case ">=":
                    return left.CompareGTE(right, ctx);
                case "&&":
                    return left.And(right, ctx);
                case "||":
                    return left.Or(right, ctx);
                default:
                    return res.Failure(new Error($"Unknown operator '{Operator}'", env.SourceCode, Start, End));
            }
        }

    }

}
