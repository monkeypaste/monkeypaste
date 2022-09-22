using System;
using System.Windows.Forms;
using System.Threading;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Collections.Specialized;
using System.Runtime.Serialization.Formatters.Binary;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
using System.Threading.Tasks;
using MonkeyPaste;
using System.Collections.Generic;
using System.Linq;

namespace MpWpfApp {
    public class MpWpfClipboardWatcher : Form, 
        MpIClipboardMonitor, 
        MpIPlatformDataObjectRegistrar {
        #region Private Variables

        private IntPtr _nextClipboardViewer;

        #endregion

        #region MpIPlatfromatDataObjectRegistrar Implmentation

        public int RegisterFormat(string format) {
            uint result = WinApi.RegisterClipboardFormatA(format);
            int id = Convert.ToInt32(result);
            return id;

        }

        #endregion

        #region MpIClipboardMonitor Implementation

        public event EventHandler<MpPortableDataObject> OnClipboardChanged;

        public bool IgnoreNextClipboardChangeEvent { get; set; } = false;

        public void StartMonitor() => Start();

        public void StopMonitor() => Stop();

        #endregion

        #region Public Methods

        public void Start() {
            var t = new Thread(
                new ParameterizedThreadStart(x => Application.Run(this)));
            t.SetApartmentState(ApartmentState.STA); // give the [STAThread] attribute
            t.Start();
        }

        // stop listening (dispose form)
        public void Stop() {
            Invoke(new System.Windows.Forms.MethodInvoker(() => {
                WinApi.ChangeClipboardChain(Handle, _nextClipboardViewer);
            }));
            Invoke(new System.Windows.Forms.MethodInvoker(Close));

            Dispose();

        }

        #endregion

        #region Protected Methods
        protected override void SetVisibleCore(bool value) {
            // on load: (hide this window)
            CreateHandle();

            _nextClipboardViewer = WinApi.SetClipboardViewer(Handle);

            base.SetVisibleCore(false);
        }

        protected override void WndProc(ref System.Windows.Forms.Message m) {
            switch (m.Msg) {
                case WinApi.WM_DRAWCLIPBOARD:
                    ClipboardChanged();
                    WinApi.SendMessage(_nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    break;

                case WinApi.WM_CHANGECBCHAIN:
                    if (m.WParam == _nextClipboardViewer)
                        _nextClipboardViewer = m.LParam;
                    else
                        WinApi.SendMessage(_nextClipboardViewer, m.Msg, m.WParam, m.LParam);
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        #endregion

        #region Private Methods

        private void ClipboardChanged() {
            if (IgnoreNextClipboardChangeEvent) {
                MpConsole.WriteLine("Ignoring this clipboard changed event");
                IgnoreNextClipboardChangeEvent = false;             
                return;
            }

            MpPortableDataObject clipboardData = MpClipboardHandlerCollectionViewModel.Instance.ReadClipboardOrDropObject();

            if(clipboardData.DataFormatLookup.Where(x=>x.Value != null).Count() > 0) {
                MpConsole.WriteLine("CB Changed: " + DateTime.Now);                
                OnClipboardChanged?.Invoke(typeof(MpWpfClipboardWatcher).ToString(), clipboardData);
            }
        }


        #endregion

    }
}
