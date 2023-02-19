using MonkeyPaste.Common;
using System;
using System.Diagnostics;
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

        public static int ThisAppIconId { get; private set; }
        public static int ThisAppIconDbImageId { get; private set; }
        public static int ThisAppId { get; private set; }
        public static int ThisOsFileManagerAppId { get; private set; }


        #endregion

        #region Constructors

        #endregion

        #region Public Methods

        public static async Task<Tuple<string, int>> DiscoverPrefInfoAsync(MpIDbInfo dbInfo, MpIPlatformInfo osInfo) {
            bool wouldBeNewDb = await MpDb.InitDbConnectionAsync(dbInfo, false);
            if (wouldBeNewDb) {
                //this should be caught in pref init so somethings wrong
                Debugger.Break();
                return null;
            }
            await MpDb.CreateTableAsync<MpUserDevice>();

            MpUserDevice this_device = await MpDataModelProvider.GetUserDeviceByMembersAsync(osInfo.OsMachineName, osInfo.OsType);
            if (this_device == null) {
                // maybe user changed machine name so fallback and query just by device type
                this_device = await MpDataModelProvider.GetUserDeviceByMembersAsync(null, osInfo.OsType);
                if (this_device == null) {
                    // reset error
                    Debugger.Break();
                    return null;
                }
            }
            ThisUserDeviceGuid = this_device.Guid;
            ThisUserDeviceId = this_device.Id;

            await MpDb.CreateTableAsync<MpApp>();

            string thisAppPath = osInfo.ExecutingPath;
            var this_app = await MpDataModelProvider.GetAppByMembersAsync(thisAppPath, null, this_device.Id);
            if (this_app == null) {
                // reset error
                Debugger.Break();
            }
            ThisAppId = this_app.Id;


            if (osInfo.IsDesktop) {
                var this_os_file_manager = await MpDataModelProvider.GetAppByMembersAsync(osInfo.OsFileManagerPath, null, this_device.Id);
                if (this_os_file_manager == null) {
                    // reset error
                    Debugger.Break();
                }
                ThisOsFileManagerAppId = this_os_file_manager.Id;
            }

            await MpDb.CreateTableAsync<MpCopyItem>();
            int total_count = await MpDataModelProvider.GetTotalCopyItemCountAsync();

            await MpDb.CloseConnectionAsync();
            return new Tuple<string, int>(ThisUserDeviceGuid, total_count);
        }

        public static async Task CreateAsync() {

            // User Device

            ThisUserDeviceGuid = Guid.NewGuid().ToString();

            var thisDevice = new MpUserDevice() {
                UserDeviceGuid = Guid.Parse(MpPrefViewModel.Instance.ThisDeviceGuid),
                PlatformType = MpPlatform.Services.PlatformInfo.OsType,
                MachineName = Environment.MachineName
            };

            await thisDevice.WriteToDatabaseAsync();

            ThisUserDeviceId = thisDevice.Id;

            // Icon

            var thisAppIcon = await MpIcon.CreateAsync(MpBase64Images.AppIcon);

            ThisAppIconId = thisAppIcon.Id;

            // This App

            string thisAppName = MpPrefViewModel.Instance.ApplicationName;
            var thisApp = await MpApp.CreateAsync(
                appPath: MpPlatform.Services.PlatformInfo.ExecutingPath,
                appName: thisAppName,
                iconId: thisAppIcon.Id);

            ThisAppId = thisApp.Id;

            if (MpPlatform.Services.PlatformInfo.IsDesktop) {
                // OS App
                var osApp = await MpApp.CreateAsync(
                    appPath: MpPlatform.Services.PlatformInfo.OsFileManagerPath,
                    appName: MpPlatform.Services.PlatformInfo.OsFileManagerName);
                ThisOsFileManagerAppId = osApp.Id;
            }

        }

        public static async Task InitializeAsync() {
            // USER DEVICE
            var thisUserDevice = await MpDataModelProvider.GetUserDeviceByGuidAsync(MpPrefViewModel.Instance.ThisDeviceGuid);
            if (thisUserDevice == null) {
                // reset error
                Debugger.Break();
            }
            ThisUserDeviceId = thisUserDevice.Id;
            ThisUserDeviceGuid = thisUserDevice.Guid;

            // THIS APP

            var this_app = await MpDataModelProvider.GetAppByMembersAsync(MpPlatform.Services.PlatformInfo.ExecutingPath, null, ThisUserDeviceId);
            if (this_app == null) {
                // reset error
                Debugger.Break();
            }
            ThisAppId = this_app.Id;

            if (MpPlatform.Services.PlatformInfo.IsDesktop) {
                // OS APP

                var osApp = await MpDataModelProvider.GetAppByMembersAsync(
                    MpPlatform.Services.PlatformInfo.OsFileManagerPath,
                    null,
                    ThisUserDeviceId);
                ThisOsFileManagerAppId = osApp.Id;
            }


            // ICON

            var thisAppIcon = await MpDataModelProvider.GetItemAsync<MpIcon>(ThisAppId);
            ThisAppIconId = thisAppIcon.Id;
            ThisAppIconDbImageId = thisAppIcon.IconImageId;

        }

        #endregion

        #region Private Methods
        private static async Task InitUserDeviceAsync(string userDeviceGuid, string machineName, MpUserDeviceType? deviceType) {
            // CASE 1 - initialize
            if (!string.IsNullOrEmpty(userDeviceGuid)) {
                var thisUserDevice = await MpDataModelProvider.GetUserDeviceByGuidAsync(userDeviceGuid);
                if (thisUserDevice == null) {
                    // reset error
                    Debugger.Break();
                }
                ThisUserDeviceId = thisUserDevice.Id;
                ThisUserDeviceGuid = thisUserDevice.Guid;
            }
        }
        #endregion

        #region Commands
        #endregion
    }
}
