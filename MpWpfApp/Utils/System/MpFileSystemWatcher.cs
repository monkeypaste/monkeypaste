﻿using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public interface MpIFileSystemEventHandler {
        void OnChanged(object sender, FileSystemEventArgs e);
        void OnCreated(object sender, FileSystemEventArgs e);
        void OnDeleted(object sender, FileSystemEventArgs e);
        void OnRenamed(object sender, RenamedEventArgs e);
        void OnError(object sender, ErrorEventArgs e);
    }

    public class MpFileSystemWatcher : MpSingleton<MpFileSystemWatcher>, IDisposable {
        #region Private Variables

        private List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();
        private Dictionary<FileSystemWatcher, List<MpIFileSystemEventHandler>> _handlerLookup = new Dictionary<FileSystemWatcher, List<MpIFileSystemEventHandler>>();

        #endregion

        #region Constructors

        public MpFileSystemWatcher() {
            
        }

        public async Task Initialize() {
            await Task.Delay(1);
            _watchers.Clear();
            _handlerLookup.Clear();
        }

        #endregion

        #region Public Methods

        public void AddWatcher(string path, MpIFileSystemEventHandler handler = null) {
            if(!File.Exists(path) && !Directory.Exists(path)) {
                throw new FileNotFoundException(path);
            }
            FileSystemWatcher watcher = _watchers.FirstOrDefault(x => x.Path.ToLower() == path.ToLower());
            
            if(watcher == null) {
                watcher = new FileSystemWatcher(path);
                _watchers.Add(watcher);
            } else {
                throw new Exception("Watcher already exists");
            }

            watcher.NotifyFilter = watcher.NotifyFilter 
                                 | NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            watcher.Changed += OnChanged;
            watcher.Created += OnCreated;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;
            watcher.Error += OnError;

            if (handler != null) {
                watcher.Changed += handler.OnChanged;
                watcher.Created += handler.OnCreated;
                watcher.Deleted += handler.OnDeleted;
                watcher.Renamed += handler.OnRenamed;
                watcher.Error += handler.OnError;
            }

            if(Directory.Exists(path)) {
                watcher.Filter = "*";
                watcher.IncludeSubdirectories = true;
                watcher.EnableRaisingEvents = true;
            }
        }

        public bool RemoveWatcher(string path) {
            FileSystemWatcher watcher = _watchers.FirstOrDefault(x => x.Path.ToLower() == path.ToLower());
            if(watcher == null) {
                return false;
            }
            _watchers.Remove(watcher);

            watcher.Changed -= OnChanged;
            watcher.Created -= OnCreated;
            watcher.Deleted -= OnDeleted;
            watcher.Renamed -= OnRenamed;
            watcher.Error -= OnError;
            watcher.Dispose();

            return true;
        }

        #endregion

        #region Private Methods

        private void OnChanged(object sender, FileSystemEventArgs e) {
            if (e.ChangeType != WatcherChangeTypes.Changed) {
                return;
            }
            Console.WriteLine($"Changed: {e.FullPath}");
        }

        private void OnCreated(object sender, FileSystemEventArgs e) {
            string value = $"Created: {e.FullPath}";
            Console.WriteLine(value);
        }

        private void OnDeleted(object sender, FileSystemEventArgs e) =>
            Console.WriteLine($"Deleted: {e.FullPath}");

        private void OnRenamed(object sender, RenamedEventArgs e) {
            Console.WriteLine($"Renamed:");
            Console.WriteLine($"    Old: {e.OldFullPath}");
            Console.WriteLine($"    New: {e.FullPath}");
        }

        private  void OnError(object sender, ErrorEventArgs e) =>
            PrintException(e.GetException());

        private  void PrintException(object e) {
            if (e != null && e is Exception ex) {
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine("Stacktrace:");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine();
                PrintException(ex.InnerException);
            }
        }

        #endregion

        #region IDisposable Implementation
        // based on http://support.surroundtech.com/thread/memory-management-best-practices-in-wpf/
        // and https://web.archive.org/web/20200720045029/https://docs.microsoft.com/en-us/archive/blogs/jgoldb/finding-memory-leaks-in-wpf-based-applications

        public virtual void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            // Release unmanaged memory
            if (disposing) {
                _watchers.ForEach(x => RemoveWatcher(x.Path));
            }
        }

        ~MpFileSystemWatcher() {
            Dispose(false);
        }

        #endregion
    }
}