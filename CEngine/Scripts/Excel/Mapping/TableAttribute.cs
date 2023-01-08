using System;

namespace CYM.Excel
{
    /// <summary>
    /// Table mapping
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public sealed class TableAttribute : Attribute
    {
        /// <summary>
        /// One-based row indices to ignore when converting
        /// </summary>
        public int[] Ignore { get; private set; }

        /// <summary>
        /// Enable safe converting
        /// </summary>
        public bool SafeMode { get; set; }

        /// <summary>
        /// Mapping table
        /// </summary>
        /// <param name="ignore">One-based row indices to ignore when converting</param>
        public TableAttribute(params int[] ignore)
        {
            Ignore = ignore;
        }
    }
}