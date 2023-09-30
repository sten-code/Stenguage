using Stenguage.Errors;
using Stenguage.Runtime.Values;

namespace Stenguage.Runtime
{
    public class RuntimeResult
    {
        public RuntimeValue Value { get; set; }
        public Error Error { get; set; }
        public bool LoopContinue { get; set; }
        public bool LoopBreak { get; set; }
        public RuntimeValue ReturnValue { get; set; }
        public NumberValue SkipValue { get; set; }

        public static RuntimeResult Null(string sourceCode = "", Position start = null, Position end = null)
        {
            return new RuntimeResult().Success(new NullValue(sourceCode, start == null ? new Position(0, 0, 0) : start, end == null ? new Position(0, 0, 0) : end));
        }

        public RuntimeResult()
        {
            Value = new NullValue("", new Position(0, 0, 0), new Position(0, 0, 0));
            Reset();
        }

        public void Reset()
        {
            Value = new NullValue("", new Position(0, 0, 0), new Position(0, 0, 0));
            Error = null;
            LoopContinue = false;
            LoopBreak = false;
            ReturnValue = null;
            SkipValue = null;
        }

        public bool ShouldReturn()
        {
            return Error != null || LoopContinue || LoopBreak || SkipValue != null;
        }

        public RuntimeValue Register(RuntimeResult res)
        {
            if (res.ShouldReturn()) Error = res.Error;
            LoopContinue = res.LoopContinue;
            LoopBreak = res.LoopBreak;
            ReturnValue = res.ReturnValue;
            SkipValue = res.SkipValue;
            return res.Value;
        }

        public RuntimeResult Success(RuntimeValue value)
        {
            Value = value;
            return this;
        }

        public RuntimeResult SuccessContinue()
        {
            LoopContinue = true;
            return this;
        }

        public RuntimeResult SuccessBreak()
        {
            LoopBreak = true;
            return this;
        }

        public RuntimeResult SuccessReturn(RuntimeValue val)
        {
            Reset();
            ReturnValue = val;
            return this;
        }

        public RuntimeResult SuccessSkip(NumberValue val)
        {
            SkipValue = val;
            return this;
        }

        public RuntimeResult Failure(Error error)
        {
            Reset();
            Error = error;
            return this;
        }

    }
}
