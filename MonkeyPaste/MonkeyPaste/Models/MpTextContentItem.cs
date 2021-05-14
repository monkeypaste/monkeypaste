using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    public class MpTextContentItem : MpDbObject {
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [ForeignKey(typeof(MpCopyItem))]
        public int CopyItemId { get; set; }

        [OneToOne]
        public MpCopyItem CopyItem { get; set; }

        public string Rtf { get; set; }

        public string Text { get; set; }

        public string Csv { get; set; }

        public string Html { get; set; }

        public string Xaml { get; set; }
    }
}
