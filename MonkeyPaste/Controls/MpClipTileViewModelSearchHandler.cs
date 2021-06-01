using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpClipTileViewModelSearchHandler : SearchHandler {
        protected override void OnQueryChanged(string oldValue, string newValue) {
            base.OnQueryChanged(oldValue, newValue);

            //if (string.IsNullOrWhiteSpace(newValue)) {
            //    ItemsSource = null;
            //} else {
            //    ItemsSource = ClipViewModels
            //        .Where(ClipViewModel => ClipViewModel.Clip.ItemPlainText.ContainsByUserSensitivity(newValue))
            //        .ToList<MpClipViewModel>();
            //}
        }

        protected override async void OnItemSelected(object item) {
            base.OnItemSelected(item);

            
            // Let the animation complete
            await Task.Delay(1000);

            if (item == null || item is not MpClipTileViewModel) {
                return;
            }
            var civm = item as MpClipTileViewModel;

            ShellNavigationState state = (App.Current.MainPage as Shell).CurrentState;
            // The following route works because route names are unique in this application.
            await Shell.Current.GoToAsync($"Clipdetails?Id={civm.Clip.Id}");
        }

        //string GetNavigationTarget() {
        //    return (Shell.Current as MpMainShell).Routes.FirstOrDefault(route => route.Value.Equals(SelectedItemNavigationTarget)).Key;
        //}
    }
}
