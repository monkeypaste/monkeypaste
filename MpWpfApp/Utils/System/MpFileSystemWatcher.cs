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
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public interface MpIFileSystemEventHandler {
        bool IncludeSubdirectories { get; }
        void OnFileSystemItemChanged(object sender, FileSystemEventArgs e);
    }

    public class MpFileSystemWatcher : 
        MpISingletonViewModel<MpFileSystemWatcher>, 
        IDisposable, 
        MpIActionComponent {
        #region Private Variables

        private List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();
        private Dictionary<FileSystemWatcher, List<MpIFileSystemEventHandler>> _handlerLookup = new Dictionary<FileSystemWatcher, List<MpIFileSystemEventHandler>>();

        #endregion

        #region Properties

        #region MpIMatcherTriggerViewModel Implementation

        #region MpIUserIcon Implementation

        public bool IsReadOnly => true;
        public async Task<MpIcon> Get() {
            await Task.Delay(1);
            throw new NotImplementedException();
        }

        public async Task Set(MpIcon icon) {
            await Task.Delay(1);
            throw new NotImplementedException();
        }

        #endregion

        public void Register(MpActionViewModelBase mvm) {
            var fstvm = mvm as MpFileSystemTriggerViewModel;
            AddWatcher(fstvm.FileSystemPath, fstvm);
            MpConsole.WriteLine($"FileSystemWatcher Registered {mvm.Label} matcher");
        }

        public void Unregister(MpActionViewModelBase mvm) {
            var fstvm = mvm as MpFileSystemTriggerViewModel;
            RemoveWatcher(fstvm.FileSystemPath);
            MpConsole.WriteLine($"FileSystemWatcher Unregistered {mvm.Label} matcher");
        }

        //public ObservableCollection<MpActionViewModelBase> MatcherViewModels => new ObservableCollection<MpActionViewModelBase>(
        //            MpActionCollectionViewModel.Instance.Matchers.Where(x =>
        //                x.Action.TriggerType == MpTriggerType.FileSystemChange ||
        //                 x.Action.TriggerType == MpTriggerType.FileSystemChange).ToList());

        #endregion

        #endregion

        #region Constructors

        private static MpFileSystemWatcher _instance;
        public static MpFileSystemWatcher Instance => _instance ?? (_instance = new MpFileSystemWatcher());


        public MpFileSystemWatcher() {
            
        }

        public async Task Init() {
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
