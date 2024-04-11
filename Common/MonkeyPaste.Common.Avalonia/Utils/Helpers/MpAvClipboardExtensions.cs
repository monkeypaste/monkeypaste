using Avalonia;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Runtime.Intrinsics.X86;




#if WINDOWS

using MonkeyPaste.Common.Wpf;

#endif

#if MAC

using MonoMac.AppKit;
using MonoMac.Foundation;
using System.Text;
using Newtonsoft.Json.Linq;

#endif

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvClipboardExtensions {
        public const int OLE_RETRY_COUNT = 10;
        public static int OLE_RETRY_DELAY_MS {
            get {
                return MpRandom.Rand.Next(20, 100);
            }
        }

        static Dictionary<string, string> _MacFormatMap;
        static Dictionary<string, string> MacFormatMap {
            get {
                if (_MacFormatMap == null) {
                    _MacFormatMap = new() {
                        {MpPortableDataFormats.MacRtf1, MpPortableDataFormats.WinRtf },
                        {MpPortableDataFormats.MacImage1, MpPortableDataFormats.AvImage },
                        {MpPortableDataFormats.MacImage2, MpPortableDataFormats.AvImage },
                        {MpPortableDataFormats.MacHtml1, MpPortableDataFormats.MimeHtml },
                        {MpPortableDataFormats.MacChromeUrl, MpPortableDataFormats.MimeMozUrl },
                        {MpPortableDataFormats.MacChromeUrl2, MpPortableDataFormats.MimeMozUrl },
                    };
                }
                return _MacFormatMap;
            }
        }

        public static async Task<Dictionary<string, object>> ReadClipboardAsync(string[] formatFilter = default, int retryCount = 0) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                var output = await Dispatcher.UIThread.InvokeAsync(async() => { 
                    var result = await ReadClipboardAsync(formatFilter, retryCount); 
                    return result; 
                });
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
        
        public static async Task FinalizePlatformDataObjectAsync(Dictionary<string, object> dataFormatLookup) {
            if (!Dispatcher.UIThread.CheckAccess()) {
                await Dispatcher.UIThread.InvokeAsync(() => FinalizePlatformDataObjectAsync(dataFormatLookup));
                return;
            }
            foreach(string format in dataFormatLookup.Keys) {
                object data = dataFormatLookup[format];
                var map = GetPlatformFormatMap(format);
                Type target_type = map.outputType;
                Encoding target_enc = map.outputEncoding;
                if(data.GetType() == target_type) {
                    continue;
                }
                string str_data = ReadDataFormat<string>(format, data);
                if(target_type == typeof(string)) {
                    data = str_data;
                } else if(target_type == typeof(byte[])) { 
                    if(target_enc == Encoding.ASCII) {
                        // implies its base64
                        data = str_data.ToBytesFromBase64String();
                    } else {
                        data = str_data.ToBytesFromString(target_enc);
                    }
                } else if(target_type == typeof(IStorageItem[])) {
                    data = await str_data.SplitNoEmpty(Environment.NewLine).ToAvFilesObjectAsync();
                } else {
                    MpDebug.Break($"Warning! Unknown clipboard map from format '{format}' to target: {target_type}/{target_enc}");
                }
                dataFormatLookup[format] = data;
            }
        }

        public static T ReadDataFormat<T>(string format, object dataObj) where T : class {
            if (dataObj == null) {
                return default;
            }
            T typed_data = dataObj as T;
            if (typed_data != null) {
                return typed_data;
            }

            if (typeof(T) == typeof(string)) {
                // wants string
                if (dataObj is byte[] bytes) {
                    // bytes -> string
                    if (MpPortableDataFormats.IsFormatStrBase64(format) is true) {
                        // img bytes -> string
                        typed_data = bytes.ToBase64String() as T;
                    } else {
                        // text bytes -> string
#if WINDOWS
                        if (format == MpPortableDataFormats.Xhtml) {
                            var detected_encoding = bytes.DetectTextEncoding(out string detected_text);
                            bytes = Encoding.UTF8.GetBytes(detected_text);
                            //if (detected_text.Contains("Â")) {
                            //    MpDebug.Break();
                            //}
                        }
#endif
                        typed_data = bytes.ToDecodedString() as T;

                    }
                } else if (dataObj is IEnumerable<string> strings) {
                    // string list -> string
                    typed_data = string.Join(Environment.NewLine, strings) as T;
                } else if (dataObj is IEnumerable<IStorageItem> sil) {
                    // si[] -> string
                    typed_data = string.Join(Environment.NewLine, sil.Select(x => x.TryGetLocalPath())) as T;
                } else if (dataObj is int intVal) {
                    // int -> string (occurs internally putting actionId on clipboard)
                    typed_data = intVal.ToString() as T;
                } else if (dataObj != null) {
                    typed_data = dataObj.ToString() as T;
                }
            } else if (typeof(T) == typeof(byte[])) {
                // wants bytes
                if (dataObj is string byteStr) {
                    // string -> bytes
                    if (MpPortableDataFormats.IsFormatStrBase64(format) is true) {
                        // string -> img bytes
                        typed_data = byteStr.ToBytesFromBase64String() as T;
                    } else {
                        // string -> text bytes
                        typed_data = byteStr.ToBytesFromString() as T;
                    }
                }
            } else if (typeof(T) == typeof(IEnumerable<string>)) {
                // wants string list
                if (dataObj is string dataStr) {
                    // string -> string list
                    typed_data = dataStr.SplitNoEmpty(Environment.NewLine).AsEnumerable<string>() as T;
                } else if (dataObj is IEnumerable<Uri> uril) {
                    // uri[] -> string list
                    typed_data = uril.Select(x => x.ToFileSystemPath()) as T;
                } else if (dataObj is IEnumerable<IStorageItem> sil) {
                    // si[] -> string list
                    typed_data = sil.Select(x => x.TryGetLocalPath()) as T;
                } else if (dataObj is JArray ja) {
                    typed_data = ja.ToList().Select(x => x.ToString()) as T;
                } else {

                }
            } else if (typeof(T) == typeof(MpPortableProcessInfo)) {
                // wants process info
                if (dataObj is string ppi_json) {
                    typed_data = ppi_json.DeserializeObject<MpPortableProcessInfo>() as T;
                }
            }
            if (typed_data == null) {
                MpDebug.Break($"Unhandled dataobj get, source is '{dataObj.GetType()}' target is '{format}'");
            }

            return typed_data;
        }
        private static (Type outputType, Encoding outputEncoding) GetPlatformFormatMap(string format) {
            Type output_type = typeof(string);
            Encoding output_encoding = Encoding.UTF8;

#if MAC
            output_type = typeof(byte[]);
            if (format == MpPortableDataFormats.MacText2) {
                output_encoding = Encoding.Unicode;
            }
#elif WINDOWS
            if(format == MpPortableDataFormats.WinXhtml) {
                output_type = typeof(byte[]);
            } 
            if(format == MpPortableDataFormats.WinRtf) {
                output_type = typeof(byte[]);
            }
            if(format == MpPortableDataFormats.WinUnicode) {
                output_encoding = Encoding.Unicode;
            }
#else

#endif
            // common stuff
            if (MpPortableDataFormats.IsImageFormat(format) is true) {
                // this implies string->base64
                output_encoding = Encoding.ASCII;
            } else if (MpPortableDataFormats.IsFilesFormat(format) is true) {
                // kinda hacky but to avoid dep here since IStorageItem is an 
                // 
                output_type = typeof(IStorageItem[]);
                output_encoding = Encoding.ASCII;
            }
            if (format == MpPortableDataFormats.CefAsciiUrl) {
                output_type = typeof(byte[]);
                output_encoding = Encoding.ASCII;
            }
            return (output_type, output_encoding);
        }


        public static async Task<MpAvDataObject> ToDataObjectAsync(this IClipboard cb, string[] formatFilter = default, int retryCount = 0) {
            var avdo = new MpAvDataObject();
            if (cb == null) {
                return avdo;
            }
            var actualFormats = await cb.GetFormatsSafeAsync();
            // <PlatformFormatName,CommonFormatName>
            var mappedFormats = actualFormats.ToDictionary(x => x, x => x);

#if MAC
            for (int i = 0; i < actualFormats.Length; i++) {
                if (MacFormatMap.TryGetValue(actualFormats[i], out string mapped_format)) {
                    mappedFormats[actualFormats[i]] = mapped_format;
                }
            }
#endif

            if (formatFilter == null) {
                formatFilter = mappedFormats.Keys.ToArray();
            } else {
                formatFilter = mappedFormats.Where(x => formatFilter.Contains(x.Value)).Select(x => x.Key).ToArray();
            }

            foreach (string format in formatFilter) {
                object data = await cb.GetDataSafeAsync(format);
                if (data == null) {
                    continue;
                }
#if MAC
                if (MpAvMacDataFormatReader.TryRead(format, data, out string mac_data_str)) {
                    data = mac_data_str;
                }
#endif
                if (mappedFormats.TryGetValue(format, out string common_format_name)) {
                    avdo.SetData(common_format_name, data);
                } else {
                    MpConsole.WriteLine($"Could not find commong format for: '{format}'");
                }
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
                    var formats = await cb.GetFormatsSafeAsync(retryCount);
                    return formats;
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
            // 
            if (!Dispatcher.UIThread.CheckAccess()) {
                var result = await Dispatcher.UIThread.InvokeAsync(async () => {
                    var cb_data = await cb.GetDataSafeAsync(format, retryCount);
                    return cb_data;
                });
                return result;
            }
            try {
                object result = await cb.GetDataAsync(format).TimeoutAfter(TimeSpan.FromSeconds(1));
                CloseClipboard();
                return result;
            }
            catch (SerializationException serx) {
                MpConsole.WriteTraceLine($"Error reading cb format: '{format}'.", serx);
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
            catch(Exception ex) {
                MpConsole.WriteTraceLine($"Error reading cb format: '{format}'.", ex);
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
                try {
                    bool canOpen = WinApi.IsClipboardOpen() == IntPtr.Zero;
                    while (!canOpen) {
                        await Task.Delay(OLE_RETRY_DELAY_MS);
                        canOpen = WinApi.IsClipboardOpen() == IntPtr.Zero;
                    }
                } catch(Exception ex) {
                    MpConsole.WriteTraceLine($"WaitForClipboard async error.", ex);
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
