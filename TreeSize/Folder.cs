using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using ByteSizeLib;
using System.Collections.Concurrent;

namespace TreeSize
{
    public class Folder
    {
        private readonly object balanceLock = new object();

        public string Path { get; set; }
        public string Name
        {
            get
            {
                return new DirectoryInfo(Path).Name;
            }
        }
        public ConcurrentBag<Folder> SubFolders { get; set; }
        public Folder FilesVirtualSubFolder;
        public ConcurrentBag<FileItem> Files { get; set; }
        private double filesSize;
        private double sizeBytes;
        public double SizeBytes
        {
            get
            {
                return Math.Round(sizeBytes, 1);
            }
            private set
            {
                sizeBytes = value;
            }
        }
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

        public Folder(string folderPath)
        {
            SubFolders = new ConcurrentBag<Folder>();
            Files = new ConcurrentBag<FileItem>();
            Path = folderPath;
            FillInnerFilesList(this);
            FillInnerFoldersList();
            filesSize = GetFilesSize();
            sizeBytes = GetFolderSize();
            MakeFolderForFiles();
        }

        private double GetFilesSize()
        {
            double size = 0;
            if (Files.Count > 0)
                foreach (var item in Files)
                {
                    size += item.SizeBytes;
                }
            return size;
        }

        private double GetFolderSize()
        {
            double size = filesSize;
            if (SubFolders.Count > 0)
            {
                foreach (var item in SubFolders)
                {
                    size += item.SizeBytes;
                }
            }
            return size;
        }

        private void MakeFolderForFiles()
        {
            if (Files.Count > 3)
            {
                FilesVirtualSubFolder = new Folder(Path + @"\Files") { SizeBytes = filesSize };
                FilesVirtualSubFolder.Files = Files;
            }
        }

        private void FillInnerFoldersList()
        {
            IEnumerable<string> dirs = null;
            try
            {
                dirs = Directory.EnumerateDirectories(Path, "*", SearchOption.TopDirectoryOnly);
            }
            catch (UnauthorizedAccessException)
            {
            }
            catch (Exception)
            {
            }

            if (dirs != null)
            {
                Parallel.ForEach(dirs, directory =>
                {
                    SubFolders.Add(new Folder(directory));
                });
            }
        }

        private void FillInnerFilesList(Folder folder)
        {
            string path = folder.Path;
            IEnumerable<string> files = null;
            if (Directory.Exists(path))
                try
                {
                    files = Directory.EnumerateFiles(Path, "*", SearchOption.TopDirectoryOnly);
                }
                catch (Exception)
                {
                }
            if (files != null)
                lock (balanceLock)
                {
                    Parallel.ForEach(files, directory =>
                {
                    Files.Add(new FileItem(directory));
                });
                }
        }
    }
}
