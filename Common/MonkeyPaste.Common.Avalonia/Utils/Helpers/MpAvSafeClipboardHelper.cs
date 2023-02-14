using Avalonia.Input;
using Avalonia.Input.Platform;
using MonkeyPaste.Common.Wpf;
using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvClipboardExtensions {
        public const int OLE_RETRY_COUNT = 10;
        public static int OLE_RETRY_DELAY_MS {
            get {
                return MpRandom.Rand.Next(20, 100);
            }
        }

        public static async Task<string[]> GetFormatsSafeAsync(this IClipboard cb, int retryCount = 0) {
            await WaitForClipboardAsync();
            try {
                var result = await cb.GetFormatsAsync();
                CloseClipboard();
                return result;
            }
            catch (COMException) {
                if (retryCount >= OLE_RETRY_COUNT) {
                    return new string[] { };
                }
                await Task.Delay(OLE_RETRY_DELAY_MS);
                var retry_result = await cb.GetFormatsSafeAsync(++retryCount);
                return retry_result;
            }
        }

        public static async Task<object> GetDataSafeAsync(this IClipboard cb, string format, int retryCount = 0) {
            await WaitForClipboardAsync();

            try {
                object result = await cb.GetDataAsync(format);
                CloseClipboard();
                return result;
            }
            catch (SerializationException ex) {
                MpConsole.WriteTraceLine($"Error reading cb format: '{format}'.", ex);
                return null;
            }
            catch (COMException) {
                if (retryCount >= OLE_RETRY_COUNT) {
                    return new string[] { };
                }
                await Task.Delay(OLE_RETRY_DELAY_MS);
                var retry_result = await cb.GetDataSafeAsync(format, ++retryCount);
                return retry_result;
            }
        }

        public static async Task SetDataObjectSafeAsync(this IClipboard cb, IDataObject ido, int retryCount = 0) {
            await WaitForClipboardAsync();

            try {
                await cb.SetDataObjectAsync(ido);
                CloseClipboard();
            }
            catch (COMException) {
                if (retryCount >= OLE_RETRY_COUNT) {
                    return;
                }
                await Task.Delay(OLE_RETRY_DELAY_MS);
                await cb.SetDataObjectSafeAsync(ido, ++retryCount);
            }
        }

        private static async Task WaitForClipboardAsync() {
            if (OperatingSystem.IsWindows()) {
                bool canOpen = WinApi.IsClipboardOpen() == IntPtr.Zero;
                while (!canOpen) {
                    await Task.Delay(OLE_RETRY_DELAY_MS);
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
