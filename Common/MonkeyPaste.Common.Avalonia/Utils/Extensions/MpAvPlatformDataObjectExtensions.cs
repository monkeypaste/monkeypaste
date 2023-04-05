using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MonkeyPaste.Common.Avalonia {
    public static class MpAvPlatformDataObjectExtensions {
        public static IEnumerable<string> GetAllDataFormats(this IDataObject ido) {
            if (ido == null) {
                return null;
            }
            List<string> formats = ido.GetDataFormats().ToList();
            if (ido.GetFileNames() is IEnumerable<string> fps &&
                fps.Count() > 0) {
                formats.Add(MpPortableDataFormats.AvFileNames);
            }
            // return non-null (workaround since sysdo can't remove)
            return formats.Where(x => ido.Get(x) != null);
        }

        public static object GetAllowFiles(this IDataObject ido, string format) {
            if (ido == null) {
                return null;
            }
            if (ido.Get(format) is object obj) {
                return obj;
            }
            if (format != MpPortableDataFormats.AvFileNames) {
                return null;
            }
            if (ido.GetFileNames() is IEnumerable<string> fpl &&
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
                    var cloned_str_list = new string[source_stringArr.Length];
                    for (int j = 0; j < source_stringArr.Length; j++) {
                        cloned_str_list[j] = source_stringArr[j];
                    }
                    availableFormats.Remove(sltc);
                    cavdo.Set(sltc, cloned_str_list);
                }

            }
            availableFormats.ForEach(x => cavdo.SetData(x, ido_source.GetAllowFiles(x)));
            return cavdo;
        }

        public static bool ContainsData(this IDataObject ido, string format) {
            // NOTE used for live dnd dataObject state
            // since IDataObject doesn't allow for format removal
            // when format disabled in drop widget the item isn't
            // removed (in WriteClipboardOrDropObjectAsync)
            // just nulled out this checks if format actually has data

            if (ido == null || string.IsNullOrEmpty(format) || !ido.Contains(format)) {
                return false;
            }


            bool was_checked = false;
            if (ido.Get(format) is string idoStr) {
                was_checked = true;
                if (string.IsNullOrEmpty(idoStr)) {
                    return false;
                }
                if (idoStr == MpPortableDataFormats.PLACEHOLDER_DATAOBJECT_TEXT) {
                    // any string (or yet-to-be encoded) format not available yet
                    return false;
                }
            }
            if (ido.Get(format) is IEnumerable<string> idoStrs) {
                was_checked = true;
                if (!idoStrs.Any()) {
                    return false;
                }
                if (idoStrs.All(x => x == MpPortableDataFormats.PLACEHOLDER_DATAOBJECT_TEXT)) {
                    // any string list (file or uri) not available yet
                    return false;
                }
            }
            if (ido.Get(format) is byte[] idoBytes) {
                was_checked = true;
                if (idoBytes.Length == 0) {
                    return false;
                }
                if (idoBytes.ToDecodedString() == MpPortableDataFormats.PLACEHOLDER_DATAOBJECT_TEXT) {
                    // image not available yet
                    return false;
                }
            }

            if (!was_checked) {
                object test = ido.Get(format);
                MpDebug.Break($"Unchecked format, for type '{test.GetType()}'");
            }

            return true;
        }

        public static void CopyFrom(this IDataObject target_ido, IDataObject source_ido) {
            if (target_ido == null || source_ido == null) {
                return;
            }
            var sfl = source_ido.GetAllDataFormats();
            foreach (var sf in sfl) {
                // set all target items to available source items
                if (sf == MpPortableDataFormats.AvFileNames &&
                    source_ido.Get(sf) is string[] sourceFileArr &&
                    target_ido.Get(sf) is string[] targetFileArr) {
                    // retain file item array object, only up entries or null if new is less (edge case)
                    for (int i = 0; i < targetFileArr.Length; i++) {
                        if (i >= sourceFileArr.Length) {
                            targetFileArr[i] = null;
                            continue;
                        }
                        targetFileArr[i] = sourceFileArr[i];
                    }
                    continue;
                }
                target_ido.Set(sf, source_ido.Get(sf));
            }
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
            if (ido.TryGetData(format, out object dataObj)) {
                T typed_data = dataObj as T;
                if (typed_data == null) {
                    if (typeof(T) == typeof(string)) {
                        // wants string
                        if (dataObj is byte[] bytes) {
                            // bytes -> string
                            if (format == MpPortableDataFormats.AvPNG) {
                                // img bytes -> string
                                typed_data = bytes.ToBase64String() as T;
                            } else {
                                // text bytes -> string
                                typed_data = bytes.ToDecodedString() as T;
                            }
                        } else if (dataObj is IEnumerable<string> strings) {
                            // string list -> string
                            typed_data = string.Join(Environment.NewLine, strings) as T;
                        }
                    } else if (typeof(T) == typeof(byte[])) {
                        // wants bytes
                        if (dataObj is string byteStr) {
                            // string -> bytes
                            if (format == MpPortableDataFormats.AvPNG) {
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
                        }
                    }
                    if (typed_data == null) {
                        MpDebug.Break($"Unhandled dataobj get, source is '{dataObj.GetType()}' target is '{format}'");

                    }
                }
                data = typed_data;
            }
            return data != null;
        }

        public static bool TryRemove(this IDataObject ido, string format) {
            if (ido == null || !ido.Contains(format)) {
                return false;
            }
            ido.Set(format, null);
            return true;
            //if (ido is DataObject sysdo) {
            //    // probably exception
            //    sysdo.Set(format, null);
            //    return true;
            //} else if (ido is MpPortableDataObject mpdo &&
            //    MpPortableDataFormats.GetDataFormat(format) is MpPortableDataFormat pdf) {
            //    mpdo.DataFormatLookup.Remove(pdf);
            //    return true;
            //}
            //return false;
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
