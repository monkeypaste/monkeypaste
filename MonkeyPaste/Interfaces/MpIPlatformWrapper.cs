﻿using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpIPlatformWrapper : MpICommonTools {
        Task InitializeAsync();
        MpISingleInstanceTools SingleInstanceTools { get; set; }
        MpIPointerSimulator PointerSimulator { get; set; }
        MpIClipboard DeviceClipboard { get; set; }
        MpIDefaultDataCreator DefaultDataCreator { get; set; }
        MpIWelcomeSetupInfo WelcomeSetupInfo { get; set; }
        MpIUserDeviceInfo ThisDeviceInfo { get; set; }
        MpIShare ShareTools { get; set; }
        MpINotificationBuilder NotificationBuilder { get; set; }
        MpIShutdownTools ShutdownHelper { get; set; }
        MpILoadOnLoginTools LoadOnLoginTools { get; set; }
        MpIContentBuilder ContentBuilder { get; set; }
        MpIPlatformUserInfo PlatformUserInfo { get; set; }
        MpIDownKeyHelper KeyDownHelper { get; set; }
        MpIShortcutGestureLocator ShortcutGestureLocator { get; set; }
        MpIColorQueryTools ColorQueryTools { get; set; }
        MpIKeyConverterHub KeyConverter { get; set; }
        MpIKeyStrokeSimulator KeyStrokeSimulator { get; set; }
        MpIContentViewLocator ContentViewLocator { get; set; }

        MpIPlatformPathDialog NativePathDialog { get; set; }
        MpIStartupState StartupState { get; set; }
        MpIPlatformShorcuts PlatformShorcuts { get; set; }
        MpIFocusMonitor FocusMonitor { get; set; }
        MpIDbInfo DbInfo { get; set; }
        MpIQueryResultProvider Query { get; set; }
        MpIContentQueryPage ContentQueryTools { get; set; }
        MpITagQueryTools TagQueryTools { get; set; }
        MpIIconBuilder IconBuilder { get; set; }
        MpIUrlBuilder UrlBuilder { get; set; }
        MpIAppBuilder AppBuilder { get; set; }
        MpISourceRefTools SourceRefTools { get; set; }
        MpITransactionReporter TransactionBuilder { get; set; }
        MpICustomColorChooserMenuAsync CustomColorChooserMenuAsync { get; set; }
        MpIKeyboardInteractionService KeyboardInteractionService { get; set; }
        MpIPlatformScreenInfoCollection ScreenInfoCollection { get; set; }

        MpIDragProcessWatcher DragProcessWatcher { get; set; }
        MpIDropProcessWatcher DropProcessWatcher { get; set; }
        MpIPlatformDataObjectTools DataObjectTools { get; set; }


        MpIClipboardMonitor ClipboardMonitor { get; set; }

        MpIPlatformDataObjectRegistrar DataObjectRegistrar { get; set; }

    }
}
