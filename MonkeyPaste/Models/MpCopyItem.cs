using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    [Table(nameof(MpCopyItem))]
    public class MpCopyItem : MpDbObject {
        private static List<MpCopyItem> _AllCopyItemsList = null;

        #region Column Definitions
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        //[ForeignKey(typeof(MpSource))]
        //public int SourceId { get; set; }
        //[ManyToOne]
        //public MpSource Source { get; set; }

        [ForeignKey(typeof(MpApp))]
        public int AppId { get; set; }
        [ManyToOne]
        public MpApp App { get; set; }

        [ForeignKey(typeof(MpUrl))]
        public int UrlId { get; set; }
        [ManyToOne]
        public MpUrl Url { get; set; }

        [ForeignKey(typeof(MpColor))]
        public int ColorId { get; set; }
        [ManyToOne]
        public MpColor ItemColor { get; set; }

        public string Title { get; set; }

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

        public string ItemPlainText { get; set; }

        public string ItemRichText { get; set; }

        public string ItemCsv { get; set; }

        public byte[] ItemImage { get; set; }

        public byte[] ItemScreenShot { get; set; }

        public string ItemDescription { get; set; }

        public int CopyCount { get; set; }

        public int PasteCount { get; set; }

        public string Host { get; set; }
        #endregion

        #region Fk Objects
        [OneToMany]
        public List<MpCopyItemTemplate> Templates { get; set; }

        [OneToMany]
        public List<MpCopyItem> CompositeSubItems { get; set; }

        [OneToMany]
        public List<MpPasteHistory> PasteHistoryList { get; set; }
        #endregion

        public static async Task<List<MpCopyItem>> GetAllCopyItems() {
            if (_AllCopyItemsList == null) {
                _AllCopyItemsList = await MpDb.Instance.GetItems<MpCopyItem>();
            }
            return _AllCopyItemsList;
        }
        public static async Task<MpCopyItem> GetCopyItemById(int copyItemId) {
            if (_AllCopyItemsList == null) {
                await GetAllCopyItems();
            }
            var udbpl = _AllCopyItemsList.Where(x => x.Id == copyItemId).ToList();
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

        public static async Task<ObservableCollection<MpCopyItem>> GetPage(int tagId, int start, int count, string sortColumn = "Id", bool isDescending = false) {
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
                                      WHERE Id in 
                                        (SELECT CopyItemId FROM MpCopyItemTag 
                                         WHERE TagId=?)
                                      ORDER BY {0} {1} LIMIT ? OFFSET ?",
                                    sortColumn,
                                    (isDescending ? "DESC" : "ASC")),
                                tagId,
                                count,
                                start);

            return new ObservableCollection<MpCopyItem>(result);
        }

        public static async Task<ObservableCollection<MpCopyItem>> Search(int tagId, string searchString) {
            IEnumerable<MpCopyItem> searchResult = null;
            await Task.Run(() => {
                searchResult = (from ci in _AllCopyItemsList
                              join cit in MpCopyItemTag.GetAllCopyItemsTags().Result on
                              tagId equals cit.TagId
                              where ci.ItemPlainText.ContainsByUserSensitivity(searchString)
                              select ci);//.Skip(2).Take(2);
            });
            
            return new ObservableCollection<MpCopyItem>(searchResult);
        }

        public MpCopyItem() : base(typeof(MpCopyItem)) { }

        public MpCopyItem(object data, string sourceInfo) : this() {
            if(data == null) {
                return;
            }
        }
        public MpCopyItem(string title, string itemPlainText) : this() {
            Title = title;
            ItemPlainText = itemPlainText;
            CopyDateTime = DateTime.Now;
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

            var newCopyItem = new MpCopyItem() {
                CopyDateTime = DateTime.Now,
                Title = "Text",
                ItemPlainText = itemPlainText,
                Host = hostPackageName,
                ItemImage = hostAppImage
            };

            await MpDb.Instance.AddItem<MpCopyItem>(newCopyItem);

            //add copyitem to default tags
            var defaultTagList = await MpDb.Instance.QueryAsync<MpTag>(
                "select * from MpTag where TagName=? or TagName=?","All","Recent");

            if (defaultTagList != null) {
                foreach (var tag in defaultTagList) {
                    var copyItemTag = new MpCopyItemTag() {
                        CopyItemId = newCopyItem.Id,
                        TagId = tag.Id
                    };
                    await MpDb.Instance.AddItem<MpCopyItemTag>(copyItemTag);

                    //await MpDb.Instance.UpdateWithChildren(tag);
                }
            }

            //add source to copyitem
            if (!string.IsNullOrEmpty(hostPackageName)) {
                //add or update copyitem's source app
                MpApp app = await MpApp.GetAppByPath(hostPackageName);
                if (app == null) {
                    app = await MpApp.Create(hostPackageName, hostAppName, hostAppImage);
                } 
                newCopyItem.App = app;
                newCopyItem.AppId = app.Id;
                await MpDb.Instance.UpdateItem<MpCopyItem>(newCopyItem);
            }
            return newCopyItem;
        }
        //public override void DeleteFromDatabase() {
        //    throw new NotImplementedException();
        //}

        //public override string ToString() {
        //    throw new NotImplementedException();
        //}
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
