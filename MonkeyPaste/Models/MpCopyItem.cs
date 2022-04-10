using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions.Attributes;
using MonkeyPaste.Plugin;

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

    public class MpCopyItem : MpUserObject, MpISyncableDbObject {
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
                    var deviceTypeInt = await MpDataModelProvider.GetSortableCopyItemViewProperty<int>(ci.Id, queryPathType.ToString());
                    return (MpUserDeviceType)deviceTypeInt;
                default:
                    //UrlPath,UrlTitle,UrlDomainPath,AppPath,AppName,SourceDeviceName,SourceDeviceType
                    var resultStr = await MpDataModelProvider.GetSortableCopyItemViewProperty<string>(ci.Id, queryPathType.ToString());
                    return resultStr;

            }
        }
             
        #endregion

        #region Column Definitions

        [PrimaryKey, AutoIncrement]
        [ForeignKey(typeof(MpUserObject))]
        [Column("pk_MpCopyItemId")]
        public override int Id { get; set; } = 0;

        [Column("MpCopyItemGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        public string RootCopyItemGuid { get; set; }
        //public string ParentCopyItemGuid { get; set; }

        [ForeignKey(typeof(MpCopyItem))]
        [Column("fk_ParentCopyItemId")]
        public int CompositeParentCopyItemId { get; set; }

        public int CompositeSortOrderIdx { get; set; }


        [ForeignKey(typeof(MpSource))]
        [Column("fk_MpSourceId")]
        public int SourceId { get; set; }

        public string Title { get; set; } = string.Empty;

        [Column("fk_MpCopyItemTypeId")]
        public int TypeId { get; set; } = 0;

        public DateTime CopyDateTime { get; set; }

        [Indexed]
        public string ItemData { get; set; } = string.Empty;

        public string ItemData_rtf { get; set; } = string.Empty;

        [ForeignKey(typeof(MpIcon))]
        [Column("fk_MpIconId")]
        public int IconId { get; set; }

        [ForeignKey(typeof(MpDbImage))]
        [Column("fk_SsMpDbImageId")]
        public int SsDbImageId { get; set; }

        public string ItemDescription { get; set; } = string.Empty;

        public int CopyCount { get; set; } = 0;

        public int PasteCount { get; set; } = 0;

        [Column("HexColor")]
        public string ItemColor { get; set; } = string.Empty;

        #endregion

        #region Fk Models

        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead | CascadeOperation.CascadeInsert)]
        public MpSource Source { get; set; }

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

        #endregion

        #region Static Methods

        public static async Task<ObservableCollection<MpCopyItem>> SearchAsync(int tagId, string searchString) {
            var allCopyItems = await MpDb.GetItemsAsync<MpCopyItem>();
            var allCopyItemTags = await MpDb.GetItemsAsync<MpCopyItemTag>();

            var searchResult = (from ci in allCopyItems
                                join cit in allCopyItemTags on
                                tagId equals cit.TagId
                                where ci.ItemData.ContainsByCaseOrRegexSetting(searchString)
                                select ci);//.Skip(2).Take(2);

            return new ObservableCollection<MpCopyItem>(searchResult);
        }

        public static async Task<MpCopyItem> Create(
            MpSource source = null, 
            string data = "", 
            MpCopyItemType itemType = MpCopyItemType.None,
            string title = "",
            string description = "",
            //List<int> iconIdList = null,
            bool suppressWrite = false) {
            var dupCheck = await MpDataModelProvider.GetCopyItemByData(data);
            if (MpPreferences.IgnoreNewDuplicates && dupCheck != null) {
                //flipping pk sign notifies AddItemThread item already exists and flips it back
                dupCheck.Id *= -1;
                return dupCheck;
            }

            int count = await MpDataModelProvider.GetTotalCopyItemCountAsync();
            
            if(itemType == MpCopyItemType.FileList) {
                // NOTE when filedrop is added to clipboard the string collection is broken into file list composite item
                // sorted by given order
                MpCopyItem parentItem = null;
                var pl = data.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < pl.Length; i++) {
                    string iconBase64Str = MpNativeWrapper.Services.IconBuilder.GetApplicationIconBase64(pl[i]);

                    var icon = await MpIcon.Create(iconBase64Str);
                    int iconId = 0;
                    if(icon == null) {
                        iconId = MpPreferences.ThisAppIcon.Id;
                    } else {
                        iconId = icon.Id;
                    }
                    
                    var curItem = new MpCopyItem() {
                        CopyItemGuid = System.Guid.NewGuid(),
                        CopyDateTime = DateTime.Now,
                        Title = string.IsNullOrEmpty(title) ? "Untitled" + (++count) : title,
                        ItemData = pl[i],
                        ItemDescription = description,
                        ItemType = itemType,
                        SourceId = source.Id,
                        Source = source,
                        IconId = iconId,
                        CopyCount = 1,
                        CompositeSortOrderIdx = i,
                        CompositeParentCopyItemId = 0
                    };
                    if(i > 0) {
                        curItem.CompositeParentCopyItemId = parentItem.Id;
                    }
                    if(!suppressWrite) {
                        await curItem.WriteToDatabaseAsync();
                    }                    

                    if (i == 0) {
                        parentItem = curItem;
                    }
                }
                return parentItem;
            }
            var newCopyItem = new MpCopyItem() {
                CopyItemGuid = System.Guid.NewGuid(),
                CopyDateTime = DateTime.Now,
                Title = string.IsNullOrEmpty(title) ? "Untitled" + (++count) : title,
                ItemDescription = description,
                ItemData = data,
                ItemType = itemType,
                SourceId = source.Id,
                Source = source,
                CopyCount = 1
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

        public override async Task WriteToDatabaseAsync() {
            if(IgnoreDb) {
                MpConsole.WriteLine($"Db write for '{ToString()}' was ignored");
                return;
            }
            if(Source == null) {
                Source = await MpDb.GetItemAsync<MpSource>(SourceId);
            }
            if(CompositeParentCopyItemId == Id && Id > 0) {
                MpConsole.WriteLine("Warning! circular copy item ref detected, attempting to fix...");
                CompositeParentCopyItemId = CompositeSortOrderIdx = 0;
            }
            await base.WriteToDatabaseAsync();
        }

        public override async Task DeleteFromDatabaseAsync() {
            //
            if (IgnoreDb) {
                MpConsole.WriteLine($"Db delete for '{ToString()}' was ignored");
                return;
            }

            var citl = await MpDataModelProvider.GetTextTemplatesAsync(Id);
            await Task.WhenAll(citl.Select(x => x.DeleteFromDatabaseAsync()));
            await base.DeleteFromDatabaseAsync();
        }

        #region Sync

        public async Task<object> DeserializeDbObject(string objStr) {
            var objParts = objStr.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);
            await Task.Delay(0);
            var ci = new MpCopyItem() {
                CopyItemGuid = System.Guid.Parse(objParts[0]),
                Title = objParts[1],
                CopyCount = Convert.ToInt32(objParts[2]),
                CopyDateTime = DateTime.Parse(objParts[3]),
                ItemData = objParts[4],
                ItemDescription = objParts[5],
                ItemType = (MpCopyItemType)Convert.ToInt32(objParts[6])
            };
            ci.Source = await MpDb.GetDbObjectByTableGuidAsync("MpSource", objParts[7]) as MpSource;
            //TODO deserialize this once img and files added
            //ci.ItemType = MpCopyItemType.RichText;
            return ci;
        }

        public async Task<string> SerializeDbObject() {
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
                Source.SourceGuid.ToString()
                );
        }

        public Type GetDbObjectType() {
            return typeof(MpCopyItem);
        }

        public async Task<Dictionary<string, string>> DbDiff(object drOrModel) {
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
                Source.SourceGuid.ToString());
            diffLookup = CheckValue(
                ItemType,
                other.ItemType,
                "fk_MpCopyItemTypeId",
                diffLookup,
                ((int)ItemType).ToString());

            return diffLookup;
        }

        public async Task<object> CreateFromLogs(string dboGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
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
                        newCopyItem.Source = await MpDataModelProvider.GetSourceByGuid(li.AffectedColumnValue);
                        if(newCopyItem.Source != null) {
                            newCopyItem.Source = await MpDb.GetItemAsync<MpSource>(newCopyItem.Source.Id);
                        }
                        newCopyItem.SourceId = Convert.ToInt32(newCopyItem.Source.Id);
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

        public async Task<object> Clone(bool isReplica) {
            // NOTE isReplica is used when duplicating item which retains tag associations but not shortcuts

            if(Source == null) {
                Source = await MpDb.GetItemAsync<MpSource>(SourceId);
            }

            var newItem = new MpCopyItem() {
                ItemType = this.ItemType,
                Title = isReplica ? this.Title + " Copy":this.Title,
                ItemData = this.ItemData,
                IconId = this.IconId,
                Source = this.Source,
                SourceId = this.Source.Id,
                CopyCount = 1,
                CopyDateTime = DateTime.Now,
                CompositeParentCopyItemId = isReplica ? 0:this.CompositeParentCopyItemId,
                CompositeSortOrderIdx = isReplica ? 0:this.CompositeSortOrderIdx,
                Id = isReplica ? 0:this.Id,
                CopyItemGuid = isReplica ? System.Guid.NewGuid():this.CopyItemGuid
            };

            if(isReplica) {
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

    public enum MpCopyItemType {
        None = 0,
        Text,
        Image,
        FileList
    }
}
