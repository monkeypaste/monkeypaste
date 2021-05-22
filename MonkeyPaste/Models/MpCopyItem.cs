using System;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    public class MpCopyItem : MpDbObject {
        #region Column Definitions
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        public string Title { get; set; }

        public int TypeId { get; set; } = 0;
        [Ignore]
        public MpCopyItemType ItemType
        {
            get
            {
                return (MpCopyItemType)TypeId;
            }
            set
            {
                if (ItemType != value)
                {
                    TypeId = (int)value;
                }
            }
        }

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
        public MpColor Color { get; set; }

        public DateTime CopyDateTime { get; set; }

        public string ItemText { get; set; }

        public string ItemCsv { get; set; }

        public byte[] ItemImage { get; set; }

        public byte[] ItemScreenShot { get; set; }

        public string ItemDescription { get; set; }

        public int CopyCount { get; set; }

        public int PasteCount { get; set; }

        public string Host { get; set; }

        
        #endregion                
        
        public MpCopyItem() : base(){ }

        public MpCopyItem(object data, string sourceInfo) {
            if(data == null) {
                return;
            }
        }
        public MpCopyItem(string title, string itemPlainText) {
            Title = title;
            ItemText = itemPlainText;
            CopyDateTime = DateTime.Now;
        }

        public static async Task AddNewCopyItem(object args) {
            if (args == null) {
                return;
            }
            //create CopyItem
            string hostPackageName = (args as object[])[0] as string;
            string itemPlainText = (args as object[])[1] as string;
            var hostAppName = (args as object[])[2] as string;
            var hostAppImage = (args as object[])[3] as byte[];

            var newCopyItem = new MpCopyItem() {
                CopyDateTime = DateTime.Now,
                Title = "Text",
                ItemText = itemPlainText,
                Host = hostPackageName,
                ItemImage = hostAppImage
            };

            await MpDb.Instance.AddOrUpdate(newCopyItem);

            //add copyitem to default tags
            var defaultTagList = await MpDb.Instance.ExecuteAsync<MpTag>(
                "select * from MpTag where TagName=@tna or TagName=@tnr",
                new System.Collections.Generic.Dictionary<string, object>() {
                    {"@tna","All" },
                    {"@tnr","Recent" }
                }
             );

            if (defaultTagList != null) {
                foreach (var tag in defaultTagList) {
                    var copyItemTag = new MpCopyItemTag() {
                        CopyItemId = newCopyItem.Id,
                        TagId = tag.Id
                    };
                    await MpDb.Instance.AddItem<MpCopyItemTag>(copyItemTag);
                }
            }


            if (!string.IsNullOrEmpty(hostPackageName)) {
                //add or update copyitem's source app
                var appFromHostList = await MpDb.Instance.ExecuteAsync<MpApp>(
                    "select * from MpApp where AppPath=@hpn",
                new System.Collections.Generic.Dictionary<string, object>() {
                    {"@hpn",hostPackageName }
                }
             );
                if (appFromHostList != null && appFromHostList.Count >= 1) {
                    var app = appFromHostList[0];

                    newCopyItem.AppId = app.Id;
                    await MpDb.Instance.UpdateItem<MpCopyItem>(newCopyItem);
                } else {
                    var newIcon = new MpIcon() {
                        IconImage = hostAppImage
                    };
                    await MpDb.Instance.AddItem<MpIcon>(newIcon);

                    var newApp = new MpApp() {
                        AppPath = hostPackageName,
                        AppName = hostAppName,
                        IconId = newIcon.Id
                    };

                    await MpDb.Instance.AddItem<MpApp>(newApp);

                    newCopyItem.AppId = newApp.Id;
                    await MpDb.Instance.UpdateItem<MpCopyItem>(newCopyItem);
                }
            }
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
