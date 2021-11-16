using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;

namespace MonkeyPaste {
    public class MpUserSearch : MpDbModelBase {
        #region Columns
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpUserSearchId")]
        public override int Id { get; set; }

        [Column("MpUserSearchGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        public string Name { get; set; }

        #endregion

        #region Fk Models

        [OneToMany(CascadeOperations = CascadeOperation.All)]
        public List<MpSearchCriteriaItem> CriteriaItems { get; set; } = new List<MpSearchCriteriaItem>();

        #endregion

        #region Properties

        [Ignore]
        public Guid UserSearchGuid {
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

        public MpUserSearch() : base() { }
    }
}
