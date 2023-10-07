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

        public RuntimeResult ImportFile(string file, Runtime.Environment env)
        {
            RuntimeResult res = new RuntimeResult();
            if (!File.Exists(file))
                return res;

            string ext = Path.GetExtension(file);
            switch (ext)
            {
                case ".dll":
                    res.Register(ImportDLLFile(file, env, IsStaticImport, Names));
                    if (res.ShouldReturn()) return res;
                    break;
                case ".sten":
                    res.Register(ImportStenFile(file, env, IsStaticImport, Names));
                    if (res.ShouldReturn()) return res;
                    break;
            }

            return res;
        }

        public RuntimeResult ImportStenFile(string file, Runtime.Environment env, bool isStaticImport, List<string> names)
        {
            RuntimeResult res = new RuntimeResult();
            if (!File.Exists(file))
                return res.Failure(new Error($"Couldn't find file '{file}'", env.SourceCode, Start, End));

            if (names.Count > 0)
                isStaticImport = true;

            string code = File.ReadAllText(file);
            ParseResult r = new Parser(code).ProduceAST();
            if (r.Error != null) return res.Failure(r.Error);
            if (r.Expr == null) return res;

            Runtime.Environment localEnv = new Runtime.Environment(code);
            RuntimeResult result = r.Expr.Evaluate(localEnv);
            if (isStaticImport)
            {
                if (names.Count > 0)
                {
                    foreach (string name in names)
                    {
                        if (localEnv.Variables.ContainsKey(name))
                            env.DeclareVar(name, localEnv.Variables[name], true);
                        else
                            return res.Failure(new Error($"The name '{name}' doesn't exist in '{file}'.", env.SourceCode, Start, End));
                    }
                } else
                {
                    foreach (KeyValuePair<string, RuntimeValue> variable in localEnv.Variables)
                    {
                        // Only import it if its currently undefined
                        if (env.LookupVar(variable.Key) == null)
                            env.DeclareVar(variable.Key, variable.Value, true);
                    }
                }
            }
            else
            {
                env.DeclareVar(Path.GetFileNameWithoutExtension(file), new ObjectValue(code, localEnv.Variables), true);
            }
            return result;
        }

        public RuntimeResult ImportDLLFile(string file, Runtime.Environment env, bool isStaticImport, List<string> names)
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
                return res.Failure(new Error("File load error", env.SourceCode, Start, End));
            }
            catch (BadImageFormatException)
            {
                return res.Failure(new Error("Bad DLL image", env.SourceCode, Start, End));
            }

            Dictionary<string, RuntimeValue> properties = new Dictionary<string, RuntimeValue>();

            foreach (Type type in dll.GetExportedTypes())
            {
                if (type.IsSubclassOf(typeof(ObjectValue)))
                {
                    // Declare a constructor
                    env.DeclareVar(type.Name, new NativeFnValue((args, scope, start, end) =>
                    {
                        ObjectValue obj = (ObjectValue)Activator.CreateInstance(type, new object[] { scope.SourceCode }.Concat(args).ToArray());

                        foreach (MethodInfo method in type.GetMethods())
                        {
                            if (!method.IsPublic)
                                continue;

                            if (method.ReturnType != typeof(RuntimeResult))
                                continue;

                            obj.Properties[method.Name] = new NativeFnValue((args, scope, start, end) =>
                            {
                                return (RuntimeResult)method.Invoke(obj, args.ToArray());
                            });
                        }

                        foreach (PropertyInfo property in type.GetProperties())
                        {
                            if (!property.CanRead)
                                continue;

                            if (!property.CanWrite)
                                continue;

                            if (!property.PropertyType.IsSubclassOf(typeof(RuntimeValue)))
                                continue;

                            obj.Properties[property.Name] = (RuntimeValue)property.GetValue(obj, null);
                        }

                        foreach (FieldInfo field in type.GetFields())
                        {
                            if (!field.IsPublic)
                                continue;

                            if (!field.FieldType.IsSubclassOf(typeof(RuntimeValue)))
                                continue;

                            obj.Properties[field.Name] = (RuntimeValue)field.GetValue(obj);
                        }

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
                        value = env.LookupVar(method.Name);
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
                        NativeFnValue newFn = new NativeFnValue((args, scope, start, end) =>
                        {
                            RuntimeResult res = new RuntimeResult();
                            if (args.Count != parameters.Length - 3)
                            {
                                // Its not this method so just call the other method;
                                return ((NativeFnValue)value).Call(args, scope, start, end);
                            }

                            for (int i = 0; i < args.Count; i++)
                            {
                                Type expected = parameters[i + 3].ParameterType;
                                Type got = args[i].GetType();
                                if (expected != got && expected != typeof(RuntimeValue))
                                    return ((NativeFnValue)value).Call(args, scope, start, end);
                            }

                            return (RuntimeResult)method.Invoke(null, new object[] { scope, start, end }.Concat(args).ToArray());
                        });

                        if (isStaticImport)
                            env.AssignVar(method.Name, newFn);
                        else
                            properties[method.Name] = newFn;
                    }
                    else
                    {
                        NativeFnValue fn = new NativeFnValue((args, scope, start, end) =>
                        {
                            RuntimeResult res = new RuntimeResult();
                            if (args.Count != parameters.Length - 3)
                                return res.Failure(new Error($"Invalid parameter count, expected {parameters.Length}, got {args.Count}.", scope.SourceCode, start, end));

                            for (int i = 0; i < args.Count; i++)
                            {
                                Type expected = parameters[i + 3].ParameterType;
                                Type got = args[i].GetType();
                                if (expected != got)
                                    return res.Failure(new Error($"Invalid parameter type, expected a '{expected.Name}' type, got '{got.Name}'.", scope.SourceCode, start, end));
                            }

                            return (RuntimeResult)method.Invoke(null, new object[] { scope, start, end }.Concat(args).ToArray());
                        });

                        if (isStaticImport)
                            env.DeclareVar(method.Name, fn, false);
                        else
                            properties[method.Name] = fn;
                    }
                }
            }

            if (!isStaticImport)
                env.DeclareVar(Path.GetFileNameWithoutExtension(file), new ObjectValue(env.SourceCode, properties), true);

            return res;
        }

        public (RuntimeResult, bool) ImportFromOrigin(string origin, Runtime.Environment env)
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
                    res.Register(ImportFile(filename, env));
                    if (res.ShouldReturn()) return (res, false);
                }
            }
            else
            {
                if (!File.Exists(path + ".sten") && !File.Exists(path + ".dll"))
                    return (res, false);
                res.Register(ImportFile(path + ".sten", env));
                if (res.ShouldReturn()) return (res, false);
                res.Register(ImportFile(path + ".dll", env));
                if (res.ShouldReturn()) return (res, false);
            }

            return (RuntimeResult.Null(env.SourceCode), true);
        }

        public override RuntimeResult Evaluate(Runtime.Environment env)
        {
            RuntimeResult res = new RuntimeResult();
            bool found = false;

            (RuntimeResult r, bool success) = ImportFromOrigin(Directory.GetCurrentDirectory(), env);
            res.Register(r);
            if (res.ShouldReturn()) return res;
            if (success) found = true;

            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "libs");
            (r, success) = ImportFromOrigin(path, env);
            res.Register(r);
            if (res.ShouldReturn()) return res;
            if (success) found = true;

            if (!found)
                return res.Failure(new Error($"Couldn't find the file to import", env.SourceCode, Start, End));

            return RuntimeResult.Null(env.SourceCode);
        }
    }

}
