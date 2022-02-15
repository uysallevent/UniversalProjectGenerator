using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace UnivesalProjectGenerator.Helpers
{
    public static class FileWatcherHelper
    {
        public static Task WatchFile(string path)
        {
            return Task.Run(() =>
          {
              var watcher = new FileSystemWatcher();
              watcher.Path = path;
              watcher.EnableRaisingEvents = true;
              watcher.IncludeSubdirectories = true;
              watcher.NotifyFilter =
                  NotifyFilters.FileName |
                  NotifyFilters.DirectoryName;
              watcher.Created += Watcher_Created;
              watcher.Changed += Watcher_Changed;
              watcher.Deleted += Watcher_Deleted;
          });

        }

        private static void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            Console.Out.WriteLineAsync($"{e.ChangeType} => ' {e.Name} '");
        }

        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Console.Out.WriteLineAsync($"{e.ChangeType} => ' {e.Name} '");
        }

        private static void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            Console.Out.WriteLineAsync($"{e.ChangeType} => ' {e.Name} '");
        }
    }
}
