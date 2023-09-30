using Stenguage.Errors;
using Stenguage.Runtime;
using Stenguage.Runtime.Values;

namespace Stenguage.Ast.Expressions
{
    public class Identifier : Expr
    {
        public string Symbol { get; set; }

        public Identifier(string symbol, Position start, Position end) : base(NodeType.Identifier, start, end)
        {
            Symbol = symbol;
        }

        public override RuntimeResult Evaluate(Runtime.Environment env)
        {
            RuntimeResult res = new RuntimeResult();
            RuntimeValue value = env.LookupVar(Symbol);
            if (value == null) return res.Failure(new Error($"Cannot resolve '{Symbol}', because it doesn't exist.", env.SourceCode, Start, End));
            return res.Success(value);
        }
    }

}
