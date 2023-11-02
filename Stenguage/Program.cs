using Stenguage.Ast;
using Stenguage.Runtime;
using Stenguage.Runtime.Values;

namespace Stenguage
{
    class ProgramOptions
    {
        [Argument("v", "verbose", "Enable verbose mode", false)]
        public bool Verbose { get; set; }

        [Argument("i", "input", "Input file path", false, true)]
        public string InputFile { get; set; }
    }

    public class Application
    {
        public static void Main(string[] args)
        {
            ArgumentParser parser = new ArgumentParser();
            ProgramOptions options = parser.Parse<ProgramOptions>(args);

            if (options.Verbose)
            {
                Console.WriteLine("Verbose mode enabled");
            }

            if (options.InputFile != null)
            {
                if (!File.Exists(options.InputFile))
                {
                    Console.WriteLine($"The file \"{options.InputFile}\" doesn't exist.");
                    return;
                }
                string code = File.ReadAllText(options.InputFile);
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
                else if (result.Value.Type != RuntimeValueType.Null)
                {
                    Console.WriteLine(result.Value);
                }
            }
        }

    }
}