using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using MonkeyPaste.Common.Plugin; 
using MonkeyPaste.Common;
using CsvHelper;

namespace MonkeyPaste {
    

    public class MpUserDevice : MpDbModelBase, MpISyncableDbObject, MpISourceRef {
        #region MpISourceRef Implementation

        public int Priority => 1;
        public int SourceObjId => Id;
        public MpTransactionSourceType SourceType => MpTransactionSourceType.UserDevice;
        public object IconResourceObj => "BrainImage";
        public string LabelText => "<UserName>-<DeviceName> here";

        #endregion

        #region Columns

        [PrimaryKey,AutoIncrement]
        [Column("pk_MpUserDeviceId")]
        public override int Id { get; set; }

        [Column("MpUserDeviceGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        public string MachineName { get; set; }

        [Column("e_MpUserDeviceType")]
        public string PlatformTypeName { get; set; } = MpUserDeviceType.None.ToString();


        #endregion

        #region Properties

        [Ignore]
        public Guid UserDeviceGuid {
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
        public MpUserDeviceType PlatformType {
            get => PlatformTypeName.ToEnum<MpUserDeviceType>();
            set => PlatformTypeName = value.ToString();
        }

        [Ignore]
        public bool IsThisDevice {
            get {
                if (UserDeviceGuid == null) {
                    return false;
                }
                return UserDeviceGuid.ToString() == MpPrefViewModel.Instance.ThisDeviceGuid;
            }
        }

        [Ignore]
        public bool IsThisPlatform {
            get {
                return PlatformType == MpDefaultDataModelTools.ThisUserDeviceType;
            }
        }

        #endregion

        public MpUserDevice() { }

        public MpUserDevice(string deviceGuid, MpUserDeviceType platformTypeId) : this() {
            UserDeviceGuid = System.Guid.Parse(deviceGuid);
            PlatformType = platformTypeId;
        }

        public async Task<object> CreateFromLogsAsync(string udGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {            
            var ud = await MpDb.GetDbObjectByTableGuidAsync("MpUserDevice", udGuid) as MpUserDevice;

            foreach (var li in logs) {
                switch (li.AffectedColumnName) {
                    case "MpUserDeviceGuid":
                        ud.UserDeviceGuid = System.Guid.Parse(li.AffectedColumnValue);
                        break;
                    case "e_MpUserDeviceType":
                        ud.PlatformType = (MpUserDeviceType)Convert.ToInt32(li.AffectedColumnValue);
                        break;
                    default:
                        MpConsole.WriteTraceLine(@"Unknown table-column: " + li.DbTableName + "-" + li.AffectedColumnName);
                        break;
                }
            }
            return ud;
        }

        public async Task<object> DeserializeDbObjectAsync(string objStr) {
            await Task.Delay(0);
            var objParts = objStr.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);
            var ud = new MpUserDevice() {
                UserDeviceGuid = System.Guid.Parse(objParts[0]),
                PlatformType = (MpUserDeviceType)Convert.ToInt32(objParts[1])
            };
            return ud;
        }

        public async Task<string> SerializeDbObjectAsync() {
            await Task.Delay(1);

            return string.Format(
                @"{0}{1}{0}{2}{0}",
                ParseToken,
                UserDeviceGuid.ToString(),
                (int)PlatformType);
        }

        public Type GetDbObjectType() {
            return typeof(MpUserDevice);
        }

        public async Task<Dictionary<string, string>> DbDiffAsync(object drOrModel) {
            await Task.Delay(1);

            MpUserDevice other = null;
            if (drOrModel is MpUserDevice) {
                other = drOrModel as MpUserDevice;
            } else {
                //implies this an add so all syncable columns are returned
                other = new MpUserDevice();
            }
            //returns db column name and string value of dr that is diff
            var diffLookup = new Dictionary<string, string>();
            diffLookup = CheckValue(UserDeviceGuid, other.UserDeviceGuid,
                "MpUserDeviceGuid",
                diffLookup,
                UserDeviceGuid.ToString());
            diffLookup = CheckValue(PlatformType, other.PlatformType,
                "e_MpUserDeviceType",
                diffLookup,
                PlatformType.ToString());

            return diffLookup;
        }

    }
}
