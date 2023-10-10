using MonkeyPaste.Common;
using System.Diagnostics;
using System.Reflection;
using Windows.ApplicationModel;
//using Avalonia.Win32;

namespace MonkeyPaste.Avalonia {

    public class MpAvThisAppInfo : MpIThisAppInfo {
        // NOTE make sure Directory.build.props Application* props match these
        public string ThisAppCompanyName =>

            FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).CompanyName;

        public string ThisAppProductName =>

            FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).ProductName;
        //public string ThisAppProductVersion =>
        //    
        public string ThisAppProductVersion {
            get {
#if WINDOWS
                // from https://stackoverflow.com/a/62719001/105028
                var version = Package.Current.Id.Version;
                return string.Format("{0}.{1}.{2}.{3}",
                    version.Major,
                    version.Minor,
                    version.Build,
                    version.Revision);
#else
                return FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).ProductVersion;
#endif
            }
        }
    }
}
