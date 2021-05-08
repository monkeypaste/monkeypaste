using System;
using SQLite;

namespace MonkeyPaste.Models {
    public class MpCopyItem {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime CopyDateTime { get; set; }                
        public string ItemPlainText { get; set; }
        
        public MpCopyItem() { }

        public MpCopyItem(string title,string itemPlainText) {
            Title = title;
            ItemPlainText = itemPlainText;
            CopyDateTime = DateTime.Now;
        }
    }
}
