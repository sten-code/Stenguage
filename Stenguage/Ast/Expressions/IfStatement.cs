using Stenguage.Runtime;
using Stenguage.Runtime.Values;

namespace Stenguage.Ast.Expressions
{
    public class IfStatement : Expr
    {
        public Expr Condition { get; set; }
        public List<Expr> Body { get; set; }
        public List<Expr> ElseBody { get; set; }

        public IfStatement(Expr condition, List<Expr> body, List<Expr> elseBody, Position start, Position end) : base(NodeType.IfStatement, start, end)
        {
            Condition = condition;
            Body = body;
            ElseBody = elseBody;
        }

        public override RuntimeResult Evaluate(Runtime.Environment env)
        {
            RuntimeResult res = new RuntimeResult();
            RuntimeValue condition = res.Register(Condition.Evaluate(env));
            if (res.ShouldReturn()) return res;

            if (condition.Type != RuntimeValueType.Null && 
                condition.Type != RuntimeValueType.Number && 
                condition.Type != RuntimeValueType.Boolean || condition.Type == RuntimeValueType.Number && 
                ((NumberValue)condition).Value != 0 || condition.Type == RuntimeValueType.Boolean && ((BooleanValue)condition).Value)
            {
                RuntimeValue result = new NullValue(env.SourceCode);
                foreach (Expr expr in Body)
                {
                    result = res.Register(expr.Evaluate(env));
                    if (res.ShouldReturn()) return res;
                }
                return res.Success(result);
            }
            else
            {
                RuntimeValue result = new NullValue(env.SourceCode);
                foreach (Expr expr in ElseBody)
                {
                    result = res.Register(expr.Evaluate(env));
                    if (res.ShouldReturn()) return res;
                }
                return res.Success(result);
            }
        }
    }

}
