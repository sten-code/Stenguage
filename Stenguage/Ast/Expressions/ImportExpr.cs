﻿using Stenguage.Errors;
using Stenguage.Runtime;
using Stenguage.Runtime.Values;
using System.Reflection;

namespace Stenguage.Ast.Expressions
{
    public class ImportExpr : Expr
    {
        public string Import { get; set; }

        public ImportExpr(string import, Position start, Position end) : base(NodeType.Import, start, end)
        {
            Import = import;
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
                    res.Register(ImportDLLFile(file, env));
                    if (res.ShouldReturn()) return res;
                    break;
                case ".sten":
                    res.Register(ImportStenFile(file, env));
                    if (res.ShouldReturn()) return res;
                    break;
            }

            return res;
        }

        public RuntimeResult ImportStenFile(string file, Runtime.Environment env)
        {
            RuntimeResult res = new RuntimeResult();
            if (!File.Exists(file))
                return res;

            string code = File.ReadAllText(file);
            ParseResult r = new Parser(code).ProduceAST();
            if (r.Error != null) return res.Failure(r.Error);
            if (r.Expr == null) return res;

            Runtime.Environment localEnv = new Runtime.Environment(code);
            RuntimeResult result = r.Expr.Evaluate(localEnv);
            env.DeclareVar(Path.GetFileNameWithoutExtension(file), new ObjectValue(localEnv.Variables, code, new Position(0, 0, 0), new Position(0, 0, 0)), true);
            return result;
        }

        public RuntimeResult ImportDLLFile(string file, Runtime.Environment env)
        {
            RuntimeResult res = new RuntimeResult();

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

            foreach (Type type in dll.GetExportedTypes())
            {
                try
                {
                    type.InvokeMember("Initialize", BindingFlags.InvokeMethod, null, Activator.CreateInstance(type), new object[] { env });
                }
                catch { }
            }

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

            return (res.Success(new NullValue(env.SourceCode, new Position(0, 0, 0), new Position(0, 0, 0))), true);
        }

        public override RuntimeResult Evaluate(Runtime.Environment env)
        {
            RuntimeResult res = new RuntimeResult();
            bool found = false;

            (RuntimeResult r, bool success) = ImportFromOrigin(Directory.GetCurrentDirectory(), env);
            res.Register(r);
            if (res.ShouldReturn()) return res;
            if (success) found = true;

            (r, success) = ImportFromOrigin(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "libs"), env);
            res.Register(r);
            if (res.ShouldReturn()) return res;
            if (success) found = true;

            if (!found)
                return res.Failure(new Error("File not found", env.SourceCode, Start, End));

            return res.Success(new NullValue(env.SourceCode, new Position(0, 0, 0), new Position(0, 0, 0)));
        }
    }

}