 namespace Stenguage.Runtime
{
    public class Context
    {
        public Position Start { get; set; }
        public Position End { get; set; }
        public Environment Env { get; set; }

        public Context(Position start, Position end, Environment environment)
        {
            Start = start;
            End = end;
            Env = environment;
        }
        public Context(Context ctx)
        {
            Start = ctx.Start;
            End = ctx.End;
            Env = ctx.Env;
        }
    }
}
