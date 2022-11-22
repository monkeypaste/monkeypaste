using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using MonkeyPaste.Common.Plugin; 
using MonkeyPaste.Common;

using System.Diagnostics;

namespace MonkeyPaste {
    public enum MpCopyItemPropertyPathType {
        None = 0,
        ItemData,
        ItemType,
        ItemDescription,
        Title, //seperator
        AppPath,
        AppName,
        UrlPath,
        UrlTitle,
        UrlDomainPath, //seperator
        CopyDateTime,
        LastPasteDateTime, //seperator
        CopyCount,
        PasteCount,
        SourceDeviceName,
        SourceDeviceType,
        LastOutput
    }

    public class MpCopyItem : MpDbModelBase, MpISyncableDbObject, MpISourceRef {
        #region Statics

        public static string[] PhysicalComparePropertyPaths {
            get {
                var paths = new List<string>();
                for (int i = 0; i < Enum.GetNames(typeof(MpCopyItemPropertyPathType)).Length; i++) {
                    string path = string.Empty;
                    MpCopyItemPropertyPathType cppt = (MpCopyItemPropertyPathType)i;
                    switch (cppt) {
                        case MpCopyItemPropertyPathType.ItemData:
                        case MpCopyItemPropertyPathType.ItemType:
                        case MpCopyItemPropertyPathType.ItemDescription:
                        case MpCopyItemPropertyPathType.Title:
                        case MpCopyItemPropertyPathType.CopyDateTime:
                        case MpCopyItemPropertyPathType.CopyCount:
                        case MpCopyItemPropertyPathType.PasteCount:
                            path = cppt.ToString();
                            break;
                        case MpCopyItemPropertyPathType.AppName:
                        case MpCopyItemPropertyPathType.AppPath:
                            path = string.Format(@"Source.App.{0}", cppt.ToString());
                            break;
                        case MpCopyItemPropertyPathType.UrlPath:
                        case MpCopyItemPropertyPathType.UrlTitle:
                        case MpCopyItemPropertyPathType.UrlDomainPath:
                            path = string.Format(@"Source.App.{0}", cppt.ToString());
                            break;
                        default:
                            break;
                    }
                    paths.Add(path);
                }
                return paths.ToArray();
            }
        }

        public static async Task<object> QueryProperty(MpCopyItem ci, MpCopyItemPropertyPathType queryPathType) {
            if(ci == null) {
                return null;
            }
            switch (queryPathType) {
                case MpCopyItemPropertyPathType.None:
                case MpCopyItemPropertyPathType.LastOutput:
                    return null;
                case MpCopyItemPropertyPathType.ItemData:
                case MpCopyItemPropertyPathType.ItemType:
                case MpCopyItemPropertyPathType.ItemDescription:
                case MpCopyItemPropertyPathType.Title:
                case MpCopyItemPropertyPathType.CopyDateTime:
                case MpCopyItemPropertyPathType.CopyCount:
                case MpCopyItemPropertyPathType.PasteCount:
                    return ci.GetPropertyValue(queryPathType.ToString());
                case MpCopyItemPropertyPathType.SourceDeviceType:
                    var deviceTypeInt = await MpDataModelProvider.GetSortableCopyItemViewPropertyAsync<int>(ci.Id, queryPathType.ToString());
                    return (MpUserDeviceType)deviceTypeInt;
                default:
                    //UrlPath,UrlTitle,UrlDomainPath,AppPath,AppName,SourceDeviceName,SourceDeviceType
                    var resultStr = await MpDataModelProvider.GetSortableCopyItemViewPropertyAsync<string>(ci.Id, queryPathType.ToString());
                    return resultStr;

            }
        }

