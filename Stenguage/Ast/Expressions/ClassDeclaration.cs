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
                        FunctionValue fnValue = new FunctionValue(fnDecl.Name, fnDecl.Parameters, env, fnDecl.Body, env.SourceCode);
                        if (env.DeclareVar(fnDecl.Name, fnValue, true) == null)
                            return res.Failure(new Error("Variable already exists", env.SourceCode, fnDecl.Start, fnDecl.End));
                        break;
                    case NodeType.VarDeclaration:
                        VarDeclaration varDecl = (VarDeclaration)expr;
                        RuntimeValue value = res.Register(varDecl.Value.Evaluate(env));
                        if (res.ShouldReturn()) return res;
                        if (env.DeclareVar(varDecl.Identifier, value, varDecl.Constant) == null)
                            return res.Failure(new Error("Variable already exists", env.SourceCode, varDecl.Start, varDecl.End));
                        break;
                    default:
                        return res.Failure(new Error($"You can't have a '{expr.Kind}' inside a class declaration.", env.SourceCode, Start, End));
                }
            }
            return new RuntimeResult().Success(new ObjectValue(env.SourceCode, env.Variables));
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
                        FunctionValue fnValue = new FunctionValue(fnDecl.Name, fnDecl.Parameters, env, fnDecl.Body, env.SourceCode);
                        if (env.DeclareVar(Name, new NativeFnValue((args, scope, start, end) =>
                        {
                            RuntimeResult res = new RuntimeResult();
                            if (args.Count != fnValue.Parameters.Count)
                                return res.Failure(new Error($"Invalid parameter count, expected {fnValue.Parameters.Count}, got {args.Count}.", scope.SourceCode, start, end));

                            Runtime.Environment newScope = new Runtime.Environment(env.SourceCode, env);
                            Runtime.Environment fnScope = new Runtime.Environment(newScope.SourceCode, newScope);
                            for (int i = 0; i < fnValue.Parameters.Count; i++)
                            {
                                if (fnScope.DeclareVar(fnValue.Parameters[i], args[i], false) == null)
                                    return res.Failure(new Error("Variable already exists", scope.SourceCode, start, end));
                            }

                            RuntimeValue value = res.Register(GetObject(newScope));
                            if (res.ShouldReturn()) return res;

                            foreach (Expr expr in fnValue.Body)
                            {
                                res.Register(expr.Evaluate(fnScope));
                                if (res.ShouldReturn()) return res;
                            }

                            return res.Success(value);
                        }), true) == null)
                            return new RuntimeResult().Failure(new Error("Variable already exists", env.SourceCode, fnDecl.Start, fnDecl.End));
                    }
                } else if (expr.Kind != NodeType.VarDeclaration)
                {
                    return new RuntimeResult().Failure(new Error($"You can't have a '{expr.Kind}' inside a class declaration.", env.SourceCode, Start, End));
                }
            }
            if (!constructor)
            {
                if (env.DeclareVar(Name, new NativeFnValue((args, scope, start, end) =>
                {
                    Runtime.Environment newScope = new Runtime.Environment(env.SourceCode, env);
                    return GetObject(newScope);
                }), true) == null)
                    return new RuntimeResult().Failure(new Error("Variable already exists", env.SourceCode, Start, End));
            }
            return RuntimeResult.Null(env.SourceCode);
        }

    }
}
