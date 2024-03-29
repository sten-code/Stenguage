﻿using Stenguage.Errors;
using Stenguage.Runtime;
using Stenguage.Runtime.Values;

namespace Stenguage.Ast.Expressions
{
    public class FunctionDeclaration : Expr
    {
        public List<string> Parameters { get; set; }
        public string Name { get; set; }
        public List<Expr> Body { get; set; }

        public FunctionDeclaration(List<string> parameters, string name, List<Expr> body, Position start, Position end) : base(NodeType.FunctionDeclaration, start, end)
        {
            Parameters = parameters;
            Name = name;
            Body = body;
        }

        public override RuntimeResult Evaluate(Runtime.Environment env)
        {
            RuntimeValue value = env.DeclareVar(Name, new FunctionValue(Name, Parameters, env, Body), true);
            if (value == null)
                return new RuntimeResult().Failure(new Error("Variable already exists", env.SourceCode, Start, End));
            return new RuntimeResult().Success(value);
        }
    }

}
