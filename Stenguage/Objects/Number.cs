﻿using Stenguage.Errors;
using System;

namespace Stenguage.Objects
{
    public class Number : Object
    {
        public static Number Null { get; } = new Number(0);

        public new float Value;
        
        public Number(float value) : base()
        {
            Value = value;
        }

        public override (Object, Error) AddedTo(Object other)
        {
            if (other is Number)
            {
                return (new Number(Value + ((Number)other).Value).SetContext(Context), null);
            } 
            else if (other is String)
            {
                return (new String(Value + ((String)other).Value).SetContext(Context), null);
            }
            return (null, IllegalOperation(other));
        }

        public override (Object, Error) SubtractedFrom(Object other)
        {
            if (other is Number)
            {
                return (new Number(Value - ((Number)other).Value).SetContext(Context), null);
            }
            return (null, IllegalOperation(other));
        }

        public override (Object, Error) MultipliedBy(Object other)
        {
            if (other is Number)
            {
                return (new Number(Value * ((Number)other).Value).SetContext(Context), null);
            }
            return (null, IllegalOperation(other));
        }

        public override (Object, Error) DividedBy(Object other)
        {
            if (other is Number)
            {
                if (((Number)other).Value == 0f)
                {
                    return (null, new RuntimeError(other.Start, other.End, "Division by zero", Context));
                }

                return (new Number(Value / ((Number)other).Value).SetContext(Context), null);
            }
            return (null, IllegalOperation(other));
        }

        public override (Object, Error) Power(Object other)
        {
            if (other is Number)
            {
                return (new Number((float)Math.Pow(Value, ((Number)other).Value)).SetContext(Context), null);
            }
            return (null, IllegalOperation(other));
        }

        public override (Boolean, Error) GetComparisonEE(Object other)
        {
            if (other is Number)
            {
                return ((Boolean)new Boolean(Value.Equals(((Number)other).Value)).SetContext(Context), null);
            }
            return (null, IllegalOperation(other));
        }

        public override (Boolean, Error) GetComparisonNE(Object other)
        {
            if (other is Number)
            {
                return ((Boolean)new Boolean(!Value.Equals(((Number)other).Value)).SetContext(Context), null);
            }
            return (null, IllegalOperation(other));
        }

        public override (Boolean, Error) GetComparisonLT(Object other)
        {
            if (other is Number)
            {
                return ((Boolean)new Boolean(Value < ((Number)other).Value).SetContext(Context), null);
            }
            return (null, IllegalOperation(other));
        }

        public override (Boolean, Error) GetComparisonGT(Object other)
        {
            if (other is Number)
            {
                return ((Boolean)new Boolean(Value > ((Number)other).Value).SetContext(Context), null);
            }
            return (null, IllegalOperation(other));
        }

        public override (Boolean, Error) GetComparisonLTE(Object other)
        {
            if (other is Number)
            {
                return ((Boolean)new Boolean(Value <= ((Number)other).Value).SetContext(Context), null);
            }
            return (null, IllegalOperation(other));
        }

        public override (Boolean, Error) GetComparisonGTE(Object other)
        {
            if (other is Number)
            {
                return ((Boolean)new Boolean(Value >= ((Number)other).Value).SetContext(Context), null);
            }
            return (null, IllegalOperation(other));
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
