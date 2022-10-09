using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CYM.Excel
{
    /// <summary>
    /// A collection of <see cref="TCell"/>
    /// </summary>
    public class Row : ReadOnlyCollection<Cell>, ICloneable<Row>
    {
        public IList<Cell> Cells { get { return Items; } }

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
                return this.First(cell => cell.Address == address);
            }
        }

        public IEnumerable<Cell> this[Range range]
        {
            get
            {
                return this.Where(cell => range.Contains(cell.Address));
            }
        }

        public Row(IList<Cell> list) : base(list)
        {
        }

        public Row() : base(new List<Cell>())
        {
        }

        public Row DeepClone()
        {
            return new Row(Items.Select(cell => cell.DeepClone()).ToList());
        }

        public Row ShallowClone()
        {
            return (Row)MemberwiseClone();
        }
    }
}