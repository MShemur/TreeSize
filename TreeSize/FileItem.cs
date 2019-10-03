using System;
using System.Collections.Specialized;
using System.IO;

namespace TreeSize
{
    public class FileItem
    {
        private const double BYTES_IN_KILOBYTES = 1024;
        private const double BYTES_IN_MEGABYTES = 1048576;
        private const double BYTES_IN_GIGABYTES = 1073741824;
        public string Path { get; set; }
        public string Name
        {
            get
            {
                return System.IO.Path.GetFileName(Path);
            }
        }
        private double sizeBytes;
        public string Size
        {
            get
            {
                if (sizeBytes < BYTES_IN_KILOBYTES)
                {
                    return Math.Round(sizeBytes, 1).ToString() + " b";
                }
                else if (sizeBytes < BYTES_IN_MEGABYTES)
                {
                    return Math.Round(sizeBytes.BytesToKilobytes(), 1).ToString() + " kB";
                }
                else if (sizeBytes < BYTES_IN_GIGABYTES)
                {
                    return Math.Round(sizeBytes.BytesToMegabytes(), 1).ToString() + " MB";
                }
                else
                {
                    return Math.Round(sizeBytes.BytesToGigabytes(), 1).ToString() + " GB";
                }
            }
        }

        public double SizeBytes
        {
            get
            {
                return Math.Round(sizeBytes, 1);
            }
            private set { }
        }

        public FileItem(string path)
        {
            Path = path;
            sizeBytes = GetFileSize();
        }

        private double GetFileSize()
        {
            double size = 0;
            try
            {
                var info = new FileInfo(Path);
                size = info.Length;
            }
            catch (Exception)
            {
                throw;
            }
            return size;
        }
    }
}