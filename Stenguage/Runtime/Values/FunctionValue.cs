using Stenguage.Ast.Expressions;
using Stenguage.Errors;
using System;

namespace Stenguage.Runtime.Values
{
    public class FunctionValue : FunctionBase
    {
        public string Name { get; set; }
        public List<string> Parameters { get; set; }
        public Environment Environment { get; set; }
        public List<Expr> Body { get; set; }

        public FunctionValue(string name, List<string> parameters, Environment env, List<Expr> body)
        {
            Name = name;
            Parameters = parameters;
            Environment = env;
            Body = body;
        }

        public override string ValueString()
        {
            return ToString();
        }

        public override string ToString()
        {
            return Name;
        }

        public override RuntimeResult Call(List<RuntimeValue> args, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            Environment scope = new Environment(ctx.Env.SourceCode, ctx.Env);
            for (int i = 0; i < Parameters.Count; i++)
            {
                if (scope.DeclareVar(Parameters[i], args[i], false) == null)
                    return res.Failure(new Error("Variable already exists", ctx.Env.SourceCode, ctx.Start, ctx.End));
            }

            foreach (Expr expr in Body)
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
            return RuntimeResult.Null();
        }
    }
}
