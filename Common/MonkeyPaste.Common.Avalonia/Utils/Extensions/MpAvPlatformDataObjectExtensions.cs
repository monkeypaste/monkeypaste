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
            var availableFormats = ido_source.GetAllDataFormats();
            availableFormats.ForEach(x => cavdo.SetData(x, ido_source.GetAllowFiles(x)));
            return cavdo;
        }

        public static void CopyFrom(this IDataObject ido, IDataObject other_ido) {
            if (ido == null || other_ido == null) {
                return;
            }
            var format_diff = ido.GetAllDataFormats().Difference(other_ido.GetAllDataFormats());
            if (format_diff.Count() > 0 && ido is DataObject) {
                // NOTE can't remove from sys dataobject
                Debugger.Break();
            }
            if (ido is DataObject sysdo) {
                other_ido.GetAllDataFormats().ForEach(x => sysdo.Set(x, other_ido.GetAllowFiles(x)));
            } else if (ido is MpPortableDataObject mpdo) {
                mpdo.DataFormatLookup.Clear();
                other_ido.GetAllDataFormats().ForEach(x => mpdo.SetData(x, other_ido.Get(x)));
            }
        }

        public static bool TryRemove(this IDataObject ido, string format) {
            if (ido == null) {
                return false;
            }
            if (ido is DataObject sysdo) {
                // probably exception
                sysdo.Set(format, null);
                return true;
            } else if (ido is MpPortableDataObject mpdo &&
                MpPortableDataFormats.GetDataFormat(format) is MpPortableDataFormat pdf) {
                mpdo.DataFormatLookup.Remove(pdf);
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
        public static bool TryClear(this IDataObject ido) {
            if (ido == null) {
                return false;
            }
            bool result = true;
            foreach (var format in ido.GetAllDataFormats()) {
                if (!ido.TryRemove(format)) {
                    result = false;
                }
            }
            return result;
        }

    }
}
