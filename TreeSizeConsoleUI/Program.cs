using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TreeSize;

namespace TreeSizeConsoleUI
{
   public class Program
    {
        static void Main(string[] args)
        {

            // ThreadPool.QueueUserWorkItem(JobForAThread);

            var watch = System.Diagnostics.Stopwatch.StartNew();

            Do();

            Console.WriteLine("done");

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
               Console.WriteLine("Main finished in " + elapsedMs);

            Console.ReadKey();
        }

        static void JobForAThread(object state)
        {

            Folder folder = new Folder(@"H:\Флешка");
            Console.WriteLine("выполнение внутри потока из пула {1}", Thread.CurrentThread.ManagedThreadId);

            //foreach (var item in folder.SubFolders)
            //{
            //    Console.WriteLine(item.Path);
            //}
        }

        static void Do()
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            Folder folder = new Folder(@"E:\");

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
        //    Console.WriteLine("Main finished in " + elapsedMs);

            //foreach (var item in folder.SubFolders)
            //{
            //    Console.WriteLine(item.Path);
            //}
        }
    }
}
