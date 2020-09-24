using System;
using System.Collections.Specialized;
using System.IO;

namespace TreeSize
{
    public class FileItem : BaseItem
    {
        public string Name
        {
            get
            {
                return System.IO.Path.GetFileName(Path);
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