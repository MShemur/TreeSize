using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TreeSize
{
    public class Folder
    {
        public StringCollection log = new StringCollection();

        private const double BYTES_IN_KILOBYTES = 1024;
        private const double BYTES_IN_MEGABYTES = 1048576;
        private const double BYTES_IN_GIGABYTES = 1073741824;
        private const int FILES_WITHOUT_VIRTUAL_SUBFOLDER = 3;
        private readonly object balanceLock = new object();
        public string Path { get; }
        public string Name
        {
            get
            {
                return new DirectoryInfo(Path).Name;
            }
        }
        public ConcurrentBag<Folder> SubFolders { get; }
        public Folder FilesVirtualSubFolder { get; private set; }
        public ConcurrentBag<FileItem> Files { get; private set; }
        private double filesSize;
        private double sizeBytes;
        private double SizeBytes
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
                if (sizeBytes < BYTES_IN_KILOBYTES)
                {
                    return Math.Round(sizeBytes, 1) + " b";
                }
                else if (sizeBytes < BYTES_IN_MEGABYTES)
                {
                    return Math.Round(sizeBytes.BytesToKilobytes(), 1) + " kB";

                }
                else if (sizeBytes < BYTES_IN_GIGABYTES)
                {
                    return Math.Round(sizeBytes.BytesToMegabytes(), 1) + " MB";
                }
                else
                {
                    return Math.Round(sizeBytes.BytesToGigabytes(), 1) + " GB";
                }
            }
        }

        public Folder(string folderPath)
        {
            SubFolders = new ConcurrentBag<Folder>();
            Files = new ConcurrentBag<FileItem>();
            Path = folderPath;
            FillFilesAndFolders();
            filesSize = GetFilesSize();
            sizeBytes = GetFolderSize();
            MakeFolderForFiles();
        }

        private void FillFilesAndFolders()
        {
            try
            {
                FillInnerFilesList(this);
                FillInnerFoldersList();
            }
            catch (AggregateException ae)
            {
                foreach (var ex in ae.Flatten().InnerExceptions)
                {
                    log.Add(ex.Message);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                log.Add(ex.Message);
            }
            catch (Exception ex)
            {
                log.Add(ex.Message);
            }
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
                FilesVirtualSubFolder = new Folder(Path + @"\Files") { SizeBytes = filesSize, Files = Files };
            }
        }

        private void FillInnerFoldersList()
        {
            IEnumerable<string> dirs = null;
            dirs = Directory.EnumerateDirectories(Path, "*", SearchOption.TopDirectoryOnly);

            if (dirs != null)
            {
                try
                {
                    var exceptions = new ConcurrentQueue<Exception>();

                    Parallel.ForEach(dirs, directory =>
                {
                    try
                    {
                        Folder baseFolder = new Folder(directory);
                        SubFolders.Add(baseFolder);
                        foreach (var item in baseFolder.log)
                        {
                            log.Add(item);
                        }
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
                catch (AggregateException e)
                {
                    throw e;
                }
            }
        }

        private void FillInnerFilesList(Folder folder)
        {
            string path = folder.Path;
            IEnumerable<string> files = null;
            if (Directory.Exists(path)) files = Directory.EnumerateFiles(Path, "*", SearchOption.TopDirectoryOnly);
            if (files != null)
                lock (balanceLock)
                {
                    Parallel.ForEach(files, directory =>
                {

                    FileItem item = new FileItem(directory);
                    Files.Add(item);
                    foreach (var lg in item.log)
                    {
                        log.Add(lg);
                    }
                });
                }
        }
    }
}
