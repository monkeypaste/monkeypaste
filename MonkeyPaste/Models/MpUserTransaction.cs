using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public enum MpUserTransactionType {
        None = 0,
        Input,
        Paste,
        Drop
    }
    public class MpUserTransaction : MpDbModelBase, MpITransactionError {
        #region Protected variables 
        //uses manifest iconUrl for MpISourceItem interface
        protected int iconId { get; set; } = 0;

        #endregion

        #region Columns

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpUserTransactionId")]
        public override int Id { get; set; }

        [Column("MpUserTransactionGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }


        [Column("fk_MpCopyItemId")]
        public int CopyItemId { get; set; }

        public string TransactionData { get; set; }

        [Column("fk_MpUserDeviceId")]
        public int DeviceId { get; set; }

        public string TransactionErrorMessage { get; set; }

        [Column("e_MpUserTransactionTypeId")]
        public int UserTransactionTypeId { get; set; } = 0;

        public DateTime TransactionDateTime { get; set; }


        #endregion

        #region Properties

        [Ignore]
        public Guid UserTransactionGuid {
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
        public MpUserTransactionType UserTransactionType {
            get => (MpUserTransactionType)UserTransactionTypeId;
            set => UserTransactionTypeId = (int)value;
        }
        #endregion

        public static async Task<MpUserTransaction> Create(
            int copyItemId = 0,
            int deviceId = 0,
            string transactionData = "",
            MpUserTransactionType transType = MpUserTransactionType.None,
            DateTime? transDateTime = null,
            string errorMsg = null,
            bool suppressWrite = false) {
            if(copyItemId == 0) {
                throw new Exception("Must specifiy copyitemId");
            }
            if (deviceId <= 0) {
                deviceId = MpDefaultDataModelTools.ThisUserDeviceId;
            }
            var mr = new MpUserTransaction() {
                UserTransactionGuid = System.Guid.NewGuid(),
                DeviceId = deviceId,
                CopyItemId = copyItemId,
                TransactionData = transactionData,
                UserTransactionType = transType,
                TransactionDateTime = !transDateTime.HasValue ? DateTime.Now : transDateTime.Value,
                TransactionErrorMessage = errorMsg
            };
            if (copyItemId > 0) {
                var preset = await MpDataModelProvider.GetItemAsync<MpPluginPreset>(copyItemId);
                if (preset != null) {
                    mr.iconId = preset.IconId;
                }
            }
            if (!suppressWrite) {
                await mr.WriteToDatabaseAsync();
            }
            return mr;
        }

        public MpUserTransaction() { }

        #region MpISourceItem Implementation

        [Ignore]
        public int IconId => 0;
        [Ignore]
        public string SourcePath => UserTransactionTypeId.ToString();
        [Ignore]
        public string SourceName => UserTransactionType.ToString();
        [Ignore]
        public int RootId => Id;
        [Ignore]
        public bool IsUser => true;
        [Ignore]
        public bool IsUrl => false;
        [Ignore]
        public bool IsDll => true;
        [Ignore]
        public bool IsExe => false;
        [Ignore]
        public bool IsRejected => false;
        [Ignore]
        public bool IsSubRejected => false;


        #endregion
    }
}
