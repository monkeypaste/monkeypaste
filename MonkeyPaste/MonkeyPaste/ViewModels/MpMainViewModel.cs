using MonkeyPaste.Repositories;
using MonkeyPaste.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MonkeyPaste.ViewModels {
    public class MpMainViewModel : Base.MpViewModelBase {
        private readonly MpCopyItemRepository _repository;

        public MpMainViewModel(MpCopyItemRepository repository) {
            _repository = repository;
            Task.Run(async () => await LoadData());
        }
        private async Task LoadData() {
        }

        public ICommand AddItem => new Command(async () => {
            var itemView = MpResolver.Resolve<MpItemView>();
            await Navigation.PushAsync(itemView);
        });

        
    }
}
