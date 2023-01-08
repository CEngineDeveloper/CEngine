using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CYM.Excel
{
    /// <summary>
    /// A collection of <see cref="string"/> storing shared values
    /// </summary>
    public sealed class SharedStringCollection : ReadOnlyCollection<string>
    {
        public SharedStringCollection(IList<string> list) : base(list)
        {
        }
    }
}