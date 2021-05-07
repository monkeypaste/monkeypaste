using MonkeyPaste.Models;
using MonkeyPaste.Repositories;
using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace MonkeyPaste.ViewModels {
    public class MpItemViewModel : Base.MpViewModelBase {
        private readonly MpCopyItemRepository _repository;

        public MpCopyItem Item { get; set; }

        public MpItemViewModel(MpCopyItemRepository repository) {
            _repository = repository;
            Item = new MpCopyItem() { CopyDateTime = DateTime.Now };
        }

        public ICommand Save => new Command(async () => {
            await _repository.AddOrUpdate(Item);
            await Navigation.PopAsync();
        });
    }
}


