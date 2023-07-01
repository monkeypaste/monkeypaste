using MonkeyPaste.Common;
using System.Diagnostics;
using System.Reflection;
//using Avalonia.Win32;

namespace MonkeyPaste.Avalonia {

    public class MpAvThisAppInfo : MpIThisAppInfo {
        public string ThisAppCompanyName =>

            FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).CompanyName;

        public string ThisAppProductName =>

            FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).ProductName;
        public string ThisAppProductVersion =>
            FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).ProductVersion;
    }
}
