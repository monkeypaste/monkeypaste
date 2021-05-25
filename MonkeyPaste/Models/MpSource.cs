using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public class MpSource : MpDbObject {
        [PrimaryKey,AutoIncrement]
        public override int Id { get; set; }

        [ForeignKey(typeof(MpUrl))]
        public int UrlId { get; set; }
        [Ignore]
        public MpUrl Url { get; set; }

        [ForeignKey(typeof(MpApp))]
        public int AppId { get; set; }
        [Ignore]
        public MpApp App { get; set; }

        [Ignore]
        public MpICopyItemSource PrimarySource {
            get {
                if (UrlId <= 0) {
                    if (AppId <= 0) {
                        return null;
                    } else if (App != null) {
                        return App;
                    }
                } else if (Url != null) {
                    return Url;
                }
                return null;
            }
        }

        [Ignore]
        public MpICopyItemSource SecondarySource {
            get {
                var ps = PrimarySource;
                if(ps != null) {
                    return ps is MpApp ? Url : App;
                }
                return null;
            }
        }

        public MpSource() : base(typeof(MpSource)) { }
    }
}
