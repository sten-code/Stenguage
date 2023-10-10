using Stenguage.Runtime;
using Stenguage.Runtime.Values;
using System.Xml.Linq;

namespace Stenguage.Ast.Expressions
{
    public class VarDeclaration : Expr
    {
        public string Identifier { get; set; }
        public Expr Value { get; set; }
        public bool Constant { get; set; }

        public VarDeclaration(string identifier, Expr value, bool constant, Position start, Position end) : base(NodeType.VarDeclaration, start, end)
        {
            Identifier = identifier;
            Value = value;
            Constant = constant;
        }

        public override RuntimeResult Evaluate(Runtime.Environment env)
        {
            RuntimeResult res = new RuntimeResult();
            RuntimeValue value = new NullValue();
            if (Value != null)
            {
                value = res.Register(Value.Evaluate(env));
                if (res.ShouldReturn()) return res;
            }
            return res.Success(env.DeclareVar(Identifier, value, Constant));
        }
    }

}
