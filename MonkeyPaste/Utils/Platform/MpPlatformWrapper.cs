using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common;

namespace MonkeyPaste {
    public interface MpIPlatformWrapper {
        MpINotificationBalloonView NotificationView { get; set; }
        MpIProcessWatcher ProcessWatcher { get; set; }
        MpICursor Cursor { get; set; }
        MpIDbInfo DbInfo { get; set; }
        MpIQueryInfo QueryInfo { get; set; }
        MpIIconBuilder IconBuilder { get; set; }
        MpIUrlBuilder UrlBuilder { get; set; }
        MpIAppBuilder AppBuilder { get; set; }
        MpICustomColorChooserMenu CustomColorChooserMenu { get; set; }
        MpICustomColorChooserMenuAsync CustomColorChooserMenuAsync { get; set; }
        MpIKeyboardInteractionService KeyboardInteractionService { get; set; }
        MpIGlobalTouch GlobalTouch { get; set; }
        MpIUiLocationFetcher LocationFetcher { get; set; }
        MpIPlatformResource PlatformResource { get; set; }
        MpIPlatformScreenInfoCollection ScreenInfoCollection { get; set; }

        MpIContextMenuCloser ContextMenuCloser { get; set; }
        MpIMainThreadMarshal MainThreadMarshal { get; set; }
        MpIStringTools StringTools { get; set; }
        MpIOsInfo OsInfo { get; set; }
        MpIPlatformDataObjectHelper DataObjectHelper { get; set; }
        MpIPlatformDataObjectHelperAsync DataObjectHelperAsync { get; set; }
        MpINativeMessageBox NativeMessageBox { get; set; }

        MpIClipboardMonitor ClipboardMonitor { get; set; }

        MpIClipboardFormatDataHandlers ClipboardData { get; set; }

        MpIExternalPasteHandler ExternalPasteHandler { get; set; }

        MpIPlatformDataObjectRegistrar DataObjectRegistrar { get; set; }

        MpICopyItemBuilder CopyItemBuilder { get; set; }
    }


    public static class MpPlatformWrapper {
        public static MpIPlatformWrapper Services { get; private set; }

        public static async Task InitAsync(MpIPlatformWrapper niw) {
            //await MpPrefViewModel.InitAsync();
            //MpPrefViewModel.Instance.MainWindowOrientation = "Bottom";
            await Task.Delay(1);
            Services = niw;
        }


    }
}
