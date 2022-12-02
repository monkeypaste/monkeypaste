using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public interface MpAvIWebViewBindingResponseHandler {
        Task HandleBindingNotificationAsync(MpAvEditorBindingFunctionType notificationType, string msgJsonBase64Str);
    }
}
