using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpCopyItemViewModelSearchHandler : SearchHandler {
        protected override void OnBindingContextChanged() {
            base.OnBindingContextChanged();
            if(BindingContext != null) {
                Focused += MpCopyItemViewModelSearchHandler_Focused;
            }
        }

        private void MpCopyItemViewModelSearchHandler_Focused(object sender, EventArgs e) {
            throw new NotImplementedException();
        }

        protected override void OnQueryChanged(string oldValue, string newValue) {
            base.OnQueryChanged(oldValue, newValue);

            //if (string.IsNullOrWhiteSpace(newValue)) {
            //    ItemsSource = null;
            //} else {
            //    ItemsSource = CopyItemViewModels
            //        .Where(CopyItemViewModel => CopyItemViewModel.CopyItem.ItemPlainText.ContainsByUserSensitivity(newValue))
            //        .ToList<MpCopyItemViewModel>();
            //}
        }

        protected override async void OnItemSelected(object item) {
            base.OnItemSelected(item);

            
            // Let the animation complete
            await Task.Delay(1000);

            if (item == null || item is not MpCopyItemViewModel) {
                return;
            }
            var civm = item as MpCopyItemViewModel;

            ShellNavigationState state = (App.Current.MainPage as Shell).CurrentState;
            // The following route works because route names are unique in this application.
            await Shell.Current.GoToAsync($"CopyItemdetails?Id={civm.CopyItem.Id}");
        }

        //string GetNavigationTarget() {
        //    return (Shell.Current as MpMainShell).Routes.FirstOrDefault(route => route.Value.Equals(SelectedItemNavigationTarget)).Key;
        //}
    }
}
