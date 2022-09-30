using MonkeyPaste;
using System.IO;
using MonkeyPaste.Common;
using System;

namespace MpWpfApp {
    public class MpWpfOsInfo : MpIOsInfo {
        public string OsMachineName => Environment.MachineName;
        public string OsFileManagerPath => Path.Combine(@"%windir%".ExpandEnvVars(), "explorer.exe");

        public string OsFileManagerName => "Explorer";

        public MpUserDeviceType OsType => MpUserDeviceType.Windows;

        public bool IsAvalonia => false;
    }
}
