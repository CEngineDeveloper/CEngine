using System;
using System.Text.RegularExpressions;

namespace CYM.Excel
{
    /// <summary>
    /// A value which represents the position of a <see cref="Cell"/> in a <see cref="Table"/>
    /// </summary>
    public struct Address : IEquatable<Address>
    {
        #region Equality members

        bool IEquatable<Address>.Equals(Address other)
        {
            return Column == other.Column && Row == other.Row;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Address && Equals((Address)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Column * 397) ^ Row;
            }
        }

        #endregion

        /// <summary>
        /// One-based column index
        /// </summary>
        public int Column { get; private set; }

        /// <summary>
        /// One-based row index
        /// </summary>
        public int Row { get; private set; }

        /// <summary>
        /// Initializes a new instance of the Address class
        /// </summary>
        /// <param name="address">Excel cell address; e.g. A1</param>
        /// <exception cref="FormatException"/>
        public Address(string address)
        {
            var column = Regex.Match(address, "^[A-Z]+");
            var row = Regex.Match(address, "\\d+$");
            if (!column.Success || !row.Success)
                throw new FormatException("Invalid address: " + address);
            Column = ParseColumn(column.Value);
            Row = int.Parse(row.Value);
        }

        /// <summary>
        /// Initializes a new instance of the Address class
        /// </summary>
        /// <param name="column">One-based column index</param>
        /// <param name="row">One-based row index</param>
        public Address(int column, int row)
        {
            Column = column;
            Row = row;
        }

        /// <summary>
        /// Column name; e.g. A
        /// </summary>
        public string ColumnName
        {
            get { return ParseColumn(Column); }
        }

        public override string ToString()
        {
            return string.Format("{0}{1}", ColumnName, Row);
        }

        public static bool operator ==(Address address, Address other)
        {
            return address.Row == other.Row && address.Column == other.Column;
        }

        public static bool operator !=(Address address, Address other)
        {
            return address.Row != other.Row || address.Column != other.Column;
        }

        public static bool operator >=(Address address, Address other)
        {
            return address.Row >= other.Row && address.Column >= other.Column;
        }

        public static bool operator <=(Address address, Address other)
        {
            return address.Row <= other.Row && address.Column <= other.Column;
        }

        public static bool operator >(Address address, Address other)
        {
            return address.Row > other.Row && address.Column > other.Column;
        }

        public static bool operator <(Address address, Address other)
        {
            return address.Row < other.Row && address.Column < other.Column;
        }

        public static Range operator +(Address from, Address to)
        {
            return new Range(from, to);
        }

        public static Address operator >>(Address address, int column)
        {
            return new Address(address.Column + column, address.Row);
        }

        public static Address operator <<(Address address, int column)
        {
            return new Address(address.Column - column, address.Row);
        }

        public static Address operator +(Address address, int row)
        {
            return new Address(address.Column, address.Row + row);
        }

        public static Address operator -(Address address, int row)
        {
            return new Address(address.Column, address.Row - row);
        }

        /// <summary>
        /// Parse one-based column index to excel-style
        /// </summary>
        /// <param name="column">One-based column index</param>
        /// <returns>Converted column index in excel-style</returns>
        public static string ParseColumn(int column)
        {
            if (column <= 0)
                throw new ArgumentException("Column value must be greater than 0");
            if (column <= 26)
                return Convert.ToChar(column + 64).ToString();
            var div = column / 26;
            var mod = column % 26;
            if (mod != 0) return ParseColumn(div) + ParseColumn(mod);
            mod = 26;
            div--;
            return ParseColumn(div) + ParseColumn(mod);
        }

        /// <summary>
        /// Parse excel-style column to one-based index
        /// </summary>
        /// <param name="column">Excel-style column</param>
        /// <returns>Converted one-based column index</returns>
        /// <exception cref="FormatException"/>
        public static int ParseColumn(string column)
        {
            if (!Regex.IsMatch(column, "^[A-Z]+$"))
                throw new FormatException("Invalid address: " + column);
            var digits = new int[column.Length];
            for (var i = 0; i < column.Length; ++i)
                digits[i] = Convert.ToInt32(column[i]) - 64;
            var mul = 1;
            var res = 0;
            for (var pos = digits.Length - 1; pos >= 0; --pos)
            {
                res += digits[pos] * mul;
                mul *= 26;
            }
            return res;
        }

        /// <summary>
        /// Check if the given address is valid
        /// </summary>
        /// <param name="address"></param>
        /// <returns>True if valid</returns>
        public static bool IsValid(string address)
        {
            return Regex.IsMatch(address, "^[A-Z]+\\d+$");
        }
    }
}