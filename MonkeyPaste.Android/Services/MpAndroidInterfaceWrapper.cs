using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonkeyPaste;
using MonkeyPaste.Plugin;

namespace MonkeyPaste.Droid {
    public class MpAndroidInterfaceWrapper : MpIPlatformWrapper {
        public MpICursor Cursor { get; }
        public MpIDbInfo DbInfo { get; }
        public MpIPreferenceIO PreferenceIO { get; }
        public MpIQueryInfo QueryInfo { get; }
        public MpIIconBuilder IconBuilder { get; }
        public MpICustomColorChooserMenu CustomColorChooserMenu { get; }
        public MpIKeyboardInteractionService KeyboardInteractionService { get; }
        public MpIGlobalTouch GlobalTouch { get; }
        public MpIUiLocationFetcher LocationFetcher { get; }
        public MpINativeResource NativeResource { get; }
        public MpIContextMenuCloser ContextMenuCloser { get; }
        public MpIMainThreadMarshal MainThreadMarshal { get; }
        public MpIStringTools StringTools { get; }
        public MpIOsInfo OsInfo { get; }
        public MpIPlatformDataObjectHelper DataObjectHelper { get; }
        public MpINativeMessageBox NativeMessageBox { get; }
        public MpIClipboardMonitor ClipboardMonitor { get; set; }

        public MpAndroidInterfaceWrapper() {
            KeyboardInteractionService = new MpKeyboardInteractionService();
            GlobalTouch = new MpGlobalTouch();
            LocationFetcher = new MpUiLocationFetcher();
            DbInfo = new MpDbFilePath_Android();
        }

    }
}