namespace CYM.Excel
{
    /// <summary>
    /// Object generator
    /// </summary>
    public interface IGenerator
    {
        /// <summary>
        /// Instantiate target row to an object instance
        /// </summary>
        /// <param name="row">Row</param>
        /// <returns>Object instance</returns>
        object Instantiate(Row row);
    }

    /// <summary>
    /// Object generator
    /// </summary>
    /// <typeparam name="T">Target type</typeparam>
    public interface IGenerator<T>
    {
        /// <summary>
        /// Instantiate target row to an object instance
        /// </summary>
        /// <param name="row">Row</param>
        /// <returns>Object instance</returns>
        T Instantiate(Row row);
    }
}