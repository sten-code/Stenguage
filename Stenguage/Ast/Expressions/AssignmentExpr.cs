using Stenguage.Errors;
using Stenguage.Runtime;
using Stenguage.Runtime.Values;

namespace Stenguage.Ast.Expressions
{
    public class AssignmentExpr : Expr
    {
        public Expr Assigne { get; set; }
        public Expr Value { get; set; }

        public AssignmentExpr(Expr assigne, Expr value, Position start, Position end) : base(NodeType.AssignmentExpr, start, end)
        {
            Assigne = assigne;
            Value = value;
        }

        public override RuntimeResult Evaluate(Runtime.Environment env)
        {
            RuntimeResult res = new RuntimeResult();
            RuntimeValue value = res.Register(Value.Evaluate(env));
            if (res.ShouldReturn()) return res;

            if (Assigne.Kind == NodeType.Identifier)
            {
                string name = ((Identifier)Assigne).Symbol;
                RuntimeValue newValue = env.AssignVar(name, value);
                if (newValue == null)
                    return res.Failure(new Error($"Couldn't assign '{name}', it either doesn't exist or it's a constant.", env.SourceCode, Start, End));
                return res.Success(newValue);
            }
            else if (Assigne.Kind == NodeType.MemberExpr)
            {
                MemberExpr memberExpr = (MemberExpr)Assigne;
                RuntimeValue val = res.Register(memberExpr.Object.Evaluate(env));
                if (res.ShouldReturn()) return res;

                RuntimeValue index;
                if (memberExpr.Property.Kind == NodeType.Identifier)
                {
                    if (val.Type == RuntimeValueType.Object)
                    {
                        index = new StringValue(((Identifier)memberExpr.Property).Symbol);
                    }
                    else
                    {
                        string var = ((Identifier)memberExpr.Property).Symbol;
                        index = env.LookupVar(var);
                        if (index == null) return res.Failure(new Error($"Cannot resolve '{var}', because it doesn't exist.", env.SourceCode, Start, End));
                    }
                }
                else
                {
                    index = res.Register(memberExpr.Property.Evaluate(env));
                    if (res.ShouldReturn()) return res;
                }

                RuntimeValue result = res.Register(val.SetIndex(index, value, new Context(memberExpr.Start, memberExpr.End, env)));
                if (res.ShouldReturn()) return res;

                return res.Success(result);
            }

            return res.Failure(new Error($"Can only assign a value to an identifier, got '{Assigne.Kind}'.", env.SourceCode, Start, End));
        }

    }
}
