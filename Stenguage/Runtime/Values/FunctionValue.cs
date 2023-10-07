using Stenguage.Ast.Expressions;

namespace Stenguage.Runtime.Values
{
    public class FunctionValue : RuntimeValue
    {
        public string Name { get; set; }
        public List<string> Parameters { get; set; }
        public Environment Environment { get; set; }
        public List<Expr> Body { get; set; }

        public FunctionValue(string name, List<string> parameters, Environment env, List<Expr> body, string sourceCode) : base(RuntimeValueType.Function, sourceCode)
        {
            Name = name;
            Parameters = parameters;
            Environment = env;
            Body = body;
        }

        public override string ValueString()
        {
            return ToString();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
