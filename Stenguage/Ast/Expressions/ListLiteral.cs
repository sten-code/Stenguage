using Stenguage.Runtime;
using Stenguage.Runtime.Values;

namespace Stenguage.Ast.Expressions
{
    public class ListLiteral : Expr
    {
        public List<Expr> Items { get; set; }

        public ListLiteral(List<Expr> items, Position start, Position end) : base(NodeType.ListLiteral, start, end)
        {
            Items = items;
        }

        public override RuntimeResult Evaluate(Runtime.Environment env)
        {
            RuntimeResult res = new RuntimeResult();
            List<RuntimeValue> list = new List<RuntimeValue>();
            foreach (Expr expr in Items)
            {
                RuntimeValue value = res.Register(expr.Evaluate(env));
                if (res.ShouldReturn()) return res;
                list.Add(value);
            }
            return res.Success(new ListValue(list));
        }
    }

}
