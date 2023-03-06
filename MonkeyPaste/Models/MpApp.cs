using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpApp :
        MpDbModelBase,
        MpISyncableDbObject,
        MpISourceRef,
        MpIIconResource,
        MpIDbIconId,
        MpILabelText,
        MpIUriSource {
        #region Columns

        [Column("pk_MpAppId")]
        [PrimaryKey, AutoIncrement]
        public override int Id { get; set; }

        [Column("MpAppGuid")]
        public new string Guid { get => base.Guid; set => base.Guid = value; }

        [Indexed]
        public string AppPath { get; set; } = string.Empty;

        public string AppName { get; set; } = string.Empty;

        public string Arguments { get; set; } = string.Empty;

        [Column("IsAppRejected")]
        public int IsRejectedVal { get; set; } = 0;

        [Column("fk_MpIconId")]
        public int IconId { get; set; }

        [Column("fk_MpUserDeviceId")]
        public int UserDeviceId { get; set; }

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



        #region MpILabelText Implementation
        string MpILabelText.LabelText => AppName;

        #endregion

        #region MpIUriSource Implementation
        string MpIUriSource.Uri => AppPath;

        #endregion

        #region MpISourceRef Implementation

        [Ignore]
        public object IconResourceObj => IconId;
        [Ignore]
        int MpISourceRef.Priority => 2;
        [Ignore]
        int MpISourceRef.SourceObjId => Id;

        [Ignore]
        MpTransactionSourceType MpISourceRef.SourceType => MpTransactionSourceType.App;

        #endregion

        public static async Task<MpApp> CreateAsync(
            string appPath = "",
            string appName = "",
            string arguments = null,
            int iconId = 0,
            int appUserDeviceId = 0,
            string guid = "",
            bool suppressWrite = false) {
            if (appPath.IsNullOrEmpty()) {
                throw new Exception("App must have path");
            }
            if (appPath != null) {
                appPath = appPath.ToLower();
            }
            if (string.IsNullOrWhiteSpace(arguments)) {
                arguments = null;
            }
            // NOTE checking app by path and arguments and device here
            // NOTE when args are differnt should be treated as unique app since it could be significantly different
            var dupApp = await MpDataModelProvider.GetAppByMembersAsync(appPath, arguments, MpDefaultDataModelTools.ThisUserDeviceId);
            if (dupApp != null) {
                if (dupApp.IconId != iconId && iconId > 0) {
                    // this means app icon has changed (probably from an update)
                    dupApp.IconId = iconId;
                    await dupApp.WriteToDatabaseAsync();
                }
                dupApp.WasDupOnCreate = true;
                return dupApp;
            }

            if (iconId == 0) {
                string iconImgBase64 = Mp.Services.IconBuilder.GetApplicationIconBase64(appPath);

                var icon = await MpIcon.CreateAsync(
                        iconImgBase64: iconImgBase64,
                        createBorder: true,
                        suppressWrite: suppressWrite);
                iconId = icon.Id;
            }
            var newApp = new MpApp() {
                AppGuid = string.IsNullOrEmpty(guid) ? System.Guid.NewGuid() : System.Guid.Parse(guid),
                AppPath = appPath.ToLower(),
                AppName = appName,
                Arguments = arguments,
                IconId = iconId,
                UserDeviceId = appUserDeviceId == 0 ? MpDefaultDataModelTools.ThisUserDeviceId : appUserDeviceId
            };

            await newApp.WriteToDatabaseAsync();

            return newApp;
        }

        public MpApp() { }

        public override string ToString() {
            return $"Id: '{Id}' | Name: '{AppName}' | Path: '{AppPath}' | Args: '{Arguments}' DeviceId: '{UserDeviceId}'";
        }
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
                        if (userDevice == null) {
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
            if (icon == null) {
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
            var ud = await MpDataModelProvider.GetItemAsync<MpUserDevice>(UserDeviceId);
            var icon = await MpDataModelProvider.GetItemAsync<MpIcon>(IconId);
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
                MpDataModelProvider.GetItem<MpUserDevice>(UserDeviceId).Guid);
            diffLookup = CheckValue(IconId, other.IconId,
                "fk_MpIconId",
                diffLookup,
                MpDataModelProvider.GetItem<MpIcon>(IconId).Guid);
            diffLookup = CheckValue(AppPath, other.AppPath,
                "AppPath",
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
