using MonkeyPaste.Common;
using System.IO;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppInstanceTools : MpISingleInstanceTools {
        /*
        Multi instance issues:
        -new sources may be duplicated
        */
        private static FileStream _lockFile;
        const string LOCK_FILE_DEFAULT_NAME = ".lock";
        
        string LockFilePath {
            get {
                string lock_file_dir = Mp.Services.PlatformInfo.StorageDir.LocalStoragePathToPackagePath(false);
                string lock_file_path = Path.Combine(lock_file_dir, LOCK_FILE_DEFAULT_NAME);
                return lock_file_path;
            }
        }
        public bool IsFirstInstance {
            get {
                if(_lockFile != null) {
                    return true;
                }
                try {
                    _lockFile = File.Open(LockFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                    _lockFile.Lock(0, 0);
                }
                catch { }
                return _lockFile != null;
            }
        }
    }
}
