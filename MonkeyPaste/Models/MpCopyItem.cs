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

        public string ItemText { get; set; } = string.Empty;

        public string ItemRtf { get; set; } = string.Empty;

        public string ItemHtml { get; set; } = string.Empty;

        public string ItemCsv { get; set; } = string.Empty;

        [ForeignKey(typeof(MpDbImage))]
        [Column("fk_MpDbImageId")]
        public int ItemImageId { get; set; }


        [ForeignKey(typeof(MpDbImage))]
        [Column("fk_SsMpDbImageId")]
        public int SsDbImageId { get; set; }

        public string ItemDescription { get; set; } = string.Empty;

        public int CopyCount { get; set; } = 0;

        public int PasteCount { get; set; } = 0;

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

        [Column("HexColor")]
        public string ItemColor { get; set; }

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
        public static async Task<List<MpCopyItem>> GetAllCopyItemsByTagId(int tagId) {
            var citl = await MpCopyItemTag.GetAllCopyItemsForTagId(tagId);
            var cil = new List<MpCopyItem>();
            foreach (var cit in citl) {
                var ci = await MpCopyItem.GetCopyItemByIdAsync(cit.CopyItemId);
                cil.Add(ci);
            }
            return cil;
        }

        public static async Task<ObservableCollection<MpCopyItem>> GetPageAsync(
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

        public static async Task<ObservableCollection<MpCopyItem>> SearchAsync(int tagId, string searchString) {
            var allCopyItems = await MpDb.Instance.GetItemsAsync<MpCopyItem>();
            var allCopyItemTags = await MpDb.Instance.GetItemsAsync<MpCopyItemTag>();

            var searchResult = (from ci in allCopyItems
                                join cit in allCopyItemTags on
                                tagId equals cit.TagId
                                where ci.ItemText.ContainsByUserSensitivity(searchString)
                                select ci);//.Skip(2).Take(2);

            return new ObservableCollection<MpCopyItem>(searchResult);
        }

        public static MpCopyItem Create(string sourcePath, string sourceName, string sourceIconImg64, string data, MpCopyItemType itemType) {
            MpApp app = MpApp.GetAppByPath(sourcePath);
            if (app == null) {
                var icon = MpIcon.GetIconByImageStr(sourceIconImg64);
                if (icon == null) {
                    icon = MpIcon.Create(sourceIconImg64);
                }
                app = MpApp.Create(sourcePath, sourceName, icon);
            } 

            var newCopyItem = new MpCopyItem() { 
                CopyDateTime = DateTime.Now,
                Title = "Untitled",
                ItemText = data,
                ItemType = itemType,
                ItemColor = MpHelpers.Instance.GetRandomColor().ToHex(),
                AppId = app.Id,
                App = app,
                CopyCount = 1
                //ItemImage = hostAppImage
            };

            MpDb.Instance.AddOrUpdate<MpCopyItem>(newCopyItem);

            //add CopyItem to default tags
            //var defaultTagList = MpDb.Instance.Query<MpTag>(
            //    "select * from MpTag where pk_MpTagId=? or pk_MpTagId=?", MpTag.AllTagId, MpTag.RecentTagId);

            //if (defaultTagList != null) {
            //    foreach (var tag in defaultTagList) {
            //        var CopyItemTag = new MpCopyItemTag() {
            //            CopyItemId = newCopyItem.Id,
            //            TagId = tag.Id
            //        };
            //        MpDb.Instance.AddItem<MpCopyItemTag>(CopyItemTag);
            //    }
            //}
            return newCopyItem;
        }

        public static async Task<MpCopyItem> CreateAsync(object args) {
            if (args == null) {
                return null;
            }

            //create CopyItem
            string hostPackageName = (args as object[])[0] as string;
            string itemPlainText = (args as object[])[1] as string;
            var hostAppName = (args as object[])[2] as string;
            var hostAppImage = (args as object[])[3] as byte[];
            var hostAppImageBase64 = (args as object[])[4] as string;


            MpIcon icon = null;
            MpApp app = MpApp.GetAppByPath(hostPackageName);
            if (app == null) {
                icon = await MpIcon.GetIconByImageStrAsync(hostAppImageBase64);
                if (icon == null) {
                    icon = await MpIcon.CreateAsync(hostAppImageBase64);
                }
                app = await MpApp.CreateAsync(hostPackageName, hostAppName, icon);
            } else {
                icon = app.Icon;
            }

            var newCopyItem = new MpCopyItem() {
                CopyDateTime = DateTime.Now,
                Title = "Untitled",
                ItemText = itemPlainText,
                ItemType = MpCopyItemType.RichText,
                ItemColor = MpHelpers.Instance.GetRandomColor().ToHex(),
                AppId = app.Id,
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

        public MpCopyItem() : base() { }

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
            var ci = new MpCopyItem() {
                CopyItemGuid = System.Guid.Parse(objParts[0]),
                Title = objParts[1],
                CopyCount = Convert.ToInt32(objParts[2]),
                CopyDateTime = DateTime.Parse(objParts[3]),
                ItemText = objParts[4],
                ItemRtf = objParts[5],
                ItemHtml = objParts[6],
                ItemDescription = objParts[7],
                ItemCsv = objParts[8],
                ItemType = (MpCopyItemType)Convert.ToInt32(objParts[9])
            };
            ci.App = MpDb.Instance.GetDbObjectByTableGuid("MpApp", objParts[10]) as MpApp;
            //TODO deserialize this once img and files added
            //ci.ItemType = MpCopyItemType.RichText;
            return ci;
        }

        public string SerializeDbObject() {
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}{0}{9}{0}{10}{0}{11}{0}",
                ParseToken,
                CopyItemGuid.ToString(),
                Title,
                CopyCount,
                CopyDateTime.ToString(),
                ItemText,
                ItemRtf,
                ItemHtml,
                ItemDescription,
                ItemCsv,
                ((int)ItemType).ToString(),
                App.AppGuid.ToString()
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
            diffLookup = CheckValue(
                AppId,
                other.AppId,
                "fk_MpAppId",
                diffLookup,
                App.AppGuid.ToString());
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
                    case "fk_MpAppId":
                        newCopyItem.App = await MpApp.GetAppByGuid(li.AffectedColumnValue);
                        newCopyItem.AppId = Convert.ToInt32(newCopyItem.App.Id);
                        break;
                    case "fk_MpCopyItemTypeId":
                        newCopyItem.ItemType = (MpCopyItemType)Convert.ToInt32(li.AffectedColumnValue);
                        break;
                    default:
                        MpConsole.WriteTraceLine(@"Unknown table-column: " + li.DbTableName + "-" + li.AffectedColumnName);
                        break;
                }
            }
            if (string.IsNullOrEmpty(newCopyItem.ItemHtml) &&
               !string.IsNullOrEmpty(newCopyItem.ItemText)) {
                newCopyItem.ItemHtml = newCopyItem.ItemText;
            }
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
