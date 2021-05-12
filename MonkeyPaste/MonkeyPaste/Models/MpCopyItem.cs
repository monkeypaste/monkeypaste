using System;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    [Table(nameof(MpCopyItem))]
    public class MpCopyItem : MpDbObject {
        #region Column Definitions
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [Column(nameof(CopyDateTime))]
        public string Title { get; set; }

        [Column(nameof(CopyDateTime))]
        private int CopyItemTypeId { get; set; }

        [Column(nameof(CopyDateTime))]
        private int CopyItemAppId { get; set; }

        [Column(nameof(CopyDateTime))]
        private int CopyItemUrlId { get; set; }

        [Column(nameof(CopyDateTime))]
        private int CopyItemColorId { get; set; }

        [Column(nameof(CopyDateTime))]
        public DateTime CopyDateTime { get; set; }

        [Column(nameof(CopyItemText))]
        public string CopyItemText { get; set; }

        [Column(nameof(CopyItemCsv))]
        public string CopyItemCsv { get; set; }

        [Column(nameof(CopyItemImage))]
        public byte[] CopyItemImage { get; set; }

        [Column(nameof(CopyItemScreenShot))]
        public byte[] CopyItemScreenShot { get; set; }

        [Column(nameof(CopyItemDescription))]
        public string CopyItemDescription { get; set; }

        [Column(nameof(CopyCount))]
        public int CopyCount { get; set; }

        [Column(nameof(PasteCount))]
        public int PasteCount { get; set; }
        #endregion
                
        
        public MpCopyItem() {
        }

        public MpCopyItem(string title,string itemPlainText) {
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
