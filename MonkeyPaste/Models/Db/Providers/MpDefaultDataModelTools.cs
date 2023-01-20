using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        public static MpUserDeviceType ThisUserDeviceType { get; private set; }

        #endregion

        public static int ThisAppIconId { get; private set; }
        public static int ThisAppIconDbImageId { get; private set; }
        public static int ThisAppId { get; private set; }
        public static int ThisOsFileManagerAppId { get; private set; }


        #endregion

        #region Constructors

        #endregion

        #region Public Methods

        public static async Task<string> DiscoverThisDeviceGuidAsync(MpIDbInfo dbInfo, MpIOsInfo osInfo) {
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
            ThisUserDeviceType = osInfo.OsType;

            await MpDb.CreateTableAsync<MpApp>();

            using var process = Process.GetCurrentProcess();
            string thisAppPath = process.MainModule.FileName;
            var this_app = await MpDataModelProvider.GetAppByMembersAsync(thisAppPath, null, this_device.Id);
            if (this_app == null) {
                // reset error
                Debugger.Break();
            }
            ThisAppId = this_app.Id;


            var this_os_file_manager = await MpDataModelProvider.GetAppByMembersAsync(osInfo.OsFileManagerPath, null, this_device.Id);
            if (this_os_file_manager == null) {
                // reset error
                Debugger.Break();
            }
            ThisOsFileManagerAppId = this_os_file_manager.Id;
            //var osAppSource = await MpDataModelProvider.GetSourceByMembersAsync(osApp.Id,0);
            //MpPrefViewModel.Instance.ThisOsFileManagerAppId = osAppSource.Id;

            await MpDb.CloseConnectionAsync();
            return ThisUserDeviceGuid;
        }

        public static async Task CreateAsync() {

            // User Device

            ThisUserDeviceType = MpPlatformWrapper.Services.OsInfo.OsType;

            ThisUserDeviceGuid = Guid.NewGuid().ToString();

            var thisDevice = new MpUserDevice() {
                UserDeviceGuid = Guid.Parse(MpPrefViewModel.Instance.ThisDeviceGuid),
                PlatformType = ThisUserDeviceType,
                MachineName = Environment.MachineName
            };

            await thisDevice.WriteToDatabaseAsync();

            ThisUserDeviceId = thisDevice.Id;

            // Icon

            var thisAppIcon = await MpIcon.CreateAsync(MpBase64Images.AppIcon);

            ThisAppIconId = thisAppIcon.Id;

            // This App

            var process = Process.GetCurrentProcess();
            string thisAppPath = process.MainModule.FileName;
            string thisAppName = MpPrefViewModel.Instance.ApplicationName;
            var thisApp = await MpApp.CreateAsync(
                appPath: thisAppPath,
                appName: thisAppName,
                iconId: thisAppIcon.Id);

            ThisAppId = thisApp.Id;

            // OS App
            var osApp = await MpApp.CreateAsync(
                appPath: MpPlatformWrapper.Services.OsInfo.OsFileManagerPath,
                appName: MpPlatformWrapper.Services.OsInfo.OsFileManagerName);
            ThisOsFileManagerAppId = osApp.Id;

        }

        public static async Task InitializeAsync(
            string userDeviceGuid,
            MpUserDeviceType osType,
            string osFileManagerPath) {

            // USER DEVICE

            var thisUserDevice = await MpDataModelProvider.GetUserDeviceByGuidAsync(userDeviceGuid);
            if (thisUserDevice == null) {
                // reset error
                Debugger.Break();
            }
            ThisUserDeviceId = thisUserDevice.Id;
            ThisUserDeviceGuid = thisUserDevice.Guid;
            ThisUserDeviceType = osType;

            // THIS APP

            using var process = Process.GetCurrentProcess();
            string thisAppPath = process.MainModule.FileName;
            var this_app = await MpDataModelProvider.GetAppByMembersAsync(thisAppPath, null, ThisUserDeviceId);
            if (this_app == null) {
                // reset error
                Debugger.Break();
            }
            ThisAppId = this_app.Id;

            // OS APP
            
            var osApp = await MpDataModelProvider.GetAppByMembersAsync(
                osFileManagerPath, 
                null, 
                ThisUserDeviceId);
            ThisOsFileManagerAppId = osApp.Id;


            // ICON

            var thisAppIcon = await MpDataModelProvider.GetItemAsync<MpIcon>(ThisAppId);
            ThisAppIconId = thisAppIcon.Id;
            ThisAppIconDbImageId = thisAppIcon.IconImageId;
           
        }

        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
