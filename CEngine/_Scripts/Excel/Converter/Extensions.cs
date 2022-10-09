using System;
using System.Collections.Generic;
using System.Linq;

namespace CYM.Excel
{
    /// <summary>
    /// Extensions
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Convert cell to target type
        /// </summary>
        /// <param name="cell">Cell</param>
        /// /// <param name="type">Target type</param>
        /// <returns>Target instance</returns>
        public static object Convert(this Cell cell, Type type)
        {
            return ValueConverter.Convert(type, cell.Text);
        }

        /// <summary>
        /// Convert cell to target type
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="cell">Cell</param>
        /// <returns>Target instance</returns>
        public static T Convert<T>(this Cell cell)
        {
            return (T)Convert(cell, typeof(T));
        }

        /// <summary>
        /// Convert row to object instance
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="row">Row</param>
        /// <param name="generator">Generator</param>
        /// <returns>Object instance</returns>
        public static T Convert<T>(this Row row, IGenerator<T> generator) where T : new()
        {
            return (T)generator.Instantiate(row);
        }

        /// <summary>
        /// Convert row to object instance
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="row">Row</param>
        /// <param name="expression">Mapping expression</param>
        /// <returns>Object instance</returns>
        public static T Convert<T>(this Row row, string expression) where T : new()
        {
            return Convert(row, new Mapper<T>().Map(expression));
        }

        /// <summary>
        /// Convert row to object instance
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="row">Row</param>
        /// <param name="cols">Mapping provider</param>
        /// <returns>Object instance</returns>
        public static T Convert<T>(this Row row, IEnumerable<Cell> cols) where T : new()
        {
            return Convert(row, new Mapper<T>().Map(cols));
        }

        /// <summary>
        /// Convert row to object instance
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="type">Target type</param>
        /// <param name="expression">Mapping expression</param>
        /// <returns>Object instance</returns>
        public static object Convert(this Row row, Type type, string expression)
        {
            return Convert(row, new Mapper(type).Map(expression));
        }

        /// <summary>
        /// Convert row to object instance
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="type">Target type</param>
        /// <param name="cols">Mapping provider</param>
        /// <returns>Object instance</returns>
        public static object Convert(this Row row, Type type, IEnumerable<Cell> cols)
        {
            return Convert(row, new Mapper(type).Map(cols));
        }

        /// <summary>
        /// Convert row to object instance
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="generator">Generator</param>
        /// <returns>Object instance</returns>
        public static object Convert(this Row row, IGenerator generator)
        {
            return generator.Instantiate(row);
        }

        /// <summary>
        /// Convert table to target enumerable
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="table">Table</param>
        /// <returns>Target enumerable</returns>
        public static IEnumerable<T> Convert<T>(this Table table) where T : new()
        {
            var mapper = new TableMapper<T>();
            mapper.Extract();
            return Convert<T>(table, mapper);
        }

        /// <summary>
        /// Convert table to target enumerable
        /// </summary>
        /// <param name="table">Table</param>
        /// <param name="type">Target type</param>
        /// <returns>Target enumerable</returns>
        public static IEnumerable<object> Convert(this Table table, Type type)
        {
            var mapper = new TableMapper(type);
            mapper.Extract();
            return Convert(table, mapper);
        }

        /// <summary>
        /// Convert table to target enumerable
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="table">Table</param>
        /// <param name="generator">Generator</param>
        /// <returns>Target enumerable</returns>
        public static IEnumerable<T> Convert<T>(this Table table, ITableGenerator<T> generator) where T : new()
        {
            return generator.Instantiate(table);
        }

        /// <summary>
        /// Convert table to target enumerable
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="table">Table</param>
        /// <param name="row">One-based mapping provider row index</param>
        /// <returns>Target enumerable</returns>
        public static IEnumerable<T> Convert<T>(this Table table, int row) where T : new()
        {
            if (row < 1)
            {
                throw new ArgumentException("One-based row index must be greater than 0");
            }
            ITableGenerator<T> generator = new TableMapper<T>().Map(table[row - 1]).Exclude(row);
            return generator.Instantiate(table);
        }

        /// <summary>
        /// Convert table to target enumerable
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="table">Table</param>
        /// <param name="expression">Mapping expression</param>
        /// <param name="exclude">One-based row indices to exclude</param>
        /// <returns>Target enumerable</returns>
        public static IEnumerable<T> Convert<T>(this Table table, string expression, params int[] exclude) where T : new()
        {
            var generator = new TableMapper<T>().Map(expression);
            if (exclude != null && exclude.Length > 0)
                generator.Exclude(exclude);
            return Convert(table, generator);
        }

        /// <summary>
        /// Convert table to target enumerable
        /// </summary>
        /// <param name="table">Table</param>
        /// <param name="type">Target type</param>
        /// <param name="expression">Mapping expression</param>
        /// <param name="exclude">One-based row indices to exclude</param>
        /// <returns>Target enumerable</returns>
        public static IEnumerable<object> Convert(this Table table, Type type, string expression, params int[] exclude)
        {
            var generator = new TableMapper(type).Map(expression);
            if (exclude != null && exclude.Length > 0)
                generator.Exclude(exclude);
            return Convert(table, generator);
        }

