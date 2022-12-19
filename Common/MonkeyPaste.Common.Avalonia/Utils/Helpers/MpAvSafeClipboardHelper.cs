using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using MonkeyPaste.Common.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvClipboardExtensions {
        public const int OLE_RETRY_COUNT = 10;

        public static async Task<string[]> GetFormatsSafeAsync(this IClipboard cb) {
            await WaitForClipboardAsync();
            var result = await cb.GetFormatsAsync();
            CloseClipboard();
            return result;
        }

        public static async Task<object> GetDataSafeAsync(this IClipboard cb,string format) {
            await WaitForClipboardAsync();
            var result = await cb.GetDataAsync(format);
            CloseClipboard();
            return result;
        }

        public static async Task SetDataObjectSafeAsync(this IClipboard cb, IDataObject ido) {
            await WaitForClipboardAsync();
            await cb.SetDataObjectAsync(ido);
            CloseClipboard();
        }

        private static async Task WaitForClipboardAsync() {
            if (OperatingSystem.IsWindows()) {
                bool canOpen = WinApi.IsClipboardOpen() == IntPtr.Zero;
                while (!canOpen) {
                    await Task.Delay(50);
                    canOpen = WinApi.IsClipboardOpen() == IntPtr.Zero;
                }
            }
        }

        private static void CloseClipboard() {
            if (OperatingSystem.IsWindows()) {
                WinApi.CloseClipboard();
            }
        }
    }
}
