using System;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    public class MpCopyItem : MpDbObject {
        #region Column Definitions
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        public string Title { get; set; }

        private int TypeId { get; set; }

        [ForeignKey(typeof(MpApp))]
        private int AppId { get; set; }

        private int CopyItemUrlId { get; set; }

        private int CopyItemColorId { get; set; }

        public DateTime CopyDateTime { get; set; }

        public string CopyItemText { get; set; }

        public byte[] CopyItemImage { get; set; }

        public byte[] CopyItemScreenShot { get; set; }

        public string CopyItemDescription { get; set; }

        public int CopyCount { get; set; }

        public int PasteCount { get; set; }
        #endregion
                
        
        public MpCopyItem() : base(){ }

        public MpCopyItem(object data, string sourceInfo) {
            if(data == null) {
                return;
            }

        }
        public MpCopyItem(string title, string itemPlainText) {
            Title = title;
            CopyItemText = itemPlainText;
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
