using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public interface MpAvIWebViewBindingResponseHandler {
        void HandleBindingNotification(MpAvEditorBindingFunctionType notificationType, string msgJsonBase64Str);
    }
}
