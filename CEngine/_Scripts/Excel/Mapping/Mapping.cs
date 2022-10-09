using System;
using System.Reflection;

namespace CYM.Excel
{
    public class Mapping
    {
        private object _default;
        private int _column;
        public MemberInfo Member { get; private set; }
        public Type Type { get; private set; }
        public int Column
        {
            get { return _column; }
            set
            {
                _column = Math.Max(0, value);
            }
        }
        public object Default
        {
            get { return _default; }
            set
            {
                if (value != null && !Validator.CanCast(value.GetType(), Type))
                {
                    throw new ArgumentException(string.Format("Incompatible value '{0}' for type '{1}'", value, Type.FullName), "default");
                }
                _default = value;
            }
        }
        public bool Fallback { get; set; }
        public bool Failed { get; set; }

        public Mapping(MemberInfo member)
        {
            Member = member;
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    Type = ((FieldInfo)member).FieldType;
                    break;
                case MemberTypes.Property:
                    Type = ((PropertyInfo)member).PropertyType;
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public Mapping(MemberInfo member, int column) : this(member)
        {
            Column = column;
        }

        public Mapping(MemberInfo member, int column, object @default) : this(member, column)
        {
            if (@default == null)
            {
                if (Type.IsValueType)
                    throw new ArgumentException(string.Format("null cannot be assigned to value type '{0}'", Type.FullName), "default");
            }
            Default = @default;
            Fallback = true;
        }
    }
}