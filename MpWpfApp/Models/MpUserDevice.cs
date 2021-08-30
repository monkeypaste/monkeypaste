using Azure.Core;
using FFImageLoading.Helpers.Exif;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SQLite;
using MonkeyPaste;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpUserDevice : MpDbModelBase, MonkeyPaste.MpISyncableDbObject {
        private static List<MpUserDevice> _AllUserDeviceList = null;

        public int UserDeviceId { get; set; } = 0;
        public Guid UserDeviceGuid { get; set; }

        public MonkeyPaste.MpUserDeviceType PlatformTypeId { get; set; }

        public bool IsThisDevice {
            get {
                if(UserDeviceGuid == null) {
                    return false;
                }
                return UserDeviceGuid.ToString() == Properties.Settings.Default.ThisDeviceGuid;
            }
        }

        public bool IsThisPlatform {
            get {
                return PlatformTypeId == MpUserDeviceType.Windows;
            }
        }

        public static MpUserDevice GetUserDeviceByGuid(string deviceGuid) {
            return GetAllUserDevices().Where(x => x.UserDeviceGuid.ToString() == deviceGuid).FirstOrDefault();
        }

        public static MpUserDevice GetUserDeviceById(int udid) {
            return GetAllUserDevices().Where(x => x.UserDeviceId == udid).FirstOrDefault();
        }

        public static List<MpUserDevice> GetAllUserDevices() {
            if (_AllUserDeviceList == null) {
                _AllUserDeviceList = new List<MpUserDevice>();
                DataTable dt = MpDb.Instance.Execute("select * from MpUserDevice", null);
                if (dt != null && dt.Rows.Count > 0) {
                    foreach (DataRow dr in dt.Rows) {
                        _AllUserDeviceList.Add(new MpUserDevice(dr));
                    }
                }
            }
            return _AllUserDeviceList;
        }

        public MpUserDevice() { }

        public MpUserDevice(string deviceGuid, MonkeyPaste.MpUserDeviceType platformTypeId) :this() {
            UserDeviceGuid = System.Guid.Parse(deviceGuid);
            PlatformTypeId = platformTypeId;
        }

        public MpUserDevice(DataRow dr) : this() {
            LoadDataRow(dr);
        }
        public override void LoadDataRow(DataRow dr) {
            UserDeviceId = Convert.ToInt32(dr["pk_MpUserDeviceId"].ToString());
            UserDeviceGuid = Guid.Parse(dr["MpUserDeviceGuid"].ToString());
            PlatformTypeId = (MonkeyPaste.MpUserDeviceType)Convert.ToInt32(dr["PlatformTypeId"].ToString());
        }

        public override void WriteToDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {
            if (UserDeviceId == 0) {
                MpDb.Instance.ExecuteWrite(
                    "insert into MpUserDevice(MpUserDeviceGuid, PlatformTypeId) values(@udg,@ptId)",
                    new Dictionary<string, object> {
                        { "@udg", UserDeviceGuid.ToString() },
                        { "@ptId", (int)PlatformTypeId }
                    }, UserDeviceGuid.ToString(),sourceClientGuid,this,ignoreTracking,ignoreSyncing);
                UserDeviceId = MpDb.Instance.GetLastRowId("MpUserDevice", "pk_MpUserDeviceId");
                GetAllUserDevices().Add(this);
            } else {
                MpDb.Instance.ExecuteWrite(
                    "update MpUserDevice set MpUserDeviceGuid=@udg, PlatformTypeId=@ptId where pk_MpUserDeviceId=@udid",
                    new Dictionary<string, object> {
                        { "@udid",UserDeviceId },
                        { "@udg", UserDeviceGuid.ToString() },
                        { "@ptId", (int)PlatformTypeId }
                    }, UserDeviceGuid.ToString(), sourceClientGuid, this, ignoreTracking, ignoreSyncing);
                var c = GetAllUserDevices().Where(x => x.UserDeviceId == UserDeviceId).FirstOrDefault();
                if (c != null) {
                    _AllUserDeviceList[_AllUserDeviceList.IndexOf(c)] = this;
                }
            }
        }
        public override void WriteToDatabase() {
            if (IsSyncing) {
                WriteToDatabase(SyncingWithDeviceGuid, false, true);
            } else {
                WriteToDatabase(Properties.Settings.Default.ThisDeviceGuid);
            }
        }
        public void WriteToDatabase(bool ignoreTracking, bool ignoreSyncing) {
            WriteToDatabase(Properties.Settings.Default.ThisDeviceGuid, ignoreTracking, ignoreSyncing);
        }

        public override void DeleteFromDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {
            if (string.IsNullOrEmpty(sourceClientGuid)) {
                sourceClientGuid = Properties.Settings.Default.ThisDeviceGuid;
            }

            MpDb.Instance.ExecuteWrite(
                "delete from MpUserDevice where pk_MpUserDeviceId=@udid",
                new Dictionary<string, object> {
                    { "@udid", UserDeviceId }
                    }, UserDeviceGuid.ToString(), sourceClientGuid, this, ignoreTracking, ignoreSyncing);

            GetAllUserDevices().Remove(this);
        }

        public void DeleteFromDatabase() {
            if (IsSyncing) {
                DeleteFromDatabase(SyncingWithDeviceGuid, false, true);
            } else {
                DeleteFromDatabase(Properties.Settings.Default.ThisDeviceGuid);
            }
        }

        public async Task<object> CreateFromLogs(string udGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
            await Task.Delay(1);
            var ud = MpDb.Instance.GetDbObjectByTableGuid("MpUserDevice", udGuid) as MpUserDevice;

            foreach (var li in logs) {
                switch (li.AffectedColumnName) {
                    case "MpUserDeviceGuid":
                        ud.UserDeviceGuid = System.Guid.Parse(li.AffectedColumnValue);
                        break;
                    case "PlatformTypeId":
                        ud.PlatformTypeId = (MpUserDeviceType)Convert.ToInt32(li.AffectedColumnValue);
                        break;
                    default:
                        MpConsole.WriteTraceLine(@"Unknown table-column: " + li.DbTableName + "-" + li.AffectedColumnName);
                        break;
                }
            }
            return ud;
        }

        public async Task<object> DeserializeDbObject(string objStr) {
            await Task.Delay(0);
            var objParts = objStr.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);
            var ud = new MpUserDevice() {
                UserDeviceGuid = System.Guid.Parse(objParts[0]),
                PlatformTypeId = (MpUserDeviceType)Convert.ToInt32(objParts[1])
            };
            return ud;
        }

        public string SerializeDbObject() {
            return string.Format(
                @"{0}{1}{0}{2}{0}",
                ParseToken,
                UserDeviceGuid.ToString(),
                (int)PlatformTypeId);
        }

        public Type GetDbObjectType() {
            return typeof(MpUserDevice);
        }

        public Dictionary<string, string> DbDiff(object drOrModel) {
            MpUserDevice other = null;
            if (drOrModel is DataRow) {
                other = new MpUserDevice(drOrModel as DataRow);
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
            diffLookup = CheckValue(PlatformTypeId, other.PlatformTypeId,
                "PlatformTypeId",
                diffLookup,
                ((int)PlatformTypeId).ToString());

            return diffLookup;
        }
    }
}
