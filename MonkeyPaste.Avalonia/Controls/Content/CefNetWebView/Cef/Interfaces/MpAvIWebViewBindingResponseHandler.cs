using MonkeyPaste;
using System.Threading.Tasks;
namespace MonkeyPaste.Avalonia {
    public interface MpAvIWebViewBindingResponseHandler {
        void HandleBindingNotification(MpEditorBindingFunctionType notificationType, string msgJsonBase64Str, string contentHandle);
    }
}
