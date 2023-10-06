using Stenguage.Errors;
using Stenguage.Runtime;
using Stenguage.Runtime.Values;

namespace Stenguage.Ast.Expressions
{
    public class ClassDeclaration : Expr
    {
        public string Name { get; set; }
        public List<Expr> Body { get; set; }

        public ClassDeclaration(string name, List<Expr> body, Position start, Position end) : base(NodeType.ClassDeclaration, start, end)
        {
            Name = name;
            Body = body;
        }

        public RuntimeResult GetObject(Runtime.Environment env)
        {
            RuntimeResult res = new RuntimeResult();
            foreach (Expr expr in Body)
            {
                switch (expr.Kind)
                {
                    case NodeType.FunctionDeclaration:
                        FunctionDeclaration fnDecl = (FunctionDeclaration)expr;
                        FunctionValue fnValue = new FunctionValue(fnDecl.Name, fnDecl.Parameters, env, fnDecl.Body, env.SourceCode, Start, End);
                        env.DeclareVar(fnDecl.Name, fnValue, true);
                        break;
                    case NodeType.VarDeclaration:
                        VarDeclaration varDecl = (VarDeclaration)expr;
                        RuntimeValue value = res.Register(varDecl.Value.Evaluate(env));
                        if (res.ShouldReturn()) return res;
                        env.DeclareVar(varDecl.Identifier, value, varDecl.Constant);
                        break;
                    default:
                        return res.Failure(new Error($"You can't have a '{expr.Kind}' inside a class declaration.", env.SourceCode, Start, End));
                }
            }
            return new RuntimeResult().Success(new ObjectValue(env.SourceCode, Start, End, env.Variables));
        }

        public override RuntimeResult Evaluate(Runtime.Environment env)
        {
            bool constructor = false;
            foreach (Expr expr in Body)
            {
                if (expr.Kind == NodeType.FunctionDeclaration)
                {
                    FunctionDeclaration fnDecl = (FunctionDeclaration)expr;
                    if (fnDecl.Name == Name)
                    {
                        // This means that its a constructor function
                        constructor = true;
                        FunctionValue fnValue = new FunctionValue(fnDecl.Name, fnDecl.Parameters, env, fnDecl.Body, env.SourceCode, expr.Start, expr.End);
                        env.DeclareVar(Name, new NativeFnValue((args, scope, start, end) =>
                        {
                            RuntimeResult res = new RuntimeResult();
                            if (args.Count != fnValue.Parameters.Count)
                                return res.Failure(new Error($"Invalid parameter count, expected {fnValue.Parameters.Count}, got {args.Count}.", scope.SourceCode, start, end));

                            Runtime.Environment newScope = new Runtime.Environment(env.SourceCode, env);
                            for (int i = 0; i < fnValue.Parameters.Count; i++)
                            {
                                newScope.DeclareVar(fnValue.Parameters[i], args[i], false);
                            }

                            RuntimeValue value = res.Register(GetObject(newScope));
                            if (res.ShouldReturn()) return res;

                            foreach (Expr expr in fnValue.Body)
                            {
                                res.Register(expr.Evaluate(newScope));
                                if (res.ShouldReturn()) return res;
                            }

                            return res.Success(value);
                        }), true);
                    }
                }
            }
            if (!constructor)
            {
                env.DeclareVar(Name, new NativeFnValue((args, scope, start, end) =>
                {
                    Runtime.Environment newScope = new Runtime.Environment(env.SourceCode, env);
                    return GetObject(newScope);
                }), true);
            }
            return RuntimeResult.Null(env.SourceCode, Start, End);
        }

    }
}
