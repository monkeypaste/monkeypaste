﻿using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;
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
        MpIUrlBuilder UrlBuilder { get; }
        MpIAppBuilder AppBuilder { get; }
        MpICustomColorChooserMenu CustomColorChooserMenu { get; }
        MpIKeyboardInteractionService KeyboardInteractionService { get; }
        MpIGlobalTouch GlobalTouch { get; }
        MpIUiLocationFetcher LocationFetcher { get; }
        MpIPlatformResource PlatformResource { get; }
        MpIPlatformScreenInfoCollection ScreenInfoCollection { get; }

        MpIContextMenuCloser ContextMenuCloser { get; }
        MpIMainThreadMarshal MainThreadMarshal { get; }
        MpIStringTools StringTools { get; }
        MpIOsInfo OsInfo { get; }
        MpIPlatformDataObjectHelper DataObjectHelper { get; }
        MpINativeMessageBox NativeMessageBox { get; }

        MpIClipboardMonitor ClipboardMonitor { get; }

        MpIClipboardFormatDataHandlers ClipboardData { get; }

        MpIExternalPasteHandler ExternalPasteHandler { get; }

        MpIPlatformDataObjectRegistrar DataObjectRegistrar { get; set; }

        MpICopyItemBuilder CopyItemBuilder { get; }
    }


    public static class MpPlatformWrapper {
        public static MpIPlatformWrapper Services { get; private set; }

        public static void Init(MpIPlatformWrapper niw) {
            Services = niw;
        }


    }
}