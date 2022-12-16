using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpCliTransaction : MpDbModelBase, MpITransactionError {
        #region Protected variables 

        protected int iconId { get; set; } = 0;

        #endregion

        #region Columns

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpCliTransactionId")]
        public override int Id { get; set; }

        [Column("MpCliTransactionGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("fk_MpAnalyticItemPresetId")]
        public int PresetId { get; set; }

        //[Column("fk_MpUserDeviceId")]
        public int DeviceId { get; set; }

        //public string CliPath { get; set; }
        //public string CliName { get; set; }

        public int AppId { get; set; }
        public string WorkingDirectory { get; set; }
        public string Args { get; set; }

        public string TransactionErrorMessage { get; set; }

        [Column("IsAdmin")]
        public int Admin { get; set; }

        [Column("IsSilent")]
        public int Silent { get; set; }

        public DateTime TransactionDateTime { get; set; }


        #endregion

        #region Properties

        [Ignore]
        public Guid CliTransactionGuid {
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
        public bool IsSilent {
            get {
                return Silent == 1;
            }
            set {
                Silent = value == true ? 1 : 0;
            }
        }

        [Ignore]
        public bool IsAdmin {
            get {
                return Admin == 1;
            }
            set {
                Admin = value == true ? 1 : 0;
            }
        }
        #endregion

        public static async Task<MpCliTransaction> Create(
            int presetId = 0,
            int deviceId = 0,
            //string cliPath = "",
            //string cliName = "",
            int appId = 0,
            string workingDirectory = "",
            string args = "",
            bool isAdmin = false,
            bool isSilent = false,
            DateTime? transDateTime = null,
            string errorMsg = null,
            bool suppressWrite = false) {
            if (appId <= 0) {
                throw new Exception("Must  have app id");
            }
            if (deviceId <= 0) {
                deviceId = MpDefaultDataModelTools.ThisUserDeviceId;
            }

            var mr = new MpCliTransaction() {
                CliTransactionGuid = System.Guid.NewGuid(),
                PresetId = presetId,
                AppId = appId,
                DeviceId = deviceId,
                //CliPath = cliPath,
                //CliName = string.IsNullOrWhiteSpace(cliName) ? Path.GetFileNameWithoutExtension(cliPath) : cliName,
                WorkingDirectory = string.IsNullOrEmpty(workingDirectory) ? Directory.GetCurrentDirectory():workingDirectory,
                Args = args,
                IsAdmin = isAdmin,
                IsSilent = isSilent,
                TransactionDateTime = !transDateTime.HasValue ? DateTime.Now : transDateTime.Value,
                TransactionErrorMessage = errorMsg
            };
            
            if(presetId > 0) {
                var preset = await MpDataModelProvider.GetItemAsync<MpPluginPreset>(presetId);
                if(preset != null) {
                    mr.iconId = preset.IconId;
                }
            }
            if(!suppressWrite) {
                await mr.WriteToDatabaseAsync();
            }
            return mr;
        }

        public MpCliTransaction() { }


        //#region MpISourceItem Implementation
        //[Ignore]
        //public int IconId => iconId;
        //[Ignore]
        //public string SourcePath => CliPath;
        //[Ignore]
        //public string SourceName => CliName;
        //[Ignore]
        //public int RootId => Id;

        //[Ignore]
        //public bool IsUser => false;
        //[Ignore]
        //public bool IsUrl => false;
        //[Ignore]
        //public bool IsContent => false;
        //[Ignore]
        //public bool IsDll => false;
        //[Ignore]
        //public bool IsExe => true;
        //[Ignore]
        //public bool IsRejected => false;
        //[Ignore]
        //public bool IsSubRejected => false;

        

        //#endregion
    }
}
