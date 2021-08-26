using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
        public override int Id { get; set; }

        [ForeignKey(typeof(MpCopyItem))]
        [Column("fk_MpCopyItemId")]
        public int CompositeParentCopyItemId { get; set; }

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

        [ForeignKey(typeof(MpApp))]
        [Column("fk_MpAppId")]
        public int AppId { get; set; }

        [ForeignKey(typeof(MpUrl))]
        [Column("fk_MpUrlId")]
        public int UrlId { get; set; }

        [ForeignKey(typeof(MpColor))]
        [Column("fk_MpColorId")]
        public int ColorId { get; set; }

        public string Title { get; set; }

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

        public string ItemText { get; set; }

        public string ItemRtf { get; set; }

        public string ItemHtml { get; set; }

        public string ItemCsv { get; set; }

        [ForeignKey(typeof(MpDbImage))]
        [Column("fk_MpDbImageId")]
        public int ItemImageId { get; set; }


        [ForeignKey(typeof(MpDbImage))]
        [Column("fk_SsMpDbImageId")]
        public int SsDbImageId { get; set; }

        public string ItemDescription { get; set; }

        public int CopyCount { get; set; }

        public int PasteCount { get; set; }

        //public string Host { get; set; }
        #endregion

        #region Fk Objects
        //[ManyToMany(typeof(MpCopyItemTag))]
        //public List<MpTag> Tags { get; set; }

        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead | CascadeOperation.CascadeInsert)]
        public MpApp App { get; set; }

        [ManyToOne(CascadeOperations = CascadeOperation.All)]
        public MpUrl Url { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        public MpDbImage SsDbImage { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        public MpDbImage ItemDbImage { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        public MpColor ItemColor { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.CascadeDelete | CascadeOperation.CascadeRead)]
        public List<MpCopyItemTemplate> Templates { get; set; }

        [OneToMany(inverseProperty:nameof(CompositeParentCopyItem), CascadeOperations = CascadeOperation.All)]
        public List<MpCopyItem> CompositeSubItems { get; set; }

        [ManyToOne(inverseProperty:nameof(CompositeSubItems), CascadeOperations = CascadeOperation.CascadeRead)]
        public MpCopyItem CompositeParentCopyItem { get; set; }

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<MpPasteHistory> PasteHistoryList { get; set; }
        #endregion


        #region Static Methods
        public static async Task<MpCopyItem> GetCopyItemById(int CopyItemId) {
            var allItems = await MpDb.Instance.GetItemsAsync<MpCopyItem>();
            var udbpl = allItems.Where(x => x.Id == CopyItemId).ToList();
            if (udbpl.Count > 0) {
                return udbpl[0];
            }
            return null;
        }
        public static async Task<List<MpCopyItem>> GetAllCopyItemsByTagId(int tagId) {
            var citl = await MpCopyItemTag.GetAllCopyItemsForTagId(tagId);
            var cil = new List<MpCopyItem>();
            foreach (var cit in citl) {
                var ci = await MpCopyItem.GetCopyItemById(cit.CopyItemId);
                cil.Add(ci);
            }
            return cil;
        }

        public static async Task<ObservableCollection<MpCopyItem>> GetPage(
            int tagId, 
            int start, 
            int count, 
            string sortColumn = "pk_MpCopyItemId", 
            bool isDescending = false) {
            //SELECT
            //user_number,
            //user_name
            //FROM user_table
            //WHERE(user_name LIKE '%{1}%' OR user_number LIKE '%{2}%')
            //AND user_category = { 3 } OR user_category = { 4 }
            //ORDER BY user_uid LIMIT { 5}
            //OFFSET { 6}
            //Where { 5} is page size and { 6 } is page number * page size.

            var result = await MpDb.Instance.QueryAsync<MpCopyItem>(
                                string.Format(
                                    @"SELECT * from MpCopyItem
                                      WHERE pk_MpCopyItemId in 
                                        (SELECT fk_MpCopyItemId FROM MpCopyItemTag 
                                         WHERE fk_MpTagId=?)
                                      ORDER BY {0} {1} LIMIT ? OFFSET ?",
                                    sortColumn,
                                    (isDescending ? "DESC" : "ASC")),
                                tagId,
                                count,
                                start);

            var results = new ObservableCollection<MpCopyItem>();
            foreach(var r in result) {
                var ci = await MpDb.Instance.GetItemAsync<MpCopyItem>(r.Id);
                results.Add(ci);
            }

            return results;
        }

        public static async Task<ObservableCollection<MpCopyItem>> Search(int tagId, string searchString) {
            var allCopyItems = await MpDb.Instance.GetItemsAsync<MpCopyItem>();
            var allCopyItemTags = await MpDb.Instance.GetItemsAsync<MpCopyItemTag>();

            var searchResult = (from ci in allCopyItems
                                join cit in allCopyItemTags on
                                tagId equals cit.TagId
                                where ci.ItemText.ContainsByUserSensitivity(searchString)
                                select ci);//.Skip(2).Take(2);

            return new ObservableCollection<MpCopyItem>(searchResult);
        }


        public static async Task<MpCopyItem> Create(object args) {
            if (args == null) {
                return null;
            }

            //create CopyItem
            string hostPackageName = (args as object[])[0] as string;
            string itemPlainText = (args as object[])[1] as string;
            var hostAppName = (args as object[])[2] as string;
            var hostAppImage = (args as object[])[3] as byte[];
            var hostAppImageBase64 = (args as object[])[4] as string;

            MpApp app = await MpApp.GetAppByPath(hostPackageName);
            if (app == null) {
                app = await MpApp.Create(hostPackageName, hostAppName, hostAppImageBase64);
            }

            var newCopyItem = new MpCopyItem() {
                CopyDateTime = DateTime.Now,
                Title = "Untitled",
                ItemText = itemPlainText,
                ItemType = MpCopyItemType.RichText,
                ItemColor = new MpColor(MpHelpers.Instance.GetRandomColor()),
                App = app,
                CopyCount = 1
                //ItemImage = hostAppImage
            };

            await MpDb.Instance.AddOrUpdateAsync<MpCopyItem>(newCopyItem);

            //add CopyItem to default tags
            var defaultTagList = await MpDb.Instance.QueryAsync<MpTag>(
                "select * from MpTag where pk_MpTagId=? or pk_MpTagId=?", MpTag.AllTagId, MpTag.RecentTagId);

            if (defaultTagList != null) {
                foreach (var tag in defaultTagList) {
                    var CopyItemTag = new MpCopyItemTag() {
                        CopyItemId = newCopyItem.Id,
                        TagId = tag.Id
                    };
                    await MpDb.Instance.AddItemAsync<MpCopyItemTag>(CopyItemTag);
                }
            }
            return newCopyItem;
        }
        #endregion

        public MpCopyItem() {
        }

        public MpCopyItem(object data, string sourceInfo) : this() {
            if(data == null) {
                return;
            }
        }
        public MpCopyItem(string title, string itemPlainText) : this() {
            Title = title;
            ItemText = itemPlainText;
            CopyDateTime = DateTime.Now;
        }

        //public override void DeleteFromDatabase() {
        //    throw new NotImplementedException();
        //}

        //public override string ToString() {
        //    throw new NotImplementedException();
        //}

        public override string ToString() {
            return $"Id:{Id} Text:{ItemText}" + Environment.NewLine;
        }

        public async Task<object> DeserializeDbObject(string objStr) {
            var objParts = objStr.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);
            await Task.Delay(0);
            var dbLog = new MpCopyItem() {
                CopyItemGuid = System.Guid.Parse(objParts[0]),
                Title = objParts[1],
                CopyCount = Convert.ToInt32(objParts[2]),
                CopyDateTime = DateTime.Parse(objParts[3]),
                ItemText = objParts[4],
                ItemRtf = objParts[5],
                ItemHtml = objParts[6],
                ItemDescription = objParts[7],
                ItemCsv = objParts[8]
            };
            //TODO deserialize this once img and files added
            dbLog.ItemType = MpCopyItemType.RichText;
            return dbLog;
        }

        public string SerializeDbObject() {
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}",
                ParseToken,
                CopyItemGuid.ToString(),
                Title,
                CopyCount,
                CopyDateTime.ToString(),
                ItemText,
                ItemRtf,
                ItemHtml,
                ItemDescription,
                ItemCsv);
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
                ItemText,
                other.ItemText,
                "ItemText",
                diffLookup);
            diffLookup = CheckValue(
                ItemRtf,
                other.ItemRtf,
                "ItemRtf",
                diffLookup);
            diffLookup = CheckValue(
                ItemHtml,
                other.ItemHtml,
                "ItemHtml",
                diffLookup);
            diffLookup = CheckValue(
                ItemDescription,
                other.ItemDescription,
                "ItemDescription",
                diffLookup);
            diffLookup = CheckValue(
                ItemCsv,
                other.ItemCsv,
                "ItemCsv",
                diffLookup);

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
                    case "ItemText":
                        newCopyItem.ItemText = li.AffectedColumnValue;
                        break;
                    case "ItemRtf":
                        newCopyItem.ItemRtf = li.AffectedColumnValue;
                        break;
                    case "ItemHtml":
                        newCopyItem.ItemHtml = li.AffectedColumnValue;
                        break;
                    case "ItemDescription":
                        newCopyItem.ItemDescription = li.AffectedColumnValue;
                        break;
                    case "ItemCsv":
                        newCopyItem.ItemCsv = li.AffectedColumnValue;
                        break;
                    default:
                        MpConsole.WriteTraceLine(@"Unknown table-column: " + li.DbTableName + "-" + li.AffectedColumnName);
                        break;
                }
            }
            //newTag.WriteToDatabase(fromClientGuid);
            return newCopyItem;
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
        Composite,
        Csv //this is only used during runtime
    }
}
