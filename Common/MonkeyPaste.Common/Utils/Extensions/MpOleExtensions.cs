using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MonkeyPaste.Common {
    [Flags]
    public enum MpOleOperationFlags {
        None = 0,
        Cut = 1,
        Copy = 2,
        Paste = 4,
    }
    public static class MpOleExtensions {
        public static void AddOrCreateUri(this MpPortableDataObject mpdo, string uri) {
            if(string.IsNullOrWhiteSpace(uri)) {
                throw new Exception("Uri must be non-whitespace");
            }
            List<string> uri_list = null;
            if(mpdo.GetUriList() is IEnumerable<string> uri_col) {
                uri_list = uri_col.ToList();
            } else {
                uri_list = new List<string>();
            }

            if (uri_list.Any(x=>x.ToLower() == uri.ToLower())) {
                // don't duplicate
                return;
            }
            uri_list.Add(uri);
            mpdo.SetData(MpPortableDataFormats.INTERNAL_SOURCE_URI_LIST_FORMAT, uri_list.AsEnumerable<string>());
        }
        
        public static IEnumerable<string> GetUriList(this MpPortableDataObject mpdo) {

            if (mpdo.TryGetData(MpPortableDataFormats.INTERNAL_SOURCE_URI_LIST_FORMAT, out object uri_list_obj)) {
                if (uri_list_obj is IEnumerable<string> uri_coll) {
                    return uri_coll.ToList();
                } else if (uri_list_obj is string uril_str) {
                    if (uril_str.StartsWith("[") && MpJsonConverter.DeserializeObject<List<string>>(uril_str) is List<string> urilist) {
                        uril_str = string.Join("\r\n", urilist);
                    }
                    return uril_str.SplitByLineBreak().ToList();
                } else {
                    // what type is the uris?
                    Debugger.Break();
                }
            }
            return null;
        }


    }
}
