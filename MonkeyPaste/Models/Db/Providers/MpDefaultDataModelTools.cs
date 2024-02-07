using MonkeyPaste.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public static class MpDefaultDataModelTools {
        #region Private Variables

        #endregion

        #region Statics

        #endregion

        #region Properties

        #region Device
        public static int ThisUserDeviceId { get; private set; }
        public static string ThisUserDeviceGuid { get; private set; }

        #endregion

        public static int UnknownIconId => 1;
        public static int UnknownIconDbImageId => 1;
        public static int ThisAppIconId => 2;
        public static int ThisAppId => 1;

        #endregion

        #region Constructors

        #endregion

        #region Public Methods

        public static async Task<string> DiscoverPrefInfoAsync(MpIDbInfo dbInfo, MpIPlatformInfo osInfo) {
            bool? wouldBeNewDb = await MpDb.InitDbConnectionAsync(dbInfo, false);
            if (!wouldBeNewDb.HasValue || wouldBeNewDb.Value) {
                //this should be caught in pref init so somethings wrong
                MpDebug.Break();
                return null;
            }
            await MpDb.CreateTableAsync<MpUserDevice>();

            MpUserDevice thisDevice = await MpDataModelProvider.GetUserDeviceByMembersAsync(osInfo.OsMachineName, osInfo.OsType);
            if (thisDevice == null) {
                // maybe user changed machine name so fallback and query just by device type
                thisDevice = await MpDataModelProvider.GetUserDeviceByMembersAsync(null, osInfo.OsType);
                if (thisDevice == null) {
                    // reset error
                    MpDebug.Break();
                    return null;
                }
            }
            ThisUserDeviceGuid = thisDevice.Guid;
            ThisUserDeviceId = thisDevice.Id;

            await MpDb.CreateTableAsync<MpApp>();

            string thisAppPath = osInfo.ExecutingPath;
            var thisApp = await MpDataModelProvider.GetAppByMembersAsync(thisAppPath, string.Empty, thisDevice.Id);
            MpDebug.Assert(thisApp != null && thisApp.Id == ThisAppId, $"ThisApp should be id={ThisAppId}");


            await MpDb.CreateTableAsync<MpCopyItem>();

            await MpDb.CloseConnectionAsync();

            return ThisUserDeviceGuid;
        }

        public static async Task CreateAsync(string thisDeviceGuid) {
            // NOTE on initial startup 

            // Unknown Icon

            var unknownAppIcon = await MpIcon.CreateAsync(MpBase64Images.QuestionMark);

            MpDebug.Assert(unknownAppIcon != null && unknownAppIcon.Id == UnknownIconId, $"Unknown icon should be id={UnknownIconId}");
            MpDebug.Assert(unknownAppIcon != null && unknownAppIcon.IconImageId == UnknownIconDbImageId, $"Unknown icon IMAGE should be id={UnknownIconDbImageId}");

            // User
            var new_user = await MpUser.CreateAsync(
                email: Mp.Services.PlatformUserInfo.UserEmail);

            // User Device

            var thisDevice = await MpUserDevice.CreateAsync(
                guid: thisDeviceGuid,
                userId: new_user.Id,
                deviceType: Mp.Services.PlatformInfo.OsType,
                machineName: Mp.Services.PlatformInfo.OsMachineName,
                versionInfo: Mp.Services.PlatformInfo.OsVersion);

            await thisDevice.WriteToDatabaseAsync();

            ThisUserDeviceId = thisDevice.Id;
            ThisUserDeviceGuid = thisDevice.Guid;
            // This Icon

            var thisAppIcon = await MpIcon.CreateAsync(MpBase64Images.AppIcon);
            MpDebug.Assert(thisAppIcon != null && thisAppIcon.Id == ThisAppIconId, $"ThisAppIcon should be id={ThisAppIconId}");


            // This App

            string thisAppName = Mp.Services.ThisAppInfo.ThisAppProductName;
            var thisApp = await MpApp.CreateAsync(
                appPath: Mp.Services.PlatformInfo.ExecutingPath,
                appName: thisAppName,
                iconId: thisAppIcon.Id);
            MpDebug.Assert(thisApp != null && thisApp.Id == ThisAppId, $"ThisApp should be id={ThisAppId}");
        }

        public static async Task InitializeAsync() {
            // USER DEVICE
            var thisUserDevice = await MpDataModelProvider.GetUserDeviceByGuidAsync(Mp.Services.ThisDeviceInfo.ThisDeviceGuid);
            if (thisUserDevice == null) {
                // reset error
                var device_check = await MpDataModelProvider.GetItemsAsync<MpUserDevice>();
                MpDebug.Break($"Missing user device '{Mp.Services.ThisDeviceInfo.ThisDeviceGuid}'");

                // BUG sandboxed local storage issues keep hitting this spot, testing using the available guid...
                thisUserDevice = device_check.FirstOrDefault();
                Mp.Services.ThisDeviceInfo.ThisDeviceGuid = thisUserDevice == null ? Mp.Services.ThisDeviceInfo.ThisDeviceGuid : thisUserDevice.Guid;
            }
            ThisUserDeviceId = thisUserDevice.Id;
            ThisUserDeviceGuid = thisUserDevice.Guid;

            // THIS APP

            var thisApp = await MpDataModelProvider.GetAppByMembersAsync(Mp.Services.PlatformInfo.ExecutingPath, string.Empty, ThisUserDeviceId);
            if (thisApp == null) {
                var firstApp = await MpDataModelProvider.GetItemAsync<MpApp>(ThisAppId);
                if (firstApp == null) {
                    MpDebug.Break($"No thisApp record found using path '{Mp.Services.PlatformInfo.ExecutingPath}'");
                } else {
                    string stored_exe_name = Path.GetFileName(firstApp.AppPath);
                    string running_exe_name = Path.GetFileName(Mp.Services.PlatformInfo.ExecutingPath);
                    MpDebug.Assert(stored_exe_name.ToLowerInvariant() == running_exe_name.ToLowerInvariant(), $"ThisApp should be Id=1. But path '{firstApp.AppPath}' was found instead");
                }
            }


            // THIS ICON

            var thisAppIcon = await MpDataModelProvider.GetItemAsync<MpIcon>(ThisAppIconId);
            MpDebug.Assert(thisAppIcon != null && thisAppIcon.Id == ThisAppIconId, $"ThisAppIcon should be id={ThisAppIconId}");
        }

        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
