using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpCopyItemViewModel : MpViewModelBase {
        public MpCopyItemViewModel(MpCopyItem item) => CopyItem = item;

        public event EventHandler ItemStatusChanged;

        public MpCopyItem CopyItem { get; private set; }

        public string StatusText => CopyItem.Title;

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

