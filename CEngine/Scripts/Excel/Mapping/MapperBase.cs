using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;

namespace CYM.Excel
{
    public abstract class MapperBase<T> where T : MapperBase<T>
    {
        protected readonly Mapping[] mappings;
        protected readonly Type type;

        /// <summary>
        /// No exception will be thrown when parsing columns
        /// </summary>
        /// <returns></returns>
        public bool SafeMode { get; set; }

        protected MapperBase(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            this.type = type;
            var members = GetMembers();
            mappings = members.Select(m => new Mapping(m)).ToArray();
        }

        /// <summary>
        /// Get serializable members
        /// </summary>
        /// <returns>Members</returns>
        protected virtual MemberInfo[] GetMembers()
        {
            var ret = new List<MemberInfo>(type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.GetField | BindingFlags.SetField));
            ret.RemoveAll(x => x.MemberType == MemberTypes.Method || x.MemberType == MemberTypes.Constructor || x.MemberType == MemberTypes.Event || x.MemberType == MemberTypes.TypeInfo);
            return ret.ToArray();
            //return FormatterServices.GetSerializableMembers(type);
        }

        /// <summary>
        /// Create an object instance
        /// </summary>
        /// <returns></returns>
        protected virtual object CreateInstance()
        {
            return FormatterServices.GetUninitializedObject(type);
        }

        /// <summary>
        /// Assign data to members
        /// </summary>
        /// <param name="obj">Object instance</param>
        /// <param name="members">Members</param>
        /// <param name="data">Data</param>
        protected virtual void Assign(object obj, MemberInfo[] members, object[] data)
        {
            for (int i=0;i<members.Length;++i)
            {
                if (members[i].MemberType == MemberTypes.Property)
                {
                    (members[i] as PropertyInfo).SetValue(obj, data[i]);
                }
                else if (members[i].MemberType == MemberTypes.Field)
                {
                    (members[i] as FieldInfo).SetValue(obj, data[i]);
                }
            }
            //FormatterServices.PopulateObjectMembers(obj, members, data);
        }

        /// <summary>
        /// Extract mapping info from class attributes
        /// </summary>
        public virtual void Extract()
        {
            foreach (var mapping in mappings)
            {
                var attribute = Attribute.GetCustomAttribute(mapping.Member, typeof(ColumnAttribute)) as ColumnAttribute;
                if (attribute == null)
                    mapping.Column = 0;
                else
                {
                    if (attribute.Column < 1)
                    {
                        throw new ArgumentException("One-based column index must be greater than 0");
                    }
                    mapping.Column = attribute.Column;
                    mapping.Default = attribute.Default;
                    mapping.Fallback = attribute.Fallback;
                }
            }
        }

        protected IEnumerable<object> Cast(Row row)
        {
            foreach (var mapping in mappings.Where(m => m.Column > 0))
            {
                var index = mapping.Column - 1;
                if (index >= row.Count)
                {
                    throw new InvalidOperationException(string.Format("Column '{0}' index '{1}' out of range '{2}' on type '{3}'", mapping.Member.Name, index, row.Count, type.FullName));
                }
                Object value = null;
                try
                {
                    value = row[index].Convert(mapping.Type);
                }
                catch (FormatException)
                {
                    if (mapping.Fallback)
                        value = mapping.Default;
                    else if (SafeMode)
                        mapping.Failed = true;
                    else
                        throw new FormatException(string.Format("Input cell at {0} was not in the correct format for type {1}", row[index].Address, mapping.Type));
                }
                if (!mapping.Failed)
                    yield return value;
            }
        }

        /// <summary>
        /// Map member to column
        /// </summary>
        /// <param name="member">Member name</param>
        /// <param name="column">One-based column index</param>
        /// <returns>Mapping instance</returns>
        public T Map(string member, int column)
        {
            var mapping = Array.Find(mappings, m => m.Member.Name == member);
            if (mapping == null)
            {
                CLog.Error(string.Format("Member '{0}' not found in type '{1}' , ÊÇ·ñÍü¼Ç¼Ó#ºÅ×¢ÊÍ?", member, type.FullName));
                return (T)this;
            }
            if (mapping.Column > 0)
            {
                CLog.Error(string.Format("Member '{0}' for type '{1}' already mapped to Column '{2}'", member, type.FullName, mapping.Column));
            }
            if (column < 1)
            {
                CLog.Error("One-based column index must be greater than 0");
            }
            mapping.Column = column;
            return (T)this;
        }

