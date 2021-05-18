using System;
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
