using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvFileSystemWatcher :
        IDisposable,
        MpIActionComponent {
        #region Private Variables

        private Dictionary<FileSystemWatcher, List<MpIFileSystemEventHandler>> _handlerLookup = new Dictionary<FileSystemWatcher, List<MpIFileSystemEventHandler>>();

        #endregion

        #region Properties

        #region MpIActionComponent Implementation

        void MpIActionComponent.RegisterActionComponent(MpIInvokableAction mvm) {
            if (_handlerLookup.Any(x => x.Value.Contains(mvm as MpIFileSystemEventHandler))) {
                return;
            }
            var fstvm = mvm as MpAvFolderWatcherTriggerViewModel;
            AddWatcher(fstvm.FolderPath, fstvm);
            MpConsole.WriteLine($"{GetType()} Registered {mvm.Label}");
        }

        void MpIActionComponent.UnregisterActionComponent(MpIInvokableAction mvm) {
            if (!_handlerLookup.Any(x => x.Value.Contains(mvm as MpIFileSystemEventHandler))) {
                return;
            }
            var fstvm = mvm as MpAvFolderWatcherTriggerViewModel;
            RemoveWatcher(fstvm.FolderPath);
            MpConsole.WriteLine($"{GetType()} Unregistered {mvm.Label}");
        }

        #endregion

        #endregion

        #region Constructors


        public MpAvFileSystemWatcher() { }

        public async Task InitAsync() {
            await Task.Delay(1);
            _handlerLookup.Clear();
        }

        #endregion

        #region Public Methods

        public bool AddWatcher(string path, MpIFileSystemEventHandler handler) {
            if (!path.IsFileOrDirectory()) {
                return false;
            }
            if (_handlerLookup.Select(x => x.Key).FirstOrDefault(x => x.Path.ToLower() == path.ToLower())
                is FileSystemWatcher existing_watcher) {
                _handlerLookup[existing_watcher].Add(handler);
                return true;
            }
            FileSystemWatcher watcher = new FileSystemWatcher(path);
            _handlerLookup.Add(watcher, new[] { handler }.ToList());

            watcher.NotifyFilter = watcher.NotifyFilter
                                 //| NotifyFilters.Attributes
                                 //| NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 //| NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite;
            //| NotifyFilters.Security
            //| NotifyFilters.Size;

            watcher.Changed += OnChanged;
            watcher.Created += OnCreated;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;
            watcher.Error += OnError;

            if (handler != null) {
                watcher.Changed += handler.OnFileSystemItemChanged;
                watcher.Created += handler.OnFileSystemItemChanged;
                watcher.Deleted += handler.OnFileSystemItemChanged;
                watcher.Renamed += handler.OnFileSystemItemChanged;
                //watcher.Error += handler.OnError;
            }

            if (Directory.Exists(path)) {
                watcher.Filter = "*";
                watcher.IncludeSubdirectories = watcher.IncludeSubdirectories;
                watcher.EnableRaisingEvents = true;
            }
            return true;
        }

        public bool RemoveWatcher(string path) {
            FileSystemWatcher watcher = _handlerLookup.Select(x => x.Key).FirstOrDefault(x => x.Path.ToLower() == path.ToLower());
            if (watcher == null) {
                return false;
            }
            _handlerLookup.Remove(watcher);

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
            MpConsole.WriteLine($"Changed: {e.FullPath}");
        }

        private void OnCreated(object sender, FileSystemEventArgs e) {
            string value = $"Created: {e.FullPath}";
            MpConsole.WriteLine(value);
        }

        private void OnDeleted(object sender, FileSystemEventArgs e) =>
            MpConsole.WriteLine($"Deleted: {e.FullPath}");

        private void OnRenamed(object sender, RenamedEventArgs e) {
            MpConsole.WriteLine($"Renamed:");
            MpConsole.WriteLine($"    Old: {e.OldFullPath}");
            MpConsole.WriteLine($"    New: {e.FullPath}");
        }

        private void OnError(object sender, ErrorEventArgs e) =>
            PrintException(e.GetException());

        private void PrintException(object e) {
            if (e != null && e is Exception ex) {
                MpConsole.WriteLine($"Message: {ex.Message}");
                MpConsole.WriteLine("Stacktrace:");
                MpConsole.WriteLine(ex.StackTrace);
                MpConsole.WriteLine("");
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
                _handlerLookup.Select(x => x.Key).ForEach(x => RemoveWatcher(x.Path));
                _handlerLookup.Clear();
            }
        }

        ~MpAvFileSystemWatcher() {
            Dispose(false);
        }

        #endregion
    }
}
