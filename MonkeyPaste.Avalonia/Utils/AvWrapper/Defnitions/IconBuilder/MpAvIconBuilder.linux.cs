#if LINUX

using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvIconBuilder {

        public string GetPathIconBase64(string appPath, nint handle, MpIconSize iconSize = MpIconSize.MediumIcon32) {
            return GetPathIconBase64(appPath, iconSize);
        }
        public string GetPathIconBase64(string appPath, MpIconSize iconSize = MpIconSize.MediumIcon32) {
            if (string.IsNullOrEmpty(appPath)) {
                return null;
            }
            //return MpBase64Images.LinuxPenguin;
            string iconBase64 = MpAvX11PathIconHelper.GetIconBase64FromX11Path(appPath, "EXECUTABLE");
            MpConsole.WriteLine("Icon for path: " + appPath);
            MpConsole.WriteLine(iconBase64);
            if (string.IsNullOrEmpty(iconBase64)) {
                return MpBase64Images.QuestionMark;
            }
            return iconBase64;

            // if(!MpAvX11ProcessWatcher_xlib.IsXDisplayAvailable()) {
            //     return MpBase64Images.QuestionMark;
            // }
            // var _displayPtr = Xlib.XOpenDisplay(null);

            // if (_displayPtr == IntPtr.Zero) {
            //     MpConsole.WriteTraceLine("Unable to open the default X display");
            //     return MpBase64Images.QuestionMark;
            // }

            // var _rootWindow = Xlib.XDefaultRootWindow(_displayPtr);

            // if (_rootWindow == default) {
            //     MpConsole.WriteTraceLine("Unable to open root window");
            //     return MpBase64Images.QuestionMark;
            // }
            // return MpBase64Images.QuestionMark;
        }
    }
}
#endif
