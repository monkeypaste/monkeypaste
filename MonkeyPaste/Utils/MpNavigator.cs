using System.Threading.Tasks;
using MonkeyPaste;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpNavigator : MpINavigate {
        public async Task NavigateTo(string route) {
            await Shell.Current.GoToAsync(route);
        }
        public async Task PushModal(Page page) {
            await Shell.Current.Navigation.PushModalAsync(page);
        }
        public async Task PopModal() {
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}
