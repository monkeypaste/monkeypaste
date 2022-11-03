using SQLite;

using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste {
    public enum MpBillableItemType {
        None = 0,
        ThisApp,
        Analyzer
    }

    public enum MpPeriodicCycleType {
        None = 0,
        Daily,
        Weekly,
        BiWeekly,
        Monthly,
        Yearly,
        Indefinite
    }

    public class MpBillableItem : MpDbModelBase {
        #region Columns
        [Column("pk_MpBillableItemId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [Column("MpBillableItemGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("e_MpBillableItemTypeId")]
        public int BillableItemTypeId { get; set; } = 0;

        [Column("e_MpPeriodicCycleTypeId")]
        public int PeriodicCycleTypeId { get; set; } = 0;

        public string PluginGuid { get; set; } = string.Empty;

        public string BillableItemName { get; set; } = string.Empty;

        public int MaxRequestCountPerCycle { get; set; }
        public int CurrentCycleRequestCount { get; set; }

        public int MaxRequestByteCount { get; set; }

        public int MaxRequestByteCountPerCycle { get; set; }
        public int CurrentCycleRequestByteCount { get; set; }

        public int MaxResponseBytesPerCycle { get; set; }
        public int CurrentCycleResponseByteCount { get; set; }

        public DateTime NextPaymentDateTime { get; set; }
    #endregion

    #region Fk Models


    #endregion

    #region Properties

    [Ignore]
        public MpBillableItemType BillableItemType {
            get {
                return (MpBillableItemType)BillableItemTypeId;
            }
            set {
                BillableItemTypeId = (int)value;
            }
        }

        [Ignore]
        public MpPeriodicCycleType CycleType {
            get {
                return (MpPeriodicCycleType)PeriodicCycleTypeId;
            }
            set {
                PeriodicCycleTypeId = (int)value;
            }
        }

        #endregion
    }
}
