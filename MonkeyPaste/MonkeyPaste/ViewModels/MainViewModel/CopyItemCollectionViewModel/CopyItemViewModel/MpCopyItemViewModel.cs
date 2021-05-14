using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpCopyItemViewModel : MpViewModelBase {
        #region Private Variables
        public event EventHandler ItemStatusChanged;
        #endregion

        #region Properties
        public bool IsSelected { get; set; } = false;

        public MpCopyItem CopyItem { get; set; }

        public string StatusText => CopyItem.Title;
        #endregion

        #region Public Methods
        public MpCopyItemViewModel(MpCopyItem item) => CopyItem = item;
        #endregion

        #region Private Methods

        #endregion

        #region Commands

        public ICommand Save => new Command(async () => {
            await MpDb.Instance.AddOrUpdate<MpCopyItem>(CopyItem);
            await Navigation.PopAsync();
        });

        private Command _setClipboardToItemCommand = null;
        public ICommand SetClipboardToItemCommand {
            get {
                if (_setClipboardToItemCommand == null) {
                    _setClipboardToItemCommand = new Command(SetClipboardToItem);
                }
                return _setClipboardToItemCommand;
            }
        }
        private void SetClipboardToItem() {
            Clipboard.SetTextAsync(CopyItem.CopyItemText);
            ItemStatusChanged?.Invoke(this, new EventArgs());
        }
        #endregion
    }
}

