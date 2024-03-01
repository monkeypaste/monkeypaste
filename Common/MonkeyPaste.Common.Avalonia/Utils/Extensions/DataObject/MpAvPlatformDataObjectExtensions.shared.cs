using Avalonia.Input;
using Avalonia.Platform.Storage;
using MonkeyPaste.Common.Plugin;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyPaste.Common.Avalonia {
    public static partial class MpAvPlatformDataObjectExtensions {

        public static IDataObject ToDataObject(this Dictionary<string, object> dict) {
            return new MpAvDataObject(dict);
        }
        public static Dictionary<string, object> ToDictionary(this IDataObject ido) {
            if (ido == null) {
                return null;
            }
            return ido.GetAllDataFormats().ToDictionary(x => x, x => ido.Get(x));
        }
        public static IEnumerable<string> GetAllDataFormats(this IDataObject ido) {
            try {
                if (ido == null ||
                ido.GetDataFormats() is not IEnumerable<string> dfl) {
                    return new string[] { };
                }
                List<string> formats = dfl.ToList();
                if (ido.GetFiles() is IEnumerable<object> fps) {
                    // only inlcude file names if present
                    if (fps.Count() > 0) {
                        if (!formats.Contains(MpPortableDataFormats.Files)) {
                            formats.Add(MpPortableDataFormats.Files);
                        }
                    } else {
                        formats.Remove(MpPortableDataFormats.Files);
                    }

                }
                // return non-null (workaround since sysdo can't remove)
                return formats.Where(x => ido.Get(x) != null);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error reading data formats. ", ex);
                return new List<string>();
            }
        }

        public static IEnumerable<string> GetFilesAsPaths(this IDataObject dataObject) {
            return (dataObject.Get(MpPortableDataFormats.Files) as IEnumerable<string>)
                ?? dataObject.GetFiles()?
                .Select(f => f.TryGetLocalPath())
                .Where(p => !string.IsNullOrEmpty(p))
                .OfType<string>();
        }

        public static object GetAllowFiles(this IDataObject ido, string format) {
            if (ido == null) {
                return null;
            }
            if (ido.Get(format) is object obj) {
                return obj;
            }
            if (format != MpPortableDataFormats.Files) {
                return null;
            }
            if (ido.GetFiles() is IEnumerable<object> fpl &&
                fpl.Count() > 0) {
                return fpl;
            }
            return null;
        }

        public static IDataObject Clone(this IDataObject ido_source) {
            if (ido_source == null) {
                return null;
            }
            var cavdo = new MpAvDataObject();
            var availableFormats = ido_source.GetAllDataFormats().ToList();

            // need duplicate string lists or the reference will be passed to clone
            var string_lists_to_clone = availableFormats.Where(x => ido_source.Get(x) is IEnumerable<string>).ToArray();
            for (int i = 0; i < string_lists_to_clone.Length; i++) {
                string sltc = string_lists_to_clone[i];
                if (ido_source.Get(sltc) is string[] source_stringArr) {
                    var cloned_str_arr = new string[source_stringArr.Length];
                    //source_stringArr.CopyTo(cloned_str_arr, 0);
                    for (int j = 0; j < cloned_str_arr.Length; j++) {
                        cloned_str_arr[j] = source_stringArr[j];
                    }

                    availableFormats.Remove(sltc);
                    cavdo.Set(sltc, cloned_str_arr);
                } else if (ido_source.Get(sltc) is byte[] source_byteArr) {
                    var cloned_byte_arr = new byte[source_byteArr.Length];
                    source_byteArr.CopyTo(cloned_byte_arr, 0);
                    availableFormats.Remove(sltc);
                    cavdo.Set(sltc, cloned_byte_arr);
                }

            }
            availableFormats.ForEach(x => cavdo.SetData(x, ido_source.GetAllowFiles(x)));
            return cavdo;
        }

        public static bool ContainsPlaceholderFormat(this IDataObject ido, string format) {
            object f_data = ido.Get(format);
            if (f_data == null) {
                return false;
            }
            if (f_data is string dataStr &&
                dataStr == MpPortableDataFormats.PLACEHOLDER_DATAOBJECT_TEXT) {
                return true;
            }
            if (f_data is IEnumerable<string> dl &&
                dl.Any(x => x == MpPortableDataFormats.PLACEHOLDER_DATAOBJECT_TEXT)) {
                return true;
            }
            if (f_data is byte[] dataBytes &&
                dataBytes.ToDecodedString() == MpPortableDataFormats.PLACEHOLDER_DATAOBJECT_TEXT) {
                return true;
            }
            return false;
        }
        public static IEnumerable<string> GetPlaceholderFormats(this IDataObject ido) {
            return
                ido
                .GetAllDataFormats()
                .Where(x => ido.ContainsPlaceholderFormat(x));
        }
        public static bool IsAnyPlaceholderData(this IDataObject ido) {
            return ido.GetPlaceholderFormats().Count() > 0;
        }

        public static object GetFormatPlaceholderData(string format) {
            if (MpPortableDataFormats.InternalFormats.Contains(format) &&
                format != MpPortableDataFormats.CefAsciiUrl) {
                return null;
            }
            switch (format) {
                case MpPortableDataFormats.Image:
                case MpPortableDataFormats.CefAsciiUrl:
                    return MpPortableDataFormats.PLACEHOLDER_DATAOBJECT_TEXT.ToBytesFromString();
                case MpPortableDataFormats.Files:
                case MpPortableDataFormats.INTERNAL_SOURCE_URI_LIST_FORMAT:
                    return new string[] { MpPortableDataFormats.PLACEHOLDER_DATAOBJECT_TEXT };
                default:
                    return MpPortableDataFormats.PLACEHOLDER_DATAOBJECT_TEXT;
            }
        }


        public static bool ContainsData(this IDataObject ido, string format) {
            // NOTE used for live dnd dataObjectLookup state
            // since IDataObject doesn't allow for format removal
            // when format disabled in drop widget the item isn't
            // removed (in WriteClipboardOrDropObjectAsync)
            // just nulled out this checks if format actually has data

            if (ido == null ||
                string.IsNullOrEmpty(format) ||
                !ido.Contains(format) ||
                ido.ContainsPlaceholderFormat(format)) {
                return false;
            }

            object data = ido.Get(format);
            return MpAvClipboardExtensions.IsValidClipboardData(data);
        }

        public static void CopyTo(this IDataObject source_ido, IDataObject target_ido) {
            if (target_ido == null || source_ido == null) {
                return;
            }
            var sfl = source_ido.GetAllDataFormats();
            //foreach (var sf in sfl) {
            //    // set all target items to available source items
            //    if (source_ido.Get(sf) is string[] sourceFileArr &&
            //        target_ido.Get(sf) is string[] targetFileArr) {
            //        // retain file item array object, only up entries or null if new is less (edge case)
            //        for (int i = 0; i < targetFileArr.Length; i++) {
            //            if (i >= sourceFileArr.Length) {
            //                targetFileArr[i] = null;
            //                continue;
            //            }
            //            targetFileArr[i] = sourceFileArr[i];
            //        }
            //        continue;
            //    }
            //    target_ido.Set(sf, source_ido.Get(sf));
            //}
            sfl.ForEach(x => target_ido.Set(x, source_ido.Get(x)));
            var tfl_to_clear = target_ido.GetAllDataFormats().Where(x => !sfl.Contains(x));
            foreach (var tf_to_clear in tfl_to_clear) {
                // clear all target data not found in source
                target_ido.Set(tf_to_clear, null);
            }
        }

        public static bool TryGetData(this IDataObject ido, string format, out object data) {
            data = null;
            if (!ido.ContainsData(format)) {
                return false;
            }

            data = ido.Get(format);
            return true;
        }

        public static bool TryGetData<T>(this IDataObject ido, string format, out T data) where T : class {
            data = null;
            if (!ido.TryGetData(format, out object dataObj)) {
                return false;
            }
            data = ReadDataFormat<T>(format, dataObj);
            return data != null;
        }

        public static bool TryGetValue<T>(this Dictionary<string, object> dict, string key, out T value) where T : class {
            value = null;
            if (key == null || !dict.TryGetValue(key, out object dataObj)) {
                return false;
            }
            value = ReadDataFormat<T>(key, dataObj);
            return value != null;
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
                    if (format == MpPortableDataFormats.Image) {
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
                    if (format == MpPortableDataFormats.Image) {
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

        public static bool TryRemove(this IDataObject ido, string format) {
            if (ido == null || !ido.Contains(format)) {
                return false;
            }
            //ido.Set(format, null);
            //return true;
            if (ido is DataObject sysdo) {
                // probably exception
                try {
                    sysdo.Set(format, null);
                }
                catch { }

                return true;
            } else if (ido is MpPortableDataObject mpdo &&
                        mpdo.Remove(format)) {
                return true;
            }
            return false;
        }

        public static void Set(this IDataObject ido, string format, object data) {
            if (ido == null) {
                return;
            }
            if (ido is DataObject sysdo) {
                // probably exception
                sysdo.Set(format, data);
            } else if (ido is MpPortableDataObject mpdo) {
                mpdo.SetData(format, data);
            }
        }

    }
}