using MonkeyPaste.Common;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste {

    public enum MpTransactionType {
        None = 0,
        Created,
        Dropped,
        Dragged,
        Pasted,
        Copied,
        Cut,
        Edited,
        Analyzed,
        Error
    }

    public class MpCopyItemTransaction : MpDbModelBase {
        #region Columns

        [Column("pk_MpCopyItemTransactionId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; } = 0;


        [Column("MpCopyItemTransactionGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("fk_MpCopyItemId")]
        [Indexed]
        public int CopyItemId { get; set; }

        public string TransactionLabel { get; set; }


        [Column("e_MpJsonMessageFormatType_request")]
        public string RequestMessageFormatTypeName { get; set; }

        public string RequestMessageJson { get; set; }

        [Column("e_MpJsonMessageFormatType_response")]
        public string ResponseMessageFormatTypeName { get; set; }

        public string ResponseMessageJson { get; set; }

        [Column("fk_MpUserDeviceId")]
        public int TransactionUserDeviceId { get; set; }

        public DateTime TransactionDateTime { get; set; }

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
        public MpJsonMessageFormatType RequestMessageType {
            get => RequestMessageFormatTypeName.ToEnum<MpJsonMessageFormatType>();
            set => RequestMessageFormatTypeName = value.ToString();
        }

        [Ignore]
        public MpJsonMessageFormatType ResponseMessageType {
            get => ResponseMessageFormatTypeName.ToEnum<MpJsonMessageFormatType>();
            set => ResponseMessageFormatTypeName = value.ToString();
        }

        [Ignore]
        public MpTransactionType TransactionType {
            get => TransactionLabel.ToEnum<MpTransactionType>();
            set => TransactionLabel = value.ToString();
        }

        #endregion


        public static async Task<MpCopyItemTransaction> CreateAsync(
            int copyItemId = 0,
            MpJsonMessageFormatType reqMsgType = MpJsonMessageFormatType.None,
            string reqMsgJsonStr = "",
            MpJsonMessageFormatType respMsgType = MpJsonMessageFormatType.None,
            string respMsgJsonStr = "",
            int transUserDeviceId = 0,
            DateTime? transDateTime = null,
            MpTransactionType transactionType = MpTransactionType.None,
            bool suppressWrite = false) {

            if (copyItemId == 0 && !suppressWrite) {
                throw new Exception("Must have CopyItemId if to be written");
            }
            if (transactionType == MpTransactionType.None) {
                throw new Exception("Must have type");
            }

            var ndio = new MpCopyItemTransaction() {
                CopyItemTransactionGuid = System.Guid.NewGuid(),

                CopyItemId = copyItemId,

                //TransactionType = transType,
                //TransactionObjId = transObjId,

                RequestMessageType = reqMsgType,
                RequestMessageJson = reqMsgJsonStr,

                ResponseMessageType = respMsgType,
                ResponseMessageJson = respMsgJsonStr,

                TransactionType = transactionType,

                TransactionUserDeviceId = transUserDeviceId == 0 ? MpDefaultDataModelTools.ThisUserDeviceId : transUserDeviceId,

                TransactionDateTime = transDateTime.HasValue ? transDateTime.Value : DateTime.Now
            };

            if (!suppressWrite) {
                await ndio.WriteToDatabaseAsync();
            }
            return ndio;
        }

        public override async Task DeleteFromDatabaseAsync() {
            List<Task> delete_tasks = new List<Task>();

            var cisl = await MpDataModelProvider.GetCopyItemTransactionSourcesAsync(Id);
            delete_tasks.AddRange(cisl.Select(x => x.DeleteFromDatabaseAsync()));
            delete_tasks.Add(base.DeleteFromDatabaseAsync());

            await Task.WhenAll(delete_tasks);
        }
        public MpCopyItemTransaction() { }

    }
}
