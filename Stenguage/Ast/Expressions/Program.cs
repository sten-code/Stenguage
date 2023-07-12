using Stenguage.Runtime;
using Stenguage.Runtime.Values;

namespace Stenguage.Ast.Expressions
{
    public class Program : Expr
    {
        public List<Expr> Body { get; set; }

        public Program(List<Expr> body, Position start, Position end) : base(NodeType.Program, start, end)
        {
            Body = body;
        }

        public override RuntimeResult Evaluate(Runtime.Environment env)
        {
            RuntimeResult res = new RuntimeResult();
            RuntimeValue last = new NullValue(env.SourceCode, new Position(0, 0, 0), new Position(0, 0, 0));
            foreach (Expr expr in Body)
            {
                last = res.Register(expr.Evaluate(env));
                if (res.ShouldReturn()) return res;
            }
            return res.Success(last);
        }
    }

}
