using Avalonia.Input;
using Avalonia.Input.Platform;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System;
using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Linq;

#if WINDOWS

using MonkeyPaste.Common.Wpf;

#endif

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
            catch (AccessViolationException avex) {
                MpConsole.WriteTraceLine($"Error reading cb format: '{format}'.", avex);
                return null;
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

        public static bool IsValidClipboardData(object data) {
            // NOTE when setting clipboard data needs to meet this checks or 
            // will get AccesViolationException (windows)

            if (data == null) {
                return false;
            }
            bool was_checked = false;
            if (data is string idoStr) {
                was_checked = true;
                if (string.IsNullOrEmpty(idoStr)) {
                    return false;
                }
            }
            if (data is IEnumerable<string> idoStrs) {
                was_checked = true;
                if (!idoStrs.Any()) {
                    return false;
                }
            }
            if (data is byte[] idoBytes) {
                was_checked = true;
                if (idoBytes.Length == 0) {
                    return false;
                }
            }
            if (data is IEnumerable<IStorageItem> sil) {
                was_checked = true;
                if (!sil.Any()) {
                    return false;
                }
            }

            if (!was_checked) {
                object test = data;
                //MpDebug.Break($"Unchecked format, for type '{test.GetType()}'");
            }
            return true;
        }
        private static async Task WaitForClipboardAsync() {
            await Task.Delay(0);
#if WINDOWS
            if (OperatingSystem.IsWindows()) {
                bool canOpen = WinApi.IsClipboardOpen() == IntPtr.Zero;
                while (!canOpen) {
                    await Task.Delay(OLE_RETRY_DELAY_MS);
                    canOpen = WinApi.IsClipboardOpen() == IntPtr.Zero;
                }
            }
#endif
        }

        private static void CloseClipboard() {
#if WINDOWS
            if (OperatingSystem.IsWindows()) {
                WinApi.CloseClipboard();
            }
#endif
        }
    }
}
