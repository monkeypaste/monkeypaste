using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    public class MpFileContentItem : MpDbObject {
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [ForeignKey(typeof(MpCopyItem))]
        public int CopyItemId { get; set; }

        [OneToOne]
        public MpCopyItem CopyItem { get; set; }

        public string FileOrDirectoryPath { get; set; }
    }
}
