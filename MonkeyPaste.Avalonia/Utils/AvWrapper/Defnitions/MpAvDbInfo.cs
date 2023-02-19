using MonkeyPaste.Common;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MonkeyPaste.Avalonia {
    public class MpAvDbInfo : MonkeyPaste.MpIDbInfo {
        public string DbExtension => "mpcdb";
        private string _dbName = null;
        public string DbName {
            get {
                if (_dbName == null) {
                    // NOTE this accessed in cefnet init for renderer thread ref
                    // so can't use platform wrapper
                    MpAvPlatformInfo osi = new MpAvPlatformInfo();
                    _dbName = $"mp_{osi.OsShortName}.{DbExtension}";
                }
                return _dbName;
            }
        }


        public string DbPath => Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), DbName);

    }
}