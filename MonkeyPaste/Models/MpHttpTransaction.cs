using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Plugin;

namespace MonkeyPaste {
    public class MpHttpTransaction : MpDbModelBase, MpISourceTransaction {
        #region Protected variables 

        protected int iconId { get; set; } = 0;

        #endregion

        #region Columns

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpHttpTransactionId")]
        public override int Id { get; set; }

        [Column("MpHttpTransactionGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [ForeignKey(typeof(MpAnalyticItemPreset))]
        [Column("fk_MpAnalyticItemPresetId")]
        public int PresetId { get; set; }

        [ForeignKey(typeof(MpUserDevice))]
        [Column("fk_MpUserDeviceId")]
        public int DeviceId { get; set; }

        public string DestinationUrl { get; set; }
        public string DestinationName { get; set; }

        public string SourceIp { get; set; }

        public int BytesSent { get; set; }
        public int BytesReceived { get; set; }

        public string TransactionErrorMessage { get; set; }

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
            int presetId = 0,
            int deviceId = 0,
            string url = "",
            string urlName = "",
            string ip = "",
            DateTime? timeSent = null,
            DateTime? timeReceived = null,
            int bytesSent = 0,
            int bytesReceived = 0, 
            string errorMsg = null,
            bool suppressWrite = false) {
            if(string.IsNullOrEmpty(url)) {
                MpConsole.WriteTraceLine("Http transaction must have destination url");
            }
            if (string.IsNullOrEmpty(ip)) {
                MpConsole.WriteTraceLine("Http transaction must have source ip");
            }
            if (deviceId <= 0) {
                var device = await MpDataModelProvider.GetUserDeviceByGuid(MpPreferences.ThisDeviceGuid);
                if (device != null) {
                    deviceId = device.Id;
                }
            }


            var mr = new MpHttpTransaction() {
                HttpTransactionGuid = System.Guid.NewGuid(),
                PresetId = presetId,
                DeviceId = deviceId,
                SourceIp = ip,
                DestinationUrl = url,
                DestinationName = string.IsNullOrWhiteSpace(urlName) ? MpUrlHelpers.GetUrlDomain(url) : urlName,
                DateTimeSent = timeSent.HasValue ? timeSent.Value : DateTime.Now,
                DateTimeReceived = timeReceived.HasValue ? timeReceived.Value : null,
                BytesSent = bytesSent,
                BytesReceived = bytesReceived,
                TransactionErrorMessage = errorMsg
            };
            if (presetId > 0) {
                var preset = await MpDb.GetItemAsync<MpAnalyticItemPreset>(presetId);
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

        #region MpISourceItem Implementation
        [Ignore]
        public int IconId => iconId;
        [Ignore]
        public string SourcePath => DestinationUrl;
        [Ignore]
        public string SourceName => DestinationName;
        [Ignore]
        public int RootId => Id;
        [Ignore]
        public bool IsUrl => true;
        [Ignore]
        public bool IsDll => false;
        [Ignore]
        public bool IsExe => false;
        [Ignore]
        public bool IsRejected => false;
        [Ignore]
        public bool IsSubRejected => false;

        #endregion
    }
}
