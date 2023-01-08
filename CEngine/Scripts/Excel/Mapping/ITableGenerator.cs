using System.Collections.Generic;

namespace CYM.Excel
{
    /// <summary>
    /// Objects generator
    /// </summary>
    public interface ITableGenerator
    {
        /// <summary>
        /// Instantiate target table to object instances
        /// </summary>
        /// <param name="table">Table</param>
        /// <returns>Object instance</returns>
        IEnumerable<object> Instantiate(Table table);
    }

    /// <summary>
    /// Objects generator
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    public interface ITableGenerator<T>
    {
        // <summary>
        /// Instantiate target table to object instances
        /// </summary>
        /// <param name="table">Table</param>
        /// <returns>Object instance</returns>
        IEnumerable<T> Instantiate(Table table);
    }
}