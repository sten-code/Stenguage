using Stenguage.Ast;
using Stenguage.Json;
using Stenguage.Runtime;
using System.Diagnostics;
using System.Reflection;

namespace Stenguage
{
    [Command("program", "Main program command")]
    class ProgramOptions
    {
        [Argument("v", "verbose", "Enable verbose mode")]
        public bool Verbose { get; set; }

        [Argument("i", "input", "The input file to run.", false, true)]
        public string InputFile { get; set; }

        // Sub-commands
        public InstallOptions Install { get; set; }
        public RunOptions Run { get; set; }
    }

    [Command("install", "Install Library through the package manager.")]
    class InstallOptions
    {
        [Argument("r", "repo", "The url to the GitHub Repository.", true, true)]
        public string Repo { get; set; }
    }

    [Command("run", "Run a Stenguage script.")]
    class RunOptions
    {
        [Argument("i", "input", "The input file to run.", true, true)]
        public string InputFile { get; set; }
    }

    public class Application
    {
        public static void Main(string[] args)
        {
            ArgumentParser parser = new ArgumentParser();
            ProgramOptions options = parser.Parse<ProgramOptions>(args);

            if (options.Install?.Repo != null)
            {
                Install(options.Install.Repo);
                return;
            }

            if (options.Run?.InputFile != null)
            {
                if (!File.Exists(options.Run.InputFile))
                {
                    Console.WriteLine($"The file \"{options.Run.InputFile}\" doesn't exist.");
                    return;
                }
                string code = File.ReadAllText(options.Run.InputFile);
                Run(code);
                return;
            }

            if (options.InputFile != null)
            {
                if (!File.Exists(options.InputFile))
                {
                    Console.WriteLine($"The file \"{options.InputFile}\" doesn't exist.");
                    return;
                }
                string code = File.ReadAllText(options.InputFile);
                Run(code);
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
                Run(input, env);
            }
        }

        private static void DeleteDirectory(string directory)
        {
            foreach (string subdirectory in Directory.EnumerateDirectories(directory))
            {
                DeleteDirectory(subdirectory);
            }

            foreach (string fileName in Directory.EnumerateFiles(directory))
            {
                var fileInfo = new FileInfo(fileName)
                {
                    Attributes = FileAttributes.Normal
                };
                fileInfo.Delete();
            }

            Directory.Delete(directory);
        }

        private static void Install(string repo)
        {
            if (!Uri.IsWellFormedUriString(repo, UriKind.Absolute))
                repo = "https://github.com/" + repo;

            string cache = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "cache");
            string name = new Uri(repo).PathAndQuery;
            string path = Path.Combine(cache, Path.Combine(string.Join("", name.Split('.').SkipLast(name.Count(c => c == '.'))).Split('/')));

            if (Directory.Exists(path))
                DeleteDirectory(path);

            Console.WriteLine($"Cloning {repo}");

            if (RunCmd("git", $"clone {repo} \"{path}\"") != 0)
                return;

            string[] solutionFiles = Directory.GetFiles(path, "*.sln", SearchOption.TopDirectoryOnly);
            if (solutionFiles.Length > 0)
            {
                if (BuildSolution(solutionFiles[0]))
                    Console.WriteLine("Successfully installed the library.");
                else
                    Console.WriteLine("Error: An error occurred while installing the library.");
            }
            else
            {
                Console.WriteLine("Error: No .sln files found in the downloaded repository.");
                return;
            }
        }

        private static int RunCmd(string file, string args, string workingDirectory = null)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = file;
                process.StartInfo.Arguments = args;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.WorkingDirectory = workingDirectory == null ? Directory.GetCurrentDirectory() : workingDirectory;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                Console.WriteLine(output);
                Console.WriteLine(error);

                process.WaitForExit();
                return process.ExitCode;
            }
        }

        private static bool BuildSolution(string solutionFile)
        {
            string cache = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "cache");
            string workingDir = Path.GetDirectoryName(solutionFile);
            string name = Path.GetFileNameWithoutExtension(solutionFile);

            if (RunCmd("dotnet", "restore", workingDir) != 0)
                return false;

            if (!Directory.Exists(Path.Combine(cache, "Stenguage")))
            {
                if (RunCmd("git", $"clone https://github.com/sten-code/Stenguage.git \"{Path.Combine(cache, "Stenguage")}\"") != 0)
                    return false;
            }

            if (RunCmd("dotnet", $"add \"{Path.Combine(name, $"{name}.csproj")}\" reference \"{Path.Combine(cache, "Stenguage", "Stenguage", "Stenguage.csproj")}\"", workingDir) != 0)
                return false;

            if (RunCmd("dotnet", "build -c Release --output build", workingDir) != 0)
                return false;

            File.Copy(
                Path.Combine(workingDir, "build", $"{name}.dll"), 
                Path.Combine(
                    Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                    "libs",
                    $"{name}.dll"
                ),
                true
            );
            return true;
        }

        private static void Run(string code, Runtime.Environment env = null)
        {
            ParseResult parseResult = new Parser(code).ProduceAST();
            if (parseResult.ShouldReturn())
            {
                Console.WriteLine(parseResult.Error);
                return;
            }
            //Console.WriteLine(parseResult.ToJson().FormatJson());

            RuntimeResult result = parseResult.Expr.Evaluate(env == null ? new Runtime.Environment(code) : env);
            if (result.Error != null)
            {
                Console.WriteLine(result.Error);
            }
        }

    }
}