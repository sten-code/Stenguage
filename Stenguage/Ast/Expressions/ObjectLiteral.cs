using Stenguage.Runtime;
using Stenguage.Runtime.Values;

namespace Stenguage.Ast.Expressions
{
    public class ObjectLiteral : Expr
    {
        public List<Property> Properties { get; set; }

        public ObjectLiteral(List<Property> properties, Position start, Position end) : base(NodeType.ObjectLiteral, start, end)
        {
            Properties = properties;
        }

        public override RuntimeResult Evaluate(Runtime.Environment env)
        {
            RuntimeResult res = new RuntimeResult();
            ObjectValue objValue = new ObjectValue(new Dictionary<string, RuntimeValue>(), env.SourceCode, Start, End);

            foreach (Property property in Properties)
            {
                RuntimeValue value;
                if (property.Value == null)
                {
                    value = env.LookupVar(property.Key);
                }
                else
                {
                    value = res.Register(property.Value.Evaluate(env));
                    if (res.ShouldReturn()) return res;
                }
                objValue.Properties[property.Key] = value;
            }

            return res.Success(objValue);
        }
    }

}
