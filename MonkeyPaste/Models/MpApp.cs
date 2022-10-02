using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using SQLiteNetExtensions.Attributes;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;
using System.Diagnostics;

namespace MonkeyPaste {
    public class MpApp : MpDbModelBase, MpISourceItem, MpISyncableDbObject {        
        #region Columns

        [Column("pk_MpAppId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [Column("MpAppGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Indexed]
        [Column("SourcePath")]
        public string AppPath { get; set; } = string.Empty;

        public string AppName { get; set; } = string.Empty;

        [Column("IsAppRejected")]
        public int IsRejectedVal { get; set; } = 0;        

        //[ForeignKey(typeof(MpIcon))]
        [Column("fk_MpIconId")]
        public int IconId { get; set; }

        //[ForeignKey(typeof(MpUserDevice))]
        [Column("fk_MpUserDeviceId")]
        public int UserDeviceId { get; set; }

        #endregion

        #region Fk Models

        //[ManyToOne(CascadeOperations = CascadeOperation.CascadeRead)]
        //public MpUserDevice UserDevice { get; set; }

        #endregion

        #region Properties
       
        [Ignore]
        public bool IsAppRejected {
            get {
                return IsRejectedVal == 1;
            }
            set {
                if (IsAppRejected != value) {
                    IsRejectedVal = value ? 1 : 0;
                }
            }
        }

        [Ignore]
        public bool IsUrl => false;

        [Ignore]
        public Guid AppGuid {
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

        #region MpICopyItemSource Implementation

        [Ignore]
        public bool IsDll => false;

        [Ignore]
        public bool IsExe => false;
        [Ignore]
        public bool IsUser => false;

        [Ignore]
        public bool IsRejected => IsAppRejected;

        [Ignore]
        public bool IsSubRejected => IsRejected;

        //[Ignore]
        //public MpIcon SourceIcon => Icon;

        [Ignore]
        public string SourcePath => AppPath;

        [Ignore]
        public string SourceName => AppName;

        [Ignore]
        public int RootId => Id;
        #endregion

        public static async Task<MpApp> CreateAsync(
            string appPath = "", 
            string appName = "", 
            int iconId = 0,
            string guid = "",
            bool suppressWrite = false) {
            if(appPath.IsNullOrEmpty()) {
                throw new Exception("App must have path");
            }
            if(appPath != null) {
                appPath = appPath.ToLower();
            }
            // NOTE checking app by path and device here
            var dupApp = await MpDataModelProvider.GetAppByPathAsync(appPath, MpPrefViewModel.Instance.ThisUserDevice.Id);
            if (dupApp != null) {
                if(dupApp.IconId != iconId && iconId > 0) {
                    // this means app icon has changed (probably from an update)
                    dupApp.IconId = iconId;
                    await dupApp.WriteToDatabaseAsync();                    
                }
                return dupApp;
            }
            //if app doesn't exist create image,icon,app and source

            var thisDevice = await MpDataModelProvider.GetUserDeviceByGuidAsync(MpPrefViewModel.Instance.ThisDeviceGuid);

            if(thisDevice == null) {
                //not sure why this happens but duplicating MpDb.InitDefaultData...
                thisDevice = new MpUserDevice() {
                    UserDeviceGuid = System.Guid.Parse(MpPrefViewModel.Instance.ThisDeviceGuid),
                    PlatformType = MpPrefViewModel.Instance.ThisDeviceType
                };
                await thisDevice.WriteToDatabaseAsync();
            }

            if(iconId == 0) {
                string iconImgBase64 = MpPlatformWrapper.Services.IconBuilder.GetApplicationIconBase64(appPath);

                var icon = await MpIcon.Create(
                        iconImgBase64: iconImgBase64,
                        createBorder: true,
                        suppressWrite: suppressWrite);
                iconId = icon.Id;
            }
            var newApp = new MpApp() {
                AppGuid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid() : System.Guid.Parse(guid),
                AppPath = appPath.ToLower(),
                AppName = appName,
                IconId = iconId,
                UserDeviceId = thisDevice.Id
            };

            await newApp.WriteToDatabaseAsync();

            return newApp;
        }

        public MpApp() { }


        public async Task<object> CreateFromLogsAsync(string appGuid, List<MonkeyPaste.MpDbLog> logs, string fromClientGuid) {
            var adr = await MpDb.GetDbObjectByTableGuidAsync("MpApp", appGuid);
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
                        var userDevice = await MpDb.GetDbObjectByTableGuidAsync("MpUserDevice", li.AffectedColumnValue) as MpUserDevice;
                        if(userDevice == null) {
                            Debugger.Break();
                            return appFromLog;
                        }
                        appFromLog.UserDeviceId = userDevice.Id;
                        break;
                    case "fk_MpIconId":
                        var icon = await MpDb.GetDbObjectByTableGuidAsync("MpIcon", li.AffectedColumnValue) as MpIcon;
                        appFromLog.IconId = icon.Id;
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

        public async Task<object> DeserializeDbObjectAsync(string objStr) {
            await Task.Delay(0);
            var objParts = objStr.Split(new string[] { ParseToken }, StringSplitOptions.RemoveEmptyEntries);
            var a = new MpApp() {
                AppGuid = System.Guid.Parse(objParts[0])
            };
            var userDevice = await MpDb.GetDbObjectByTableGuidAsync("MpUserDevice", objParts[1]) as MpUserDevice;
            if (userDevice == null) {
                Debugger.Break();
            } else {
                a.UserDeviceId = userDevice.Id;
            }
            
            var icon = await MpDb.GetDbObjectByTableGuidAsync("MpIcon", objParts[2]) as MpIcon;
            if(icon == null) {
                Debugger.Break();
            } else {
                a.IconId = icon.Id;
            }
            

            a.AppPath = objParts[3];
            a.AppName = objParts[4];
            a.IsAppRejected = objParts[5] == "1";
            return a;
        }

        public async Task<string> SerializeDbObjectAsync() {
            var ud = await MpDb.GetItemAsync<MpUserDevice>(UserDeviceId);
            var icon = await MpDb.GetItemAsync<MpIcon>(IconId);
            return string.Format(
                @"{0}{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}",
                ParseToken,
                AppGuid.ToString(),
                ud.Guid,
                icon.Guid,
                AppPath,
                AppName,
                IsAppRejected ? "1" : "0");
        }

        public Type GetDbObjectType() {
            return typeof(MpApp);
        }

        public async Task<Dictionary<string, string>> DbDiffAsync(object drOrModel) {
            await Task.Delay(1);

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
                MpDb.GetItem<MpUserDevice>(UserDeviceId).Guid);
            diffLookup = CheckValue(IconId, other.IconId,
                "fk_MpIconId",
                diffLookup,
                MpDb.GetItem<MpIcon>(IconId).Guid);
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
