using Foundation;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.IO;

namespace MonkeyPaste.Avalonia {
    public class MpAvAppInstanceTools : MpISingleInstanceTools {
        /*
        Multi instance issues:
        -new sources may be duplicated
        */
        private static object _lockFileObj;
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
                if(_lockFileObj != null) {
                    return true;
                }
                try {
#if WINDOWS
                    _lockFileObj = File.Open(LockFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                    _lockFileObj.Lock(0, 0); 
#elif MAC
                    // File.Open throws exception and .Lock not supported on mac
                    return true;
#else
                    // untested
                    // from https://learn.microsoft.com/en-us/dotnet/core/compatibility/core-libraries/6.0/filestream-file-locks-unix#type-of-breaking-change
                    if(!LockFilePath.IsFile()) {
                        MpFileIo.TouchFile(LockFilePath);
                    }
                    _lockFileObj = File.OpenRead(LockFilePath);
                    _lockFileObj.Lock(0,0);
#endif
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Single instance error (if only instance otherwise expected).", ex);
                    _lockFileObj = null;
                }
                return _lockFileObj != null;
            }
        }
    }
}
