using Stenguage.Errors;
using Stenguage.Runtime;
using Stenguage.Runtime.Values;

namespace Stenguage.Ast.Expressions
{
    public class WhileLoopExpr : Expr
    {
        public Expr Condition { get; set; }
        public List<Expr> Body { get; set; }

        public WhileLoopExpr(Expr condition, List<Expr> body, Position start, Position end) : base(NodeType.WhileLoop, start, end)
        {
            Condition = condition;
            Body = body;
        }

        public override RuntimeResult Evaluate(Runtime.Environment env)
        {
            RuntimeResult res = new RuntimeResult();
            RuntimeValue conditionValue = res.Register(Condition.Evaluate(env));
            if (res.ShouldReturn()) return res;

            if (conditionValue.Type != Runtime.Values.RuntimeValueType.Boolean)
            {
                return res.Failure(new Error($"Condition must be a boolean, got '{conditionValue.Type}'.", env.SourceCode, Condition.Start, Condition.End));
            }
            BooleanValue condition = (BooleanValue)conditionValue;

            while (true)
            {
                if (!condition.Value)
                    break;

                Runtime.Environment scope = new Runtime.Environment(env.SourceCode, env);
                foreach (Expr expr in Body)
                {
                    RuntimeValue result = res.Register(expr.Evaluate(scope));
                    if (res.ReturnValue != null)
                        return res.Success(result);
                    if (res.ShouldReturn())
                    {
                        if (res.LoopBreak || res.LoopContinue)
                            res.Reset();
                        return res;
                    }
                }

                conditionValue = res.Register(Condition.Evaluate(env));
                if (res.ShouldReturn()) return res;
                condition = (BooleanValue)conditionValue;
            }

            return res.Success(new NullValue(env.SourceCode, new Position(0, 0, 0), new Position(0, 0, 0)));
        }
    }

}
