using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MonkeyPaste {
    public class MpTempFileManager {
        private static readonly Lazy<MpTempFileManager> _Lazy = new Lazy<MpTempFileManager>(() => new MpTempFileManager());
        public static MpTempFileManager Instance { get { return _Lazy.Value; } }


        private string _lastTempListFileName = @"temps.txt";
        private string _lastTempListFilePath = string.Empty;
        private bool _isLoaded = false;

        public string TempFilePath {
            get {
                return _lastTempListFilePath;
            }
        }

        private List<string> _tempFileList = new List<string>();

        private MpTempFileManager() {
            Init();
        }

        public void Init() {
            if(_isLoaded) {
                return;
            }
            _lastTempListFilePath = Path.Combine(MpHelpers.Instance.AppStorageFilePath, _lastTempListFilePath);

            try {
                // since app shutdown cannot effectively be caught every time on start
                // check if temp file list existed and log file and remove all temps
                if (File.Exists(_lastTempListFilePath)) {
                    string[] lastTempFileList = MpHelpers.Instance.ReadTextFromFile(_lastTempListFilePath).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    foreach(var lastTempFileToDelete in lastTempFileList) {
                        if (File.Exists(lastTempFileToDelete)) {
                            File.Delete(lastTempFileToDelete);
                        }
                    }
                    File.Delete(_lastTempListFilePath);
                }

                _isLoaded = true;
            } catch (Exception ex) {
                MpConsole.WriteLine(@"Could not read or delete temp file at path: " + _lastTempListFilePath);
                MpConsole.WriteLine(@"With Exception: " + ex);
            }
        }

        public void AddTempFilePath(string filePathToAppend) {
            MpHelpers.Instance.AppendTextToFile(TempFilePath, filePathToAppend);
        }
    }
}
