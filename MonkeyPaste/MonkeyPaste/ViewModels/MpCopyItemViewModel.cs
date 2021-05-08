
using MonkeyPaste.Models;
using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MonkeyPaste.ViewModels {
    public class MpCopyItemViewModel : Base.MpViewModelBase {
        public MpCopyItemViewModel(MpCopyItem item) => CopyItem = item;

        public event EventHandler ItemStatusChanged;

        public MpCopyItem CopyItem { get; private set; }

        public string StatusText => CopyItem.Title;

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
            Clipboard.SetTextAsync(CopyItem.ItemPlainText);
            ItemStatusChanged?.Invoke(this, new EventArgs());
        }
    }
}

