using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using MonkeyPaste;

namespace MonkeyPaste.UWP {
    public class MpClipboardManager {
        #region Singleton
        private static readonly Lazy<MpClipboardManager> _Lazy = new Lazy<MpClipboardManager>(() => new MpClipboardManager());
        public static MpClipboardManager Instance { get { return _Lazy.Value; } }

        private MpClipboardManager() {
            Init();
        }
        #endregion

        #region Properties
        //DataPackage dataPackage = new DataPackage();
        #endregion

        #region Events
        public event EventHandler<object> OnClipboardChanged;
        #endregion

        #region Public Methods

        #endregion

        #region Private Methods
        private void Init() {
            Clipboard.ContentChanged += Clipboard_ContentChanged;
        }

        private async void Clipboard_ContentChanged(object sender, object e) {
            var dataPackageView = Clipboard.GetContent();
            var dataList = new List<object>();
            foreach(var format in dataPackageView.AvailableFormats) {
                var data = await dataPackageView.GetDataAsync(format);
                dataList.Add(data);
            }
            if (dataPackageView.Contains(StandardDataFormats.Text)) {
                string text = await dataPackageView.GetTextAsync();
                MpConsole.WriteLine("Clipboard now contains: " + text);
            }

            OnClipboardChanged?.Invoke(this, dataList);
        }
        #endregion
    }
}
