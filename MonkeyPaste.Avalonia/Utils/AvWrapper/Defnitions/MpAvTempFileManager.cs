using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvTempFileManager : MpITempFileManager {
        private object _tempLock = new object();
        private string _lastTempListFileName = @"temps.txt";

        private StreamWriter _tempListStream;
        public string TempFilePath {
            get {
                return Path.Combine(MpCommonTools.Services.PlatformInfo.StorageDir, _lastTempListFileName);
            }
        }

        private List<string> _tempFileList = new List<string>();
        public MpAvTempFileManager() {
            Init();
        }
        public void Init() {
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);

            if (_tempListStream == null) {
                // first call / init
                string temps_text = MpFileIo.ReadTextFromFile(TempFilePath);
                _tempFileList = temps_text.SplitNoEmpty(Environment.NewLine).ToList();
                DeleteAll();
                _tempListStream = new StreamWriter(File.Create(TempFilePath));
            }
        }
        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ContentPasted:
                case MpMessageType.ItemDragEnd:
                    DeleteAll();
                    break;
            }
        }
        public void Shutdown() {
            DeleteAll();

            if (_tempListStream == null) {
                return;
            }

            _tempListStream.Dispose();
            _tempListStream = null;
        }

        public void AddTempFilePath(string filePathToAppend) {
            try {
                lock (_tempLock) {
                    _tempListStream.WriteLine(filePathToAppend);
                    _tempListStream.Flush();
                }
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Temporary file manager shutdown error.", ex);
            }
            if (_tempFileList.Contains(filePathToAppend)) {
                return;
            }

            _tempFileList.Add(filePathToAppend);
        }

        public void DeleteAll() {
            try {
                var to_delete_paths = _tempFileList.Where(x => x.IsFileOrDirectory()).ToList();
                if (_tempListStream == null) {
                    // this stream should only be null during init and temp file shouldn't be deleted/reset on init
                    to_delete_paths.Add(TempFilePath);
                }
                MpConsole.WriteLine("Temp files deleted: ");
                foreach (var to_delete_path in to_delete_paths) {
                    bool success = MpFileIo.DeleteFileOrDirectory(to_delete_path);
                    MpConsole.WriteLine($"{(success ? "SUCCESS" : "FAILED")} '{to_delete_path}'");
                }
                _tempFileList.Clear();
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Temporary file manager shutdown error.", ex);
            }
        }
    }
}
