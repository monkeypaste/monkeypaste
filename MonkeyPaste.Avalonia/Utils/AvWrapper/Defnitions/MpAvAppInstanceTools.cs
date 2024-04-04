using MonkeyPaste.Common;
using System.IO;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppInstanceTools : MpISingleInstanceTools {
        /*
        Multi instance issues:
        -new sources may be duplicated
        */
        private static FileStream _lockFile;
        private bool _hasCheckedLock;
        const string LOCK_FILE_DEFAULT_NAME = "temp.lock";
        
        string LockFilePath {
            get {
                string lock_file_dir = Mp.Services.PlatformInfo.StorageDir.LocalStoragePathToPackagePath(false);
                string lock_file_path = Path.Combine(lock_file_dir, LOCK_FILE_DEFAULT_NAME);
                return lock_file_path;
            }
        }
        public bool IsFirstInstance =>
            _lockFile != null;
        public bool DoInstanceCheck() {
            // NOTE this should only be run ONCE at startup 
            // AFTER avalonia and any cef-based init or weird things will happen
            if(_hasCheckedLock) {
                return IsFirstInstance;
            }
            _hasCheckedLock = true;
            try {
                _lockFile = File.Open(LockFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                _lockFile.Lock(0, 0);
            }
            catch {
            }
            return IsFirstInstance;
        }

        public bool RemoveInstanceLock() {
            bool success = MpFileIo.DeleteFile(LockFilePath);
            return success;
        }
    }
}
