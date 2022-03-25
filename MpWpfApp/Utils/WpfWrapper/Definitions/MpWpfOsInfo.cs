using MonkeyPaste;
using System.IO;

namespace MpWpfApp {
    public class MpWpfOsInfo : MpIOsInfo {
        public string OsFileManagerPath => Path.Combine(@"%windir%".ExpandEnvVars(), "explorer.exe");

        public string OsFileManagerName => "Explorer";
    }
}
