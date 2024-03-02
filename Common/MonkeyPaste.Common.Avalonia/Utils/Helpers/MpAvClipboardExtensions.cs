using Avalonia;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;



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

        public static async Task<Dictionary<string, object>> ReadClipboardAsync(string[] formatFilter = default, int retryCount = 0) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                var output = await Dispatcher.UIThread.InvokeAsync(() => ReadClipboardAsync(formatFilter, retryCount));
                return output;
            }

            if (Application.Current.GetMainTopLevel() is not { } tl ||
                tl.Clipboard is not { } cb) {
                return new();
            }
            var result = await cb.ToDataObjectAsync(formatFilter, retryCount);
            return result.DataFormatLookup.ToDictionary(x => x.Key, x => x.Value);
        }
        public static async Task WriteToClipboardAsync(Dictionary<string, object> dataFormatLookup) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                await Dispatcher.UIThread.InvokeAsync(() => WriteToClipboardAsync(dataFormatLookup));
                return;
            }

            if (Application.Current.GetMainTopLevel() is not { } tl ||
                tl.Clipboard is not { } cb) {
                return;
            }
            await cb.SetDataObjectAsync(new MpAvDataObject(dataFormatLookup));
        }


        public static async Task<MpAvDataObject> ToDataObjectAsync(this IClipboard cb, string[] formatFilter = default, int retryCount = 0) {
            var avdo = new MpAvDataObject();
            if (cb == null) {
                return avdo;
            }
            var actualFormats = await cb.GetFormatsSafeAsync();

            if (formatFilter == null) {
                formatFilter = actualFormats;
            } else {
                formatFilter = formatFilter.Where(x => actualFormats.Contains(x)).ToArray();
            }

            foreach (string format in formatFilter) {
                object data = await cb.GetDataSafeAsync(format);
                if (data == null) {
                    continue;
                }
                avdo.SetData(format, data);
            }
            return avdo;
        }

        public static async Task LogClipboardAsync(this IClipboard cb) {
            var avdo = await cb.ToDataObjectAsync();
            avdo.LogDataObject();
        }
        public static async Task<string[]> GetFormatsSafeAsync(this IClipboard cb, int retryCount = 0) {
            if (cb == null) {
                return new string[] { };
            }
            await WaitForClipboardAsync();
            if (!Dispatcher.UIThread.CheckAccess()) {
                var result = await Dispatcher.UIThread.InvokeAsync(async () => {
                    return await cb.GetFormatsSafeAsync(retryCount);
                });
                return result;
            }
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
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error getting clipboard formats", ex);
                return new string[] { };
            }
        }

        public static async Task<object> GetDataSafeAsync(this IClipboard cb, string format, int retryCount = OLE_RETRY_COUNT) {
            await WaitForClipboardAsync();
            //if (!Dispatcher.UIThread.CheckAccess()) {
            //    var result = await Dispatcher.UIThread.InvokeAsync(async () => {
            //        return await cb.GetDataSafeAsync(format, retryCount);
            //    });
            //    return result;
            //}
            try {
                object result = await cb.GetDataAsync(format).TimeoutAfter(TimeSpan.FromSeconds(1));
                CloseClipboard();
                return result;
            }
            catch (SerializationException ex) {
                MpConsole.WriteTraceLine($"Error reading cb format: '{format}'.", ex);
                CloseClipboard();
                return null;
            }
            catch (COMException) {
                if (retryCount <= 0) {
                    CloseClipboard();
                    return null;
                }
                await Task.Delay(OLE_RETRY_DELAY_MS);
                var retry_result = await cb.GetDataSafeAsync(format, --retryCount);
                return retry_result;
            }
            catch (AccessViolationException avex) {
                MpConsole.WriteTraceLine($"Error reading cb format: '{format}'.", avex);
                CloseClipboard();
                return null;
            }
        }

        public static async Task SetDataObjectSafeAsync(this IClipboard cb, IDataObject ido, int retryCount = 0) {
            await WaitForClipboardAsync();

            try {
                //#if MAC
                //                await ido.WriteToPasteboardAsync(false);
                //#else

                await cb.SetDataObjectAsync(ido);
                //#endif
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
