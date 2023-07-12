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
                return res.Success(env.AssignVar(((Identifier)Assigne).Symbol, value));
            }
            else if (Assigne.Kind == NodeType.MemberExpr)
            {
                MemberExpr memberExpr = (MemberExpr)Assigne;
                RuntimeValue val = res.Register(memberExpr.Object.Evaluate(env));
                if (res.ShouldReturn()) return res;

                RuntimeValue index;
                if (memberExpr.Property.Kind == NodeType.Identifier)
                {
                    index = new StringValue(((Identifier)memberExpr.Property).Symbol, env.SourceCode, memberExpr.Start, memberExpr.End);
                }
                else
                {
                    index = res.Register(memberExpr.Property.Evaluate(env));
                    if (res.ShouldReturn()) return res;
                }

                RuntimeValue result = res.Register(val.SetIndex(index, value, memberExpr.Start, memberExpr.End));
                if (res.ShouldReturn()) return res;

                return res.Success(result);
            }

            return res.Failure(new Error($"Can only assign a value to an identifier, got '{Assigne.Kind}'.", env.SourceCode, Start, End));
        }
    }
}
