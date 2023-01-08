using System;
using System.Text.RegularExpressions;

namespace CYM.Excel
{
    /// <summary>
    /// A range of <see cref="Address"/>
    /// </summary>
    public struct Range : IEquatable<Range>
    {
        #region Equality members

        public bool Equals(Range other)
        {
            return From.Equals(other.From) && To.Equals(other.To);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Range && Equals((Range)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (From.GetHashCode() * 397) ^ To.GetHashCode();
            }
        }

        #endregion

        /// <summary>
        /// Range from
        /// </summary>
        public Address From { get; private set; }

        /// <summary>
        /// Range to
        /// </summary>
        public Address To { get; private set; }


        /// <summary>
        /// Initializes a new instance of the Range class
        /// </summary>
        /// <param name="from">Range from</param>
        /// <param name="to">Range to</param>
        public Range(Address @from, Address to)
        {
            if (@from >= to)
                throw new ArgumentException("begin address is larger than or equal to end address");
            From = @from;
            To = to;
        }

        /// <summary>
        /// Initializes a new instance of the Range class
        /// </summary>
        /// <param name="range">Range expression; e.g. A1:C12</param>
        public Range(string range)
        {
            var args = range.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            if (args.Length != 2)
                throw new FormatException();
            var @from = new Address(args[0]);
            var to = new Address(args[1]);
            if (@from >= to)
                throw new ArgumentException("begin address is larger than or equal to end address");
            From = @from;
            To = @to;
        }

        /// <summary>
        /// Check if this range contains target address
        /// </summary>
        /// <param name="address">Address to check</param>
        /// <returns>True if contains</returns>
        public bool Contains(Address address)
        {
            return address >= From && address <= To;
        }

        /// <summary>
        /// Check if this range contains target range
        /// </summary>
        /// <param name="address">Range to check</param>
        /// <returns>True if contains</returns>
        public bool Contains(Range range)
        {
            return range.From >= From && range.To <= To;
        }

        public override string ToString()
        {
            return string.Format("{0}:{1}", From, To);
        }

        public static bool operator ==(Range range, Range other)
        {
            return range.From == other.From && range.To == other.To;
        }

        public static bool operator !=(Range range, Range other)
        {
            return range.From != other.From || range.To != other.To;
        }

        /// <summary>
        /// Check if the given range is valid
        /// </summary>
        /// <param name="range"></param>
        /// <returns>True if valid</returns>
        public static bool IsValid(string range)
        {
            return Regex.IsMatch(range, "^[A-Z]+\\d+:[A-Z]+\\d+$");
        }
    }
}