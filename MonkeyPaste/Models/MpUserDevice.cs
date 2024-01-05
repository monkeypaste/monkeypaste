using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using SQLite;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste {


    public class MpUserDevice :
        MpDbModelBase,
        MpISyncableDbObject,
        MpISourceRef {
        #region Statics

        public static async Task<MpUserDevice> CreateAsync(
            string guid = "",
            int userId = 0,
            MpUserDeviceType deviceType = MpUserDeviceType.None,
            string machineName = "",
            string versionInfo = "",
            bool suppressWrite = false) {
            var dupCheck = await MpDataModelProvider.GetUserDeviceByMembersAsync(machineName, deviceType);
            if (dupCheck != null) {
                bool needsUpdate = false;
                if (dupCheck.VersionInfo != versionInfo) {
                    needsUpdate = true;
                    MpConsole.WriteLine($"UserDevice '{dupCheck}' version info changed, updating...");
                    dupCheck.VersionInfo = versionInfo;
                }
                if (needsUpdate) {
                    await dupCheck.WriteToDatabaseAsync();
                }
                return dupCheck;
            }

            var ud = new MpUserDevice() {
                Guid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid().ToString() : guid,
                UserDeviceType = deviceType,
                MachineName = machineName,
                VersionInfo = versionInfo
            };
            if (!suppressWrite) {
                await ud.WriteToDatabaseAsync();
            }
            return ud;
        }
        #endregion

        #region Interfaces

        #region MpISourceRef Implementation

        public int Priority => (int)MpTransactionSourceType.UserDevice;
        public int SourceObjId => Id;
        public MpTransactionSourceType SourceType => MpTransactionSourceType.UserDevice;
        public object IconResourceObj => "BrainImage";
        public string LabelText => "<UserName>-<DeviceName> here";

        #endregion

        #endregion

        #region Properties

        #region Columns

        [PrimaryKey, AutoIncrement]
        [Column("pk_MpUserDeviceId")]
        public override int Id { get; set; }

        [Column("MpUserDeviceGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Column("fk_MpUserId")]
        public int UserId { get; set; }

        public string MachineName { get; set; }

        public string VersionInfo { get; set; }

        [Column("e_MpUserDeviceType")]
        public string UserDeviceTypeName { get; set; } = MpUserDeviceType.None.ToString();


        #endregion

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
        public MpUserDeviceType UserDeviceType {
            get => UserDeviceTypeName.ToEnum<MpUserDeviceType>();
            set => UserDeviceTypeName = value.ToString();
        }

        [Ignore]
        public bool IsThisDevice {
            get {
                if (UserDeviceGuid == null) {
                    return false;
                }
                return UserDeviceGuid.ToString() == Mp.Services.ThisDeviceInfo.ThisDeviceGuid;
            }
        }

        [Ignore]
        public bool IsThisPlatform {
            get {
                return UserDeviceType == Mp.Services.PlatformInfo.OsType;
            }
        }

        #endregion

        #region Constructors

        public MpUserDevice() { }

        #endregion

        #region Public Methods

        public override string ToString() {
            return $"[{MachineName}] - '{VersionInfo}'";
        }
        #endregion

        #region Sync
        public async Task<object> CreateFromLogsAsync(string udGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
            var ud = await MpDb.GetDbObjectByTableGuidAsync("MpUserDevice", udGuid) as MpUserDevice;

            foreach (var li in logs) {
                switch (li.AffectedColumnName) {
                    case "MpUserDeviceGuid":
                        ud.UserDeviceGuid = System.Guid.Parse(li.AffectedColumnValue);
                        break;
                    case "e_MpUserDeviceType":
                        ud.UserDeviceType = (MpUserDeviceType)Convert.ToInt32(li.AffectedColumnValue);
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
                UserDeviceType = (MpUserDeviceType)Convert.ToInt32(objParts[1])
            };
            return ud;
        }

        public async Task<string> SerializeDbObjectAsync() {
            await Task.Delay(1);

            return string.Format(
                @"{0}{1}{0}{2}{0}",
                ParseToken,
                UserDeviceGuid.ToString(),
                (int)UserDeviceType);
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
            diffLookup = CheckValue(UserDeviceType, other.UserDeviceType,
                "e_MpUserDeviceType",
                diffLookup,
                UserDeviceType.ToString());

            return diffLookup;
        }
        #endregion
    }
}
