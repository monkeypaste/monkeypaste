using Avalonia.Input;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public static class MpAvInternalDataObjectExtensions {

        #region Editor/Json Helpers

        #endregion

        #region Converters

        public static MpAvDataObject ToPlatformDataObject(this IDataObject ido) {
            if (ido is MpAvDataObject ido_mpdo) {
                return ido_mpdo;
            }
            // will only be avalonia dataObject
            var avdo = new MpAvDataObject();
            if (ido == null) {
                return avdo;
            }
            ido.GetAllDataFormats().ForEach(x => avdo.Set(x, ido.Get(x)));
            return avdo;
        }

        public static void CreatePseudoFileEntry(this IDataObject ido) {
            if (ido.GetAllDataFormats().Contains(MpPortableDataFormats.AvFileNames)) {
                // file exists, ignore pseudo file
                return;
            }

        }
        #endregion

        #region Source Refs

        public static void AddOrCreateUri(this IDataObject ido, string uri) {
            if (string.IsNullOrWhiteSpace(uri)) {
                throw new Exception("Uri must be non-whitespace");
            }
            List<string> uri_list = null;
            if (ido.TryGetUriList(out List<string> uri_col)) {
                uri_list = uri_col.ToList();
            } else {
                uri_list = new List<string>();
            }

            if (uri_list.Any(x => x.ToLower() == uri.ToLower())) {
                // don't duplicate
                return;
            }
            uri_list.Add(uri);
            ido.Set(MpPortableDataFormats.INTERNAL_SOURCE_URI_LIST_FORMAT, uri_list.AsEnumerable<string>());
        }

        public static bool ContainsUris(this IDataObject ido) {
            if (ido == null) {
                return false;
            }
            return ido.Contains(MpPortableDataFormats.INTERNAL_SOURCE_URI_LIST_FORMAT);
        }
        public static bool TryGetUriList(this IDataObject ido, out List<string> uris) {
            uris = null;
            if (!ido.ContainsUris()) {
                return false;
            }
            var uri_list_obj = ido.Get(MpPortableDataFormats.INTERNAL_SOURCE_URI_LIST_FORMAT);

            if (uri_list_obj is IEnumerable<string> uri_coll) {
                uris = uri_coll.ToList();
            } else if (uri_list_obj is string uril_str) {
                if (uril_str.StartsWith("[") &&
                    MpJsonConverter.DeserializeObject<List<string>>(uril_str) is List<string> urilist) {
                    // uri's maybe a json array if from editor
                    uril_str = string.Join("\r\n", urilist);
                }
                uris = uril_str.SplitByLineBreak().ToList();
            }


            if (uris == null || !uris.Any()) {
                // what type were the uris? or why is the format available?
                MpDebug.Break("broken uri format detected");
                return false;
            }
            var bad_uris = uris.Where(x => !Uri.IsWellFormedUriString(x, UriKind.Absolute));
            if (bad_uris.Any()) {
                MpDebug.Break("improper uri format detected, check bad_uris");
                return false;
            }
            return true;
        }

        public static bool TryGetSourceRefIdBySourceType(this IDataObject ido, MpTransactionSourceType tst, out int source_ref_id) {
            source_ref_id = 0;
            if (ido.TryGetUriList(out List<string> uris) &&
                uris
                .Select(x => Mp.Services.SourceRefTools.ParseUriForSourceRef(x))
                .FirstOrDefault(x => x.Item1 == tst) is Tuple<MpTransactionSourceType, int> match) {
                source_ref_id = match.Item2;
            }
            return source_ref_id > 0;
        }

        #endregion

        #region Content

        public static void FinalizeContentOleTitle(this IDataObject ido, bool isFullContentReference, bool isCopy) {
            // title should be unaltered ci.title
            if (isFullContentReference && !isCopy) {
                // no changes to full non-copy item
                return;
            }
            string title = ido.Get(MpPortableDataFormats.INTERNAL_CONTENT_TITLE_FORMAT) as string;
            if (title == null) {
                return;
            }
            if (isCopy) {
                title += " (Copy)";
            }
            if (!isFullContentReference) {
                // give priority to fragment 
                // since c
                title += " [Part]";
            }
            ido.Set(MpPortableDataFormats.INTERNAL_CONTENT_TITLE_FORMAT, title);
        }
        public static void AddContentReferences(this IDataObject ido, MpCopyItem ci, bool isFullContentReference) {
            if (ci == null || ci.Id == 0) {
                return;
            }

            // always add copyItem uri
            ido.AddOrCreateUri(Mp.Services.SourceRefTools.ConvertToInternalUrl(ci));

            if (!isFullContentReference) {
                ido.Set(MpPortableDataFormats.INTERNAL_CONTENT_HANDLE_FORMAT, ci.PublicHandle);
            }
            ido.Set(MpPortableDataFormats.INTERNAL_CONTENT_TYPE_FORMAT, ci.ItemType.ToString());
            ido.Set(MpPortableDataFormats.INTERNAL_CONTENT_TITLE_FORMAT, ci.Title);
        }

        public static bool ContainsContentRef(this IDataObject ido) {
            if (ido.TryGetUriList(out List<string> uris)) {
                return uris.Any(x => Mp.Services.SourceRefTools.ParseUriForSourceRef(x).Item1 == MpTransactionSourceType.CopyItem);
            }
            return false;
        }

        public static bool ContainsPartialContentRef(this IDataObject ido) {
            // NOTE public handle is used for sub-selection OLE because
            // public handle is only available for active tiles which is only when
            // sub-selection would be available

            // NOTE2 sub-selection needs to be known so new item will be created
            // otherwise clone (for copy ole) or item in reference will be instantiated

            if (ido == null) {
                return false;
            }
            return ido.Contains(MpPortableDataFormats.INTERNAL_CONTENT_HANDLE_FORMAT);
        }

        public static async Task<MpCopyItem> ToCopyItemAsync(
            this IDataObject avdo,
            //bool addAsNewItem = false,
            bool is_copy = false) {
            var result = await Mp.Services.ContentBuilder.BuildFromDataObjectAsync(avdo, is_copy);
            return result;
        }


        #endregion

        #region Tags
        public static bool ContainsTagItem(this IDataObject ido) {
            if (ido == null) {
                return false;
            }
            return ido.Contains(MpPortableDataFormats.INTERNAL_TAG_ITEM_FORMAT);
        }
        public static bool TryGetDragTagViewModel(this IDataObject avdo, out MpAvTagTileViewModel ttvm) {
            //ttvm = null;
            //if (!avdo.ContainsTagItem()) {
            //    return false;
            //}
            ttvm = avdo.Get(MpPortableDataFormats.INTERNAL_TAG_ITEM_FORMAT) as MpAvTagTileViewModel;
            return ttvm != null;
        }

        #endregion

        #region Search Criteria
        public static bool ContainsSearchCriteria(this IDataObject ido) {
            if (ido == null) {
                return false;
            }
            return ido.Contains(MpPortableDataFormats.INTERNAL_SEARCH_CRITERIA_ITEM_FORMAT);
        }

        #endregion
    }
}
