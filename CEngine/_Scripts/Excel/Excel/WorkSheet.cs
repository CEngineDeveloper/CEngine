using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CYM.Excel
{
    /// <summary>
    /// A collection of <see cref="Row"/>
    /// </summary>
    public sealed class WorkSheet : Table
    {
        /// <summary>
        /// WorkSheet ID(index)
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// WorkSheet name
        /// </summary>
        public string Name { get; private set; }

        public ReadOnlyCollection<Range> Spans { get; private set; }

        public WorkSheet(string id, string name, IList<Row> rows) : base(rows)
        {
            ID = id;
            Name = name;
        }

        public WorkSheet(string id, string name, IList<Row> rows, IList<Range> spans) : base(rows)
        {
            ID = id;
            Name = name;
            Spans = new ReadOnlyCollection<Range>(spans);
        }

        /// <summary>
        /// Apply merge to all span cells
        /// </summary>
        public void Merge()
        {
            foreach (var row in this)
            {
                foreach (var cell in row.Where(c => c.IsSpan))
                {
                    foreach (var range in Spans)
                    {
                        if (range.Contains(cell.Address))
                        {
                            cell.Value = this[range.From].Value;
                            cell.IsSpan = false;
                            break;
                        }
                    }
                }
            }
            Spans = new List<Range>().AsReadOnly();
        }

        public override Table DeepClone()
        {
            return new WorkSheet(ID, Name, Items.Select(row => row.DeepClone()).ToList(), Spans.ToList());
        }

        public override Table ShallowClone()
        {
            return (Table)MemberwiseClone();
        }
    }
}