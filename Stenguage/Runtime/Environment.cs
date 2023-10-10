using Stenguage.Errors;
using Stenguage.Runtime.Values;

namespace Stenguage.Runtime
{
    public class Environment
    {
        public static int CheckArguments(List<RuntimeValue> args, List<List<RuntimeValueType>> filters = null)
        {
            /*
            Example:
                [
                    [Number, Number, Number],
                    [String, Number]
                ]
            */

            if (filters == null)
            {
                return args.Count == 0 ? 0 : -1;
            }

            List<int> lengths = filters.Select(x => x.Count).ToList();
            if (!lengths.Contains(args.Count))
            {
                Console.WriteLine("Error: Invalid arguments.");
                return -1;
            }

            int filterIndex = 0;
            foreach (List<RuntimeValueType> filter in filters)
            {
                if (filter.Count != args.Count)
                {
                    filterIndex++;
                    continue;
                }

                bool valid = true;
                for (int i = 0; i < filter.Count; i++)
                {
                    if (args[i].Type != filter[i] && filter[i] != RuntimeValueType.Any)
                    {
                        valid = false;
                    }
                }
                if (valid)
                    return filterIndex;
                filterIndex++;
            }

            Console.WriteLine("Error: Invalid argument types.");
            return -1;
        }

        public void Setup(Environment env)
        {
            env.DeclareVar("true", new BooleanValue(true), true);
            env.DeclareVar("false", new BooleanValue(false), true);
            env.DeclareVar("null", new NullValue(), true);

            env.DeclareVar("print", new NativeFnValue((args, ctx) =>
            {
                foreach (RuntimeValue arg in args)
                    Console.Write(arg.ToString() + " ");

                Console.WriteLine();
                return RuntimeResult.Null();
            }), true);

            env.DeclareVar("exit", new NativeFnValue((args, ctx) =>
            {
                RuntimeResult res = new RuntimeResult();
                int type = CheckArguments(args, new List<List<RuntimeValueType>>
                {
                    new List<RuntimeValueType> {  },
                    new List<RuntimeValueType> { RuntimeValueType.Number }
                });
                if (type == -1)
                    return res.Failure(new Error("Invalid parameter types.", ctx.Env.SourceCode, ctx.Start, ctx.End));

                switch (type)
                {
                    case 0:
                        System.Environment.Exit(0);
                        return RuntimeResult.Null();
                    case 1:
                        System.Environment.Exit((int)((NumberValue)args[0]).Value);
                        return RuntimeResult.Null();
                    default:
                        return res.Failure(new Error($"Cannot convert '{args[0].Type}' to a double.", ctx.Env.SourceCode, ctx.Start, ctx.End));
                }

            }), true);

            env.DeclareVar("input", new NativeFnValue((args, ctx) =>
            {
                RuntimeResult res = new RuntimeResult();
                if (args.Count > 0)
                {
                    foreach (RuntimeValue arg in args)
                    {
                        Console.Write(arg);
                    }
                }

                string input = Console.ReadLine();
                if (input == null)
                    return res.Success(new NullValue());

                return res.Success(new StringValue(input));
            }), true);

            env.DeclareVar("int", new NativeFnValue((args, ctx) =>
            {
                RuntimeResult res = new RuntimeResult();
                int type = CheckArguments(args, new List<List<RuntimeValueType>>
                {
                    new List<RuntimeValueType> { RuntimeValueType.Number },
                    new List<RuntimeValueType> { RuntimeValueType.Boolean },
                    new List<RuntimeValueType> { RuntimeValueType.String },
                    new List<RuntimeValueType> { RuntimeValueType.String, RuntimeValueType.Number },
                });
                if (type == -1)
                    return res.Failure(new Error("Invalid parameter types.", ctx.Env.SourceCode, ctx.Start, ctx.End));

                switch (type)
                {
                    case 0:
                        return res.Success(new NumberValue((int)((NumberValue)args[0]).Value));
                    case 1:
                        return res.Success(new NumberValue(((BooleanValue)args[0]).Value ? 1 : 0));
                    case 2:
                        StringValue str = (StringValue)args[0];
                        if (!int.TryParse(str.Value, out int value))
                        {
                            return res.Failure(new Error($"Cannot convert '{str.Value}' to an integer.", ctx.Env.SourceCode, ctx.Start, ctx.End));
                        }

                        return res.Success(new NumberValue(value));
                    case 3:
                        str = (StringValue)args[0];
                        NumberValue b = (NumberValue)args[1];
                        if (b.Value % 1 != 0)
                        {
                            return res.Failure(new Error("Base must be an integer.", ctx.Env.SourceCode, ctx.Start, ctx.End));
                        }

                        try
                        {
                            return res.Success(new NumberValue(Convert.ToInt16(str.Value, (int)b.Value)));
                        } 
                        catch (FormatException)
                        {
                            return res.Failure(new Error("Input must be a valid value.", ctx.Env.SourceCode, ctx.Start, ctx.End));
                        }
                    default:
                        return res.Failure(new Error($"Error: Cannot convert '{args[0].Type}' to an integer.", ctx.Env.SourceCode, ctx.Start, ctx.End));
                }
            }), true);

            env.DeclareVar("double", new NativeFnValue((args, ctx) =>
            {
                RuntimeResult res = new RuntimeResult();
                int type = CheckArguments(args, new List<List<RuntimeValueType>>
                {
                    new List<RuntimeValueType> { RuntimeValueType.String }
                });
                if (type == -1)
                    return res.Failure(new Error("Invalid parameter types.", ctx.Env.SourceCode, ctx.Start, ctx.End));

                switch (type)
                {
                    case 0:
                        StringValue str = (StringValue)args[0];
                        if (!double.TryParse(str.Value, out double value))
                        {
                            return res.Failure(new Error($"Cannot convert '{str.Value}' to a double.", ctx.Env.SourceCode, ctx.Start, ctx.End));
                        }

                        return res.Success(new NumberValue(value));
                    default:
                        return res.Failure(new Error($"Cannot convert '{args[0].Type}' to a double.", ctx.Env.SourceCode, ctx.Start, ctx.End));
                }
            }), true);

            env.DeclareVar("time", new ObjectValue(new Dictionary<string, RuntimeValue>
            {
                ["epoch"] = new NativeFnValue((args, ctx) =>
                {
                    RuntimeResult res = new RuntimeResult();
                    if (CheckArguments(args) == -1)
                        return res.Failure(new Error("Invalid parameter types.", ctx.Env.SourceCode, ctx.Start, ctx.End));
                    return res.Success(new NumberValue(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
                }),
                ["sleep"] = new NativeFnValue((args, ctx) =>
                {
                    RuntimeResult res = new RuntimeResult();
                    int type = CheckArguments(args, new List<List<RuntimeValueType>>
                    {
                        new List<RuntimeValueType> { RuntimeValueType.Number }
                    });
                    if (type == -1)
                        return res.Failure(new Error("Invalid parameter types.", ctx.Env.SourceCode, ctx.Start, ctx.End));

                    double value = ((NumberValue)args[0]).Value;
                    if (value % 1 != 0)
                    {
                        return res.Failure(new Error("Invalid First argument of sleep function must be an integer.", ctx.Env.SourceCode, ctx.Start, ctx.End));
                    }

                    Thread.Sleep((int)value);
                    return RuntimeResult.Null();
                })
            }), true);

            env.DeclareVar("range", new NativeFnValue((args, ctx) =>
            {
                RuntimeResult res = new RuntimeResult();
                int type = CheckArguments(args, new List<List<RuntimeValueType>>
                {
                    new List<RuntimeValueType> { RuntimeValueType.Number },
                    new List<RuntimeValueType> { RuntimeValueType.Number, RuntimeValueType.Number },
                    new List<RuntimeValueType> { RuntimeValueType.Number, RuntimeValueType.Number, RuntimeValueType.Number },
                });
                if (type == -1)
                    return res.Failure(new Error("Invalid parameter types.", ctx.Env.SourceCode, ctx.Start, ctx.End));

                double s = 0;
                double e = 0;
                double step = 1;

                switch (type)
                {
                    case 0:
                        e = ((NumberValue)args[0]).Value;
                        break;
                    case 1:
                        s = ((NumberValue)args[0]).Value;
                        e = ((NumberValue)args[1]).Value;
                        break;
                    case 2:
                        s = ((NumberValue)args[0]).Value;
                        e = ((NumberValue)args[1]).Value;
                        step = ((NumberValue)args[2]).Value;
                        break;
                    default:
                        break;
                }

                List<RuntimeValue> result = new List<RuntimeValue>();
                for (double i = s; i < e; i += step)
                {
                    result.Add(new NumberValue(i));
                }

                return res.Success(new ListValue(result));
            }), true);

            env.DeclareVar("len", new NativeFnValue((args, ctx) =>
            {
                RuntimeResult res = new RuntimeResult();
                int type = CheckArguments(args, new List<List<RuntimeValueType>>
                {
                    new List<RuntimeValueType> { RuntimeValueType.String },
                    new List<RuntimeValueType> { RuntimeValueType.List },
                });
                if (type == -1)
                    return res.Failure(new Error("Invalid parameter types.", ctx.Env.SourceCode, ctx.Start, ctx.End));

                switch (type)
                {
                    case 0:
                        return res.Success(new NumberValue(((StringValue)args[0]).Value.Length));
                    case 1:
                        return res.Success(new NumberValue(((ListValue)args[0]).Items.Count));
                    default:
                        return res.Success(new NullValue());
                }
            }), true);

            env.DeclareVar("type", new NativeFnValue((args, ctx) =>
            {
                RuntimeResult res = new RuntimeResult();
                int type = CheckArguments(args, new List<List<RuntimeValueType>>
                {
                    new List<RuntimeValueType> { RuntimeValueType.Any },
                });
                if (type == -1)
                    return res.Failure(new Error("Invalid parameter types.", ctx.Env.SourceCode, ctx.Start, ctx.End));

                switch (type)
                {
                    case 0:
                        return res.Success(new StringValue(args[0].Type.ToString()));
                    default:
                        return res.Success(new NullValue());
                }
            }), true);
        }

        public Environment Parent { get; set; }
        public Dictionary<string, RuntimeValue> Variables { get; set; }
        public List<string> Constants { get; set; }
        public string SourceCode { get; set; }

        public Environment(string sourceCode, Environment parent = null)
        {
            Parent = parent;
            Variables = new Dictionary<string, RuntimeValue>();
            Constants = new List<string>();
            SourceCode = sourceCode;

            if (parent == null)
            {
                Setup(this);
            }
        }

        public RuntimeValue DeclareVar(string name, RuntimeValue value, bool isConstant)
        {
            if (Variables.ContainsKey(name))
                return null;

            Variables[name] = value;
            if (isConstant)
                Constants.Add(name);
            return value;
        }

        public RuntimeValue AssignVar(string name, RuntimeValue value)
        {
            Environment env = Resolve(name);
            if (env == null)
                return null;

            if (Constants.Contains(name))
            {
                Console.WriteLine($"Error: Cannot reassign a constant '{name}'");
                return null;
            }

            env.Variables[name] = value;
            return value;
        }

        public RuntimeValue LookupVar(string name)
        {
            Environment env = Resolve(name);
            if (env == null)
                return null;
            return env.Variables[name];
        }

        public Environment Resolve(string name)
        {
            if (Variables.ContainsKey(name))
                return this;

            if (Parent == null)
                return null;

            return Parent.Resolve(name);
        }

    }
}