        /// <summary>
        /// Map member to column
        /// </summary>
        /// <param name="member">Member name</param>
        /// <param name="column">One-based column index</param>
        /// <param name="default">Fallback value</param>
        /// <returns>Mapping instance</returns>
        public T Map(string member, int column, object @default)
        {
            var mapping = Array.Find(mappings, m => m.Member.Name == member);
            if (mapping == null)
            {
                throw new InvalidOperationException(string.Format("Member '{0}' not found in type '{1}'", member, type.FullName));
            }
            if (mapping.Column > 0)
            {
                throw new InvalidOperationException(string.Format("Member '{0}' for type '{1}' already mapped to Column '{2}'", member, type.FullName, mapping.Column));
            }
            if (column < 1)
            {
                throw new ArgumentException("One-based column index must be greater than 0");
            }
            mapping.Default = @default;
            mapping.Column = column;
            mapping.Fallback = true;
            return (T)this;
        }

        /// <summary>
        /// Map member to column
        /// </summary>
        /// <param name="member">Member name</param>
        /// <param name="column">Column name</param>
        /// <returns>Mapping instance</returns>
        public T Map(string member, string column)
        {
            return Map(member, Address.ParseColumn(column));
        }

        /// <summary>
        /// Map member to column
        /// </summary>
        /// <param name="member">Member name</param>
        /// <param name="column">Column name</param>
        /// <param name="default">Fallback value</param>
        /// <returns>Mapping instance</returns>
        public T Map(string member, string column, object @default)
        {
            return Map(member, Address.ParseColumn(column), @default);
        }

        /// <summary>
        /// Map members from expressions(e.g. name:1, age:2, role:3)
        /// </summary>
        /// <remarks>
        /// Expressions format: member:column, member:column...
        /// </remarks>
        /// <param name="expression">Expression</param>
        /// <returns>Mapping instance</returns>
        public T Map(string expression)
        {
            if (!Regex.IsMatch(expression, "^(\\w+\\s*:(\\d+|[A-Z]+)\\s*)(,\\s*\\w+\\s*:(\\d+|[A-Z]+)\\s*)*$"))
            {
                throw new FormatException("Invalid mapping expression");
            }
            var groups = expression.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var group in groups)
            {
                var entires = group.Split(new[] { ":" }, StringSplitOptions.RemoveEmptyEntries);
                var column = entires[1].Trim();
                if (Regex.IsMatch(column, "^\\d+$"))
                    Map(entires[0].Trim(), Int32.Parse(column));
                else
                    Map(entires[0].Trim(), column);
            }
            return (T)this;
        }

        /// <summary>
        /// Map members from columns 
        /// </summary>
        /// <remarks>
        /// Each column provides mapping member name and column index
        /// </remarks>
        /// <param name="row">Row</param>
        /// <returns>Mapping instance</returns>
        public T Map(IEnumerable<Cell> row)
        {
            foreach (var column in row)
            {
                if (column.Text.IsInv())
                    continue;
                if (column.Text.StartsWith("#"))
                    continue;
                Map(column.Text, column.Address.Column);
            }
            return (T)this;
        }

        /// <summary>
        /// Remove member mapping
        /// </summary>
        /// <param name="member">Member name</param>
        /// <returns>Mapping instance</returns>
        public T Remove(string member)
        {
            var mapping = Array.Find(mappings, m => m.Member.Name == member);
            if (mapping == null)
            {
                throw new InvalidOperationException(string.Format("Member '{0}' not found in type '{1}'", member, type.FullName));
            }
            mapping.Column = 0;
            return (T)this;
        }

        /// <summary>
        /// Clear member mapping
        /// </summary>
        /// <returns>Mapping instance</returns>
        public T Clear()
        {
            Array.ForEach(mappings, m => m.Column = 0);
            return (T)this;
        }

        /// <summary>
        /// Copy mapping from target mapping instance
        /// </summary>
        /// <param name="mapping">Mapping instance to copy from</param>
        /// <returns>Mapping instance</returns>
        public T Copy(T mapping)
        {
            Array.ForEach(mappings, m => m.Column = 0);
            foreach (var source in mapping.mappings)
            {
                var target = Array.Find(mappings, m => m.Member.Name == source.Member.Name && m.Member.DeclaringType == source.Member.DeclaringType);
                if (target != null)
                {
                    target.Column = source.Column;
                    target.Default = source.Default;
                    target.Fallback = source.Fallback;
                }
            }
            return (T)this;
        }

        /// <summary>
        /// Create an object instance from target row
        /// </summary>
        /// <param name="row">Row</param>
        /// <returns>Object instance</returns>
        protected object Instantiate(Row row)
        {
            if (row == null)
                return null;
            var obj = CreateInstance();
            var data = Cast(row).ToArray();
            var members = mappings.Where(m => m.Column > 0 && !m.Failed).Select(m => m.Member).ToArray();
            Array.ForEach(mappings, m => m.Failed = false);
            Assign(obj, members, data);
            return obj;
        }
    }
}