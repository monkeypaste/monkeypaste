//using McMaster.NETCore.Plugins;
using Avalonia;
using Avalonia.Input.Platform;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipboardWrapper : MpIClipboard {

        public MpAvClipboardWrapper() { }

        IClipboard _avClipboard;
        IClipboard AvClipboard {
            get {
                if (_avClipboard == null &&
                    Application.Current.GetMainTopLevel() is { } tl) {
                    _avClipboard = tl.Clipboard;
                }
                return _avClipboard;
            }
        }
        public async Task ClearAsync() {
            if (AvClipboard is not IClipboard cb) {
                return;
            }
            await cb.ClearAsync();
        }
        public async Task SetDataObjectAsync(Dictionary<string, object> data) {
            if (AvClipboard is not IClipboard cb ||
                data.ToDataObject() is not { } ido) {
                return;
            }
            await cb.SetDataObjectSafeAsync(ido);
        }
        public async Task<string[]> GetFormatsAsync() {
            if (AvClipboard is not IClipboard cb) {
                return [];
            }
            var result = await cb.GetFormatsSafeAsync();
            return result;
        }
        public async Task<object> GetDataAsync(string format) {
            if (AvClipboard is not IClipboard cb) {
                return null;
            }
            var result = await cb.GetDataSafeAsync(format);
            return result;
        }
    }
}
