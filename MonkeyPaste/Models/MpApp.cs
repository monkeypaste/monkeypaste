using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using SQLiteNetExtensions.Attributes;
using System.Threading.Tasks;
using System.Linq;
using System.Data;

namespace MonkeyPaste {
    public class MpApp : MpDbModelBase, MpICopyItemSource, MpISyncableDbObject {
        [Column("pk_MpAppId")]
        [PrimaryKey,AutoIncrement]
        public override int Id { get; set; }

        [Column("MpAppGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Ignore]
        public Guid AppGuid {
            get {
                if(string.IsNullOrEmpty(Guid)) {
                    return System.Guid.Empty;
                }
                return System.Guid.Parse(Guid);
            }
            set {
                Guid = value.ToString();
            }
        }

        [Indexed]
        [Column("SourcePath")]
        public string AppPath { get; set; } = string.Empty;
        
        public string AppName { get; set; } = string.Empty;

        [Column("IsAppRejected")]
        public int IsRejected { get; set; } = 0;

        [Ignore]
        public bool IsAppRejected
        {
            get
            {
                return IsRejected == 1;
            }
            set
            {
                if(IsAppRejected != value)
                {
                    IsRejected = value ? 1 : 0;
                }
            }
        }

        [ForeignKey(typeof(MpIcon))]
        [Column("fk_MpIconId")]
        public int IconId { get; set; }

        [OneToOne(CascadeOperations = CascadeOperation.All)]
        public MpIcon Icon { get; set; }

        [ForeignKey(typeof(MpUserDevice))]
        [Column("fk_MpUserDeviceId")]
        public int UserDeviceId { get; set; }

        [ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        public MpUserDevice UserDevice { get; set; }

        public static async Task<MpApp> GetAppByPath(string appPath) {
            var allApps = await MpDb.Instance.GetItemsAsync<MpApp>();
            return allApps.Where(x => x.AppPath.ToLower() == appPath.ToLower()).FirstOrDefault();
        }

        public static async Task<MpApp> GetAppById(int appId) {
            var allApps = await MpDb.Instance.GetItemsAsync<MpApp>();
            return allApps.Where(x => x.Id == appId).FirstOrDefault();
        }

        public static async Task<MpApp> GetAppByGuid(string appGuid) {
            var allApps = await MpDb.Instance.GetItemsAsync<MpApp>();
            return allApps.Where(x => x.Guid == appGuid).FirstOrDefault();
        }

        public static async Task<MpApp> Create(string appPath,string appName, string appIconBase64) {
            //if app doesn't exist create image,icon,app and source

            var newIcon = await MpIcon.Create(appIconBase64);

            var newApp = new MpApp() {
                AppPath = appPath,
                AppName = appName,
                IconId = newIcon.Id,
                Icon = newIcon
            };

            await MpDb.Instance.AddItemAsync<MpApp>(newApp);

            return newApp;
        }
        public MpApp() {
        }

        #region MpICopyItemSource Implementation
        public MpIcon SourceIcon => Icon;

        public string SourcePath => AppPath;

        public string SourceName => AppName;

        public int RootId => Id;

        public bool IsSubSource => false;
        #endregion

        public async Task<object> CreateFromLogs(string appGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
            var adr = await MpDb.Instance.GetDbObjectByTableGuidAsync("MpApp", appGuid);
            MpApp appFromLog = null;
            if (adr == null) {
                appFromLog = new MpApp();
            } else {
                appFromLog = adr as MpApp;
            }
            foreach (var li in logs) {
                switch (li.AffectedColumnName) {
                    case "MpAppGuid":
                        appFromLog.AppGuid = System.Guid.Parse(li.AffectedColumnValue);
                        break;
                    case "fk_MpUserDeviceId":
                        appFromLog.UserDevice = await MpDb.Instance.GetDbObjectByTableGuidAsync("MpUserDevice", li.AffectedColumnValue) as MpUserDevice;
                        appFromLog.UserDeviceId = appFromLog.UserDevice.Id;
                        break;
                    case "fk_MpIconId":
                        appFromLog.Icon = await MpDb.Instance.GetDbObjectByTableGuidAsync("MpIcon", li.AffectedColumnValue) as MpIcon;
                        appFromLog.IconId = appFromLog.Icon.Id;
                        break;
                    case "SourcePath":
                        appFromLog.AppPath = li.AffectedColumnValue;
                        break;
                    case "AppName":
                        appFromLog.AppName = li.AffectedColumnValue;
                        break;
                    case "IsAppRejected":
                        appFromLog.IsAppRejected = li.AffectedColumnValue == "1";
                        break;
                    default:
                        MpConsole.WriteTraceLine(@"Unknown table-column: " + li.DbTableName + "-" + li.AffectedColumnName);
                        break;
                }
            }
            return appFromLog;
        }

        public async Task<object> DeserializeDbObject(string objStr) {
            await Task.Delay(0);
            var objParts = objStr.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);
            var a = new MpApp() {
                AppGuid = System.Guid.Parse(objParts[0])
            };
            a.UserDevice = MpDb.Instance.GetDbObjectByTableGuid("MpUserDevice", objParts[1]) as MpUserDevice;
            a.UserDeviceId = a.UserDevice.Id;
            a.Icon = MpDb.Instance.GetDbObjectByTableGuid("MpIcon", objParts[2]) as MpIcon;            
            a.IconId = a.Icon.Id;

            a.AppPath = objParts[3];
            a.AppName = objParts[4];
            a.IsAppRejected = objParts[5] == "1";
            return a;
        }

        public string SerializeDbObject() {
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}",
                ParseToken,
                AppGuid.ToString(),
                UserDevice.UserDeviceGuid.ToString(),
                Icon.IconGuid.ToString(),
                AppPath,
                AppName,
                IsAppRejected ? "1" : "0");
        }

        public Type GetDbObjectType() {
            return typeof(MpApp);
        }

        public Dictionary<string, string> DbDiff(object drOrModel) {
            MpApp other = null;
            if (drOrModel is MpApp) {
                other = drOrModel as MpApp;
            } else {
                //implies this an add so all syncable columns are returned
                other = new MpApp();
            }
            //returns db column name and string value of dr that is diff
            var diffLookup = new Dictionary<string, string>();
            diffLookup = CheckValue(AppGuid, other.AppGuid,
                "MpAppGuid",
                diffLookup,
                AppGuid.ToString());
            diffLookup = CheckValue(UserDeviceId, other.UserDeviceId,
                "fk_MpUserDeviceId",
                diffLookup,
                UserDevice.UserDeviceGuid.ToString());
            diffLookup = CheckValue(IconId, other.IconId,
                "fk_MpIconId",
                diffLookup,
                Icon.IconGuid.ToString());
            diffLookup = CheckValue(AppPath, other.AppPath,
                "SourcePath",
                diffLookup);
            diffLookup = CheckValue(AppName, other.AppName,
                "AppName",
                diffLookup);
            diffLookup = CheckValue(IsAppRejected, other.IsAppRejected,
                "IsAppRejected",
                diffLookup,
                IsAppRejected ? "1" : "0");
            return diffLookup;
        }
    }
}
