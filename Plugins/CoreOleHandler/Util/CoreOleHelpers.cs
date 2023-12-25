using Avalonia.Input.Platform;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;

namespace CoreOleHandler {
    public static class CoreOleHelpers {

        private static IClipboard _clipboardRef;
        public static IClipboard ClipboardRef {
            get {
                if (_clipboardRef == null) {
                    if (MpAvCommonTools.Services != null) {
                        _clipboardRef = MpAvCommonTools.Services.DeviceClipboard;
                    }
                }
                return _clipboardRef;
            }
        }

        public static void SetCulture(MpPluginRequestFormatBase req, List<MpPluginUserNotificationFormat> ntfl = default) {
            if (req == null) {
                return;
            }
            try {
                if (Resources.Culture != null &&
                    Resources.Culture.Name == req.cultureCode) {
                    return;
                }
                Resources.Culture = new System.Globalization.CultureInfo(req.cultureCode);
            }
            catch (Exception ex) {

                if (ntfl != default) {
                    ntfl.Add(Util.CreateNotification(
                    MpPluginNotificationType.PluginResponseWarning,
                    Resources.NtfFormatIgnoredTitle,
                    ex.ToString(), req.cultureCode));
                }
                MpConsole.WriteTraceLine($"Error settings culture code to '{req.cultureCode}'");
            }
        }


    }
}