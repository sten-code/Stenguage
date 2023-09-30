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
            return new RuntimeResult().Success(env.DeclareVar(Name, new FunctionValue(Name, Parameters, env, Body, env.SourceCode, Start, End), true));
        }
    }

}
