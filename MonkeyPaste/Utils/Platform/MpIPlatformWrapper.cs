﻿using MonkeyPaste.Common;

namespace MonkeyPaste {
    public interface MpIPlatformWrapper : MpICommonTools {
        MpIPlatformCompatibility PlatformCompatibility { get; set; }
        MpIStartupState StartupState { get; set; }
        MpIPlatformShorcuts PlatformShorcuts { get; set; }
        MpINotificationManager NotificationManager { get; set; }

        MpIFocusMonitor FocusMonitor { get; set; }
        //MpIProcessWatcher ProcessWatcher { get; set; }
        MpICursor Cursor { get; set; }
        MpIDbInfo DbInfo { get; set; }
        MpIQueryInfo QueryInfo { get; set; }
        MpIApplicationCommandManager AppCommandManager { get; set; }
        MpIIconBuilder IconBuilder { get; set; }
        MpIUrlBuilder UrlBuilder { get; set; }
        MpIAppBuilder AppBuilder { get; set; }
        MpISourceRefBuilder SourceRefBuilder { get; set; }
        MpITransactionBuilder TransactionBuilder { get; set; }
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
        MpIPlatformDataObjectHelperAsync DataObjectHelperAsync { get; set; }
        MpINativeMessageBox NativeMessageBox { get; set; }

        MpIClipboardMonitor ClipboardMonitor { get; set; }

        MpIClipboardFormatDataHandlers ClipboardData { get; set; }

        //MpIExternalPasteHandler ExternalPasteHandler { get; set; }

        MpIPlatformDataObjectRegistrar DataObjectRegistrar { get; set; }

        MpICopyItemBuilder CopyItemBuilder { get; set; }
    }
}