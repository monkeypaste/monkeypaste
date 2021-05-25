using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    public class MpApp : MpDbObject, MpICopyItemSource {
        public override int Id { get; set; } = 0;
        
        public string AppPath { get; set; } = string.Empty;
        
        public string AppName { get; set; } = string.Empty;

        public int IsRejected { get; set; } = 0;

        [Ignore]
        public bool IsAppRejected
        {
            get
            {
                return IsRejected == 1;
            }
            set
            {
                if(IsAppRejected != value)
                {
                    IsRejected = value ? 1 : 0;
                }
            }
        }

        [ForeignKey(typeof(MpIcon))]
        public int IconId { get; set; }

        [OneToOne]
        public MpIcon Icon { get; set; }

        public MpApp() : base(typeof(MpApp)) { }

        #region MpICopyItemSource Implementation
        public MpIcon SourceIcon => Icon;

        public string SourcePath => AppPath;

        public string SourceName => AppName;

        public int RootId => Id;

        public bool IsSubSource => false;
        #endregion
    }
}
