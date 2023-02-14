using System.Threading.Tasks;
//using Xamarin.Forms;

namespace MonkeyPaste {
    public interface MpINavigate {
        Task NavigateToAsync(string route);
        Task PushModalAsync(Page page);
        Task PopModalAsync();
    }
}