using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    [Table("MpCopyItem")]
    public class MpClip : MpDbModelBase {
        #region Column Definitions
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpCopyItemId")]
        public override int Id { get; set; }

        [Column("MpCopyItemGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }
        [Ignore]
        public Guid ClipGuid {
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
        [ManyToOne]
        public MpApp App { get; set; }

        [ForeignKey(typeof(MpUrl))]
        [Column("fk_MpUrlId")]
        public int UrlId { get; set; }
        [ManyToOne]
        public MpUrl Url { get; set; }

        [ForeignKey(typeof(MpColor))]
        [Column("fk_MpColorId")]
        public int ColorId { get; set; }
        [ManyToOne]
        public MpColor ItemColor { get; set; }

        public string Title { get; set; }

        [Column("fk_MpCopyItemTypeId")]
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

        public string ItemText { get; set; }

        public string ItemRtf { get; set; }

        public string ItemHtml { get; set; }

        public string ItemCsv { get; set; }

        [ForeignKey(typeof(MpDbImage))]
        [Column("fk_MpDbImageId")]
        public int ItemImageId { get; set; }
        [OneToOne]
        [Ignore]
        public MpDbImage ItemDbImage{ get; set; }

        [ForeignKey(typeof(MpDbImage))]
        [Column("fk_SsMpDbImageId")]
        public int SsDbImageId { get; set; }
        [OneToOne]
        [Ignore]
        public MpDbImage SsDbImage { get; set; }

        

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

        #region Static Methods
        public static async Task<MpClip> GetClipById(int ClipId) {
            var allItems = await MpDb.Instance.GetItemsAsync<MpClip>();
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

        public static async Task<ObservableCollection<MpClip>> GetPage(
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

            var result = await MpDb.Instance.QueryAsync<MpClip>(
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

            return new ObservableCollection<MpClip>(result);
        }

        public static async Task<ObservableCollection<MpClip>> Search(int tagId, string searchString) {
            var allClips = await MpDb.Instance.GetItemsAsync<MpClip>();
            var allClipTags = await MpDb.Instance.GetItemsAsync<MpClipTag>();

            var searchResult = (from ci in allClips
                                join cit in allClipTags on
                                tagId equals cit.TagId
                                where ci.ItemText.ContainsByUserSensitivity(searchString)
                                select ci);//.Skip(2).Take(2);

            return new ObservableCollection<MpClip>(searchResult);
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
            var hostAppImageBase64 = (args as object[])[4] as string;
            var newClip = new MpClip() {
                CopyDateTime = DateTime.Now,
                Title = "Text",
                ItemText = itemPlainText,
                //Host = hostPackageName,
                //ItemImage = hostAppImage
            };

            //await MpDb.Instance.AddItem<MpClip>(newClip);

            //add source to Clip
            if (!string.IsNullOrEmpty(hostPackageName)) {
                //add or update Clip's source app
                MpApp app = await MpApp.GetAppByPath(hostPackageName);
                if (app == null) {
                    app = await MpApp.Create(hostPackageName, hostAppName, hostAppImageBase64);
                }
                newClip.App = app;
                newClip.AppId = app.Id;
                await MpDb.Instance.AddOrUpdateAsync<MpClip>(newClip);

                //add Clip to default tags
                var defaultTagList = await MpDb.Instance.QueryAsync<MpTag>(
                    "select * from MpTag where pk_MpTagId=? or pk_MpTagId=?", "1", "2");

                if (defaultTagList != null) {
                    foreach (var tag in defaultTagList) {
                        var ClipTag = new MpClipTag() {
                            ClipId = newClip.Id,
                            TagId = tag.Id
                        };
                        await MpDb.Instance.AddItemAsync<MpClipTag>(ClipTag);
                    }
                }
                return newClip;
            }

            MpConsole.WriteTraceLine($"Error creating clip, there was no source application");
            return newClip;
        }
        #endregion

        public MpClip() {
        }

        public MpClip(object data, string sourceInfo) : this() {
            if(data == null) {
                return;
            }
        }
        public MpClip(string title, string itemPlainText) : this() {
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
