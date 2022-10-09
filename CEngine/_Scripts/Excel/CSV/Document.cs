using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Linq;

namespace CYM.Excel
{
    /// <summary>
    /// A collection of <see cref="Row"/>
    /// </summary>
    public sealed class Document : Table
    {
        /// <summary>
        /// Column delimiter
        /// </summary>
        public static char Delimiter = ',';

        /// <summary>
        /// Column escaping enclose
        /// </summary>
        public static char Enclose = '"';

        public Document(IList<Row> list) : base(list)
        {
        }

        public Document() : this(new List<Row>())
        {
        }

        /// <summary>
        /// Load at path
        /// </summary>
        /// <param name="path">path</param>
        /// <returns>Document</returns>
        public static Document LoadAt(string path)
        {
            return Load(File.ReadAllText(path));
        }

        /// <summary>
        /// Load string content
        /// </summary>
        /// <param name="content">Plain text content</param>
        /// <returns>Document</returns>
        public static Document Load(string content)
        {
            using (var reader = new StringReader(content))
            {
                return Load(reader);
            }
        }

        /// <summary>
        /// Load stream content
        /// </summary>
        /// <param name="stream">Stream content</param>
        /// <returns>Document</returns>
        public static Document Load(Stream stream)
        {
            return Load(new StreamReader(stream));
        }

        /// <summary>
        /// Load bytes content
        /// </summary>
        /// <param name="buffer">CSV bytes content</param>
        /// <returns>Document</returns>
        public static Document Load(byte[] buffer)
        {
            using (var stream = new MemoryStream(buffer))
            {
                return Load(stream);
            }
        }

        /// <summary>
        /// Load and parse document
        /// </summary>
        /// <param name="reader">Content reader</param>
        /// <returns>Document</returns>
        public static Document Load(TextReader reader)
        {
            var doc = new Document();
            var record = new Row();
            int col = 1, row = 1, pos = 0;
            var sb = new StringBuilder();
            while (reader.Peek() != -1)
            {
                var token = (char)reader.Read();
                pos++;
                switch (token)
                {
                    case '\n':
                    case '\r':
                        if (token == '\r' && reader.Peek() == (int)'\n')
                        {
                            reader.Read();
                            pos++;
                        }
                        record.Cells.Add(new Cell(sb.ToString(), new Address(col, row)));
                        doc.Items.Add(record);
                        col = 1;
                        pos = 0;
                        sb = new StringBuilder();
                        row++;
                        record = new Row();
                        break;
                    default:
                        if (token == Delimiter)
                        {
                            record.Cells.Add(new Cell(sb.ToString(), new Address(col, row)));
                            col++;
                            sb = new StringBuilder();
                        }
                        else if (token == Enclose)
                        {
                            while (reader.Peek() != -1)
                            {
                                token = (char)reader.Read();
                                pos++;
                                if (token != Enclose)
                                    sb.Append(token);
                                else
                                {
                                    if (reader.Peek() != (int)Enclose)
                                        break;
                                    else
                                    {
                                        reader.Read();
                                        pos++;
                                        sb.Append(token);
                                    }
                                }
                            }
                        }
                        else
                        {
                            sb.Append(token);
                        }
                        break;
                }
            }
            record.Cells.Add(new Cell(sb.ToString(), new Address(col, row)));
            doc.Items.Add(record);

            return doc;
        }

        /// <summary>
        /// Restore default settings
        /// </summary>
        public static void Reset()
        {
            Delimiter = ',';
            Enclose = '"';
        }

        public override Table DeepClone()
        {
            return new Document(Items.Select(row => row.DeepClone()).ToList());
        }

        public override Table ShallowClone()
        {
            return (Table)MemberwiseClone();
        }
    }
}