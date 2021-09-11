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

        #endregion

        #region Fk Objects

        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead | CascadeOperation.CascadeInsert)]
        public MpSource Source { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        public MpDbImage SsDbImage { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        public MpDbImage ItemDbImage { get; set; }

        [Column("HexColor")]
        public string ItemColor { get; set; }

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
            return MpDb.Instance.GetItems<MpCopyItem>().Where(x => x.CompositeParentCopyItemId == ci.Id).OrderBy(x => x.CompositeSortOrderIdx).ToList();
        }

        public static MpCopyItem Merge(
            MpCopyItem fromItem,
            MpCopyItem toItem,
            bool useFileData = false,
            bool isFileDataMerged = false,
            int forceIdx = -1) {
            if (toItem == null) {
                return fromItem;
            }
            if (fromItem.ItemType == MpCopyItemType.Image || toItem.ItemType == MpCopyItemType.Image) {
                // for now, do not allow combining with image types
                return null;
            }

            if (fromItem == toItem) {
                return toItem;
            }

            toItem.LinkCompositeChild(fromItem, forceIdx);

            return toItem;
        }

        public static List<MpCopyItem> GetPage(
            int tagId,
            int start,
            int count,
            string sortColumn = "pk_MpCopyItemId",
            bool isDescending = false) {
            List<MpCopyItem> result = new List<MpCopyItem>();

            switch(tagId) {
                case MpTag.AllTagId:
                    result = MpDb.Instance.Query<MpCopyItem>(
                            @"SELECT * from MpCopyItem " +
                            "ORDER BY ? " + (isDescending ? "DESC" : "ASC") +
                            " LIMIT ? OFFSET ?",
                            sortColumn,
                            count,
                            start);
                    break;
                case MpTag.RecentTagId:
                    sortColumn = "CopyDateTime";
                    if(isDescending) {
                        result = MpDb.Instance.GetItems<MpCopyItem>()
                                 .OrderByDescending(x => x.CopyDateTime)
                                 .Take(count)
                                 .Skip(start)
                                 .ToList();
                    } else {
                        result = MpDb.Instance.GetItems<MpCopyItem>()
                                 .OrderBy(x => x.CopyDateTime)
                                 .Take(count)
                                 .Skip(start)
                                 .ToList();
                    }
                    
                             
                    break;
                // Add other sudo tags here

                default:
                    if (isDescending) {
                        result = MpDb.Instance.GetItems<MpCopyItem>()
                                 .OrderByDescending(x => x.CopyDateTime)
                                 .Take(count)
                                 .Skip(start)
                                 .ToList();
                    } else {
                        result = MpDb.Instance.GetItems<MpCopyItem>()
                                 .OrderBy(x => x.CopyDateTime)
                                 .Take(count)
                                 .Skip(start)
                                 .ToList();
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
            var newCopyItem = new MpCopyItem() { 
                CopyItemGuid = System.Guid.NewGuid(),
                CopyDateTime = DateTime.Now,
                Title = "Untitled",
                ItemData = data,
                ItemType = itemType,
                ItemColor = MpHelpers.Instance.GetRandomColor().ToHex(),
                SourceId = source.Id,
                Source = source,
                CopyCount = 1
            };

            MpDb.Instance.AddOrUpdate<MpCopyItem>(newCopyItem);
            return newCopyItem;
        }

         #endregion

        public MpCopyItem() : base() { }

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

        public override string ToString() {
            return $"Id:{Id} Text:{ItemData}" + Environment.NewLine;
        }

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

        public object Clone() {
            var newItem = new MpCopyItem() {
                ItemType = this.ItemType,
                Title = this.Title,
                ItemData = this.ItemData,
                ItemColor = this.ItemColor,
                Source = this.Source,
                CopyCount = this.CopyCount,
                CopyDateTime = this.CopyDateTime,
                Id = 0
            };

            newItem.WriteToDatabase();

            foreach (var cit in MpDb.Instance.GetItems<MpCopyItemTag>().Where(x=>x.CopyItemId == this.Id).ToList()) {
                var ncit = new MpCopyItemTag() {
                    CopyItemId = newItem.Id,
                    TagId = cit.TagId
                };
                ncit.WriteToDatabase();
            }
            foreach (var cci in MpCopyItem.GetCompositeChildren(this)) {
                newItem = MpCopyItem.Merge((MpCopyItem)cci.Clone(), newItem);
            }
            return newItem;
        }
    }

    public enum MpCopyItemDetailType {
        None = 0,
        DateTimeCreated,
        DataSize,
        UsageStats
    }

    public enum MpCopyItemType {
        None = 0,
        RichText,
        Image,
        FileList,
        //Composite,
        Csv, //this is only used during runtime
        RichHtml,
        Html
    }
}