        public static MpPortableDataFormat GetDefaultFormatForItemType(MpCopyItemType itemType) {
            if(MpPlatformWrapper.Services.OsInfo.IsAvalonia) {
                switch (itemType) {
                    case MpCopyItemType.Text:
                        return MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.AvHtml_bytes);
                    case MpCopyItemType.Image:
                        return MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.AvPNG);
                    case MpCopyItemType.FileList:
                        return MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.AvFileNames);
                }

            } else {
                // this is bad but its only so wpf still builds...
                switch (itemType) {
                    case MpCopyItemType.Text:
                        return MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.WinRtf);
                    case MpCopyItemType.Image:
                        return MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.WinBitmap);
                    case MpCopyItemType.FileList:
                        return MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.WinFileDrop);
                }

            }
            return null;
        }
             
        #endregion

        #region Column Definitions

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpCopyItemId")]
        public override int Id { get; set; } = 0;

        [Column("MpCopyItemGuid")]
        [Indexed]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        public string CopyItemSourceGuid { get; set; }
        //public string ParentCopyItemGuid { get; set; }



        //[ForeignKey(typeof(MpSource))]
        [Column("fk_MpSourceId")]
        public int SourceId { get; set; }

        public string Title { get; set; } = string.Empty;

        [Column("fk_MpCopyItemTypeId")]
        public int TypeId { get; set; } = 0;

        public DateTime CopyDateTime { get; set; }

        [Indexed]
        public string ItemData { get; set; } = string.Empty;

        //public string ItemData_rtf { get; set; } = string.Empty;

        //[ForeignKey(typeof(MpIcon))]
        [Column("fk_MpIconId")]
        public int IconId { get; set; }

        [Column("fk_MpDataObjectId")]
        public int DataObjectId { get; set; }

        //[ForeignKey(typeof(MpDbImage))]
        //[Column("fk_SsMpDbImageId")]
        //public int SsDbImageId { get; set; }

        public string ItemDescription { get; set; } = string.Empty;

        public int CopyCount { get; set; } = 0;

        public int PasteCount { get; set; } = 0;

        [Column("HexColor")]
        public string ItemColor { get; set; } = string.Empty;

        public string PrefferedFormatName { get; set; } = string.Empty;


        #endregion

        #region Fk Models

        //[ManyToOne(CascadeOperations = CascadeOperation.CascadeRead | CascadeOperation.CascadeInsert)]
        //public MpSource Source { get; set; }

        //[OneToOne(CascadeOperations = CascadeOperation.All)]
        //public MpDbImage SsDbImage { get; set; }

        //[OneToOne(CascadeOperations = CascadeOperation.All)]
        //public MpDbImage ItemDbImage { get; set; }


        //[OneToMany(inverseProperty: nameof(Parent), CascadeOperations = CascadeOperation.CascadeRead)]
        //public List<MpCopyItem> CompositeItems { get; set; }

        //[ManyToOne(inverseProperty: nameof(CompositeItems), CascadeOperations = CascadeOperation.CascadeRead)]
        //public MpCopyItem Parent { get; set; }

        //[OneToMany]
        //public List<MpTextToken> Templates { get; set; }

        //[OneToMany]
        //public List<MpShortcut> Shortcuts { get; set; }
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
                return (MpCopyItemType)TypeId;
            }
            set {
                if (ItemType != value) {
                    TypeId = (int)value;
                }
            }
        }

        [Ignore]
        public MpPortableDataFormat PreferredFormat {
            get => MpPortableDataFormats.GetDataFormat(PrefferedFormatName);
            set => PrefferedFormatName = value == null ? null : value.Name;
        }

        #endregion

        #region MpISourceRef Implementation

        [Ignore]
        int MpISourceRef.SourceObjId => Id;

        [Ignore]
        MpCopyItemSourceType MpISourceRef.SourceType => MpCopyItemSourceType.CopyItem;

        #endregion

        #region Static Methods
        public static async Task<MpCopyItem> Create(
            int sourceId = 0,
            string data = "", 
            string preferredFormatName = null,
            string copyItemSourceGuid = "",
            MpCopyItemType itemType = MpCopyItemType.None,
            string title = "",
            string description = "",
            int dataObjectId = 0,
            bool suppressWrite = false) {
            var dupCheck = await MpDataModelProvider.GetCopyItemByDataAsync(data);
            if (MpPrefViewModel.Instance.IgnoreNewDuplicates && 
                dupCheck != null && !suppressWrite) {
                //flipping pk sign notifies AddItemThread item already exists and flips it back
                //dupCheck.Id *= -1;
                dupCheck.WasDupOnCreate = true;
                return dupCheck;
            }

            if(MpPrefViewModel.Instance.UniqueContentItemIdx == 0 && !suppressWrite) {
                MpPrefViewModel.Instance.UniqueContentItemIdx = await MpDataModelProvider.GetTotalCopyItemCountAsync();
            }
            
            if(itemType == MpCopyItemType.None) {
                //derive content type from data
                if(data.IsStringBase64()) {
                    itemType = MpCopyItemType.Image;
                } else if(data.IsStringWindowsFileOrPathFormat()) {
                    // TODO this check will not work if data is list of files need to check for EOL char, split and check first item
                    itemType = MpCopyItemType.FileList;
                } else {
                    itemType = MpCopyItemType.Text;
                }
            }

            preferredFormatName = string.IsNullOrEmpty(preferredFormatName) ?
                                    GetDefaultFormatForItemType(itemType).Name :
                                    preferredFormatName;

            var newCopyItem = new MpCopyItem() {
                CopyItemGuid = System.Guid.NewGuid(),
                CopyDateTime = DateTime.Now,
                Title = string.IsNullOrEmpty(title) ? "Untitled" + (++MpPrefViewModel.Instance.UniqueContentItemIdx) : title,
                ItemDescription = description,
                ItemData = data,
                ItemType = itemType,
                PrefferedFormatName = preferredFormatName,                                        
                SourceId = sourceId,
                CopyCount = 1,
                CopyItemSourceGuid = copyItemSourceGuid,
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
            if(IgnoreDb) {
                MpConsole.WriteLine($"Db write for '{ToString()}' was ignored");
                return;
            }

            if(ItemData == "<p><br></p>") {
                // what IS this nasty shit??
                Debugger.Break();
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

            // NOTE copy item tag is handled by tag when copy item is deleted
            var delete_tasks = new List<Task>();

            //var citl = await MpDataModelProvider.GetCopyItemTagsForCopyItemAsync(Id);
            //delete_tasks.AddRange(citl.Select(x => x.DeleteFromDatabaseAsync()));

            var doil = await MpDataModelProvider.GetDataObjectItemsByDataObjectId(DataObjectId);
            delete_tasks.AddRange(doil.Select(x => x.DeleteFromDatabaseAsync()));

            var do_model = await MpDataModelProvider.GetItemAsync<MpDataObject>(DataObjectId);
            if(do_model != null) {
                delete_tasks.Add(do_model.DeleteFromDatabaseAsync());
            }
            delete_tasks.Add(base.DeleteFromDatabaseAsync());

            await Task.WhenAll(delete_tasks);
        }

        #region Sync

        public async Task<object> DeserializeDbObjectAsync(string objStr) {
            var objParts = objStr.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);
            await Task.Delay(0);

            var source = await MpDb.GetDbObjectByTableGuidAsync("MpSource", objParts[7]) as MpSource;

            var ci = new MpCopyItem() {
                CopyItemGuid = System.Guid.Parse(objParts[0]),
                Title = objParts[1],
                CopyCount = Convert.ToInt32(objParts[2]),
                CopyDateTime = DateTime.Parse(objParts[3]),
                ItemData = objParts[4],
                ItemDescription = objParts[5],
                ItemType = (MpCopyItemType)Convert.ToInt32(objParts[6]),
                SourceId = source == null ? MpPrefViewModel.Instance.ThisAppSourceId : source.Id
            };
            //ci.Source = await MpDb.GetDbObjectByTableGuidAsync("MpSource", objParts[7]) as MpSource;
            //TODO deserialize this once img and files added
            //ci.ItemType = MpCopyItemType.RichText;
            return ci;
        }

        public async Task<string> SerializeDbObjectAsync() {
            await Task.Delay(1);

            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}",
                ParseToken,
                CopyItemGuid.ToString(),
                Title,
                CopyCount,
                CopyDateTime.ToString(),
                ItemData,
                ItemDescription,
                ((int)ItemType).ToString(),
                MpDataModelProvider.GetItem<MpSource>(SourceId).Guid//Source.SourceGuid.ToString()
                );
        }

        public Type GetDbObjectType() {
            return typeof(MpCopyItem);
        }

        public async Task<Dictionary<string, string>> DbDiffAsync(object drOrModel) {
            await Task.Delay(1);

            MpCopyItem other = null;
            if(drOrModel == null) {
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
            diffLookup = CheckValue(
                ItemDescription,
                other.ItemDescription,
                "ItemDescription",
                diffLookup);
            diffLookup = CheckValue(
                SourceId,
                other.SourceId,
                "fk_MpSourceId",
                diffLookup,
                MpDataModelProvider.GetItem<MpSource>(SourceId).Guid);
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
                    case "ItemDescription":
                        newCopyItem.ItemDescription = li.AffectedColumnValue;
                        break;
                    case "fk_MpSourceId":
                        var source = await MpDataModelProvider.GetSourceByGuidAsync(li.AffectedColumnValue);
                        if(source != null) {
                            source = await MpDataModelProvider.GetItemAsync<MpSource>(source.Id);
                        }
                        newCopyItem.SourceId = Convert.ToInt32(source.Id);
                        break;
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

        public async Task<object> Clone(bool isDeepClone) {
            // NOTE isReplica is used when duplicating item which retains tag associations but not shortcuts

            //if(Source == null) {
            //    Source = await MpDataModelProvider.GetItemAsync<MpSource>(SourceId);
            //}

            var newItem = new MpCopyItem() {
                ItemType = this.ItemType,
                Title = this.Title,
                ItemData = this.ItemData,
                IconId = this.IconId,
                SourceId = this.SourceId,
                CopyCount = 1,
                CopyDateTime = DateTime.Now,
                Id = 0,
                CopyItemGuid = System.Guid.NewGuid()                
            };

            if(isDeepClone) {
                await newItem.WriteToDatabaseAsync();

                var tags = await MpDataModelProvider.GetCopyItemTagsForCopyItemAsync(this.Id);
                foreach (var tag in tags) {
                    await MpCopyItemTag.Create(tag.Id, newItem.Id);
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

    }

    public static class MpCopyItemExtensions {
        public static string ToRichText(this MpCopyItem ci) {
            return null;
        }
    }

    public enum MpCopyItemDetailType {
        None = 0,
        DateTimeCreated,
        DataSize,
        UsageStats,
        UrlInfo,
        AppInfo
    }

    public enum MpContentTableContextActionType {
        None = 0,
        InsertColumnRight,
        InsertColumnLeft,
        InsertRowUp,
        InsertRowDown,
        MergeSelectedCells,
        UnmergeCells,
        //separator
        DeleteSelectedColumns,
        DeleteSelectedRows,
        DeleteTable,
        //separator
        //ChangeSelectedBackgroundColor
    }

    public enum MpCopyItemType {
        None = 0,
        Text,
        Image,
        FileList
    }

}
