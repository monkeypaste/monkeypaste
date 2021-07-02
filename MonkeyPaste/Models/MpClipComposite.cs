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
    public class MpClipComposite : MpDbModelBase {
        #region Columns
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpCompositeCopyItemId")]
        public override int Id { get; set; }

        [Column("MpCompositeCopyItemGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Ignore]
        public Guid ClipCompositeGuid {
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

        [ForeignKey(typeof(MpClip))]
        [Column("fk_MpCopyItemId")]
        public int ClipId { get; set; }

        [ForeignKey(typeof(MpClip))]
        [Column("fk_ParentMpCopyItemId")]
        public int ParentClipId { get; set; }

        public int SortOrderIdx { get; set; }

        #endregion

        public MpClipComposite() : base(typeof(MpClipComposite)) {
            ClipCompositeGuid = System.Guid.NewGuid();
        }
    }
}
