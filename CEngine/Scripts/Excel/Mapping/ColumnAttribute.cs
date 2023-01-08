using System;

namespace CYM.Excel
{
    /// <summary>
    /// Column mapping
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false)]
    public sealed class ColumnAttribute : Attribute
    {
        /// <summary>
        /// One-based column index
        /// </summary>
        public int Column { get; private set; }

        /// <summary>
        /// Default value when fallback
        /// </summary>
        public object Default { get; private set; }

        /// <summary>
        /// Enable fallback
        /// </summary>
        public bool Fallback { get; private set; }

        /// <summary>
        /// Mapping column
        /// </summary>
        /// <param name="column">One-based column index</param>
        public ColumnAttribute(int column)
        {
            Column = column;
        }

        /// <summary>
        /// Mapping column
        /// </summary>
        /// <param name="column">One-based column index</param>
        /// <param name="default">Fallback value</param>
        public ColumnAttribute(int column, object @default) : this(column)
        {
            Default = @default;
            Fallback = true;
        }

        /// <summary>
        /// Mapping column
        /// </summary>
        /// <param name="column">Column formula, e.g. A, AB</param>
        public ColumnAttribute(string column) : this(Address.ParseColumn(column))
        {

        }

        /// <summary>
        /// Mapping column
        /// </summary>
        /// <param name="column">Column formula, e.g. A, AB</param>
        /// <param name="default">Fallback value</param>
        public ColumnAttribute(string column, object @default) : this(Address.ParseColumn(column), @default)
        {

        }
    }
}