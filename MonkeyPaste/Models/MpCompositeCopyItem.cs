using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Attributes;
using System.Linq;

namespace MonkeyPaste {
    [Table("MpCompositeCopyItem")]
    public class MpCompositeCopyItem : MpDbModelBase {
        #region Columns
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpCompositeCopyItemId")]
        public override int Id { get; set; }

        [Column("MpCompositeCopyItemGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Ignore]
        public Guid CopyItemCompositeGuid {
            get {
                if (string.IsNullOrEmpty(Guid)) {
                    return System.Guid.Empty;
                }
                return System.Guid.Parse(Guid);
            }
            set {
                Guid = value.ToString();
            }
        }

        [ForeignKey(typeof(MpCopyItem))]
        [Column("fk_MpCopyItemId")]
        public int CopyItemId { get; set; }

        [ForeignKey(typeof(MpCopyItem))]
        [Column("fk_ParentMpCopyItemId")]
        public int ParentCopyItemId { get; set; }

        public int SortOrderIdx { get; set; }

        #endregion

        public MpCompositeCopyItem() {
        }
    }
}
