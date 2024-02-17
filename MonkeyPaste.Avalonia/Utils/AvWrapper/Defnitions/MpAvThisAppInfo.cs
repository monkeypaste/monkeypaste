using MonkeyPaste.Common;
#if WINDOWS
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
                return MpPlatformHelpers.GetAppVersion().ToString();
                //return THIS_APP_VERSION;
                //#if WINDOWS && WAP
                //                // from https://stackoverflow.com/a/62719001/105028
                //                try {
                //                    var version = Windows.ApplicationModel.Package.Current.Id.Version;
                //                    return string.Format("{0}.{1}.{2}.{3}",
                //                        version.Major,
                //                        version.Minor,
                //                        version.Build,
                //                        version.Revision);
                //                }
                //                catch (Exception ex) {
                //                    MpConsole.WriteTraceLine($"Error reading package info. ", ex);
                //                    return string.Empty;
                //                } 
                //#elif ANDROID
                //                return VersionTracking.CurrentVersion;
                //#else
                //                if (Assembly.GetEntryAssembly() is Assembly ass &&
                //                    ass.Location.IsFileOrDirectory() &&
                //                    FileVersionInfo.GetVersionInfo(ass.Location) is { } fvi) {
                //                    return fvi.FileVersion.ToVersion().ToString();
                //                }
                //                return "1.0.0.0";
                //#endif
            }

        }
        public MpAvThisAppInfo() {
#if ANDROID
            VersionTracking.Track();
#endif
        }
    }
}
