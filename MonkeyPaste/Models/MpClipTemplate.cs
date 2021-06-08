using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;
using System.Linq;

namespace MonkeyPaste {
    public class MpClipTemplate : MpDbModelBase {
        private static List<MpClipTag> _AllClipTagList = null;
        #region Columns
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; } = -1;

        [ForeignKey(typeof(MpClip))]
        public int ClipId { get; set; }

        [ForeignKey(typeof(MpColor))]
        public int ColorId { get; set; }

        public string TemplateName { get; set; }

        [ManyToOne]
        public MpColor Color { get; set; }
        #endregion

        public MpClipTemplate() : base(typeof(MpClipTemplate)) { }
    }
}
