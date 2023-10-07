using Stenguage.Errors;
using Stenguage.Json;
using Stenguage.Runtime;
using Stenguage.Runtime.Values;

namespace Stenguage.Ast.Expressions
{
    public class ForLoopExpr : Expr
    {
        public string ItemName { get; set; }
        public Expr Iterator { get; set; }
        public List<Expr> Body { get; set; }

        public ForLoopExpr(string itemName, Expr iterator, List<Expr> body, Position start, Position end) : base(NodeType.ForLoop, start, end)
        {
            ItemName = itemName;
            Iterator = iterator;
            Body = body;
        }

        public override RuntimeResult Evaluate(Runtime.Environment env)
        {
            RuntimeResult res = new RuntimeResult();
            RuntimeValue iteratorValue = res.Register(Iterator.Evaluate(env));
            if (res.ShouldReturn()) return res;

            (RuntimeResult r, List<RuntimeValue> iterator) = iteratorValue.Iterate(Iterator.Start, Iterator.End);
            res.Register(r);
            if (res.ShouldReturn()) return res;

            if (iterator.Count == 0)
                return res.Success(new NullValue(env.SourceCode, new Position(0, 0, 0), new Position(0, 0, 0)));

            int skipCount = 0;
            foreach (RuntimeValue item in iterator)
            {
                if (skipCount > 0)
                {
                    skipCount--;
                    continue;
                }
                Runtime.Environment scope = new Runtime.Environment(env.SourceCode, env);
                if (scope.DeclareVar(ItemName, item, true) == null)
                    return res.Failure(new Error("Variable already exists", env.SourceCode, Start, End));
                foreach (Expr expr in Body)
                {
                    RuntimeValue result = res.Register(expr.Evaluate(scope));
                    if (res.Error != null) return res;

                    if (res.ReturnValue != null)
                        return res.Success(result);

                    if (res.SkipValue != null)
                    {
                        skipCount = (int)res.SkipValue.Value;
                        res.SkipValue = null;
                        break;
                    }
                    if (res.LoopContinue)
                    {
                        res.Reset();
                        break;
                    }
                    if (res.LoopBreak)
                    {
                        res.Reset();
                        return res;
                    }

                    if (res.ShouldReturn()) return res;
                }
            }

            return res.Success(new NullValue(env.SourceCode, new Position(0, 0, 0), new Position(0, 0, 0)));
        }
    }

}
