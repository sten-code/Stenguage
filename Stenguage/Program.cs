using Stenguage.Ast;
using Stenguage.Json;
using Stenguage.Runtime;
using Stenguage.Runtime.Values;

namespace Stenguage
{
    public class Application
    {
        public static void LogResult(RuntimeValue result)
        {
            switch (result.Type)
            {
                case RuntimeValueType.Function:
                    Console.WriteLine($"<Function>");
                    break;
                case RuntimeValueType.Object:
                    ObjectValue obj = (ObjectValue)result;
                    foreach (KeyValuePair<string, RuntimeValue> val in obj.Properties)
                    {
                        Console.Write(val.Key + " ");
                        LogResult(val.Value);
                    }
                    break;
                default:
                    Console.WriteLine(result.ToString());
                    break;
            }
        }

        public static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                string code = File.ReadAllText(args[0]);
                ParseResult parseResult = new Parser(code).ProduceAST();
                if (parseResult.ShouldReturn())
                {
                    Console.WriteLine(parseResult.Error);
                    return;
                }
                //Console.WriteLine(parseResult.ToJson().FormatJson());

                RuntimeResult result = parseResult.Expr.Evaluate(new Runtime.Environment(code));
                if (result.Error != null)
                {
                    Console.WriteLine(result.Error);
                }
                return;
            }

            Console.WriteLine("Stenguage v0.1");

            Runtime.Environment env = new Runtime.Environment("");

            while (true)
            {
                Console.Write(">>> ");
                string input = Console.ReadLine();
                if (input == null)
                    continue;
                env.SourceCode = input;

                ParseResult parseResult = new Parser(input).ProduceAST();
                if (parseResult.ShouldReturn())
                {
                    Console.WriteLine(parseResult.Error);
                    continue;
                }
                //Console.WriteLine(parseResult.ToJson().FormatJson());

                // Run the code
                RuntimeResult result = parseResult.Expr.Evaluate(env);
                if (result.Error != null)
                {
                    Console.WriteLine(result.Error);
                }
                else if (result.Value.Type != Runtime.Values.RuntimeValueType.Null)
                {
                    LogResult(result.Value);
                }
            }
        }

    }
}