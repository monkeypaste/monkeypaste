using MonkeyPaste.Common;
using System;
using System.Diagnostics;
#if WINDOWS
using Windows.ApplicationModel;
#else
using Xamarin.Essentials;
#endif

namespace MonkeyPaste.Avalonia {

    public class MpAvThisAppInfo : MpIThisAppInfo {
        // NOTE make sure Directory.build.props Application* props match these
        public string ThisAppCompanyName =>
            "Monkey LLC";

        public string ThisAppProductName =>
            "MonkeyPaste";
        public string ThisAppProductVersion {
            get {
#if WINDOWS
                // from https://stackoverflow.com/a/62719001/105028
                try {
                    var version = Package.Current.Id.Version;
                    return string.Format("{0}.{1}.{2}.{3}",
                        version.Major,
                        version.Minor,
                        version.Build,
                        version.Revision);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Error reading package info. ", ex);
                    return string.Empty;
                }
#else
                return VersionTracking.CurrentVersion;
                //#else
                //           return FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).ProductVersion;
#endif
            }

        }
        public MpAvThisAppInfo() {
#if !WINDOWS
            VersionTracking.Track(); 
#endif
        }
    }
}
