using Stenguage.Errors;
using Stenguage.Runtime;
using Stenguage.Runtime.Values;
using System.Reflection;

namespace Stenguage.Ast.Expressions
{
    public class ImportExpr : Expr
    {
        public string Import { get; set; }
        public bool IsStaticImport { get; set; }
        public List<string> Names { get; set; }

        public ImportExpr(string import, bool isStaticImport, List<string> names, Position start, Position end) : base(NodeType.Import, start, end)
        {
            Import = import;
            IsStaticImport = isStaticImport;
            Names = names;
        }

        public RuntimeResult ImportFile(string file, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();
            if (!File.Exists(file))
                return res;

            string ext = Path.GetExtension(file);
            switch (ext)
            {
                case ".dll":
                    res.Register(ImportDLLFile(file, ctx, IsStaticImport, Names));
                    if (res.ShouldReturn()) return res;
                    break;
                case ".sten":
                    res.Register(ImportStenFile(file, ctx, IsStaticImport, Names));
                    if (res.ShouldReturn()) return res;
                    break;
            }

            return res;
        }

        public RuntimeResult ImportStenFile(string file, Context ctx, bool isStaticImport, List<string> names)
        {
            RuntimeResult res = new RuntimeResult();
            if (!File.Exists(file))
                return res.Failure(new Error($"Couldn't find file '{file}'", ctx.Env.SourceCode, ctx.Start, ctx.End));

            if (names.Count > 0)
                isStaticImport = true;

            string code = File.ReadAllText(file);
            ParseResult script = new Parser(code).ProduceAST();
            if (script.Error != null) return res.Failure(script.Error);
            if (script.Expr == null) return res;

            Runtime.Environment localEnv = new Runtime.Environment(code);
            RuntimeResult result = script.Expr.Evaluate(localEnv);
            if (isStaticImport)
            {
                if (names.Count > 0)
                {
                    foreach (string name in names)
                    {
                        if (localEnv.Variables.ContainsKey(name))
                            ctx.Env.DeclareVar(name, localEnv.Variables[name], true);
                        else
                            return res.Failure(new Error($"The name '{name}' doesn't exist in '{file}'.", ctx.Env.SourceCode, ctx.Start, ctx.End));
                    }
                } else
                {
                    foreach (KeyValuePair<string, RuntimeValue> variable in localEnv.Variables)
                    {
                        // Only import it if its currently undefined
                        if (ctx.Env.LookupVar(variable.Key) == null)
                            ctx.Env.DeclareVar(variable.Key, variable.Value, true);
                    }
                }
            }
            else
            {
                ctx.Env.DeclareVar(Path.GetFileNameWithoutExtension(file), new ObjectValue(localEnv.Variables), true);
            }
            return result;
        }

