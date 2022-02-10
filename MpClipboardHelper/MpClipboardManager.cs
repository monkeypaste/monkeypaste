using System;
using System.Runtime.InteropServices;
using System.Text;
using MonkeyPaste;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Linq;

namespace MpClipboardHelper {
    public static partial class MpClipboardManager {
        #region Private Variables

        private static MpDataObject _LastDataObject = null;
        private static MpDataObject _TempDataObject = null; // used when restoring clipboard
        public static bool IgnoreClipboardChangeEvent = false;

        #endregion

        #region Events

        public static event EventHandler<MpDataObject> OnClipboardChange;

        #endregion

        #region Public Methods

        public static void Init() {
            //MpClipboardWatcher.Start();
            //MpClipboardWatcher.OnClipboardChange += MpClipboardWatcher_OnClipboardChange;

            MpClipboardTimer.Start();
            MpClipboardTimer.ClipboardChanged += MpClipboardWatcher_OnClipboardChange;
        }

        public static async Task PasteDataObject(MpDataObject mpdo, IntPtr handle, bool finishWithEnterKey = false) {
            await MpClipboardWatcher.PasteDataObject(mpdo, handle);
            if (finishWithEnterKey) {
                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
            }
        }

        public static void SetDataObjectWrapper(MpDataObject mpdo) {
            MpClipboardWatcher.SetDataObjectWrapper(mpdo);
        }

        public static void Stop() {
            OnClipboardChange = null;
            MpClipboardWatcher.Stop();
        }
        #endregion

        #region Private Methods

        private static void MpClipboardWatcher_OnClipboardChange(object sender, MpDataObject e) {
            OnClipboardChange?.Invoke(sender, e);
        }

        #endregion
    }
}