        /// <summary>
        /// Convert table to target enumerable
        /// </summary>
        /// <param name="table">Table</param>
        /// <param name="generator">Generator</param>
        /// <returns>Target enumerable</returns>
        public static IEnumerable<object> Convert(this Table table, ITableGenerator generator)
        {
            return generator.Instantiate(table);
        }

        /// <summary>
        /// Convert table to target enumerable
        /// </summary>
        /// <param name="table">Table</param>
        /// <param name="type">Target type</param>
        /// <param name="row">One-based mapping provider row index</param>
        /// <returns>Target enumerable</returns>
        public static IEnumerable<object> Convert(this Table table, Type type, int row,bool safeMode=false)
        {
            if (row < 1)
            {
                throw new ArgumentException("One-based row index must be greater than 0");
            }
            var map = new TableMapper(type).Map(table[row - 1]).Exclude(row);
            map.SafeMode = safeMode;
            ITableGenerator generator = map;
            return generator.Instantiate(table);
        }

        /// <summary>
        /// Dump row data to string
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="delimiter">Column delimiter</param>
        /// <param name="enclose">Column escaping enclose</param>
        /// <returns>Plain text</returns>
        public static string Dump(this Row row, char delimiter, char enclose)
        {
            return row.Cells.Select(c => Dump(c, delimiter, enclose)).Aggregate(string.Empty, (a, b) => string.IsNullOrEmpty(a) ? b : a + delimiter + b);
        }

        /// <summary>
        /// Dump row data to string using default delimiter and escaping enclose characters
        /// </summary>
        /// <param name="row">Row</param>
        /// <returns>Plain text</returns>
        public static string Dump(this Row row)
        {
            return Dump(row, Document.Delimiter, Document.Enclose);
        }

        /// <summary>
        /// Dump cell data to string
        /// </summary>
        /// <remarks>
        /// This will enclose delimiter and enclose characters
        /// </remarks>
        /// <param name="cell">Cell</param>
        /// <param name="delimiter">Column delimiter</param>
        /// <param name="enclose">Column escaping enclose</param>
        /// <returns>Plain text</returns>
        public static string Dump(this Cell cell, char delimiter, char enclose)
        {
            var value = cell.Text;
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            if (value.Contains(enclose) || value.Contains(delimiter))
            {
                return enclose + value.Replace(enclose.ToString(), enclose.ToString() + enclose) + enclose;
            }
            return value;
        }

        /// <summary>
        /// Dump cell data to string using default delimiter and escaping enclose characters
        /// </summary>
        /// <param name="cell">Cell</param>
        /// <returns>Plain text</returns>
        public static string Dump(this Cell cell)
        {
            return Dump(cell, Document.Delimiter, Document.Enclose);
        }

        /// <summary>
        /// Dump table data to string
        /// </summary>
        /// <param name="delimiter">Column delimiter</param>
        /// <param name="enclose">Column escaping enclose</param>
        /// <returns>Plain text</returns>
        public static string Dump(this Table table, char delimiter, char enclose)
        {
            return table.Select(r => Dump(r, delimiter, enclose)).Aggregate(string.Empty, (a, b) => string.IsNullOrEmpty(a) ? b : a + Environment.NewLine + b);
        }

        /// <summary>
        /// Dump table data to string using default delimiter and escaping enclose characters
        /// </summary>
        /// <param name="table">Table</param>
        /// <returns>Plain text</returns>
        public static string Dump(this Table table)
        {
            return Dump(table, Document.Delimiter, Document.Enclose);
        }

        /// <summary>
        /// Select cell by address formula
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="address">Address formula(e.g. A1, C20)</param>
        /// <returns>Cell or null</returns>
        /// <exception cref="FormatException"/>
        public static Cell Select(this Row row, string address)
        {
            if (!Address.IsValid(address))
                throw new FormatException();
            var ad = new Address(address);
            return row.FirstOrDefault(cell => cell.Address == ad);
        }

        /// <summary>
        /// Select cells by range formula
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="range">Range formula(e.g. A1:A5, B2:C20)</param>
        /// <returns>Cells or empty</returns>
        /// <exception cref="FormatException"/>
        public static IEnumerable<Cell> SelectRange(this Row row, string range)
        {
            if (!Range.IsValid(range))
                throw new FormatException();
            return row[new Range(range)];
        }

        /// <summary>
        /// Select cell by address formula
        /// </summary>
        /// <param name="table">Table</param>
        /// <param name="address">Address formula(e.g. A1, C20)</param>
        /// <returns>Cell or null</returns>
        /// <exception cref="FormatException"/>
        public static Cell Select(this Table table, string address)
        {
            if (!Address.IsValid(address))
                throw new FormatException();
            var ad = new Address(address);
            return table.SelectMany(row => row).FirstOrDefault(cell => cell.Address == ad);
        }

