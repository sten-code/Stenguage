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
            if (fn.Type == RuntimeValueType.Function)
            {
                FunctionBase func = (FunctionBase)fn;
                return func.Call(args, new Context(Start, End, fn.GetType() == typeof(FunctionValue) ? ((FunctionValue)fn).Environment : env));
            }

            return res.Failure(new Error("Cannot call a value that is not a function.", env.SourceCode, Start, End));
        }
    }

}
