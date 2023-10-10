using Stenguage.Runtime;
using Stenguage.Runtime.Values;

namespace Math
{
    public class Math
    {
        public static RuntimeResult cos(Context ctx,
                                        NumberValue value)
        {
            return new RuntimeResult().Success(new NumberValue(System.Math.Cos(value.Value)));
        }

        public static RuntimeResult sin(Context ctx,
                                        NumberValue value)
        {
            return new RuntimeResult().Success(new NumberValue(System.Math.Sin(value.Value)));
        }

        public static RuntimeResult atan2(Context ctx,
                                          NumberValue y, NumberValue x)
        {
            return new RuntimeResult().Success(new NumberValue(System.Math.Atan2(y.Value, x.Value)));
        }

        public static RuntimeResult sqrt(Context ctx,
                                         NumberValue value)
        {
            return new RuntimeResult().Success(new NumberValue(System.Math.Sqrt(value.Value)));
        }
    }
}