using SQLite;
using SQLiteNetExtensions.Attributes;
using System;

namespace MonkeyPaste {
    public class MpAnalyticItemActivityParameter : MpDbModelBase {
        #region Columns
        [Column("pk_MpAnalyticItemActivityParameterId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [Column("MpAnalyticItemActivityParameterGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [ForeignKey(typeof(MpAnalyticItemActivity))]
        [Column("fk_MpAnalyticItemActivityId")]
        public int AnalyticItemActivityId { get; set; }

        [ForeignKey(typeof(MpAnalyticItemParameter))]
        [Column("fk_MpAnalyticItemParameterId")]
        public int AnalyticItemParameterId { get; set; }

        [Column("SortOrderIdx")]
        public int SortOrderIdx { get; set; } = 0;

        [Column("IsRequired")]
        public int IsRequired { get; set; } = 0;
        #endregion

        #region Fk Models

        [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public MpAnalyticItemParameter Parameter { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public MpAnalyticItemActivity Activity { get; set; }

        #endregion

        #region Properties

        [Ignore]
        public Guid AnalyticItemActivityParameterGuid {
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

        public MpAnalyticItemActivityParameter() : base() { }

    }
}
