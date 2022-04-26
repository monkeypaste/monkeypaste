using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MonkeyPaste.Plugin;

namespace MonkeyPaste {
    public static class MpTempFileManager {
        private static string _lastTempListFileName = @"temps.txt";
        private static bool _isLoaded = false;

        public static string TempFilePath {
            get {
                return Path.Combine(Directory.GetCurrentDirectory(), _lastTempListFileName);
            }
        }

        private static List<string> _tempFileList = new List<string>();

        public static void Init() {
            if (_isLoaded) {
                return;
            }

            try {
                // since app shutdown cannot effectively be caught every time on start
                // check if temp file list existed and log file and remove all temps
                if (File.Exists(TempFilePath)) {
                    string[] lastTempFileList = MpFileIo.ReadTextFromFile(TempFilePath).Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

                    string msg = "Warning! Do you want to delete these? " + Environment.NewLine + string.Join(Environment.NewLine, lastTempFileList);

                    MpConsole.WriteLine(msg);
                    var result = MpPlatformWrapper.Services.NativeMessageBox.ShowOkCancelMessageBox("Temp File Manager", msg);
                    if (result) {
                        foreach (var lastTempFileToDelete in lastTempFileList) {
                            if (File.Exists(lastTempFileToDelete)) {
                                File.Delete(lastTempFileToDelete);
                            }
                        }
                    }
                    File.Delete(TempFilePath);
                }

                _isLoaded = true;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine(@"Could not read or delete temp file at path: " + TempFilePath);
                MpConsole.WriteTraceLine(@"With Exception: " + ex);
            }
        }

        public static void Shutdown() {
            string msg = "Warning! Do you want to delete these? " + Environment.NewLine + string.Join(Environment.NewLine, _tempFileList);
            var result = MpPlatformWrapper.Services.NativeMessageBox.ShowOkCancelMessageBox("Temp File Manager", msg);
            if(!result) {
                return;
            }
            foreach (string tfp in _tempFileList) {
                if (File.Exists(tfp)) {
                    try {
                        File.Delete(tfp);
                    }
                    catch (Exception ex) {
                        MpConsole.WriteLine("MainwindowViewModel Dispose error deleteing temp file '" + tfp + "' with exception:");
                        MpConsole.WriteLine(ex);
                    }
                }
            }
        }

        public static void AddTempFilePath(string filePathToAppend) {
            if(_isLoaded) {
                Init();
            }
            if(!_tempFileList.Contains(filePathToAppend)) {
                _tempFileList.Add(filePathToAppend);
                MpFileIo.AppendTextToFile(TempFilePath, filePathToAppend);
            }
            
        }
    }
}
