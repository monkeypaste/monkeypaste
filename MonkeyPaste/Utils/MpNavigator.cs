using System.Threading.Tasks;
using MonkeyPaste;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpNavigator : MpINavigate {
        public async Task NavigateToAsync(string route) {
            await Shell.Current.GoToAsync(route);
        }
        public async Task PushModalAsync(Page page) {
            await Shell.Current.Navigation.PushModalAsync(page);
        }
        public async Task PopModalAsync() {
            await Shell.Current.Navigation.PopModalAsync();
        }
    }
}
