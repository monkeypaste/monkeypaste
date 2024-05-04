using Android.Content;
using Avalonia.Input;
using Avalonia.Input.Platform;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace MonkeyPaste.Avalonia.Android {
    public class MpAvAdClipboard : MpIClipboard {
        static ClipboardManager ClipboardManager =>
            Platform.AppContext.GetSystemService(Context.ClipboardService) as ClipboardManager;

        public Task<string> GetTextAsync() =>
            Clipboard.GetTextAsync();

        public Task SetTextAsync(string text) =>
            Clipboard.SetTextAsync(text);

        public Task ClearAsync() =>
            Clipboard.SetTextAsync(string.Empty);

        public async Task SetDataObjectAsync(Dictionary<string, object> ido) {
            if (ido == null ||
                ido.TryGetValue(MpPortableDataFormats.Text, out string data_text)) {
                await ClearAsync();
                return;
            }
            await SetTextAsync(data_text);
        }

        public async Task<string[]> GetFormatsAsync() {
            await Task.Delay(0);
            if (Clipboard.HasText) {
                return new string[] { MpPortableDataFormats.Text };
            }
            return new string[] { };
        }


        public async Task<object> GetDataAsync(string format) {
            if (format == MpPortableDataFormats.Text) {
                string result = await GetTextAsync();
                return result;
            }
            return null;
        }
    }
}
