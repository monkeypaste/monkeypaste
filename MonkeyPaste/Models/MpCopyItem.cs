using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpCopyItem :
        MpDbModelBase,
        MpISyncableDbObject,
        MpIClonableDbModel<MpCopyItem>,
        MpISourceRef,
        MpIIconResource,
        MpIDbIconId,
        MpILabelText {
        #region Statics

        public static string FileItemSplitter {
            get {
                // TODO when from another device get source device to know env new line (add new line string to OsInfo)
                return Environment.NewLine;
            }
        }

        #endregion

        #region Column Definitions

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpCopyItemId")]
        public override int Id { get; set; } = 0;

        [Column("MpCopyItemGuid")]
        [Indexed]
        public new string Guid { get => base.Guid; set => base.Guid = value; }


        //[Column("fk_MpSourceId")]
        public int SourceId { get; set; }

        public string Title { get; set; } = string.Empty;

        [Column("e_MpCopyItemType")]
        public string ItemTypeName { get; set; } = MpCopyItemType.None.ToString();

        public DateTime CopyDateTime { get; set; }

        [Indexed]
        public string ItemData { get; set; } = string.Empty;

        [Column("fk_MpIconId")]
        public int IconId { get; set; }

        [Column("fk_MpDataObjectId")]
        public int DataObjectId { get; set; }

        public int CopyCount { get; set; } = 0;

        public int PasteCount { get; set; } = 0;

        [Column("HexColor")]
        public string ItemColor { get; set; } = string.Empty;

        [Column("DataFormat")]
        public string DataFormat { get; set; } = string.Empty;

        public string ItemMetaData { get; set; }

        // Text: 1/2 = chars/lines
        // Image: 1/2 = width/height
        // Files: 1/2 = bytes/count
        public int ItemSize1 { get; set; }
        public int ItemSize2 { get; set; }
        #endregion

        #region Properties

        [Ignore]
        public Guid CopyItemGuid {
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
        [Ignore]
        public MpCopyItemType ItemType {
            get {
                return ItemTypeName.ToEnum<MpCopyItemType>();
            }
            set {
                ItemTypeName = value.ToString();
            }
        }

        [Ignore]
        public string ItemRefUrl => Mp.Services.SourceRefTools.ConvertToRefUrl(this);

        #endregion

        #region Interfaces

        #region MpILabelText Implementation
        string MpILabelText.LabelText => Title;

        #endregion

        #region MpISourceRef Implementation
        [Ignore]
        int MpISourceRef.Priority => 1;
        [Ignore]
        int MpISourceRef.SourceObjId => Id;

        [Ignore]
        MpTransactionSourceType MpISourceRef.SourceType => MpTransactionSourceType.CopyItem;


        public object IconResourceObj => IconId;
        public string Uri => Mp.Services.SourceRefTools.ConvertToRefUrl(this);
        #endregion

        #region  Implementation

        public async Task<MpCopyItem> CloneDbModelAsync(
            bool deepClone = true,
            bool suppressWrite = false) {
            // NOTE parent are ignored for this model

            var newItem = new MpCopyItem() {
                ItemType = this.ItemType,
                Title = this.Title,
                ItemData = this.ItemData,
                IconId = this.IconId,
                //SourceId = this.SourceId,
                CopyCount = 1,
                CopyDateTime = DateTime.Now,
                Id = 0,
                CopyItemGuid = System.Guid.NewGuid()
            };

            if (deepClone) {
                await newItem.WriteToDatabaseAsync();

                var tags = await MpDataModelProvider.GetCopyItemTagsForCopyItemAsync(this.Id);
                foreach (var tag in tags) {
                    await MpCopyItemTag.CreateAsync(
                        tagId: tag.Id,
                        copyItemId: newItem.Id);
                }

                //var templates = await MpDataModelProvider.GetTextTemplatesAsync(this.Id);
                //foreach (var template in templates) {
                //    var templateClone = await template.CloneDbModel();
                //    templateClone.CopyItemId = newItem.Id;
                //    await templateClone.WriteToDatabaseAsync();
                //}
            }

            return newItem;
        }
        #endregion

        #endregion

        #region Static Methods
        public static async Task<MpCopyItem> CreateAsync(
            string data = "",
            string dataFormat = MpPortableDataFormats.Text,
            MpCopyItemType itemType = MpCopyItemType.Text,
            string title = "",
            int dataObjectId = 0,
            bool suppressWrite = false) {
            MpCopyItem dupCheck = null;
            if (MpPrefViewModel.Instance.IsDuplicateCheckEnabled && !suppressWrite) {
                dupCheck = await MpDataModelProvider.GetCopyItemByDataAsync(data);
                if (dupCheck != null) {
                    dupCheck.WasDupOnCreate = true;
                    return dupCheck;
                }
            }


            if (MpPrefViewModel.Instance.UniqueContentItemIdx == 0 && !suppressWrite) {
                MpPrefViewModel.Instance.UniqueContentItemIdx = await MpDataModelProvider.GetTotalCopyItemCountAsync();
            }


            var newCopyItem = new MpCopyItem() {
                CopyItemGuid = System.Guid.NewGuid(),
                CopyDateTime = DateTime.Now,
                Title = title,
                ItemData = data,
                ItemType = itemType,
                DataFormat = dataFormat,
                CopyCount = 1,
                DataObjectId = dataObjectId
            };
            if (!suppressWrite) {
                await newCopyItem.WriteToDatabaseAsync();
            }
            return newCopyItem;
        }

        #endregion

        [Ignore]
        public int ManualSortIdx { get; set; }

        [Ignore]
        public bool IgnoreDb { get; set; } = false;

        public MpCopyItem() : base() { }

        public override string ToString() {
            return $"{Title} Id:{Id}";
        }

        public async Task UpdateDataObject() {
            string searchable_text = null;
            switch (ItemType) {
                case MpCopyItemType.FileList:
                    // TODO could use parsing analytics here to get content/meta information
                    searchable_text = ItemData;
                    break;
                case MpCopyItemType.Image:
                    // TODO could use computer vision analytics here to get content/meta information

                    break;
                case MpCopyItemType.Text:
                    searchable_text = ItemData.ToPlainText("html");
                    break;
            }
            if (string.IsNullOrWhiteSpace(searchable_text)) {
                // ignore empty
                return;
            }

            var doil = await MpDataModelProvider.GetDataObjectItemsForFormatByDataObjectId(DataObjectId, MpPortableDataFormats.Text);
            if (doil.Count == 0) {
                doil.Add(new MpDataObjectItem() {
                    DataObjectId = DataObjectId,
                    ItemFormat = MpPortableDataFormats.Text
                });
            }
            if (doil.Count > 1) {
                // (currently) there should only be 1 entry
                Debugger.Break();
            }
            doil[0].ItemData = searchable_text;
            await doil[0].WriteToDatabaseAsync();
        }

        public override async Task WriteToDatabaseAsync() {
            if (IgnoreDb) {
                MpConsole.WriteLine($"Db write for '{ToString()}' was ignored");
                return;
            }

            if (ItemData.IsEmptyRichHtmlString()) {
                // what IS this nasty shit??
                Debugger.Break();
                return;
            }
            await base.WriteToDatabaseAsync();


            _ = Task.Run(UpdateDataObject);
        }

        public override async Task DeleteFromDatabaseAsync() {
            //
            if (IgnoreDb) {
                MpConsole.WriteLine($"Db delete for '{ToString()}' was ignored");
                return;
            }

            // NOTE shortcut collection handles deleting shortcuts

            var delete_tasks = new List<Task>();

            var citl = await MpDataModelProvider.GetCopyItemTagsForCopyItemAsync(Id);
            delete_tasks.AddRange(citl.Select(x => x.DeleteFromDatabaseAsync()));

            var cisl = await MpDataModelProvider.GetCopyItemSources(Id);
            delete_tasks.AddRange(cisl.Select(x => x.DeleteFromDatabaseAsync()));

            var doil = await MpDataModelProvider.GetDataObjectItemsByDataObjectId(DataObjectId);
            delete_tasks.AddRange(doil.Select(x => x.DeleteFromDatabaseAsync()));

            var dobj = await MpDataModelProvider.GetItemAsync<MpDataObject>(DataObjectId);
            delete_tasks.Add(dobj.DeleteFromDatabaseAsync());

            var do_model = await MpDataModelProvider.GetItemAsync<MpDataObject>(DataObjectId);
            if (do_model != null) {
                delete_tasks.Add(do_model.DeleteFromDatabaseAsync());
            }

            var citrl = await MpDataModelProvider.GetCopyItemTransactionsByCopyItemIdAsync(Id);
            if (citrl != null && citrl.Count > 0) {
                delete_tasks.AddRange(citrl.Select(x => x.DeleteFromDatabaseAsync()));
            }

            if (IconId > 0) {
                var ci_icon = await MpDataModelProvider.GetItemAsync<MpIcon>(IconId);
                if (ci_icon != null) {
                    delete_tasks.Add(ci_icon.DeleteFromDatabaseAsync());
                }
            }
            delete_tasks.Add(base.DeleteFromDatabaseAsync());

            await Task.WhenAll(delete_tasks);
        }

        #region Sync

        public async Task<object> DeserializeDbObjectAsync(string objStr) {
            var objParts = objStr.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);
            await Task.Delay(0);

            var ci = new MpCopyItem() {
                CopyItemGuid = System.Guid.Parse(objParts[0]),
                Title = objParts[1],
                CopyCount = Convert.ToInt32(objParts[2]),
                CopyDateTime = DateTime.Parse(objParts[3]),
                ItemData = objParts[4],
                ItemType = (MpCopyItemType)Convert.ToInt32(objParts[5]),
                //SourceId = source == null ? MpPrefViewModel.Instance.ThisAppSourceId : source.Id
            };
            //ci.Source = await MpDb.GetDbObjectByTableGuidAsync("MpSource", objParts[7]) as MpSource;
            //TODO deserialize this once img and files added
            //ci.ItemType = MpCopyItemType.RichText;
            return ci;
        }

        public async Task<string> SerializeDbObjectAsync() {
            await Task.Delay(1);

            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}",
                ParseToken,
                CopyItemGuid.ToString(),
                Title,
                CopyCount,
                CopyDateTime.ToString(),
                ItemData,
                ItemType.ToString()
                //MpDataModelProvider.GetItem<MpSource>(SourceId).Guid//Source.SourceGuid.ToString()
                );
        }

        public Type GetDbObjectType() {
            return typeof(MpCopyItem);
        }

        public async Task<Dictionary<string, string>> DbDiffAsync(object drOrModel) {
            await Task.Delay(1);

            MpCopyItem other = null;
            if (drOrModel == null) {
                other = new MpCopyItem();
            } else if (drOrModel is MpCopyItem) {
                other = drOrModel as MpCopyItem;
            } else {
                throw new Exception("Cannot compare xam model to local model");
            }
            //returns db column name and string value of dr that is diff
            var diffLookup = new Dictionary<string, string>();
            diffLookup = CheckValue(
                CopyItemGuid,
                other.CopyItemGuid,
                "MpCopyItemGuid",
                diffLookup);
            diffLookup = CheckValue(
                Title,
                other.Title,
                "Title",
                diffLookup);
            diffLookup = CheckValue(
                CopyCount,
                other.CopyCount,
                "CopyCount",
                diffLookup);
            diffLookup = CheckValue(
                CopyDateTime,
                other.CopyDateTime,
                "CopyDateTime",
                diffLookup);
            diffLookup = CheckValue(
                ItemData,
                other.ItemData,
                "ItemData",
                diffLookup);
            //diffLookup = CheckValue(
            //    SourceId,
            //    other.SourceId,
            //    "fk_MpSourceId",
            //    diffLookup,
            //    MpDataModelProvider.GetItem<MpSource>(SourceId).Guid);
            diffLookup = CheckValue(
                ItemType,
                other.ItemType,
                "fk_MpCopyItemTypeId",
                diffLookup,
                ((int)ItemType).ToString());

            return diffLookup;
        }

        public async Task<object> CreateFromLogsAsync(string dboGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
            var cidr = await MpDb.GetDbObjectByTableGuidAsync("MpCopyItem", CopyItemGuid.ToString());
            MpCopyItem newCopyItem = null;
            if (cidr == null) {
                newCopyItem = new MpCopyItem();
            } else {
                newCopyItem = cidr as MpCopyItem;
            }
            foreach (var li in logs) {
                switch (li.AffectedColumnName) {
                    case "MpCopyItemGuid":
                        newCopyItem.CopyItemGuid = System.Guid.Parse(li.AffectedColumnValue);
                        break;
                    case "Title":
                        newCopyItem.Title = li.AffectedColumnValue;
                        break;
                    case "CopyCount":
                        newCopyItem.CopyCount = Convert.ToInt32(li.AffectedColumnValue);
                        break;
                    case "CopyDateTime":
                        newCopyItem.CopyDateTime = DateTime.Parse(li.AffectedColumnValue);
                        break;
                    case "ItemData":
                        newCopyItem.ItemData = li.AffectedColumnValue;
                        break;
                    //case "fk_MpSourceId":
                    //    var source = await MpDataModelProvider.GetSourceByGuidAsync(li.AffectedColumnValue);
                    //    if(source != null) {
                    //        source = await MpDataModelProvider.GetItemAsync<MpSource>(source.Id);
                    //    }
                    //    newCopyItem.SourceId = Convert.ToInt32(source.Id);
                    //    break;
                    case "fk_MpCopyItemTypeId":
                        newCopyItem.ItemType = (MpCopyItemType)Convert.ToInt32(li.AffectedColumnValue);
                        break;
                    default:
                        MpConsole.WriteTraceLine(@"Unknown table-column: " + li.DbTableName + "-" + li.AffectedColumnName);
                        break;
                }
            }
            return newCopyItem;
        }

        #endregion


    }

}
