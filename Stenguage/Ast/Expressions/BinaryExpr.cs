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

            switch (Operator)
            {
                // Arithmetic
                case "+":
                    return left.Add(right, Start, End);
                case "-":
                    return left.Sub(right, Start, End);
                case "*":
                    return left.Mul(right, Start, End);
                case "/":
                    return left.Div(right, Start, End);
                case "%":
                    return left.Mod(right, Start, End);
                case "^":
                    return left.Pow(right, Start, End);

                // Comparisons
                case "==":
                    return left.CompareEE(right, Start, End);
                case "!=":
                    return left.CompareNE(right, Start, End);
                case "<":
                    return left.CompareLT(right, Start, End);
                case "<=":
                    return left.CompareLTE(right, Start, End);
                case ">":
                    return left.CompareGT(right, Start, End);
                case ">=":
                    return left.CompareGTE(right, Start, End);
                case "&&":
                    return left.And(right, Start, End);
                case "||":
                    return left.Or(right, Start, End);
                default:
                    return res.Failure(new Error($"Unknown operator '{Operator}'", env.SourceCode, Start, End));
            }
        }

    }

}
