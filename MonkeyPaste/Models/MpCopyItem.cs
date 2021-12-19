using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    [Table("MpCopyItem")]
    public class MpCopyItem : MpDbModelBase, MpISyncableDbObject {
        #region Column Definitions

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpCopyItemId")]
        public override int Id { get; set; } = 0;

        [ForeignKey(typeof(MpCopyItem))]
        [Column("fk_ParentCopyItemId")]
        public int CompositeParentCopyItemId { get; set; }

        public int CompositeSortOrderIdx { get; set; }

        [Column("MpCopyItemGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

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

        [ForeignKey(typeof(MpSource))]
        [Column("fk_MpSourceId")]
        public int SourceId { get; set; }

        public string Title { get; set; } = string.Empty;

        [Column("fk_MpCopyItemTypeId")]
        public int TypeId { get; set; } = 0;

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

        public DateTime CopyDateTime { get; set; }

        [Indexed]
        public string ItemData { get; set; } = string.Empty;

        [ForeignKey(typeof(MpDbImage))]
        [Column("fk_MpDbImageId")]
        public int ItemImageId { get; set; }

        [ForeignKey(typeof(MpDbImage))]
        [Column("fk_SsMpDbImageId")]
        public int SsDbImageId { get; set; }

        public string ItemDescription { get; set; } = string.Empty;

        public int CopyCount { get; set; } = 0;

        public int PasteCount { get; set; } = 0;

        [Column("HexColor")]
        public string ItemColor { get; set; }
        #endregion

        #region Fk Models

        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead | CascadeOperation.CascadeInsert)]
        public MpSource Source { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        public MpDbImage SsDbImage { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        public MpDbImage ItemDbImage { get; set; }

        //[OneToMany(inverseProperty: nameof(Parent), CascadeOperations = CascadeOperation.CascadeRead)]
        //public List<MpCopyItem> CompositeItems { get; set; }

        //[ManyToOne(inverseProperty: nameof(CompositeItems), CascadeOperations = CascadeOperation.CascadeRead)]
        //public MpCopyItem Parent { get; set; }

        //[OneToMany]
        //public List<MpCopyItemTemplate> Templates { get; set; }

        //[OneToMany]
        //public List<MpShortcut> Shortcuts { get; set; }
        #endregion

        #region Static Methods

        public static async Task<ObservableCollection<MpCopyItem>> SearchAsync(int tagId, string searchString) {
            var allCopyItems = await MpDb.Instance.GetItemsAsync<MpCopyItem>();
            var allCopyItemTags = await MpDb.Instance.GetItemsAsync<MpCopyItemTag>();

            var searchResult = (from ci in allCopyItems
                                join cit in allCopyItemTags on
                                tagId equals cit.TagId
                                where ci.ItemData.ContainsByCaseOrRegexSetting(searchString)
                                select ci);//.Skip(2).Take(2);

            return new ObservableCollection<MpCopyItem>(searchResult);
        }

        public static async Task<MpCopyItem> Create(MpSource source, string data, MpCopyItemType itemType) {
            var dupCheck = await MpDataModelProvider.Instance.GetCopyItemsByData(data);
            if (MpPreferences.Instance.IgnoreNewDuplicates && dupCheck != null && dupCheck.Count > 0) {
                //flipping pk sign notifies AddItemThread item already exists and flips it back
                dupCheck[0].Id *= -1;
                return dupCheck[0];
            }

            int count = await MpDataModelProvider.Instance.GetTotalCopyItemCountAsync();
            
            if(itemType == MpCopyItemType.FileList) {
                MpCopyItem parentItem = null;
                var pl = data.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < pl.Length; i++) {
                    var curItem = new MpCopyItem() {
                        CopyItemGuid = System.Guid.NewGuid(),
                        CopyDateTime = DateTime.Now,
                        Title = "Untitled" + (++count),
                        ItemData = pl[i],
                        ItemType = itemType,
                        SourceId = source.Id,
                        Source = source,
                        CopyCount = 1,
                        CompositeSortOrderIdx = i,
                        CompositeParentCopyItemId = 0
                    };
                    if(i > 0) {
                        curItem.CompositeParentCopyItemId = parentItem.Id;
                    }
                    await curItem.WriteToDatabaseAsync();

                    if (i == 0) {
                        parentItem = curItem;
                    }
                }
                return parentItem;
            }
            var newCopyItem = new MpCopyItem() {
                CopyItemGuid = System.Guid.NewGuid(),
                CopyDateTime = DateTime.Now,
                Title = "Untitled" + (++count),
                ItemData = data,
                ItemType = itemType,
                SourceId = source.Id,
                Source = source,
                CopyCount = 1
            };

            await newCopyItem.WriteToDatabaseAsync();
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
                Source = await MpDb.Instance.GetItemAsync<MpSource>(SourceId);
            }
            if(CompositeParentCopyItemId == Id) {
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

            var citl = await MpDataModelProvider.Instance.GetTemplatesAsync(Id);
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
            ci.Source = await MpDb.Instance.GetDbObjectByTableGuidAsync("MpSource", objParts[7]) as MpSource;
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
            var cidr = await MpDb.Instance.GetDbObjectByTableGuidAsync("MpCopyItem", CopyItemGuid.ToString());
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
                        newCopyItem.Source = await MpDataModelProvider.Instance.GetSourceByGuid(li.AffectedColumnValue);
                        if(newCopyItem.Source != null) {
                            newCopyItem.Source = await MpDb.Instance.GetItemAsync<MpSource>(newCopyItem.Source.Id);
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
            //var s = await MpDb.Instance.GetItemAsync<MpSource>(MpPreferences.Instance.ThisDeviceSourceId);
            if(Source == null) {
                Source = await MpDb.Instance.GetItemAsync<MpSource>(SourceId);
            }

            var newItem = new MpCopyItem() {
                ItemType = this.ItemType,
                Title = isReplica ? this.Title + " Copy":this.Title,
                ItemData = this.ItemData,
                ItemColor = this.ItemColor,
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

                var tags = await MpDataModelProvider.Instance.GetCopyItemTagsForCopyItemAsync(this.Id);
                foreach (var tag in tags) {
                    await MpCopyItemTag.Create(tag.Id, newItem.Id);
                }

                var templates = await MpDataModelProvider.Instance.GetTemplatesAsync(this.Id);
                foreach (var template in templates) {
                    var templateClone = template.Clone(true) as MpCopyItemTemplate;
                    templateClone.CopyItemId = newItem.Id;
                    await templateClone.WriteToDatabaseAsync();
                }
            }

            return newItem;
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
        RichText,
        Image,
        FileList,
       // Csv, //this is only used during runtime
        Html
    }
}
