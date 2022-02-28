using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MonkeyPaste {
    public static class MpTempFileManager {
        private static string _lastTempListFileName = @"temps.txt";
        private static string _lastTempListFilePath = string.Empty;
        private static bool _isLoaded = false;

        public static string TempFilePath {
            get {
                return _lastTempListFilePath;
            }
        }

        private static List<string> _tempFileList = new List<string>();

        public static void Init() {
            if (_isLoaded) {
                return;
            }
            _lastTempListFilePath = Path.Combine(MpPreferences.AppStorageFilePath, _lastTempListFilePath);

            try {
                // since app shutdown cannot effectively be caught every time on start
                // check if temp file list existed and log file and remove all temps
                if (File.Exists(_lastTempListFilePath)) {
                    string[] lastTempFileList = MpFileIoHelpers.ReadTextFromFile(_lastTempListFilePath).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var lastTempFileToDelete in lastTempFileList) {
                        if (File.Exists(lastTempFileToDelete)) {
                            File.Delete(lastTempFileToDelete);
                        }
                    }
                    File.Delete(_lastTempListFilePath);
                }

                _isLoaded = true;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine(@"Could not read or delete temp file at path: " + _lastTempListFilePath);
                MpConsole.WriteTraceLine(@"With Exception: " + ex);
            }
        }
        public static void AddTempFilePath(string filePathToAppend) {
            if(_isLoaded) {
                Init();
            }
            MpFileIoHelpers.AppendTextToFile(TempFilePath, filePathToAppend);
        }
    }
}
