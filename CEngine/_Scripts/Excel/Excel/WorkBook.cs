using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using ICSharpCode.SharpZipLib.Zip;

namespace CYM.Excel
{
    /// <summary>
    /// A collection of <see cref="WorkSheet"/>
    /// </summary>
    public sealed class WorkBook : KeyedCollection<string, WorkSheet>
    {
        /// <summary>
        /// Shared strings
        /// </summary>
        public SharedStringCollection SharedStrings { get; private set; }

        /// <summary>
        /// Initializes a new instance of the WorkBook class from binary buffer
        /// </summary>
        /// <param name="buffer">Binary buffer</param>
        public WorkBook(byte[] buffer)
        {
            using (var zip = new ZipFile(new MemoryStream(buffer)))
            {
                Read(zip);
            }
        }

        /// <summary>
        /// Initializes a new instance of the WorkBook class from binary stream
        /// </summary>
        /// <param name="stream">Binary stream</param>
        public WorkBook(Stream stream)
        {
            var zip = new ZipFile(stream);
            Read(zip);
        }

        /// <summary>
        /// Initializes a new instance of the WorkBook class from file path
        /// </summary>
        /// <param name="fileName">File path</param>
        public WorkBook(string fileName)
        {
            using (var zip = new ZipFile(fileName))
            {
                Read(zip);
            }
        }

        private void Read(ZipFile zip)
        {
            var sharedStrings = new List<string>();
            var entry = zip.GetEntry("xl/sharedStrings.xml");
            if (entry != null)
                using (var sharedStringReader = XmlReader.Create(zip.GetInputStream(entry)))
                {
                    if (sharedStringReader.ReadToFollowing("sst"))
                    {
                        while (sharedStringReader.ReadToFollowing("si"))
                        {
                            using (var itemReader = sharedStringReader.ReadSubtree())
                            {
                                var sb = new StringBuilder();
                                while (!itemReader.EOF)
                                {
                                    if (itemReader.NodeType != XmlNodeType.Element)
                                    {
                                        itemReader.Read();
                                    }
                                    else
                                    {
                                        switch (itemReader.Name)
                                        {
                                            case "t":
                                                sb.Append(itemReader.ReadElementContentAsString());
                                                break;
                                            case "r":
                                            case "si":
                                                itemReader.Read();
                                                break;
                                            default:
                                                itemReader.Skip();
                                                break;
                                        }

                                    }
                                }
                                sharedStrings.Add(sb.ToString());
                            }
                        }
                    }
                }

            using (var workBookReader = XmlReader.Create(zip.GetInputStream(zip.GetEntry("xl/workbook.xml"))))
            {
                if (workBookReader.ReadToFollowing("sheets"))
                {
                    if (workBookReader.ReadToDescendant("sheet"))
                    {
                        var index = 1;
                        do
                        {
                            var name = workBookReader.GetAttribute("name");
                            var id = workBookReader.GetAttribute("sheetId");
                            using (
                                var sheetReader =
                                    XmlReader.Create(
                                        zip.GetInputStream(zip.GetEntry(string.Format("xl/worksheets/sheet{0}.xml", index++))))
                                )
                            {
                                var rows = new List<Row>();
                                var spans = new List<Range>();

                                if (sheetReader.ReadToFollowing("sheetData"))
                                {
                                    if (sheetReader.ReadToDescendant("row"))
                                    {
                                        do
                                        {
                                            if (sheetReader.ReadToDescendant("c"))
                                            {
                                                var row = new List<Cell>();
                                                do
                                                {
                                                    var address = sheetReader.GetAttribute("r");
                                                    var type = sheetReader.GetAttribute("t");
                                                    Cell cell;
                                                    if (sheetReader.ReadToDescendant("v"))
                                                    {
                                                        if (type == "s") //string
                                                        {
                                                            var val = sheetReader.ReadElementContentAsInt();
                                                            cell = new Cell(sharedStrings[val], new Address(address));
                                                        }
                                                        else if (type == "b") //boolean
                                                        {
                                                            cell = new Cell(sheetReader.ReadElementContentAsBoolean(),
                                                                new Address(address));
                                                        }
                                                        else if (type == "str" || type == "inlineStr") // inline
                                                        {
                                                            var val = sheetReader.ReadElementContentAsString();
                                                            cell = new Cell(val, new Address(address));
                                                        }
                                                        else
                                                        {
                                                            var val = sheetReader.ReadElementContentAsString();
                                                            if (Regex.IsMatch(val, @"^[-+]?\d*(\.\d+)?$"))  //number
                                                            {
                                                                if (Regex.IsMatch(val, @"^[-+]?\d+$"))
                                                                {
                                                                    if (Regex.Matches(val, "\\d").Count <= 11) //integer
                                                                        cell = new Cell(int.Parse(val),
                                                                            new Address(address));
                                                                    else //long
                                                                        cell = new Cell(long.Parse(val),
                                                                            new Address(address));
                                                                }
                                                                else if (Regex.Matches(val, "\\d").Count <= 7)
                                                                //floating points
                                                                {
                                                                    cell = new Cell(float.Parse(val),
                                                                        new Address(address));
                                                                }
                                                                else //double
                                                                {
                                                                    cell = new Cell(double.Parse(val),
                                                                        new Address(address));
                                                                }
                                                            }
                                                            else //literal
                                                            {
                                                                cell = new Cell(val, new Address(address));
                                                            }
                                                        }
                                                    }
                                                    else //span
                                                        cell = new Cell(new Address(address)) { IsSpan = true };
                                                    var last = row.LastOrDefault();
                                                    if (last != null) // fill gap
                                                    {
                                                        var count = cell.Address.Column - last.Address.Column - 1;
                                                        if (count > 0)
                                                        {
                                                            row.AddRange(Enumerable.Range(1, count).Select(i => new Cell(new Address(last.Address.Column + i, last.Address.Row))));
                                                        }
                                                    }
                                                    else if (cell.Address.Column > 0)
                                                    {
                                                        var count = cell.Address.Column - 1;
                                                        row.AddRange(Enumerable.Range(0, count).Select(i => new Cell(new Address(i, cell.Address.Row))));
                                                    }
                                                    row.Add(cell);
                                                    // sheetReader.Skip(); //TODO: advance to the next c
                                                } while (sheetReader.ReadToNextSibling("c"));

                                                rows.Add(new Row(row));
                                                // sheetReader.Skip(); //TODO: advance to the next row
                                            }
                                        } while (sheetReader.ReadToNextSibling("row"));
                                    }

                                    if (sheetReader.ReadToFollowing("mergeCells"))
                                    {
                                        if (sheetReader.ReadToDescendant("mergeCell"))
                                        {
                                            do
                                            {
                                                var @ref = sheetReader.GetAttribute("ref");
                                                spans.Add(new Range(@ref));
                                            } while (sheetReader.ReadToNextSibling("mergeCell"));
                                        }
                                    }

                                    var sheet = new WorkSheet(id, name, rows, spans);
                                    Add(sheet);
                                }
                            }
                        } while (workBookReader.ReadToNextSibling("sheet"));
                    }
                }
            }

            SharedStrings = new SharedStringCollection(sharedStrings);
        }

        #region Overrides of KeyedCollection<string,WorkSheet>

        protected override string GetKeyForItem(WorkSheet item)
        {
            return item.Name;
        }

        #endregion

        /// <summary>
        /// Apply merge(assign shared value to each span cells)
        /// </summary>
        public void Merge()
        {
            foreach (var sheet in this)
            {
                sheet.Merge();
            }
        }
    }
}