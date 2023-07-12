using Stenguage.Ast.Expressions;

namespace Stenguage.Ast
{
    public class Property
    {
        public string Key { get; set; }
        public Expr Value { get; set; }

        public Property(string key, Expr value, Position start, Position end)
        {
            Key = key;
            Value = value;
        }
    }

}
