using System;
using System.Collections.Specialized;
using System.IO;

namespace TreeSize
{
    public class FileItem
    {
        public StringCollection log = new StringCollection();
        private const double BYTES_IN_KILOBYTE = 1024;
        private const double BYTES_IN_MEGABYTE = 1048576;
        private const double BYTES_IN_GIGABYTE = 1073741824;
        private string Path { get; set; }
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
                if (sizeBytes < BYTES_IN_KILOBYTE)
                {
                    return Math.Round(sizeBytes, 1).ToString() + " b";
                }
                else if (sizeBytes < BYTES_IN_MEGABYTE)
                {
                    return Math.Round(sizeBytes.BytesToKilobytes(), 1).ToString() + " kB";
                }
                else if (sizeBytes < BYTES_IN_GIGABYTE)
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
            catch (Exception e)
            {
                log.Add(e.Message);
            }
            return size;
        }
    }
}