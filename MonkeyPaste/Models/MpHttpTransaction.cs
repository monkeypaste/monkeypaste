using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpHttpTransaction : MpDbModelBase {
        #region Columns

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpHttpTransactionId")]
        public override int Id { get; set; }

        [Column("MpHttpTransactionGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        public string DestinationUrl { get; set; }

        public string SourceIp { get; set; }

        public int BytesSent { get; set; }
        public int BytesReceived { get; set; }

        public DateTime DateTimeSent { get; set; }
        public DateTime? DateTimeReceived { get; set; }

        #endregion

        #region Fk Objects


        #endregion

        #region Properties

        [Ignore]
        public Guid HttpTransactionGuid {
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

        public static async Task<MpHttpTransaction> Create(
            string url = "",
            string ip = "",
            DateTime? timeSent = null,
            DateTime? timeReceived = null,
            int bytesSent = 0,
            int bytesReceived = 0) {
            if(string.IsNullOrEmpty(url)) {
                MpConsole.WriteTraceLine("Http transaction must have destination url");
            }
            if (string.IsNullOrEmpty(ip)) {
                MpConsole.WriteTraceLine("Http transaction must have source ip");
            }
            //var dupCheck = await MpDataModelProvider.GetHttpTransactionByLabel(label);
            //if(dupCheck != null) {
            //    MpConsole.WriteTraceLine("HttpTransaction must have unique name");
            //    return null;
            //}


            var mr = new MpHttpTransaction() {
                HttpTransactionGuid = System.Guid.NewGuid(),
                SourceIp = ip,
                DestinationUrl = url,
                DateTimeSent = timeSent.HasValue ? timeSent.Value : DateTime.Now,
                DateTimeReceived = timeReceived.HasValue ? timeReceived.Value : null,
                BytesSent = bytesSent,
                BytesReceived = bytesReceived
            };

            await mr.WriteToDatabaseAsync();
            return mr;
        }

        public MpHttpTransaction() { }
    }
}
