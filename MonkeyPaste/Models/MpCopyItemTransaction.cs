using MonkeyPaste.Common;
using SQLite;


using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public enum MpCopyItemTransactionType {
        None = 0,
        Dll,
        Cli,
        Http,
        User
    }

    public class MpCopyItemTransaction : MpDbModelBase {
        #region Columns

        [Column("pk_MpCopyItemTransactionId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; } = 0;


        [Column("MpCopyItemTransactionGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("fk_MpCopyItemId")]
        public int CopyItemId { get; set; }

        [Column("fk_CopyItemTransactionObjectId")]
        public int CopyItemTransactionObjId { get; set; }


        [Column("e_MpCopyItemTransactionType")]
        public string CopyItemTransactionTypeName { get; set; } = MpCopyItemTransactionType.None.ToString();

        public string ResponseJson { get; set; }

        #endregion

        #region Properties 

        [Ignore]
        public Guid CopyItemTransactionGuid {
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
        public MpCopyItemTransactionType CopyItemTransactionType {
            get => CopyItemTransactionTypeName.ToEnum<MpCopyItemTransactionType>();
            set => CopyItemTransactionTypeName = value.ToString();
        }

        #endregion


        public static async Task<MpCopyItemTransaction> Create(
            MpCopyItemTransactionType transType = MpCopyItemTransactionType.None,
            int transObjId = 0,
            int copyItemId = 0,
            string responseJson = "",
            bool suppressWrite = false) {

            var ndio = new MpCopyItemTransaction() {
                CopyItemTransactionGuid = System.Guid.NewGuid(),
                CopyItemTransactionType = transType,
                CopyItemTransactionObjId = transObjId,
                CopyItemId = copyItemId,
                ResponseJson = responseJson
            };

            if(!suppressWrite) {
                await ndio.WriteToDatabaseAsync();
            }
            return ndio;
        }

        public MpCopyItemTransaction() { }
    }
}