        /// <summary>
        /// Select cells by range formula
        /// </summary>
        /// <param name="table">Table</param>
        /// <param name="range">Range formula(e.g. A1:A5, B2:C20)</param>
        /// <returns>Cells or empty</returns>
        /// <exception cref="FormatException"/>
        public static IEnumerable<Cell> SelectRange(this Table table, string range)
        {
            if (!Range.IsValid(range))
                throw new FormatException();
            return table[new Range(range)];
        }

        /// <summary>
        /// Select cell by address formula
        /// </summary>
        /// <param name="book">WorkBook</param>
        /// <param name="path">Address formula(e.g. sheet1!A1, sheet2!C20)</param>
        /// <returns>Cell or null</returns>
        /// <exception cref="FormatException"/>
        public static Cell Select(this WorkBook book, string path)
        {
            var args = path.Split(new[] { '!' }, StringSplitOptions.RemoveEmptyEntries);
            if (args.Length != 2)
                throw new FormatException();

            var sheet = book.FirstOrDefault(s => s.Name == args[0]);
            return sheet == null ? null : Select(sheet, args[1]);
        }

        /// <summary>
        /// Select cells by range formula
        /// </summary>
        /// <param name="book">WorkBook</param>
        /// <param name="path">Range formula(e.g. sheet1!A1:A5, sheet2!B2:C20)</param>
        /// <returns>Cells or empty</returns>
        /// <exception cref="FormatException"/>
        public static IEnumerable<Cell> SelectRange(this WorkBook book, string path)
        {
            var args = path.Split(new[] { '!' }, StringSplitOptions.RemoveEmptyEntries);
            if (args.Length != 2)
                throw new FormatException();

            var sheet = book.FirstOrDefault(s => s.Name == args[0]);
            return sheet == null ? null : SelectRange(sheet, args[1]);
        }

        /// <summary>
        /// Recalculate cells' addresses
        /// </summary>
        /// <param name="table">Table</param>
        public static void Recalculate(this Table table)
        {
            for (int i = 0; i < table.Count; i++)
            {
                for (int j = 0; j < table[i].Count; j++)
                {
                    table[i][j].Address = new Address(j + 1, i + 1);
                }
            }
        }

        /// <summary>
        /// Make a non-rectangular table rectangular
        /// </summary>
        /// <remarks>
        /// This also calls <seealso cref="Recalculate"/>
        /// </remarks>
        /// <param name="table">Table</param>
        /// <returns>Expanded table</returns>
        public static Table Expand(this Table table)
        {
            var expanded = table.DeepClone();
            var width = expanded.Max(row => row.Count);
            for (int i = 0; i < expanded.Count; i++)
            {
                var count = width - expanded[i].Count;
                while (count > 0)
                {
                    expanded[i].Cells.Add(new Cell());
                    count--;
                }
            }
            Recalculate(expanded);
            return expanded;
        }

        /// <summary>
        /// Remove empty boundary cells from table then remove all empty lines
        /// </summary>
        /// <remarks>
        /// This also calls <seealso cref="Recalculate"/>
        /// </remarks>
        /// <param name="table">Table</param>
        /// <returns>Collapsed table</returns>
        public static Table Collapse(this Table table)
        {
            var collapsed = table.DeepClone();
            for (int i = 0; i < collapsed.Count; i++)
            {
                for (int j = 0; j < collapsed[i].Count; j++)
                {
                    if (!string.IsNullOrEmpty(collapsed[i][j].Text))
                        break;
                    collapsed[i].Cells.RemoveAt(j--);
                }
                for (int j = collapsed[i].Count - 1; j >= 0; j--)
                {
                    if (!string.IsNullOrEmpty(collapsed[i][j].Text))
                        break;
                    collapsed[i].Cells.RemoveAt(j);
                }
            }
            for (int i = collapsed.Count - 1; i >= 0; i--)
            {
                if (collapsed[i].Count == 0)
                    collapsed.Rows.RemoveAt(i);
            }
            Recalculate(collapsed);
            return collapsed;
        }

        /// <summary>
        /// Rotate table clockwise
        /// </summary>
        /// <remarks>
        /// This also calls <seealso cref="Expand"/>
        /// </remarks>
        /// <param name="table">Table</param>
        /// <returns>Rotated table</returns>
        public static Table Rotate(this Table table)
        {
            var expanded = Expand(table);
            if (expanded.Count == 0)
            {
                return expanded;
            }
            var height = expanded[0].Count;
            if (height == 0)
            {
                return expanded;
            }
            var width = expanded.Count;
            var rotated = table.DeepClone();
            rotated.Rows.Clear();
            for (int i = 0; i < height; i++)
            {
                var row = new Row();
                for (int j = width - 1; j >= 0; j--)
                {
                    row.Cells.Add(expanded[j][i]);
                }
                rotated.Rows.Add(row);
            }
            Recalculate(rotated);
            return rotated;
        }

        /// <summary>
        /// Check if a row is empty(with no cells or all cells' values being null or empty string)
        /// </summary>
        /// <param name="row">Row</param>
        /// <returns>True if empty</returns>
        public static bool IsEmpty(this Row row)
        {
            return row.Count == 0 || row.All(col => string.IsNullOrEmpty(col.Text));
        }
    }
}