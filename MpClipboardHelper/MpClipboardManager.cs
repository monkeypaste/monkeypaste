using System;
using System.Runtime.InteropServices;
using System.Text;
using MonkeyPaste.Plugin;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;

namespace MpClipboardHelper {
    public static partial class MpClipboardManager  {
        #region Private Variables

        #endregion

        #region Properties

        public static MpIClipboardMonitor MonitorService { get; private set; }

        public static MpIClipboardInterop InteropService { get; private set; }

        public static MpIExternalPasteHandler PasteService { get; private set; }
        #endregion

        #region Events

        public static event EventHandler<MpDataObject> OnClipboardChange;

        #endregion

        #region Public Methods

        public static void Init(MpIExternalPasteHandler pasteHandler) {
            // NOTE services are abstracted into interfaces because of bug in UCRTBASE.DLL
            // and monitoring window messages (using MpClipboardWatcher) crashes application
            // intermittently when copying rtf from Visual Studio BUT maybe it can be fixed
            MonitorService = new MpClipboardTimer();
            InteropService = (MpIClipboardInterop)MonitorService;
            PasteService = pasteHandler;
            Start();
        }

        public static void Start() {
            if(MonitorService == null || InteropService == null) {
                throw new Exception("Must call init");
            }
            MonitorService.OnClipboardChanged += MpClipboardWatcher_OnClipboardChange;

            MonitorService.StartMonitor();
        }
        public static void Stop() {
            if(MonitorService == null) {
                return;
            }
            MonitorService.OnClipboardChanged -= MpClipboardWatcher_OnClipboardChange;
            MonitorService.StopMonitor();
        }
        #endregion

        #region Private Methods

        private static void MpClipboardWatcher_OnClipboardChange(object sender, MpDataObject e) {
            OnClipboardChange?.Invoke(sender, e);
        }

        #endregion
    }
}