        public RuntimeResult ImportDLLFile(string file, Context ctx, bool isStaticImport, List<string> names)
        {
            RuntimeResult res = new RuntimeResult();
            if (names.Count > 0)
                isStaticImport = true;

            Assembly dll;
            try
            {
                dll = Assembly.LoadFile(file);
            } 
            catch (FileLoadException)
            {
                return res.Failure(new Error("File load error", ctx.Env.SourceCode, ctx.Start, ctx.End));
            }
            catch (BadImageFormatException)
            {
                return res.Failure(new Error("Bad DLL image", ctx.Env.SourceCode, ctx.Start, ctx.End));
            }

            Dictionary<string, RuntimeValue> properties = new Dictionary<string, RuntimeValue>();

            foreach (Type type in dll.GetExportedTypes())
            {
                if (type.IsSubclassOf(typeof(ObjectValue)))
                {
                    // Declare a constructor
                    ctx.Env.DeclareVar(type.Name, new NativeFnValue((args, ctx) =>
                    {
                        ObjectValue obj = (ObjectValue)Activator.CreateInstance(type, args.ToArray());
                        obj.Init();
                        return new RuntimeResult().Success(obj);
                    }), true);

                    continue;
                }

                foreach (MethodInfo method in type.GetMethods())
                {
                    ParameterInfo[] parameters = method.GetParameters();

                    // only import methods that are marked as static
                    if (!method.IsStatic)
                        continue;

                    // only import methods that return a runtimeresult
                    if (method.ReturnType != typeof(RuntimeResult))
                        continue;

                    if (names.Count > 0)
                    {
                        if (!names.Contains(method.Name))
                            continue;
                    }

                    // If its a static import it will be defined differently than a normal import
                    bool isDefined;
                    RuntimeValue value;
                    if (isStaticImport)
                    {
                        value = ctx.Env.LookupVar(method.Name);
                        isDefined = value != null;
                    }
                    else
                    {
                        isDefined = properties.ContainsKey(method.Name);
                        value = isDefined ? properties[method.Name] : null;
                    }

                    if (isDefined)
                    {
                        // Reassign the value to automatically check which type it corrosponds to
                        NativeFnValue newFn = new NativeFnValue((args, ctx) =>
                        {
                            RuntimeResult res = new RuntimeResult();
                            if (args.Count != parameters.Length - 1)
                            {
                                // Its not this method so just call the other method;
                                return ((NativeFnValue)value).Call(args, ctx);
                            }

                            for (int i = 0; i < args.Count; i++)
                            {
                                Type expected = parameters[i + 1].ParameterType;
                                Type got = args[i].GetType();
                                if (expected != got && expected != typeof(RuntimeValue))
                                    return ((NativeFnValue)value).Call(args, ctx);
                            }

                            return (RuntimeResult)method.Invoke(null, args.ToArray());
                        });

                        if (isStaticImport)
                            ctx.Env.AssignVar(method.Name, newFn);
                        else
                            properties[method.Name] = newFn;
                    }
                    else
                    {
                        NativeFnValue fn = new NativeFnValue((args, ctx) =>
                        {
                            RuntimeResult res = new RuntimeResult();
                            if (args.Count != parameters.Length - 1)
                                return res.Failure(new Error($"Invalid parameter count, expected {parameters.Length}, got {args.Count}.", ctx.Env.SourceCode, ctx.Start, ctx.End));

                            for (int i = 0; i < args.Count; i++)
                            {
                                Type expected = parameters[i + 1].ParameterType;
                                Type got = args[i].GetType();
                                if (expected != got)
                                    return res.Failure(new Error($"Invalid parameter type, expected a '{expected.Name}' type, got '{got.Name}'.", ctx.Env.SourceCode, ctx.Start, ctx.End));
                            }

                            return (RuntimeResult)method.Invoke(null, new object[] { ctx }.Concat(args).ToArray());
                        });

                        if (isStaticImport)
                            ctx.Env.DeclareVar(method.Name, fn, false);
                        else
                            properties[method.Name] = fn;
                    }
                }
            }

            if (!isStaticImport)
                ctx.Env.DeclareVar(Path.GetFileNameWithoutExtension(file), new ObjectValue(properties), true);

            return res;
        }

        public (RuntimeResult, bool) ImportFromOrigin(string origin, Context ctx)
        {
            RuntimeResult res = new RuntimeResult();

            string path = Path.Combine(origin, Import);
            if (path[path.Length - 1] == '*')
            {
                path = path.Substring(0, path.Length - 1);
                if (!Directory.Exists(path))
                    return (res, false);

                foreach (string filename in Directory.GetFiles(path).Where(x => new string[] { ".sten", ".dll" }.Contains(Path.GetExtension(x))))
                {
                    res.Register(ImportFile(filename, ctx));
                    if (res.ShouldReturn()) return (res, false);
                }
            }
            else
            {
                if (!File.Exists(path + ".sten") && !File.Exists(path + ".dll"))
                    return (res, false);
                res.Register(ImportFile(path + ".sten", ctx));
                if (res.ShouldReturn()) return (res, false);
                res.Register(ImportFile(path + ".dll", ctx));
                if (res.ShouldReturn()) return (res, false);
            }

            return (RuntimeResult.Null(), true);
        }

        public override RuntimeResult Evaluate(Runtime.Environment env)
        {
            Context ctx = new Context(Start, End, env);
            RuntimeResult res = new RuntimeResult();
            bool found = false;

            (RuntimeResult r, bool success) = ImportFromOrigin(Directory.GetCurrentDirectory(), ctx);
            res.Register(r);
            if (res.ShouldReturn()) return res;
            if (success) found = true;

            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "libs");
            (r, success) = ImportFromOrigin(path, ctx);
            res.Register(r);
            if (res.ShouldReturn()) return res;
            if (success) found = true;

            if (!found)
                return res.Failure(new Error($"Couldn't find the file to import", env.SourceCode, Start, End));

            return RuntimeResult.Null();
        }
    }

}
