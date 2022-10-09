using System;
using System.IO;

namespace CYM
{
    [Serializable]
    [Unobfus]
    public class FileAttachment
    {
        private string name;
        private byte[] data;
        private string mimeType;

        /// <summary>
        /// The name of the file attachment (0 to 256 characters).
        /// </summary>
        public string Name
        {
            get { return name; }
            set
            {
                if (value.Length > 256)
                    throw new Exception("File name is too long!");

                name = value;
            }
        }

        /// <summary>
        /// Attached file data
        /// </summary>
        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }

        public string MimeType
        {
            get { return mimeType; }
            set { mimeType = value; }
        }

        /// <summary>
        /// Creates a new instance of the FileAttachment object
        /// </summary>
        /// <param name="name">The name of the attachment</param>
        /// <param name="data">The file data</param>
        public FileAttachment(string name, byte[] data, string mimeType = null)
        {
            this.name = name;
            this.data = data;
            this.mimeType = mimeType;
        }

        /// <summary>
        /// Creates a new instance of the FileAttachment object
        /// </summary>
        /// <param name="name">The name of the attachment</param>
        /// <param name="filePath">The path to the file</param>
        public FileAttachment(string filePath, string mimeType = null)
        {
            this.name = Path.GetFileName(filePath);
            this.data = File.ReadAllBytes(filePath);
            this.mimeType = mimeType;
        }

        /// <summary>
        /// Creates a new instance of the FileAttachment object
        /// </summary>
        /// <param name="name">The name of the attachment</param>
        /// <param name="filePath">The path to the file</param>
        public FileAttachment(string name, string filePath, string mimeType = null)
        {
            this.name = name;
            this.data = File.ReadAllBytes(filePath);
            this.mimeType = mimeType;
        }
    }
}
