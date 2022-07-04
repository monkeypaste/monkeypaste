using MonkeyPaste;
using System.IO;
using MonkeyPaste.Common;

namespace MpWpfApp {
    public class MpWpfOsInfo : MpIOsInfo {
        public string OsFileManagerPath => Path.Combine(@"%windir%".ExpandEnvVars(), "explorer.exe");

        public string OsFileManagerName => "Explorer";

        public MpUserDeviceType OsType => MpUserDeviceType.Windows;
        
    }
}
