using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpCopyItem :
        MpDbModelBase,
        MpISyncableDbObject,
        MpIIsValueEqual<MpCopyItem>,
        MpISourceRef,
        MpILabelText {
        #region Constants
        public const bool IS_EMPTY_HTML_CHECK_ENABLED = false;

        public const double ZOOM_FACTOR_MIN = 0.25d;
        public const double ZOOM_FACTOR_MAX = 3.0d;
        public const double ZOOM_FACTOR_DEFAULT = 1.0d;
        public const double ZOOM_FACTOR_STEP = 0.1d;

        #endregion

        #region Statics

        public static string FileItemSplitter {
            get {
                // TODO when from another device get source device to know env new line (add new line string to OsInfo)
                return Environment.NewLine;
            }
        }

        public static string GetContentCheckSum(string content) {
            // NOTE unedited content has env line breaks for text format
            // webview always returns plain text w/ \n line breaks which will break the checksum
            return content.StripLineBreaks().CheckSum();
        }
        #endregion

        #region Interfaces

        #region MpIIsFuzzyValueEqual Implementation

        public bool IsValueEqual(MpCopyItem other) {
            if (other == null) {
                return false;
            }
            return ContentCheckSum == other.ContentCheckSum;
        }

        #endregion

        #region MpILabelText Implementation
        string MpILabelText.LabelText => Title;

        #endregion

        #region MpISourceRef Implementation
        [Ignore]
        int MpISourceRef.Priority => (int)MpTransactionSourceType.CopyItem;
        [Ignore]
        int MpISourceRef.SourceObjId => Id;

        [Ignore]
        MpTransactionSourceType MpISourceRef.SourceType => MpTransactionSourceType.CopyItem;


        public object IconResourceObj =>
            IconId;
        public string Uri => Mp.Services.SourceRefTools.ConvertToInternalUrl(this);
        #endregion

        #endregion

        #region Column Definitions

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpCopyItemId")]
        public override int Id { get; set; } = 0;

        [Column("MpCopyItemGuid")]
        [Indexed]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        public string Title { get; set; } = string.Empty;

        [Indexed]
        [Column("e_MpCopyItemType")]
        public string ItemTypeName { get; set; } = MpCopyItemType.None.ToString();

        [Indexed]
        public DateTime CopyDateTime { get; set; }

        [Indexed]
        public DateTime LastCapRelatedDateTime { get; set; }

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

        public string ItemMetaData { get; set; } = string.Empty;

        // Text: 1/2 = chars/lines
        // Image: 1/2 = width/height
        // Files: 1/2 = bytes/count
        public int ItemSize1 { get; set; }
        public int ItemSize2 { get; set; }

        [Column("e_MpDataObjectSourceType")]
        public string ItemSourceTypeStr { get; set; } = MpDataObjectSourceType.None.ToString();

        public double ZoomFactor { get; set; } = ZOOM_FACTOR_DEFAULT;

        [Indexed]
        public string ContentCheckSum { get; set; }
        #endregion

        #region Properties
        [Ignore]
        public MpDataObjectSourceType DataObjectSourceType {
            get => ItemSourceTypeStr.ToEnum<MpDataObjectSourceType>();
            set => ItemSourceTypeStr = value.ToString();
        }

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
        public string ItemRefUrl => Mp.Services.SourceRefTools.ConvertToInternalUrl(this);

        [Ignore]
        public bool IgnoreDb { get; set; } = false;


        #endregion

        #region Static Methods
        public static async Task<MpCopyItem> CreateAsync(
            string guid = "",
            string data = "",
            MpCopyItemType itemType = MpCopyItemType.Text,
            string title = "",
            int dataObjectId = 0,
            int iconId = 0,
            double zoomFactor = ZOOM_FACTOR_DEFAULT,
            string checksum = default,
            MpDataObjectSourceType dataObjectSourceType = MpDataObjectSourceType.None,
            bool suppressWrite = false) {

            if (dataObjectId <= 0 && !suppressWrite) {
                throw new Exception($"Should have dataObjectId. param was {dataObjectId}");
            }
            if (dataObjectSourceType == MpDataObjectSourceType.None) {
                throw new Exception("Should have data object source type");
            }
            if (checksum == default) {
                throw new Exception("Item must have checksum");
            }

            var create_dt = DateTime.Now;
            var newCopyItem = new MpCopyItem() {
                CopyItemGuid = guid.IsNullOrEmpty() ? System.Guid.NewGuid() : System.Guid.Parse(guid),
                CopyDateTime = create_dt,
                LastCapRelatedDateTime = create_dt,
                Title = title,
                ItemData = data,
                ItemType = itemType,
                CopyCount = 1,
                ZoomFactor = zoomFactor,
                DataObjectId = dataObjectId,
                IconId = iconId,
                ContentCheckSum = checksum,
                DataObjectSourceType = dataObjectSourceType
            };
            if (!suppressWrite) {
                await newCopyItem.WriteToDatabaseAsync(true);
                if (newCopyItem.Id == 0) {
                    // didn't write, must be empty data
                    return null;
                }
            }
            return newCopyItem;
        }

        #endregion

        #region Constructors
        public MpCopyItem() : base() { }
        #endregion

        #region Public Methods

        public async Task WriteToDatabaseAsync(bool isItemDataChanged, string searchAndChecksumText) {
            if (isItemDataChanged) {
                // NOTE this is a workaround for initial loading and edge cases when search text is missing,
                // just fallback and use slower method
                if (ItemType == MpCopyItemType.Text) {
                    if (string.IsNullOrWhiteSpace(searchAndChecksumText)) {
                        searchAndChecksumText = ItemData.ToPlainText();
                    }
                } else {
                    searchAndChecksumText = ItemData;
                }
                MpDebug.Assert(searchAndChecksumText != null, $"Content change write w/o search text specified");
            }
            await WriteToDb_internal(searchAndChecksumText);
        }
        public override async Task WriteToDatabaseAsync() {
            await WriteToDb_internal(null);
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

            var cisl = await MpDataModelProvider.GetCopyItemSourcesAsync(Id);
            delete_tasks.AddRange(cisl.Select(x => x.DeleteFromDatabaseAsync()));


            var do_model = await MpDataModelProvider.GetItemAsync<MpDataObject>(DataObjectId);
            if (do_model != null) {
                delete_tasks.Add(do_model.DeleteFromDatabaseAsync());
            }

            var citrl = await MpDataModelProvider.GetCopyItemTransactionsByCopyItemIdAsync(Id);
            if (citrl != null && citrl.Count > 0) {
                delete_tasks.AddRange(citrl.Select(x => x.DeleteFromDatabaseAsync()));
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
            //returns db column name and string paramValue of dr that is diff
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
        public override string ToString() {
            return $"{Title} Id:{Id}";
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private async Task WriteToDb_internal(string searchAndChecksumText) {
            bool isContentChangeWrite = searchAndChecksumText != null;
            if (IgnoreDb) {
                MpConsole.WriteLine($"Db write for '{ToString()}' was ignored");
                return;
            }

            if (ItemData.IsNullOrWhitespaceHtmlString()) {
                // what IS this nasty shit??
                MpDebug.Break($"Empty html write detected for item {this}", !MpCopyItem.IS_EMPTY_HTML_CHECK_ENABLED);
                return;
            }
            MpDebug.Assert(IconId != MpDefaultDataModelTools.ThisAppIconId, $"This should be unknown icon id", true);
            if (searchAndChecksumText != null) {
                UpdateContentCheckSum(searchAndChecksumText);
            }
            await base.WriteToDatabaseAsync();

            if (searchAndChecksumText != null) {
                if (ItemType == MpCopyItemType.FileList) {
                    // need to wait for files i think
                    await UpdateDataObjectAsync(searchAndChecksumText);
                    return;
                }
                _ = Task.Run(() => UpdateDataObjectAsync(searchAndChecksumText));
            }
        }
        private async Task UpdateDataObjectAsync(string searchAndChecksumText) {
            switch (ItemType) {
                case MpCopyItemType.Image:
                    // TODO could use computer vision analytics here to get content/meta information
                    //if (isContentChangeWrite) {
                    // UpdateContentCheckSum(ItemData);
                    //}
                    return;
                case MpCopyItemType.FileList:
                    //if (!isContentChangeWrite) {
                    // only itemData updates affect dataObject
                    // break;
                    //}

                    // update file plain text (fire and foreget, not needed later)
                    var fl_texts = await MpDataModelProvider.GetDataObjectItemsForFormatByDataObjectIdAsync(DataObjectId, MpPortableDataFormats.Text);
                    MpDebug.Assert(fl_texts.Count <= 1, $"There should only be 1 text entry for file dataObj, there is {fl_texts.Count}");

                    MpDataObjectItem doi_fl_text = fl_texts.FirstOrDefault();
                    if (doi_fl_text == null) {
                        doi_fl_text = new MpDataObjectItem() {
                            DataObjectId = DataObjectId,
                            ItemFormat = MpPortableDataFormats.Text
                        };
                    }
                    doi_fl_text.ItemData = ItemData;
                    doi_fl_text.WriteToDatabaseAsync().FireAndForgetSafeAsync();


                    // Current Files is ItemData split by new line

                    var fpl = ItemData.SplitNoEmpty(MpCopyItem.FileItemSplitter);

                    // get current file list entries
                    var doi_fpl = await MpDataModelProvider.GetDataObjectItemsForFormatByDataObjectIdAsync(DataObjectId, MpPortableDataFormats.Files);

                    // to avoid duplicate entry issues just delete all files and recreate from item data
                    await Task.WhenAll(doi_fpl.Select(x => x.DeleteFromDatabaseAsync()));

                    await Task.WhenAll(fpl.Select(x => MpDataObjectItem.CreateAsync(
                                    dataObjectId: DataObjectId,
                                    itemFormat: MpPortableDataFormats.Files,
                                    itemData: x)));
                    return;
                case MpCopyItemType.Text:
                    string searchable_text = searchAndChecksumText;//ItemData.ToPlainText("html");
                    var doil = await MpDataModelProvider.GetDataObjectItemsForFormatByDataObjectIdAsync(DataObjectId, MpPortableDataFormats.Text);
                    if (doil.Count == 0) {
                        doil.Add(new MpDataObjectItem() {
                            DataObjectId = DataObjectId,
                            ItemFormat = MpPortableDataFormats.Text
                        });
                    }
                    MpDebug.Assert(doil.Count == 1, $"There should only be 1 text entry for dataobj but there are {doil.Count}");
                    //if (isContentChangeWrite) {
                    //    UpdateContentCheckSum(searchable_text);
                    //}
                    doil[0].ItemData = searchable_text;
                    await doil[0].WriteToDatabaseAsync();
                    return;
            }
        }

        private void UpdateContentCheckSum(string checkSumSource) {
            //_ = Task.Run(async () => {
            // NOTE this is always handled in bg, its not used in view
            string newCheckSum = GetContentCheckSum(checkSumSource);
            if (ContentCheckSum == newCheckSum) {
                // no change ignore
                return;
            }
            MpConsole.WriteLine($"Tile '{this}' checksum updated. From '{ContentCheckSum}' to '{newCheckSum}'");
            ContentCheckSum = newCheckSum;
            // await WriteToDatabaseAsync();
            //});
        }
        #endregion

    }

}
