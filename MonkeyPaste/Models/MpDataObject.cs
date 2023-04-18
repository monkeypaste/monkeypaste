using MonkeyPaste.Common;
using SQLite;


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpDataObject : MpDbModelBase {
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
                } else if (kvp.Value is IEnumerable<string> valueParts) {
                    // file list

                    if (kvp.Key.Name != MpPortableDataFormats.AvFileNames) {
                        // this table is only used for searching so no other enumerable types are currently needed
                        continue;
                    }
                    // store file/path icon with path
                    // 1. both icon and path will become redundant but is unavoidable (impossible to uniformly know a path's icon and path ref needs to be associated with that clipboard object)
                    // 2. can change so maybe best when accessed on source device to 'create' again which does dup check
                    foreach (var fp in valueParts) {
                        int fp_icon_id = 0;
                        if (fp.IsFileOrDirectory()) {
                            var fp_icon_base64_str = Mp.Services.IconBuilder.GetApplicationIconBase64(fp);
                            var fp_icon = await Mp.Services.IconBuilder.CreateAsync(fp_icon_base64_str);
                            fp_icon_id = fp_icon.Id;
                        } else {
                            // what's wrong with the path string?
                            Debugger.Break();
                        }
                        // store each file item separately
                        _ = await MpDataObjectItem.CreateAsync(
                                    dataObjectId: ndio.Id,
                                    itemFormat: kvp.Key.Name,
                                    itemData: fp,
                                    itemIconId: fp_icon_id);

                    }
                    // files are handled individually...
                    continue;

                } else if (kvp.Value is string) {
                    // text

                    itemDataStr = kvp.Value.ToString();
                } else {
                    if (kvp.Key.Name == MpPortableDataFormats.INTERNAL_SOURCE_URI_LIST_FORMAT) {
                        // don't need to worry about storing this (value type is list)
                        continue;
                    }
                    // what type is it?
                    Debugger.Break();
                    itemDataStr = kvp.Value.ToString();
                }
                _ = await MpDataObjectItem.CreateAsync(
                    dataObjectId: ndio.Id,
                    itemFormat: kvp.Key.Name,
                    itemData: itemDataStr);
            }
            return ndio;
        }

        public MpDataObject() { }

        public override async Task DeleteFromDatabaseAsync() {
            var delete_tasks = new List<Task>();

            var doil = await MpDataModelProvider.GetDataObjectItemsByDataObjectIdAsync(Id);
            if (doil != null) {

                delete_tasks.AddRange(doil.Select(x => x.DeleteFromDatabaseAsync()));
            }
            delete_tasks.Add(base.DeleteFromDatabaseAsync());
            await Task.WhenAll(delete_tasks);
        }
    }
}
