using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;

namespace MonkeyPaste {
    public class MpHttpTransaction : MpDbModelBase, MpITransactionError, MpIPluginPresetTransaction {
        #region Protected variables 

        protected int iconId { get; set; } = 0;

        #endregion

        #region Columns

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpHttpTransactionId")]
        public override int Id { get; set; }

        [Column("MpHttpTransactionGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("fk_MpAnalyticItemPresetId")]
        public int PresetId { get; set; }

        [Column("fk_MpUserDeviceId")]
        public int DeviceId { get; set; }

        //public string DestinationUrl { get; set; }
        //public string DestinationName { get; set; }
        [Column("fk_MpUrlId")]
        public int UrlId { get; set; }

        public string Args { get; set; }

        public string SourceIp { get; set; }

        public int BytesSent { get; set; }
        public int BytesReceived { get; set; }

        public string TransactionErrorMessage { get; set; }

        public DateTime DateTimeSent { get; set; }
        public DateTime? DateTimeReceived { get; set; }

        #endregion

        #region MpIPluginPresetTransaction Implementation

        [Ignore]
        MpCopyItemSourceType MpIPluginPresetTransaction.TransactionType => MpCopyItemSourceType.Http;

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
            int presetId = 0,
            int deviceId = 0,
            string reqArgs = "",
            int urlId = 0,
            //string urlName = "",
            string ip = "",
            DateTime? timeSent = null,
            DateTime? timeReceived = null,
            int bytesSent = 0,
            int bytesReceived = 0, 
            string errorMsg = null,
            bool suppressWrite = false) {
            if(urlId == 0) {
                //MpConsole.WriteTraceLine("Http transaction must have destination url");
                throw new Exception("Needs url id");
            }
            if (string.IsNullOrEmpty(ip)) {
                MpConsole.WriteTraceLine("Http transaction must have source ip");
            }
            if (deviceId <= 0) {
                deviceId = MpDefaultDataModelTools.ThisUserDeviceId;
            }
            if(string.IsNullOrEmpty(ip)) {
                ip = await MpNetworkHelpers.GetExternalIp4AddressAsync();
            }

            var mr = new MpHttpTransaction() {
                HttpTransactionGuid = System.Guid.NewGuid(),
                PresetId = presetId,
                DeviceId = deviceId,
                SourceIp = ip,
                Args = reqArgs,
                UrlId = urlId,
                DateTimeSent = timeSent.HasValue ? timeSent.Value : DateTime.Now,
                DateTimeReceived = timeReceived.HasValue ? timeReceived.Value : null,
                BytesSent = bytesSent,
                BytesReceived = bytesReceived,
                TransactionErrorMessage = errorMsg
            };
            if (presetId > 0) {
                var preset = await MpDataModelProvider.GetItemAsync<MpPluginPreset>(presetId);
                if (preset != null) {
                    mr.iconId = preset.IconId;
                }
            }

            if(!suppressWrite) {
                await mr.WriteToDatabaseAsync();
            }
            return mr;
        }

        public MpHttpTransaction() { }

        //#region MpISourceItem Implementation
        //[Ignore]
        //public int IconId => iconId;
        //[Ignore]
        //public string SourcePath => DestinationUrl;
        //[Ignore]
        //public string SourceName => DestinationName;
        //[Ignore]
        //public int RootId => Id;
        //[Ignore]
        //public bool IsUrl => true;
        //[Ignore]
        //public bool IsUser => false;
        //[Ignore]
        //public bool IsDll => false;
        //[Ignore]
        //public bool IsExe => false;
        //[Ignore]
        //public bool IsRejected => false;
        //[Ignore]
        //public bool IsSubRejected => false;

        //#endregion
    }
}
