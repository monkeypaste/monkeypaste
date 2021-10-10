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
        public static async Task<MpCopyItem> GetCopyItemByIdAsync(int CopyItemId) {
            var allItems = await MpDb.Instance.GetItemsAsync<MpCopyItem>();
            var udbpl = allItems.Where(x => x.Id == CopyItemId).ToList();
            if (udbpl.Count > 0) {
                return udbpl[0];
            }
            return null;
        }

        public static MpCopyItem GetCopyItemById(int CopyItemId) {
            var allItems = MpDb.Instance.GetItems<MpCopyItem>();
            var udbpl = allItems.Where(x => x.Id == CopyItemId).ToList();
            if (udbpl.Count > 0) {
                return udbpl[0];
            }
            return null;
        }

        public static MpCopyItem GetCopyItemByData(string data) {
            var allItems = MpDb.Instance.GetItems<MpCopyItem>();
            return allItems.Where(x => x.ItemData == data).FirstOrDefault();
        }

        public static async Task<List<MpCopyItem>> GetAllCopyItemsByTagId(int tagId) {
            var citl = await MpCopyItemTag.GetAllCopyItemsForTagIdAsync(tagId);
            var cil = new List<MpCopyItem>();
            foreach (var cit in citl) {
                var ci = await MpCopyItem.GetCopyItemByIdAsync(cit.CopyItemId);
                cil.Add(ci);
            }
            return cil;
        }

        public static List<MpCopyItem> GetCompositeChildren(MpCopyItem ci) {
            if (ci == null || ci.Id == 0) {
                return new List<MpCopyItem>();
            }
            //return MpDb.Instance.Query<MpCopyItem>(@"SELECT * FROM MpCopyItem WHERE fk_ParentCopyItemId=? ORDER BY CompositeSortOrderIdx", ci.Id);
            return MpDb.Instance.GetItems<MpCopyItem>().Where(x => x.CompositeParentCopyItemId == ci.Id).OrderBy(x => x.CompositeSortOrderIdx).ToList();
        }

        public static async Task<List<MpCopyItem>> GetCompositeChildrenAsync(MpCopyItem ci) {
            if (ci == null || ci.Id == 0) {
                return new List<MpCopyItem>();
            }
            //return MpDb.Instance.Query<MpCopyItem>(@"SELECT * FROM MpCopyItem WHERE fk_ParentCopyItemId=? ORDER BY CompositeSortOrderIdx", ci.Id);
            var cil = await MpDb.Instance.GetItemsAsync<MpCopyItem>();
            var result = cil.Where(x => x.CompositeParentCopyItemId == ci.Id).OrderBy(x => x.CompositeSortOrderIdx).ToList();
            return result;
        }


        public static async Task<List<MpCopyItem>> GetPageAsync(
            int tagId,
            int start,
            int count,
            MpClipTileSortType sortType,
            bool isDescending,
            Dictionary<int,int> manualSortOrderLookup = null) {
            MpCopyItem dummyCi = new MpCopyItem();
            List<MpCopyItem> result = await MpDb.Instance.GetItemsAsync<MpCopyItem>();

            switch (tagId) {
                case MpTag.RecentTagId:
                    result = result.Where(x => x.CompositeParentCopyItemId == 0)
                                 .OrderByDynamic(isDescending, x => x.CopyDateTime)
                                 .Take(count)
                                 .Skip(start)
                                 .ToList();
                    return result;
                case MpTag.AllTagId:
                    result = result.Where(x => x.CompositeParentCopyItemId == 0).ToList();
                    break;
                default:
                    var citl = await MpDb.Instance.GetItemsAsync<MpCopyItemTag>();
                    if (isDescending) {
                        result = (from value in
                                    (from ci in result
                                     from cit in citl
                                     where ci.Id == cit.CopyItemId &&
                                         tagId == cit.TagId
                                     select new { ci, cit })
                                  orderby value.cit.CopyItemSortIdx descending
                                  select value.ci)
                                                  .Where(x => x.CompositeParentCopyItemId == 0)
                                                 .ToList();
                    } else {
                        result = (from value in
                                    (from ci in result
                                     from cit in citl
                                     where ci.Id == cit.CopyItemId &&
                                         tagId == cit.TagId
                                     select new { ci, cit })
                                  orderby value.cit.CopyItemSortIdx ascending
                                  select value.ci)
                                                  .Where(x => x.CompositeParentCopyItemId == 0)
                                                 .ToList();
                    }
                    break;
            }
            switch (sortType) {
                case MpClipTileSortType.CopyDateTime:
                    result = result.OrderBy(x => x.GetType().GetProperty(nameof(x.CopyDateTime)).GetValue(x))
                                 .Take(count)
                                 .Skip(start)
                                 .ToList();
                    break;
                case MpClipTileSortType.ItemType:
                    result = result.OrderBy(x => x.GetType().GetProperty(nameof(x.ItemType)).GetValue(x))
                                 .Take(count)
                                 .Skip(start)
                                 .ToList();
                    break;
                // TODO add rest of sort types
                case MpClipTileSortType.Manual:
                    if (manualSortOrderLookup == null) {
                        result = result.Take(count).Skip(start).ToList();
                    } else {
                        int missingCount = 0;
                        var missingItems = new List<MpCopyItem>();
                        foreach (var ci in result) {
                            if (manualSortOrderLookup.ContainsKey(ci.Id)) {
                                ci.ManualSortIdx = manualSortOrderLookup[ci.Id];
                            } else {
                                missingCount++;
                                if (isDescending) {
                                    ci.ManualSortIdx = manualSortOrderLookup.Min(x => x.Value) - missingCount;
                                } else {
                                    ci.ManualSortIdx = manualSortOrderLookup.Max(x => x.Value) + missingCount;
                                }

                            }
                        }
                        result = result.OrderByDynamic(isDescending, x => x.ManualSortIdx).Take(count).Skip(start).ToList();
                    }
                    break;
            }
            return result;
        }

        public static async Task<ObservableCollection<MpCopyItem>> SearchAsync(int tagId, string searchString) {
            var allCopyItems = await MpDb.Instance.GetItemsAsync<MpCopyItem>();
            var allCopyItemTags = await MpDb.Instance.GetItemsAsync<MpCopyItemTag>();

            var searchResult = (from ci in allCopyItems
                                join cit in allCopyItemTags on
                                tagId equals cit.TagId
                                where ci.ItemData.ContainsByUserSensitivity(searchString)
                                select ci);//.Skip(2).Take(2);

            return new ObservableCollection<MpCopyItem>(searchResult);
        }

        public static MpCopyItem Create(MpSource source, string data, MpCopyItemType itemType) {
            int count = MpDb.Instance.GetItems<MpCopyItem>().Count;
            var dupCheck = MpDb.Instance.GetItems<MpCopyItem>().Where(x => x.ItemData == data).FirstOrDefault();
            if(dupCheck != null) {
                dupCheck.Id *= -1;
                return dupCheck;
            }
            count++;
            var newCopyItem = new MpCopyItem() { 
                CopyItemGuid = System.Guid.NewGuid(),
                CopyDateTime = DateTime.Now,
                Title = "Untitled"+count,
                ItemData = data,
                ItemType = itemType,
                SourceId = source.Id,
                Source = source,
                CopyCount = 1
            };

            MpDb.Instance.AddOrUpdate<MpCopyItem>(newCopyItem);
            return newCopyItem;
        }

         #endregion

        [Ignore]
        public int ManualSortIdx { get; set; }

        public MpCopyItem() : base() { }

        public override void DeleteFromDatabase() {
            base.DeleteFromDatabase();
        }

        public override void WriteToDatabase() {
            base.WriteToDatabase();
        }
        #region Composites

        public MpCopyItem GetCompositeParent() {
            if(CompositeParentCopyItemId == 0) {
                return null;
            }
            return MpCopyItem.GetCopyItemById(CompositeParentCopyItemId);
        }


        public void UnlinkCompositeChild(MpCopyItem cci) {
            cci.CompositeParentCopyItemId = 0;
            cci.WriteToDatabase();
        }

        public void LinkCompositeChild(MpCopyItem cci, int forceIdx = -1) {
            cci.CompositeParentCopyItemId = Id;
            var compList = MpCopyItem.GetCompositeChildren(this);
            cci.CompositeSortOrderIdx = forceIdx < 0 ? compList.Count : forceIdx < compList.Count ? forceIdx : compList.Count;
            cci.WriteToDatabase();
            WriteToDatabase();
        }

        #endregion

        public override string ToString() {
            return $"Id:{Id} Text:{ItemData}" + Environment.NewLine;
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
            ci.Source = MpDb.Instance.GetDbObjectByTableGuid("MpSource", objParts[7]) as MpSource;
            //TODO deserialize this once img and files added
            //ci.ItemType = MpCopyItemType.RichText;
            return ci;
        }

        public string SerializeDbObject() {
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

        public Dictionary<string, string> DbDiff(object drOrModel) {
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
                        newCopyItem.Source = await MpSource.GetSourceByGuid(li.AffectedColumnValue);
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

        public object Clone() {
            var s = MpSource.GetThisDeviceAppSource();
            var newItem = new MpCopyItem() {
                ItemType = this.ItemType,
                Title = this.Title,
                ItemData = this.ItemData,
                ItemColor = this.ItemColor,
                Source = s,
                SourceId = s.Id,
                CopyCount = 1,
                CopyDateTime = DateTime.Now,
                CompositeParentCopyItemId = 0,
                CompositeSortOrderIdx = 0,
                Id = 0
            };

            newItem.WriteToDatabase();

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
        //Composite,
        Csv, //this is only used during runtime
        Html
    }

    public enum MpClipTileSortType {
        None = 0,
        CopyDateTime,
        //ModifiedDateTime,
        Source,
        Title,
        PlainText,
        ItemType,
        UsageScore,
        Manual
    }
}
