﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using SQLiteNetExtensions.Attributes;

namespace MonkeyPaste {
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

    public class MpUserDevice : MpDbModelBase, MpISyncableDbObject {
        [PrimaryKey,AutoIncrement]
        [Column("pk_MpUserDeviceId")]
        public override int Id { get; set; }

        [Column("MpUserDeviceGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

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

        [Column("PlatformTypeId")]
        public int TypeId { get; set; } = 0;

        [Ignore]
        public MpUserDeviceType PlatformType {
            get {
                return (MpUserDeviceType)TypeId;
            }
            set {
                if (PlatformType != value) {
                    TypeId = (int)value;
                }
            }
        }

        [Ignore]
        public bool IsThisDevice {
            get {
                if (UserDeviceGuid == null) {
                    return false;
                }
                return UserDeviceGuid.ToString() == MpPreferences.Instance.ThisDeviceGuid;
            }
        }

        [Ignore]
        public bool IsThisPlatform {
            get {
                return PlatformType == MpPreferences.Instance.ThisDeviceType;
            }
        }

        public static MpUserDevice GetUserDeviceByGuid(string deviceGuid) {
            return MpDb.Instance.GetItems<MpUserDevice>().Where(x => x.UserDeviceGuid.ToString() == deviceGuid).FirstOrDefault();
        }

        public static MpUserDevice GetUserDeviceById(int udid) {
            return MpDb.Instance.GetItems<MpUserDevice>().Where(x => x.Id == udid).FirstOrDefault();
        }

        public MpUserDevice() { }

        public MpUserDevice(string deviceGuid, MonkeyPaste.MpUserDeviceType platformTypeId) : this() {
            UserDeviceGuid = System.Guid.Parse(deviceGuid);
            PlatformType = platformTypeId;
        }

        public async Task<object> CreateFromLogs(string udGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {            
            var ud = await MpDb.Instance.GetDbObjectByTableGuidAsync("MpUserDevice", udGuid) as MpUserDevice;

            foreach (var li in logs) {
                switch (li.AffectedColumnName) {
                    case "MpUserDeviceGuid":
                        ud.UserDeviceGuid = System.Guid.Parse(li.AffectedColumnValue);
                        break;
                    case "PlatformTypeId":
                        ud.PlatformType = (MpUserDeviceType)Convert.ToInt32(li.AffectedColumnValue);
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
                PlatformType = (MpUserDeviceType)Convert.ToInt32(objParts[1])
            };
            return ud;
        }

        public string SerializeDbObject() {
            return string.Format(
                @"{0}{1}{0}{2}{0}",
                ParseToken,
                UserDeviceGuid.ToString(),
                (int)PlatformType);
        }

        public Type GetDbObjectType() {
            return typeof(MpUserDevice);
        }

        public Dictionary<string, string> DbDiff(object drOrModel) {
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
                "PlatformTypeId",
                diffLookup,
                ((int)PlatformType).ToString());

            return diffLookup;
        }
    }
}