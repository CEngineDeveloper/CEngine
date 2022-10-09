using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CYM.Excel
{
    public static class Validator
    {
        public static bool CanCast(Type from, Type to)
        {
            if (from.IsAssignableFrom(to))
            {
                return true;
            }
            if (HasImplicitConversion(from, to) || HasImplicitConversion(from, from, to) || HasImplicitConversion(to, from, to))
            {
                return true;
            }
            if (to.IsEnum)
            {
                return CanCast(from, Enum.GetUnderlyingType(to));
            }
            if (Nullable.GetUnderlyingType(to) != null)
            {
                return CanCast(from, Nullable.GetUnderlyingType(to));
            }

            return false;
        }

        static bool HasImplicitConversion(Type definedOn, Type baseType, Type targetType)
        {
            return definedOn.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == "op_Implicit" && m.ReturnType == targetType)
                .Any(m =>
                {
                    var p = m.GetParameters().FirstOrDefault();
                    return p != null && p.ParameterType == baseType;
                });

        }

        public static bool HasImplicitConversion(Type source, Type target)
        {
            var sourceCode = Type.GetTypeCode(source);
            var targetCode = Type.GetTypeCode(target);
            switch (sourceCode)
            {
                case TypeCode.SByte:
                    switch (targetCode)
                    {
                        case TypeCode.Int16:
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Byte:
                    switch (targetCode)
                    {
                        case TypeCode.Int16:
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Int16:
                    switch (targetCode)
                    {
                        case TypeCode.Int32:
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.UInt16:
                    switch (targetCode)
                    {
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Int32:
                    switch (targetCode)
                    {
                        case TypeCode.Int64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.UInt32:
                    switch (targetCode)
                    {
                        case TypeCode.UInt32:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    switch (targetCode)
                    {
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Char:
                    switch (targetCode)
                    {
                        case TypeCode.UInt16:
                        case TypeCode.Int32:
                        case TypeCode.UInt32:
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.Single:
                        case TypeCode.Double:
                        case TypeCode.Decimal:
                            return true;
                    }
                    return false;
                case TypeCode.Single:
                    return (targetCode == TypeCode.Double);
            }
            return false;
        }
    }
}