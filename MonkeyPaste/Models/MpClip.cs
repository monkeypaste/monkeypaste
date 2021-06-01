using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    public class MpClip : MpDbModelBase {
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
        public MpClipType ItemType {
            get {
                return (MpClipType)TypeId;
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

        //public string Host { get; set; }
        #endregion

        #region Fk Objects
        [OneToMany]
        public List<MpClipTemplate> Templates { get; set; }

        [OneToMany]
        public List<MpClip> CompositeSubItems { get; set; }

        [OneToMany]
        public List<MpPasteHistory> PasteHistoryList { get; set; }
        #endregion
        public static async Task<MpClip> GetClipById(int ClipId) {
            var allItems = await MpDb.Instance.GetItems<MpClip>();
            var udbpl = allItems.Where(x => x.Id == ClipId).ToList();
            if (udbpl.Count > 0) {
                return udbpl[0];
            }
            return null;
        }
        public static async Task<List<MpClip>> GetAllClipsByTagId(int tagId) {
            var citl = await MpClipTag.GetAllClipsForTagId(tagId);
            var cil = new List<MpClip>();
            foreach (var cit in citl) {
                var ci = await MpClip.GetClipById(cit.ClipId);
                cil.Add(ci);
            }
            return cil;
        }

        public static async Task<ObservableCollection<MpClip>> GetPage(int tagId, int start, int count, string sortColumn = "Id", bool isDescending = false) {
            //SELECT
            //user_number,
            //user_name
            //FROM user_table
            //WHERE(user_name LIKE '%{1}%' OR user_number LIKE '%{2}%')
            //AND user_category = { 3 } OR user_category = { 4 }
            //ORDER BY user_uid LIMIT { 5}
            //OFFSET { 6}
            //Where { 5} is page size and { 6 } is page number * page size.

            var result = await MpDb.Instance.QueryAsync<MpClip>(
                                string.Format(
                                    @"SELECT * from MpClip
                                      WHERE Id in 
                                        (SELECT ClipId FROM MpClipTag 
                                         WHERE TagId=?)
                                      ORDER BY {0} {1} LIMIT ? OFFSET ?",
                                    sortColumn,
                                    (isDescending ? "DESC" : "ASC")),
                                tagId,
                                count,
                                start);

            return new ObservableCollection<MpClip>(result);
        }

        public static async Task<ObservableCollection<MpClip>> Search(int tagId, string searchString) {
            var allClips = await MpDb.Instance.GetItems<MpClip>();
            var allClipTags = await MpDb.Instance.GetItems<MpClipTag>();

            var searchResult = (from ci in allClips
                                join cit in allClipTags on
                                tagId equals cit.TagId
                                where ci.ItemPlainText.ContainsByUserSensitivity(searchString)
                                select ci);//.Skip(2).Take(2);

            return new ObservableCollection<MpClip>(searchResult);
        }

        public MpClip() : base(typeof(MpClip)) { }

        public MpClip(object data, string sourceInfo) : this() {
            if(data == null) {
                return;
            }
        }
        public MpClip(string title, string itemPlainText) : this() {
            Title = title;
            ItemPlainText = itemPlainText;
            CopyDateTime = DateTime.Now;
        }

        public static async Task<MpClip> Create(object args) {
            if (args == null) {
                return null;
            }

            //create Clip
            string hostPackageName = (args as object[])[0] as string;
            string itemPlainText = (args as object[])[1] as string;
            var hostAppName = (args as object[])[2] as string;
            var hostAppImage = (args as object[])[3] as byte[];

            var newClip = new MpClip() {
                CopyDateTime = DateTime.Now,
                Title = "Text",
                ItemPlainText = itemPlainText,
                //Host = hostPackageName,
                ItemImage = hostAppImage
            };

            await MpDb.Instance.AddItem<MpClip>(newClip);

            //add Clip to default tags
            var defaultTagList = await MpDb.Instance.QueryAsync<MpTag>(
                "select * from MpTag where Id=? or Id=?","1","2");

            if (defaultTagList != null) {
                foreach (var tag in defaultTagList) {
                    var ClipTag = new MpClipTag() {
                        ClipId = newClip.Id,
                        TagId = tag.Id
                    };
                    await MpDb.Instance.AddItem<MpClipTag>(ClipTag);
                }
            }

            //add source to Clip
            if (!string.IsNullOrEmpty(hostPackageName)) {
                //add or update Clip's source app
                MpApp app = await MpApp.GetAppByPath(hostPackageName);
                if (app == null) {
                    app = await MpApp.Create(hostPackageName, hostAppName, hostAppImage);
                } 
                newClip.App = app;
                newClip.AppId = app.Id;
                await MpDb.Instance.UpdateItem<MpClip>(newClip);
            }
            return newClip;
        }
        //public override void DeleteFromDatabase() {
        //    throw new NotImplementedException();
        //}

        //public override string ToString() {
        //    throw new NotImplementedException();
        //}
    }

    public enum MpClipDetailType {
        None = 0,
        DateTimeCreated,
        DataSize,
        UsageStats
    }

    public enum MpClipType {
        None = 0,
        RichText,
        Image,
        FileList,
        Composite,
        Csv //this is only used during runtime
    }
}
