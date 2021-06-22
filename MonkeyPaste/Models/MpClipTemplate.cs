using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;
using System.Linq;

namespace MonkeyPaste {
    [Table("MpCopyItemTemplate")]
    public class MpClipTemplate : MpDbModelBase {
        private static List<MpClipTag> _AllClipTagList = null;
        #region Columns
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpCopyItemTemplateId")]
        public override int Id { get; set; }

        [ForeignKey(typeof(MpClip))]
        [Column("fk_MpCopyItemId")]
        public int ClipId { get; set; }

        [ForeignKey(typeof(MpColor))]
        [Column("fk_MpColorId")]
        public int ColorId { get; set; }

        public string TemplateName { get; set; }

        [ManyToOne]
        public MpColor Color { get; set; }
        #endregion

        public MpClipTemplate() : base(typeof(MpClipTemplate)) { }
    }
}
