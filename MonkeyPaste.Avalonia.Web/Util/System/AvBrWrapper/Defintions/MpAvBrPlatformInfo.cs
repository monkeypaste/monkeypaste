using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia.Web {
    public class MpAvBrPlatformInfo : MpAvPlatformInfoBase {
        public override string OsMachineName =>
            Environment.MachineName;
        public override string OsVersionInfo =>
            Environment.OSVersion.VersionString;
        public override bool IsTouchInputEnabled =>
            false;
    }
}
