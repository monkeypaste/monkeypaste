﻿using MonkeyPaste.Common;
using System.Threading.Tasks;

namespace MonkeyPaste {

    public interface MpIPlatformWrapper : MpICommonTools {
        Task InitializeAsync();
        MpIKeyConverterHub KeyConverter { get; set; }
        MpIKeyStrokeSimulator KeyStrokeSimulator { get; set; }
        MpIContentViewLocator ContentViewLocator { get; set; }

        MpINativePathDialog NativePathDialog { get; set; }
        MpIStartupObjectLocator StartupObjectLocator { get; set; }
        MpIStartupState StartupState { get; set; }
        MpIPlatformShorcuts PlatformShorcuts { get; set; }
        MpINotificationManager NotificationManager { get; set; }
        MpIFocusMonitor FocusMonitor { get; set; }

        MpICursor Cursor { get; set; }
        MpIDbInfo DbInfo { get; set; }
        MpIQueryResultProvider Query { get; set; }

        MpIContentQueryTools ContentQueryTools { get; set; }
        MpITagQueryTools TagQueryTools { get; set; }
        MpIApplicationCommandManager AppCommandManager { get; set; }
        MpIIconBuilder IconBuilder { get; set; }
        MpIUrlBuilder UrlBuilder { get; set; }
        MpIAppBuilder AppBuilder { get; set; }
        MpISourceRefBuilder SourceRefBuilder { get; set; }
        MpITransactionReporter TransactionBuilder { get; set; }
        MpICustomColorChooserMenuAsync CustomColorChooserMenuAsync { get; set; }
        MpIKeyboardInteractionService KeyboardInteractionService { get; set; }
        MpIPlatformResource PlatformResource { get; set; }
        MpIPlatformScreenInfoCollection ScreenInfoCollection { get; set; }

        MpIDragProcessWatcher DragProcessWatcher { get; set; }
        MpIDropProcessWatcher DropProcessWatcher { get; set; }
        MpIContextMenuCloser ContextMenuCloser { get; set; }
        MpIMainThreadMarshal MainThreadMarshal { get; set; }
        MpIPlatformDataObjectHelperAsync DataObjectHelperAsync { get; set; }


        MpIClipboardMonitor ClipboardMonitor { get; set; }

        MpIClipboardFormatDataHandlers ClipboardData { get; set; }

        //MpIExternalPasteHandler ExternalPasteHandler { get; set; }

        MpIPlatformDataObjectRegistrar DataObjectRegistrar { get; set; }

        MpICopyItemBuilder CopyItemBuilder { get; set; }

    }
}