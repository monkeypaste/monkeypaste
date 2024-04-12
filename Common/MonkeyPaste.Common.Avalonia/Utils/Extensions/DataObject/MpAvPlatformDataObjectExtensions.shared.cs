using AngleSharp.Dom;
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


        public static bool ContainsData(this IDataObject ido, string format) {
            // NOTE used for live dnd dataObjectLookup state
            // since IDataObject doesn't allow for format removal
            // when format disabled in drop widget the item isn't
            // removed (in WriteClipboardOrDropObjectAsync)
            // just nulled out this checks if format actually has data

            if (ido == null ||
                string.IsNullOrEmpty(format) ||
                !ido.Contains(format)) {
                return false;
            }

            object data = ido.Get(format);
            return MpAvClipboardExtensions.IsValidClipboardData(data);
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
            data = MpAvClipboardExtensions.ReadDataFormat<T>(format, dataObj);
            return data != null;
        }

        public static bool TryGetValue<T>(this Dictionary<string, object> dict, string key, out T value) where T : class {
            value = null;
            if (key == null || !dict.TryGetValue(key, out object dataObj)) {
                return false;
            }
            value = MpAvClipboardExtensions.ReadDataFormat<T>(key, dataObj);
            return value != null;
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