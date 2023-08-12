using Avalonia.Input;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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

        public void Init() {
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
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
                    if (_tempListStream == null) {
                        // first call / init
                        _tempFileList = MpFileIo.ReadTextFromFile(TempFilePath).SplitNoEmpty(Environment.NewLine).ToList();
                        DeleteAll();
                        _tempListStream = new StreamWriter(File.Create(TempFilePath));
                    }
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
                _tempFileList.Union(new[] { TempFilePath }).Where(x => x.IsFileOrDirectory()).ForEach(x => MpFileIo.DeleteFileOrDirectory(x));
                MpConsole.WriteLine("Temp files deleted: ");
                _tempFileList.ForEach(x => MpConsole.WriteLine(x));
                _tempFileList.Clear();
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Temporary file manager shutdown error.", ex);
            }
        }
    }
}
