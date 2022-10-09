namespace CYM.Excel
{   
    /// <summary>
    /// Cloneable
    /// </summary>
    public interface ICloneable<T> where T : class
    {   
        /// <summary>
        /// Make a deep copy
        /// </summary>
        /// <returns>Cloned instance</returns>
        T DeepClone();

        /// <summary>
        /// Make a shallow copy
        /// </summary>
        /// <returns>Cloned instance</returns>
        T ShallowClone();
    }
}