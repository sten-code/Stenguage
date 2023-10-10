using Stenguage.Runtime;
using Stenguage.Runtime.Values;

namespace Stenguage.Ast.Expressions
{
    public class MemberExpr : Expr
    {
        public Expr Object { get; set; }
        public Expr Property { get; set; }
        public bool Computed { get; set; }

        public MemberExpr(Expr obj, Expr property, bool computed, Position start, Position end) : base(NodeType.MemberExpr, start, end)
        {
            Object = obj;
            Property = property;
            Computed = computed;
        }

        public override RuntimeResult Evaluate(Runtime.Environment env)
        {
            RuntimeResult res = new RuntimeResult();
            RuntimeValue val = res.Register(Object.Evaluate(env));
            if (res.ShouldReturn()) return res;

            RuntimeValue index;
            if (Computed)
            {
                index = res.Register(Property.Evaluate(env));
                if (res.ShouldReturn()) return res;
            }
            else
            {
                index = new StringValue(((Identifier)Property).Symbol);
            }

            RuntimeValue result = res.Register(val.GetIndex(index, new Context(Start, End, env)));
            if (res.ShouldReturn()) return res;
            return res.Success(result);
        }
    }

}
