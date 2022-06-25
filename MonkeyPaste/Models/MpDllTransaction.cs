using SQLite;
using SQLiteNetExtensions.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpDllTransaction : MpDbModelBase, MpISourceTransaction {
        #region Protected variables 
        //uses manifest iconUrl for MpISourceItem interface
        protected int iconId { get; set; } = 0;

        #endregion

        #region Columns

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpDllTransactionId")]
        public override int Id { get; set; }

        [Column("MpDllTransactionGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }


        [ForeignKey(typeof(MpAnalyticItemPreset))]
        [Column("fk_MpAnalyticItemPresetId")]
        public int PresetId { get; set; }

        [ForeignKey(typeof(MpUserDevice))]
        [Column("fk_MpUserDeviceId")]
        public int DeviceId { get; set; }

        public string TransactionErrorMessage { get; set; }

        public string DllPath { get; set; }
        public string DllName { get; set; }

        public string Args { get; set; }

        public DateTime TransactionDateTime { get; set; }


        #endregion

        #region Fk Objects


        #endregion

        #region Properties

        [Ignore]
        public Guid DllTransactionGuid {
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

        public static async Task<MpDllTransaction> Create(
            int presetId = 0,
            int deviceId = 0,
            string dllPath = "",
            string dllName = "",
            string args = "",
            DateTime? transDateTime = null,
            string errorMsg = null,
            bool suppressWrite = false) {
            if(string.IsNullOrWhiteSpace(dllPath)) {
                throw new Exception("Must specifiy path");
            }
            if (deviceId <= 0) {
                deviceId = MpPreferences.ThisUserDevice.Id;
            }
            var mr = new MpDllTransaction() {
                DllTransactionGuid = System.Guid.NewGuid(),
                PresetId = presetId,
                DllPath = dllPath,
                DllName = string.IsNullOrWhiteSpace(dllName) ? Path.GetFileNameWithoutExtension(dllPath) : dllName,
                Args = args,
                TransactionDateTime = !transDateTime.HasValue ? DateTime.Now : transDateTime.Value,
                TransactionErrorMessage = errorMsg
            };
            if (presetId > 0) {
                var preset = await MpDb.GetItemAsync<MpAnalyticItemPreset>(presetId);
                if (preset != null) {
                    mr.iconId = preset.IconId;
                }
            }
            if (!suppressWrite) {
                await mr.WriteToDatabaseAsync();
            }
            return mr;
        }

        public MpDllTransaction() { }

        //#region MpISourceItem Implementation

        //[Ignore]
        //public int IconId => 0;
        //[Ignore]
        //public string SourcePath => DllPath;
        //[Ignore]
        //public string SourceName => DllName;
        //[Ignore]
        //public int RootId => Id;
        //[Ignore]
        //public bool IsUser => false;
        //[Ignore]
        //public bool IsUrl => false;
        //[Ignore]
        //public bool IsDll => true;
        //[Ignore]
        //public bool IsExe => false;
        //[Ignore]
        //public bool IsRejected => false;
        //[Ignore]
        //public bool IsSubRejected => false;


        //#endregion
    }
}
