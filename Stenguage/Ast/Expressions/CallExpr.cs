using Stenguage.Errors;
using Stenguage.Runtime;
using Stenguage.Runtime.Values;

namespace Stenguage.Ast.Expressions
{
    public class CallExpr : Expr
    {
        public List<Expr> Args { get; set; }
        public Expr Caller { get; set; }

        public CallExpr(List<Expr> args, Expr caller, Position start, Position end) : base(NodeType.CallExpr, start, end)
        {
            Args = args;
            Caller = caller;
        }

        public override RuntimeResult Evaluate(Runtime.Environment env)
        {
            RuntimeResult res = new RuntimeResult();
            List<RuntimeValue> args = new List<RuntimeValue>();
            foreach (Expr arg in Args)
            {
                RuntimeValue value = res.Register(arg.Evaluate(env));
                if (res.ShouldReturn()) return res;
                args.Add(value);
            }

            RuntimeValue fn = res.Register(Caller.Evaluate(env));
            if (res.ShouldReturn()) return res;
            if (fn.Type == RuntimeValueType.NativeFn)
            {
                NativeFnValue fnValue = (NativeFnValue)fn;
                RuntimeValue value = res.Register(fnValue.Call(args, env, Start, End));
                if (res.ShouldReturn()) return res;
                return res.Success(value);
            }
            else if (fn.Type == RuntimeValueType.Function)
            {
                FunctionValue fnValue = (FunctionValue)fn;
                Runtime.Environment scope = new Runtime.Environment(env.SourceCode, fnValue.Environment);
                for (int i = 0; i < fnValue.Parameters.Count; i++)
                {
                    scope.DeclareVar(fnValue.Parameters[i], args[i], false);
                }

                foreach (Expr expr in fnValue.Body)
                {
                    res.Register(expr.Evaluate(scope));
                    if (res.ReturnValue != null)
                    {
                        RuntimeValue returnValue = res.ReturnValue;
                        res.Reset();
                        return res.Success(returnValue);
                    }
                    if (res.ShouldReturn()) return res;
                }
                return res.Success(new NullValue(env.SourceCode, new Position(0, 0, 0), new Position(0, 0, 0)));
            }

            return res.Failure(new Error("Cannot call a value that is not a function.", env.SourceCode, Start, End));
        }
    }

}
