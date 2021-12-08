using System;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
    public enum MpTagPropertyType {
        None = 0,
        DirectoryWatcher,
        Filter
    }

    [Table("MpTagProperty")]
    public class MpTagProperty : MpDbModelBase {
        #region Columns
        [PrimaryKey, AutoIncrement]
        [Column("pk_MpTagPropertyId")]
        public override int Id { get; set; }

        [Column("fk_MpTagId")]
        [ForeignKey(typeof(MpTag))]
        public int TagId { get; set; } = 0;

        [Column("MpTagPropertyGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        public int PropertyTypeId { get; set; } = 0;

        public string PropertyData { get; set; } = string.Empty;

        [Ignore]
        public Guid TagPropertyGuid {
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


        [Ignore]
        public MpTagPropertyType PropertyType {
            get {
                return (MpTagPropertyType)PropertyTypeId;
            }
            set {
                if (PropertyType != value) {
                    PropertyTypeId = (int)value;
                }
            }
        }

        #endregion

        #region Fk Models

        [ManyToOne]
        public MpTag Tag { get; set; }
        
        #endregion

        public MpTagProperty() : base() { }
    }
}
