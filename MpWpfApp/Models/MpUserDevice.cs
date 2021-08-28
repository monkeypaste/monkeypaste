using Azure.Core;
using FFImageLoading.Helpers.Exif;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SQLite;

namespace MpWpfApp {
    public enum MpUserDeviceType {
        None = 0,
        Ios,
        Android,
        Windows,
        Mac,
        Linux,
        Web,
        Unknown
    }

    public class MpUserDevice : MpDbModelBase {
        private static List<MpUserDevice> _AllUserDeviceList = null;

        public int UserDeviceId { get; set; } = 0;
        public Guid UserDeviceGuid { get; set; }

        public MpUserDeviceType PlatformTypeId { get; set; }

        public static MpUserDevice GetUserDeviceByGuid(string deviceGuid) {
            return GetAllUserDevices().Where(x => x.UserDeviceGuid.ToString() == deviceGuid).FirstOrDefault();
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

        public MpUserDevice(string deviceGuid, MpUserDeviceType platformTypeId) :this() {
            UserDeviceGuid = System.Guid.Parse(deviceGuid);
            PlatformTypeId = platformTypeId;
        }

        public MpUserDevice(DataRow dr) : this() {
            LoadDataRow(dr);
        }
        public override void LoadDataRow(DataRow dr) {
            UserDeviceId = Convert.ToInt32(dr["pk_MpUserDeviceId"].ToString());
            UserDeviceGuid = Guid.Parse(dr["MpUserDeviceGuid"].ToString());
            PlatformTypeId = (MpUserDeviceType)Convert.ToInt32(dr["PlatformTypeId"].ToString());
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
                WriteToDatabase(Properties.Settings.Default.ThisClientGuid);
            }
        }
        public void WriteToDatabase(bool ignoreTracking, bool ignoreSyncing) {
            WriteToDatabase(Properties.Settings.Default.ThisClientGuid, ignoreTracking, ignoreSyncing);
        }

        public override void DeleteFromDatabase(string sourceClientGuid, bool ignoreTracking = false, bool ignoreSyncing = false) {
            if (string.IsNullOrEmpty(sourceClientGuid)) {
                sourceClientGuid = Properties.Settings.Default.ThisClientGuid;
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
                DeleteFromDatabase(Properties.Settings.Default.ThisClientGuid);
            }
        }
    }
}
