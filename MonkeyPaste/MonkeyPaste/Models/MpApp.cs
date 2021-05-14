using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    public class MpApp : MpDbObject {
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; } = 0;
                
        public string AppPath { get; set; } = string.Empty;
        
        public string AppName { get; set; } = string.Empty;
        
        private int IsRejected { get; set; }

        [ForeignKey(typeof(MpIcon))]
        private int IconId { get; set; }

        
    }
}
