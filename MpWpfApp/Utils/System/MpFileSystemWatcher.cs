using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {

    public class MpFileSystemWatcher : 
        MpIAsyncSingletonViewModel<MpFileSystemWatcher>, 
        IDisposable, 
        MpIActionComponent {
        #region Private Variables

        private List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();
        private Dictionary<FileSystemWatcher, List<MpIFileSystemEventHandler>> _handlerLookup = new Dictionary<FileSystemWatcher, List<MpIFileSystemEventHandler>>();

        #endregion

        #region Properties

        #region MpIActionComponent Implementation


        public void RegisterActionComponent(MpIActionTrigger mvm) {
            var fstvm = mvm as MpFileSystemTriggerViewModel;
            AddWatcher(fstvm.FileSystemPath, fstvm);
            MpConsole.WriteLine($"FileSystemWatcher Registered {mvm.Label} matcher");
        }

        public void UnregisterActionComponent(MpIActionTrigger mvm) {
            var fstvm = mvm as MpFileSystemTriggerViewModel;
            RemoveWatcher(fstvm.FileSystemPath);
            MpConsole.WriteLine($"FileSystemWatcher Unregistered {mvm.Label} matcher");
        }

        #endregion

        #endregion

        #region Constructors

        private static MpFileSystemWatcher _instance;
        public static MpFileSystemWatcher Instance => _instance ?? (_instance = new MpFileSystemWatcher());


        public MpFileSystemWatcher() {
            
        }

        public async Task InitAsync() {
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

            if(Directory.Exists(path)) {
                watcher.Filter = "*";
                watcher.IncludeSubdirectories = watcher.IncludeSubdirectories;
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

        private  void OnError(object sender, ErrorEventArgs e) =>
            PrintException(e.GetException());

        private  void PrintException(object e) {
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
                _watchers.ForEach(x => RemoveWatcher(x.Path));
            }
        }

        ~MpFileSystemWatcher() {
            Dispose(false);
        }

        #endregion
    }
}
