using System.Threading.Tasks;
using Xamarin.Forms;

namespace MonkeyPaste {
    public interface MpINavigate {
        Task NavigateTo(string route);
        Task PushModal(Page page);
        Task PopModal();
    }
}