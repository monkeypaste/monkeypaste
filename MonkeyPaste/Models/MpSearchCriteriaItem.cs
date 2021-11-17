using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyPaste {
    [Table("MpSearchCriteriaItem")]
    public class MpSearchCriteriaItem : MpDbModelBase {
        #region Columns
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpSearchCriteriaItemId")]
        public override int Id { get; set; }

        [Column("MpSearchCriteriaItemGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [ForeignKey(typeof(MpUserSearch))]
        [Column("fk_MpUserSearchId")]
        public int UserSearchId { get; set; } = 0;

        public string InputValue { get; set; } 

        public int SortOrderIdx { get; set; } = 0;

        #endregion

        #region Fk Models

        [OneToMany(CascadeOperations = CascadeOperation.CascadeRead)]
        public MpUserSearch UserSearch { get; set; }

        #endregion

        #region Properties

        [Ignore]
        public Guid SearchCriteriaItemGuid {
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
        #endregion

        #region Statics


        #endregion

        public MpSearchCriteriaItem() : base() { }
    }
}
