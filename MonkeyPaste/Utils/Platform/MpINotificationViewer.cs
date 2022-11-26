using System.Threading.Tasks;

namespace MonkeyPaste {
    public interface MpINotificationViewer {
        Task<MpNotificationDialogResultType> ShowNotificationAsync(MpNotificationFormat nf);
    }
}
