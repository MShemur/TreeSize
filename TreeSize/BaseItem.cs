using System;
using System.Collections.Specialized;

namespace TreeSize
{
    public class BaseItem
    {
        protected double BYTES_IN_KILOBYTE = 1024;
        protected double BYTES_IN_MEGABYTE = 1048576;
        protected double BYTES_IN_GIGABYTE = 1073741824;
        public StringCollection log = new StringCollection();

        protected double sizeBytes;
        public double SizeBytes
        {
            get
            {
                return Math.Round(sizeBytes, 1);
            }
            set
            {
                sizeBytes = value;
            }
        }
        public string Size
        {
            get
            {
                if (sizeBytes < BYTES_IN_KILOBYTE)
                {
                    return Math.Round(sizeBytes, 1) + " b";
                }
                else if (sizeBytes < BYTES_IN_MEGABYTE)
                {
                    return Math.Round(sizeBytes.BytesToKilobytes(), 1) + " kB";

                }
                else if (sizeBytes < BYTES_IN_GIGABYTE)
                {
                    return Math.Round(sizeBytes.BytesToMegabytes(), 1) + " MB";
                }
                else
                {
                    return Math.Round(sizeBytes.BytesToGigabytes(), 1) + " GB";
                }
            }
        }
    }
}