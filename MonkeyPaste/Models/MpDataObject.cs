using Avalonia.Platform.Storage;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using SQLite;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpDataObject : MpDbModelBase {
        #region Statics
        static string[] _IgnoredFormatNames = new string[] {
            MpPortableDataFormats.INTERNAL_PROCESS_INFO_FORMAT,
            MpPortableDataFormats.INTERNAL_HTML_TO_RTF_FORMAT,
            MpPortableDataFormats.INTERNAL_RTF_TO_HTML_FORMAT,
            // TODO add more as needed when create breaks
        };
        #endregion

        #region Columns

        [Column("pk_MpDataObjectId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; } = 0;

        [Column("MpDataObjectGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        #endregion

        #region Properties 

        [Ignore]
        public Guid DataObjectGuid {
            get {
                if (string.IsNullOrEmpty(Guid)) {
                    return System.Guid.Empty;
                }
                return System.Guid.Parse(Guid);
            }
            set {
                Guid = value.ToString();
            }
        }

        #endregion

        public static async Task<MpDataObject> CreateAsync(
            string guid = "",
            int dataObjectId = 0,
            MpPortableDataObject pdo = null,
            bool suppressWrite = false) {

            var ndio = new MpDataObject() {
                DataObjectGuid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid() : System.Guid.Parse(guid),
                Id = dataObjectId
            };

            if (!suppressWrite) {
                await ndio.WriteToDatabaseAsync();
            }
            if (pdo == null) {
                return ndio;
            }

            foreach (var kvp in pdo.DataFormatLookup) {
                string itemDataStr;

                if (kvp.Value is byte[] bytes) {
                    // html, rtf or png

                    itemDataStr = bytes.ToBase64String();
                } else if (kvp.Value is IEnumerable<object> valObjs) {
                    // file list

                    if (kvp.Key != MpPortableDataFormats.Files) {
                        // this table is only used for searching so no other enumerable types are currently needed
                        continue;
                    }

                    List<string> valueParts = new List<string>();
                    foreach (var valObj in valObjs) {
                        if (valObj is string valStr) {
                            if (valStr.IsStringPathUri() && valStr.ToPathFromUri() is string uri_path) {
                                valStr = uri_path;
                            }
                            valueParts.Add(valStr);
                            continue;
                        }
                        if (valObj is IStorageItem si &&
                            si.TryGetLocalPath() is string path) {
                            valueParts.Add(path);
                            continue;
                        }
                        MpDebug.Break($"Unknown file list item type '{valObj.GetType()}'");

                    }
                    //if (!pdo.TryGetData(MpPortableDataFormats.AvFiles, out valueParts)) {
                    //    continue;
                    //}
                    // store file/path icon with path
                    // 1. both icon and path will become redundant but is unavoidable (impossible to uniformly know a path's icon and path ref needs to be associated with that clipboard object)
                    // 2. can change so maybe best when accessed on source device to 'create' again which does dup check
                    foreach (var fp in valueParts) {
                        int fp_icon_id = 0;
                        if (fp.IsFileOrDirectory()) {
                            var fp_icon_base64_str = Mp.Services.IconBuilder.GetPathIconBase64(fp);
                            var fp_icon = await Mp.Services.IconBuilder.CreateAsync(fp_icon_base64_str);
                            fp_icon_id = fp_icon.Id;
                        } else {
                            // what's wrong with the path string?
                            MpDebug.Break();
                        }
                        // store each file item separately
                        _ = await MpDataObjectItem.CreateAsync(
                                    dataObjectId: ndio.Id,
                                    itemFormat: kvp.Key,
                                    itemData: fp,
                                    itemIconId: fp_icon_id);

                    }
                    // files are handled individually...
                    continue;

                } else if (kvp.Value is string) {
                    // text

                    itemDataStr = kvp.Value.ToString();
                } else if (kvp.Value is int) {
                    // content id ref
                    itemDataStr = kvp.Value.ToString();
                } else {
                    MpDebug.Assert(
                        kvp.Key != MpPortableDataFormats.INTERNAL_SOURCE_URI_LIST_FORMAT,
                        $"DataObject error! URI List should ALWAYS be a collection but was '{kvp.Value}'");

                    if (_IgnoredFormatNames.Contains(kvp.Key)) {
                        // don't need to worry about storing this (paramValue type is list)
                        continue;
                    }
                    if (kvp.Key == MpPortableDataFormats.Files) {

                    }
                    if (kvp.Value is IEnumerable<object> ol) {
                        var test = ol.Select(x => x.ToString());

                    }
                    // what type is it?
                    MpDebug.Break();
                    itemDataStr = kvp.Value.ToString();
                }
                _ = await MpDataObjectItem.CreateAsync(
                    dataObjectId: ndio.Id,
                    itemFormat: kvp.Key,
                    itemData: itemDataStr);
            }
            return ndio;
        }

        public MpDataObject() { }

        public override async Task DeleteFromDatabaseAsync() {
            var delete_tasks = new List<Task>();

            var doil = await MpDataModelProvider.GetDataObjectItemsByDataObjectIdAsync(Id);
            delete_tasks.AddRange(doil.Select(x => x.DeleteFromDatabaseAsync()));

            delete_tasks.Add(base.DeleteFromDatabaseAsync());
            await Task.WhenAll(delete_tasks);
        }
    }
}
