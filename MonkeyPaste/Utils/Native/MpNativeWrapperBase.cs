using MonkeyPaste.Plugin;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIPlatformWrapper {
        MpICursor Cursor { get; }
        MpIDbInfo DbInfo { get; }
        MpIPreferenceIO PreferenceIO { get; }
        MpIQueryInfo QueryInfo { get; }
        MpIIconBuilder IconBuilder { get; }
        MpICustomColorChooserMenu CustomColorChooserMenu { get; }
        MpIKeyboardInteractionService KeyboardInteractionService { get; }
        MpIGlobalTouch GlobalTouch { get; }
        MpIUiLocationFetcher LocationFetcher { get; }
        MpINativeResource NativeResource { get; }
        MpIContextMenuCloser ContextMenuCloser { get; }
        MpIMainThreadMarshal MainThreadMarshal { get; }
        MpIStringTools StringTools { get; }
        MpIOsInfo OsInfo { get; }
        MpIPlatformDataObjectHelper DataObjectHelper { get; }
        MpINativeMessageBox NativeMessageBox { get; }

        MpIClipboardMonitor ClipboardMonitor { get; set; }

        MpIExternalPasteHandler ExternalPasteHandler { get; }

        MpIPlatformDataObjectRegistrar DataObjectRegistrar { get; set; }
    }


    public static class MpPlatformWrapper {
        public static MpIPlatformWrapper Services { get; private set; }

        public static void Init(MpIPlatformWrapper niw) {
            Services = niw;
        }


    }
}
