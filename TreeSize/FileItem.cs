using System;
using System.IO;
using ByteSizeLib;

namespace TreeSize
{
    public class FileItem
    {
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
                if (sizeBytes < ByteSize.BytesInKiloByte)
                {
                    return Math.Round(sizeBytes, 1).ToString() + " b";
                }
                else if (sizeBytes < ByteSize.BytesInMegaByte)
                {
                    return Math.Round(ByteSize.FromBytes(sizeBytes).KiloBytes, 1).ToString() + " kB";

                }
                else if (sizeBytes < ByteSize.BytesInGigaByte)
                {
                    return Math.Round(ByteSize.FromBytes(sizeBytes).MegaBytes, 1).ToString() + " MB";
                }
                else
                {
                    return Math.Round(ByteSize.FromBytes(sizeBytes).GigaBytes, 1).ToString() + " GB";
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
            double size=0;
            try
            {
                var info = new FileInfo(Path);
                size = info.Length;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return size;
        }
    }
}