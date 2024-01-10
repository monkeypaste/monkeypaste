using Avalonia.Input;
using Avalonia.Platform.Storage;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public static class MpAvInternalDataObjectExtensions {

        #region Editor/Json Helpers

        #endregion

        #region Data Object
        public static bool IsDataNotEqual(this MpPortableDataObject dbo1, MpPortableDataObject dbo2, bool fast_check = false) {
            if (dbo1 == null && dbo2 != null) {
                return true;
            }
            if (dbo1 != null && dbo2 == null) {
                return true;
            }
            if (dbo1.DataFormatLookup.Count != dbo2.DataFormatLookup.Count) {
                return true;
            }
            foreach (var nce in dbo2.DataFormatLookup) {
                try {
                    if (!dbo1.DataFormatLookup.ContainsKey(nce.Key)) {
                        return true;
                    }
                    if (nce.Value is byte[] newBytes &&
                        dbo1.DataFormatLookup[nce.Key] is byte[] oldBytes) {
                        // compare byte arrays
                        if (fast_check) {
                            // NOTE big byte arrays (like images) will make this really slow
                            if (newBytes.Length != oldBytes.Length) {
                                return true;
                            }
                        } else if (!newBytes.SequenceEqual(oldBytes)) {
                            return true;
                        }
                    } else if (nce.Value is IEnumerable<object> ol &&
                                dbo1.DataFormatLookup[nce.Key] is IEnumerable<object> last_ol) {
                        // compare lists
                        if (ol.Count() != last_ol.Count()) {
                            return true;
                        }
                        if (ol is IEnumerable<string> strl &&
                            last_ol is IEnumerable<string> last_strl) {
                            // compare string lists
                            return strl.Any(x => !last_strl.Contains(x));
                        } else if (ol is IEnumerable<IStorageItem> stil &&
                                    stil.Where(x => x.Path != null).Select(x => x.Path) is IEnumerable<Uri> uril &&
                                    last_ol is IEnumerable<IStorageItem> last_stil &&
                                    last_stil.Where(x => x.Path != null).Select(x => x.Path) is IEnumerable<Uri> last_uril) {
                            // compare IStorageItem lists using non-null uri
                            if (uril.Count() != last_uril.Count()) {
                                return true;
                            }
                            return uril.Any(x => !last_uril.Contains(x));
                        } else {
                            MpDebug.Break($"No list comparision found for format '{nce.Key}'");
                        }
                    }
                    // NOTE below maybe along lines of a better fast check for text but ommitting cause more likely to get wrong output
                    //else if (fast_check && nce.Value is string new_str && dbo1.DataFormatLookup[nce.Key] is string old_str) {
                    //    if(new_str.Length != old_str.Length) {
                    //        return true;
                    //    }
                    //} 
                    else {
                        if (!dbo1.DataFormatLookup[nce.Key].Equals(nce.Value)) {
                            return true;
                        }
                    }
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine("Error comparing clipbaord data. ", ex);
                }


            }
            return false;
        }

        public static IDataObject ToDataObject(this object idoObj) {
            IDataObject ido = idoObj as IDataObject;
            if (ido == null && idoObj is MpPortableDataObject mpdo) {
                ido = mpdo.ToAvDataObject();
            }
            return ido;
        }
        public static MpAvDataObject ToAvDataObject(this MpPortableDataObject mpdo) {
            if (mpdo is MpAvDataObject avdo) {
                return avdo;
            }
            return new MpAvDataObject(mpdo);
        }
        public static MpAvDataObject ToPlatformDataObject(this IDataObject ido) {
            if (ido is MpAvDataObject ido_mpdo) {
                return ido_mpdo;
            }
            // will only be avalonia dataObjectLookup
            var avdo = new MpAvDataObject();
            if (ido == null) {
                return avdo;
            }
            ido.GetAllDataFormats().ForEach(x => avdo.Set(x, ido.Get(x)));
            return avdo;
        }

        public static void CreatePseudoFileEntry(this IDataObject ido) {
            if (ido.GetAllDataFormats().Contains(MpPortableDataFormats.Files)) {
                // file exists, ignore pseudo file
                return;
            }

        }
        #endregion

        #region Source Refs

        public static void AddOrUpdateUri(this IDataObject ido, string uri) {
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
                    MpJsonExtensions.DeserializeObject<List<string>>(uril_str) is List<string> urilist) {
                    // uri's maybe a json array if from editor
                    uril_str = string.Join(Environment.NewLine, urilist);
                }
                uris = uril_str.SplitByLineBreak().ToList();
            }


            if (uris == null || !uris.Any()) {
                // what type were the uris? or why is the format available?
                //MpDebug.Break("broken uri format detected");
                return false;
            }
            var bad_uris = uris.Where(x => !Uri.IsWellFormedUriString(x, UriKind.Absolute));
            if (bad_uris.Any()) {
                //MpDebug.Break("improper uri format detected, check bad_uris");
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
            var sub_titles = new List<string>();
            if (isCopy) {
                sub_titles.Add("Copy");
            }
            if (!isFullContentReference) {
                sub_titles.Add("Part");
            }
            if (sub_titles.Any()) {
                title += $"-{string.Join("-", sub_titles)}";
            }
            ido.Set(MpPortableDataFormats.INTERNAL_CONTENT_TITLE_FORMAT, title);
        }
        public static void AddContentReferences(this IDataObject ido, MpCopyItem ci, bool isFullContentReference) {
            if (ci == null || ci.Id == 0) {
                return;
            }

            // always add copyItem uri
            string ci_uri = Mp.Services.SourceRefTools.ConvertToInternalUrl(ci);
            ido.AddOrUpdateUri(ci_uri);


            if (!isFullContentReference) {
                ido.Set(MpPortableDataFormats.INTERNAL_PARTIAL_CONTENT_VIEW_HANDLE_FORMAT, ci.PublicHandle);
            }
            ido.Set(MpPortableDataFormats.INTERNAL_CONTENT_ID_FORMAT, ci.Id);
            ido.Set(MpPortableDataFormats.INTERNAL_CONTENT_TYPE_FORMAT, ci.ItemType.ToString());
            ido.Set(MpPortableDataFormats.INTERNAL_CONTENT_TITLE_FORMAT, ci.Title);

            if (!ido.Contains(MpPortableDataFormats.CefAsciiUrl)) {
                // NOTE cefnet preprocesses drop formats so only cef supported formats get passed to editor
                // so to avoid timing issues (tracking drag item and adding it as drop source on drop but drag will be done then most likely)
                // supply content ref as cef uri bytes
                ido.Set(MpPortableDataFormats.CefAsciiUrl, ci_uri.ToBytesFromString());
            }
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
            return ido.Contains(MpPortableDataFormats.INTERNAL_PARTIAL_CONTENT_VIEW_HANDLE_FORMAT);
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
