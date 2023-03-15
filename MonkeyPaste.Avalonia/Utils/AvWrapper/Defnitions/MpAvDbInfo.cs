using MonkeyPaste;
using MonkeyPaste.Common;
using System;
using System.IO;
using System.Reflection;

namespace MonkeyPaste.Avalonia {
    public class MpAvDbInfo : MpIDbInfo {
        public string DbExtension => "mpcdb";
        private string _dbName = null;
        public string DbFileName {
            get {
                if (_dbName == null) {
                    // NOTE this accessed in cefnet init for renderer thread ref
                    // so can't use platform wrapper
                    //var osi = new MpAvPlatformInfo_desktop();
                    var osi = Mp.Services.PlatformInfo;
                    _dbName = $"mp_{osi.OsShortName}.{DbExtension}";
                }
                return _dbName;
            }
        }

        private string _dbDIr;
        public string DbDir {
            get {
                if (_dbDIr == null) {
                    //var osi = new MpAvPlatformInfo_desktop();
                    var osi = Mp.Services.PlatformInfo;
                    _dbDIr = osi.StorageDir;
                }
                return _dbDIr;
            }
        }
        public string DbPath =>
            Path.Combine(DbDir, DbFileName);

    }
}