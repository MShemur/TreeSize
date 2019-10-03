using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TreeSize
{
    public class Folder
    {
        private const double BYTES_IN_KILOBYTES = 1024;
        private const double BYTES_IN_MEGABYTES = 1048576;
        private const double BYTES_IN_GIGABYTES = 1073741824;
        private const int FILES_WITHOUT_VIRTUAL_SUBFOLDER = 3;
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
            if (Files.Count > FILES_WITHOUT_VIRTUAL_SUBFOLDER)
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
                throw;
            }
            catch (Exception)
            {
                throw;
            }

            if (dirs != null)
            {
                try
                {
                    var exceptions = new ConcurrentQueue<Exception>();

                    Parallel.ForEach(dirs, directory =>
                    {
                        try
                        {
                            SubFolders.Add(new Folder(directory));
                        }
                        catch (Exception e)
                        {
                            exceptions.Enqueue(e);
                        }
                    });
                    if (exceptions.Count > 0)
                    {
                        throw new AggregateException(exceptions);
                    }
                }
                catch (AggregateException)
                {
                    throw;
                }
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
                    throw;
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
