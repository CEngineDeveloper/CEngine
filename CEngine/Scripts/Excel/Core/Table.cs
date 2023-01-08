using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CYM.Excel
{
    /// <summary>
    /// Data table
    /// </summary>
    public abstract class Table : ReadOnlyCollection<Row>, ICloneable<Table>
    {
        public Table(IList<Row> list) : base(list)
        {
        }

        public IList<Row> Rows { get { return Items; } }

        public Cell this[string address]
        {
            get
            {
                if (!Address.IsValid(address))
                    throw new FormatException();
                return this[new Address(address)];
            }
        }

        public Cell this[Address address]
        {
            get
            {
                return this.SelectMany(row => row).First(cell => cell.Address == address);
            }
        }

        public IEnumerable<Cell> this[Range range]
        {
            get
            {
                return this.SelectMany(row => row).Where(cell => range.Contains(cell.Address));
            }
        }
        
        public abstract Table DeepClone();
        public abstract Table ShallowClone();
    }
}