using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TreeSize
{
    public class FolderItem : BaseItem
    {
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
        public ConcurrentBag<FolderItem> SubFolders { get; }
        public FolderItem FilesVirtualSubFolder { get; private set; }
        public ConcurrentBag<FileItem> Files { get; private set; }
        private double sizeOfFiles;

        public FolderItem(string folderPath)
        {
            SubFolders = new ConcurrentBag<FolderItem>();
            Files = new ConcurrentBag<FileItem>();
            Path = folderPath;
            FillFilesAndFolders();
            sizeOfFiles = GetFilesSize();
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
            double size = sizeOfFiles;
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
                FilesVirtualSubFolder = new FolderItem(Path + @"\Files") { SizeBytes = sizeOfFiles, Files = Files };
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
                        FolderItem baseFolder = new FolderItem(directory);
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

        private void FillInnerFilesList(FolderItem folder)
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
