using Stenguage.Errors;
using Stenguage.Runtime;
using Stenguage.Runtime.Values;

namespace StdLib
{
    public class StdLib
    {
        public void Initialize(Stenguage.Runtime.Environment env)
        {
            env.DeclareVar("index", new NativeFnValue((args, scope, start, end) =>
            {
                RuntimeResult res = new RuntimeResult();
                int type = Stenguage.Runtime.Environment.CheckArguments(args, new List<List<RuntimeValueType>>
                {
                    new List<RuntimeValueType> { RuntimeValueType.String, RuntimeValueType.String },
                });
                if (type == -1)
                    return res.Failure(new Error("Invalid parameter types.", scope.SourceCode, start, end));

                switch (type)
                {
                    case 0:
                        StringValue source = (StringValue)args[0];
                        StringValue value = (StringValue)args[1];
                        return res.Success(new NumberValue(source.Value.IndexOf(value.Value), scope.SourceCode, start, end));
                    default:
                        return res.Success(new NullValue(scope.SourceCode, start, end));
                }
            }), false);

            env.DeclareVar("contains", new NativeFnValue((args, scope, start, end) =>
            {
                RuntimeResult res = new RuntimeResult();
                int type = Stenguage.Runtime.Environment.CheckArguments(args, new List<List<RuntimeValueType>>
                {
                    new List<RuntimeValueType> { RuntimeValueType.String, RuntimeValueType.String },
                    new List<RuntimeValueType> { RuntimeValueType.List, RuntimeValueType.Any },
                });
                if (type == -1)
                    return res.Failure(new Error("Invalid parameter types.", scope.SourceCode, start, end));

                switch (type)
                {
                    case 0:
                        return res.Success(new BooleanValue(((StringValue)args[0]).Value.Contains(((StringValue)args[1]).Value), scope.SourceCode, start, end));
                    case 1:
                        return res.Success(new BooleanValue(((ListValue)args[0]).Items.Contains(args[1]), scope.SourceCode, start, end));
                    default:
                        return res.Success(new NullValue(scope.SourceCode, start, end));
                }
            }), false);

            env.DeclareVar("substring", new NativeFnValue((args, scope, start, end) =>
            {
                RuntimeResult res = new RuntimeResult();
                int type = Stenguage.Runtime.Environment.CheckArguments(args, new List<List<RuntimeValueType>>
                {
                    new List<RuntimeValueType> { RuntimeValueType.String, RuntimeValueType.Number, RuntimeValueType.Number }
                });
                if (type == -1)
                    return res.Failure(new Error("Invalid parameter types.", scope.SourceCode, start, end));

                switch (type)
                {
                    case 0:
                        NumberValue s = (NumberValue)args[1];
                        if (s.Value % 1 != 0)
                            return res.Failure(new Error("The start index must be an integer.", scope.SourceCode, start, end));
                        NumberValue l = (NumberValue)args[2];
                        if (s.Value % 1 != 0)
                            return res.Failure(new Error("The length must be an integer.", scope.SourceCode, start, end));

                        try
                        {
                            return res.Success(new StringValue(((StringValue)args[0]).Value.Substring((int)s.Value, (int)l.Value), scope.SourceCode, start, end));
                        }
                        catch
                        {
                            return res.Failure(new Error("Substring out of range.", scope.SourceCode, start, end));
                        }
                    default:
                        return res.Success(new NullValue(scope.SourceCode, start, end));
                }
            }), false);

        }
    }
}