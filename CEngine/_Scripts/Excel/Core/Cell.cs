using System;

namespace CYM.Excel
{
    /// <summary>
    /// Data cell
    /// </summary>
    public class Cell : ICloneable<Cell>
    {
        /// <summary>
        /// Cell address
        /// </summary>
        public Address Address { get; set; }

        /// <summary>
        /// Cell value
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// If this cell is a span cell(which has no value until <see cref="WorkSheet.Merge"/> is called)
        /// </summary>
        public bool IsSpan { get; set; }

        public Cell()
        {

        }

        public Cell(Address address) : this()
        {
            Address = address;
        }

        public Cell(string value, Address address) : this(address)
        {
            Value = value;
        }

        public Cell(bool value, Address address) : this(address)
        {
            Value = value;
        }

        public Cell(int value, Address address) : this(address)
        {
            Value = value;
        }

        public Cell(long value, Address address) : this(address)
        {
            Value = value;
        }

        public Cell(float value, Address address) : this(address)
        {
            Value = value;
        }

        public Cell(double value, Address address) : this(address)
        {
            Value = value;
        }

        /// <summary>
        /// Get plain text(value cast to string)
        /// </summary>
        /// <remarks>
        /// Null value will return an empty string
        /// </remakrs>
        public virtual string Text
        {
            get
            {
                return Convert.ToString(Value) ?? string.Empty;
            }
        }

        /// <summary>
        /// Get string value if possible; otherwise an exception will be thrown
        /// </summary>
        /// <exception cref="InvalidCastException"/>
        public string String
        {
            get
            {
                if (!IsString)
                    throw new InvalidCastException();
                return (string)Value;
            }
        }

        /// <summary>
        /// Get int value if possible; otherwise an exception will be thrown
        /// </summary>
        /// <exception cref="InvalidCastException"/>
        public int Integer
        {
            get
            {
                if (!IsInteger)
                    throw new InvalidCastException();
                return (int)Value;
            }
        }

        /// <summary>
        /// Get bool value if possible; otherwise an exception will be thrown
        /// </summary>
        /// <exception cref="InvalidCastException"/>
        public bool Boolean
        {
            get
            {
                if (!IsBoolean)
                    throw new InvalidCastException();
                return (bool)Value;
            }
        }

        /// <summary>
        /// Get float value if possible; otherwise an exception will be thrown
        /// </summary>
        /// <exception cref="InvalidCastException"/>
        public float Single
        {
            get
            {
                if (!IsSingle)
                    throw new InvalidCastException();
                return (float)Value;
            }
        }

        /// <summary>
        /// Get double value if possible; otherwise an exception will be thrown
        /// </summary>
        /// <exception cref="InvalidCastException"/>
        public double Double
        {
            get
            {
                if (!IsDouble)
                    throw new InvalidCastException();
                return (double)Value;
            }
        }

        /// <summary>
        /// Check if this cell's value type is <see cref="int"/>
        /// </summary>
        public bool IsInteger
        {
            get { return Value is int; }
        }

        /// <summary>
        /// Check if this cell's value type is <see cref="float"/>
        /// </summary>
        public bool IsSingle
        {
            get { return Value is float; }
        }

        /// <summary>
        /// Check if this cell's value type is <see cref="double"/>
        /// </summary>
        public bool IsDouble
        {
            get { return Value is double; }
        }

        /// <summary>
        /// Check if this cell's value type is <see cref="bool"/>
        /// </summary>
        public bool IsBoolean
        {
            get { return Value is bool; }
        }

        /// <summary>
        /// Check if this cell's value type is <see cref="string"/>
        /// </summary>
        public bool IsString
        {
            get { return Value is string; }
        }

        public override string ToString()
        {
            return Text;
        }

        public Cell DeepClone()
        {
            return (Cell)MemberwiseClone();
        }

        public Cell ShallowClone()
        {
            return (Cell)MemberwiseClone();
        }

        public static implicit operator string(Cell cell)
        {
            return cell.String;
        }

        public static implicit operator int(Cell cell)
        {
            return cell.Integer;
        }

        public static implicit operator bool(Cell cell)
        {
            return cell.Boolean;
        }

        public static implicit operator float(Cell cell)
        {
            return cell.Single;
        }

        public static implicit operator double(Cell cell)
        {
            return cell.Double;
        }
    }
}