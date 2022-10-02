using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using SQLitePCL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;
using SQLiteNetExtensions.Extensions;

namespace MonkeyPaste {
    public static class MpDb {
        #region Private Variables

        private static object _rdLock = new object();
        private static SQLiteAsyncConnection _connectionAsync;
        private static SQLiteConnection _connection;

        #endregion

        #region Properties

        public static bool UseWAL { get; set; } = false;
        public static string IdentityToken { get; set; }
        public static string AccessToken { get; set; }
        public static bool IsLoaded { get; set; } = false;

        public static bool IgnoreLogging { get; set; } = true;

        #endregion

        #region Events
        public static event EventHandler OnInitDefaultNativeData;

        public static event EventHandler<MpDbModelBase> OnItemAdded;
        public static event EventHandler<MpDbModelBase> OnItemUpdated;
        public static event EventHandler<MpDbModelBase> OnItemDeleted;

        public static event EventHandler<MpDbSyncEventArgs> SyncAdd;
        public static event EventHandler<MpDbSyncEventArgs> SyncUpdate;
        public static event EventHandler<MpDbSyncEventArgs> SyncDelete;
        #endregion

        #region Constructors



        #endregion

        #region Public Methods

        public static async Task InitAsync() {
            var sw = new Stopwatch();
            sw.Start();
            //MpJsonPreferenceIO.Instance.StartupDateTime = DateTime.Now;
            await InitDbAsync();
            IsLoaded = true;
            sw.Stop();
            MpConsole.WriteLine($"Db loading: {sw.ElapsedMilliseconds} ms");
        }


        public static string GetDbFileAsBase64() {
            var bytes = File.ReadAllBytes(MpPlatformWrapper.Services.DbInfo.DbPath);
            return Convert.ToBase64String(bytes);
        }
        #region Queries

        #region Async

        public static async Task<TableMapping> GetTableMappingAsync(string tableName) {
            await Task.Delay(1);
            if (_connectionAsync == null) {
                CreateConnection();
            }
            return _connectionAsync
                    .TableMappings
                    .Where(x => x.TableName.ToLower() == tableName.ToLower()).FirstOrDefault();
        }


        public static async Task<List<object>> QueryAsync(string tableName,string query, params object[] args) {
            if (_connectionAsync == null) {
                CreateConnection();
            }
            TableMapping qtm = null;
            foreach(var tm in _connectionAsync.TableMappings) {
                if(tm.TableName.ToLower() == tableName.ToLower()) {
                    qtm = tm;
                    break;
                }
            }
            if(qtm == null) {
                return new List<object>();
            }
            var result = await _connectionAsync.QueryAsync(qtm, query, args);
            return result;
        }


        public static async Task<List<T>> QueryAsync<T>(string query, params object[] args) where T : new() {
            if (_connectionAsync == null) {
                CreateConnection();
            }
            var result = await _connectionAsync.QueryAsync<T>(query, args);
            return result;
        }

        public static async Task<T> QueryScalarAsync<T>(string query, params object[] args) {
            if(_connectionAsync == null) {
                CreateConnection();
            }
            var result = await _connectionAsync.ExecuteScalarAsync<T>(query, args);
            return result;
        }

        public static async Task<List<T>> QueryScalarsAsync<T>(string query, params object[] args) {
            if (_connectionAsync == null) {
                CreateConnection();
            }
            var result = await _connectionAsync.QueryScalarsAsync<T>(query, args);
            return result;
        }

        public static async Task<List<T>> GetItemsAsync<T>() where T : new() {
            if (_connectionAsync == null) {
                CreateConnection();
            }
            var dbol = await _connectionAsync.GetAllWithChildrenAsync<T>(recursive: true);
            return dbol;
        }
        public static async Task<T> GetItemAsync<T>(int id) where T : new() {
            if (_connectionAsync == null) {
                CreateConnection();
            }
            T dbo;
            try {
                dbo = await _connectionAsync.GetWithChildrenAsync<T>(id, true);
            } catch(Exception ex) {
                MpConsole.WriteTraceLine($"Likely trying to access item of type '{typeof(T).Name}' with id {id} and it does not exist.", ex);
                return default;
            }
            return dbo;
        }

        public static async Task<List<T>> GetAllWithChildrenAsync<T>(Expression<Func<T, bool>> exp, bool recursive = true) where T : new() {
            if (_connectionAsync == null) {
                CreateConnection();
            }
            var result = await _connectionAsync.GetAllWithChildrenAsync<T>(exp, recursive);
            return result;
        }

        private static async Task AddItemAsync<T>(T item, string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {
            if (_connectionAsync == null) {
                CreateConnection();
            }
            sourceClientGuid = GetSourceClientGuid(sourceClientGuid);

            
            if (item == null) {
                MpConsole.WriteTraceLine(@"Cannot add null item, ignoring...");
                return;
            }

            await LogWriteAsync(MpDbLogActionType.Create, item as MpDbModelBase, sourceClientGuid, ignoreTracking);

            if (item is MpCopyItemTag cit) {
                if(cit.CopyItemId == 0 && cit.TagId == 0) {
                    return;
                }
            }

            //await RunInMutex(async () => {
                await _connectionAsync.InsertOrReplaceWithChildrenAsync(item, recursive: true);
            //});            

            NotifyWrite(MpDbLogActionType.Create, item as MpDbModelBase, ignoreSyncing);
        }

        private static async Task UpdateItemAsync<T>(T item,string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {
            if (_connectionAsync == null) {
                CreateConnection();
            }
            sourceClientGuid = GetSourceClientGuid(sourceClientGuid);
            

            if (item == null) {
                MpConsole.WriteTraceLine(@"Cannot update null item, ignoring...");
                return;
            }

            await LogWriteAsync(MpDbLogActionType.Modify, item as MpDbModelBase, sourceClientGuid, ignoreTracking);

            //await RunInMutex(async () => {
            await _connectionAsync.UpdateWithChildrenAsync(item);
            //});

            NotifyWrite(MpDbLogActionType.Modify, item as MpDbModelBase, ignoreSyncing);
        }

        public static async Task AddOrUpdateAsync<T>(T item, string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {
            if ((item as MpDbModelBase).Id == 0) {
                await AddItemAsync(item, sourceClientGuid, ignoreTracking, ignoreSyncing);
            } else {
                await UpdateItemAsync(item, sourceClientGuid, ignoreTracking, ignoreSyncing);
            }
        }

        public static async Task DeleteItemAsync<T>(T item, string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {
            if (_connectionAsync == null) {
                CreateConnection();
            }
            sourceClientGuid = GetSourceClientGuid(sourceClientGuid);
            
            if (item == null) {
                MpConsole.WriteTraceLine(@"Cannot delete null item, ignoring...");
                return;
            }

            await LogWriteAsync(MpDbLogActionType.Delete, item as MpDbModelBase, sourceClientGuid, ignoreTracking);

            //await RunInMutex( async()=> {
            await _connectionAsync.DeleteAsync(item, true);
            //});

            NotifyWrite(MpDbLogActionType.Delete, item as MpDbModelBase, ignoreSyncing);
        }
                
        public static async Task<object> GetDbObjectByTableGuidAsync(string tableName, string objGuid) {
            var dt = await QueryAsync(
                tableName,
                string.Format("select * from {0} where {1}=?", tableName, tableName + "Guid"),
                objGuid);

            if (dt != null && dt.Count > 0) {
                return dt[0];
            }
            var dbot = new MpXamStringToSyncObjectTypeConverter().Convert(tableName);

            var dbo = Activator.CreateInstance(dbot);
            return dbo;
        }

        public static async Task<T> GetDbObjectByTableGuidAsync<T>(string objGuid) where T : new() {
            string tableName = typeof(T).ToString().Replace("MonkeyPaste.", string.Empty);
            var dt = await QueryAsync(
                tableName,
                string.Format("select * from {0} where {1}=?", tableName, tableName + "Guid"),
                objGuid);

            if (dt != null && dt.Count > 0) {
                var item = await GetItemAsync<T>((dt[0] as MpDbModelBase).Id);
                return item;
                //return dbo;
                //return dt[0];
            }

            var dbo = Activator.CreateInstance(typeof(T));
            return (T)dbo;
        }

        #endregion

        #region Sync
        public static List<T> GetItems<T>(string dbPath = "") where T : new() {
            if (_connection == null) {
                CreateConnection(dbPath);
            }
            var dbol = _connection.GetAllWithChildren<T>(recursive: true);
            return dbol;
        }

        public static T GetItem<T>(int id) where T : new() {
            if (_connection == null) {
                CreateConnection();
            }
            var dbo = _connection.Get<T>(id);
            return dbo;
        }

        public static List<T> Query<T>(string query, params object[] args) where T : new() {
            if (_connection == null) {
                CreateConnection();
            }
            var result = _connection.Query<T>(query, args);
            return result;
        }

        public static T QueryScalar<T>(string query, params object[] args) {
            if (_connection == null) {
                CreateConnection();
            }
            var result = _connection.ExecuteScalar<T>(query, args);
            return result;
        }

        public static List<T> QueryScalars<T>(string query, params object[] args) {
            if (_connection == null) {
                CreateConnection();
            }
            var result = _connection.QueryScalars<T>(query, args);
            return result;
        }
        #endregion

        #endregion

        public static byte[] GetDbFileBytes() {
            return File.ReadAllBytes(MpPlatformWrapper.Services.DbInfo.DbPath);
        }

        #endregion

        #region Private Methods  

        private static string GetSourceClientGuid(string providedSourceClientGuid) {
            if(!IsLoaded) {
                return null;
            }
            return string.IsNullOrEmpty(providedSourceClientGuid) ? MpPrefViewModel.Instance.ThisDeviceGuid : providedSourceClientGuid;
        }

        private static async Task LogWriteAsync(MpDbLogActionType actionType, MpDbModelBase item, string sourceClientGuid, bool ignoreTracking) {
            if(!IsLoaded || IgnoreLogging) {
                return;
            }
            if (item is MpISyncableDbObject && item is not MpDbLog && item is not MpSyncHistory) {
                if (string.IsNullOrEmpty(item.Guid)) {
                    if(actionType != MpDbLogActionType.Create) {
                        throw new Exception("Syncable object must have a predefined guid");
                    }
                    MpConsole.WriteLine("Warning, object " + item.ToString() + " doesn't have a guid on add, creating one for it");
                    item.Guid = System.Guid.NewGuid().ToString();
                }
                if (!ignoreTracking) {
                    await MpDbLogTracker.LogDbWriteAsync(actionType, item, sourceClientGuid);
                }
            }
        }

        private static void NotifyWrite(MpDbLogActionType actionType, MpDbModelBase item, bool ignoreSyncing) {
            switch(actionType) {
                case MpDbLogActionType.Create:
                    OnItemAdded?.Invoke(nameof(MpDb), item);
                    break;
                case MpDbLogActionType.Modify:
                    OnItemUpdated?.Invoke(nameof(MpDb), item);
                    break;
                case MpDbLogActionType.Delete:
                    OnItemDeleted?.Invoke(nameof(MpDb), item);
                    break;
            }

            if (!ignoreSyncing && item is MpISyncableDbObject) {
            }
        }

        private static async Task<bool> InitDbConnectionAsync(MpIDbInfo dbInfo, bool allowCreate) {
            //SQLitePCL.Batteries.Init();

            var dbPath = dbInfo.DbPath;

            //File.Delete(dbPath);

            bool isNewDb = !File.Exists(dbPath);

            if (isNewDb && allowCreate) {
                using (File.Create(dbPath)) { }
            }

            CreateConnection(dbPath);

            if (UseWAL) {
                if (_connectionAsync != null) {
                    await _connectionAsync.EnableWriteAheadLoggingAsync().ConfigureAwait(false);
                }
            }
            return isNewDb;
        }

        private static async Task InitDbAsync() {
            bool isNewDb = await InitDbConnectionAsync(MpPlatformWrapper.Services.DbInfo, true);
            

            await InitTablesAsync();
            
            if (isNewDb) {
                await CreateViewsAsync();
                await InitDefaultDataAsync();

                OnInitDefaultNativeData?.Invoke(nameof(MpDb), null);
            }

            if(Environment.CurrentDirectory.Contains("MpWpfApp")) {
                MpTag.AllTagId = 2;
                MpTag.FavoritesTagId = 3;
                MpPrefViewModel.Instance.ThisAppSourceId = 5;
                MpPrefViewModel.Instance.ThisOsFileManagerSourceId = 4;
                MpPrefViewModel.Instance.ThisDeviceGuid = "f64b221e-806a-4e28-966a-f9c5ff0d9370";
            }


            MpPrefViewModel.Instance.ThisUserDevice = await MpDataModelProvider.GetUserDeviceByGuidAsync(MpPrefViewModel.Instance.ThisDeviceGuid);
            
            MpPrefViewModel.Instance.ThisAppSource = await GetItemAsync<MpSource>(MpPrefViewModel.Instance.ThisAppSourceId);
            var thisAppApp = await MpDb.GetItemAsync<MpApp>(MpPrefViewModel.Instance.ThisAppSource.AppId);
            MpPrefViewModel.Instance.ThisAppIcon = await GetItemAsync<MpIcon>(thisAppApp.IconId);
            MpPrefViewModel.Instance.ThisOsFileManagerSource = await GetItemAsync<MpSource>(MpPrefViewModel.Instance.ThisOsFileManagerSourceId);


            if(isNewDb) {
                OnInitDefaultNativeData?.Invoke(nameof(MpDb), null);
            }


            MpConsole.WriteLine(@"Db file located: " + MpPlatformWrapper.Services.DbInfo.DbPath);
            MpConsole.WriteLine(@"This Client Guid: " + MpPrefViewModel.Instance.ThisDeviceGuid);
            MpConsole.WriteLine("Write ahead logging: " + (UseWAL ? "ENABLED" : "DISABLED"));
        }

        private static void CreateConnection(string dbPath = "") {
            if (string.IsNullOrEmpty(dbPath)) {
                dbPath = MpPlatformWrapper.Services.DbInfo.DbPath;
            }
            if(_connection != null && _connectionAsync != null) {
                return;
            }
            
            var _connStr = new SQLiteConnectionString(
                            databasePath: dbPath,
                            storeDateTimeAsTicks: true,
                            openFlags: SQLiteOpenFlags.ReadWrite |
                                       SQLiteOpenFlags.Create |
                                       SQLiteOpenFlags.SharedCache |
                                       SQLiteOpenFlags.FullMutex);

            if (_connectionAsync == null) {
                SQLitePCL.Batteries.Init();

                _connectionAsync = new SQLiteAsyncConnection(_connStr) { Trace = true };
                SQLitePCL.raw.sqlite3_create_function(_connectionAsync.GetConnection().Handle,"REGEXP",2, null, MatchRegex);                
            }
            if(_connection == null) {
                _connection = new SQLiteConnection(_connStr) { Trace = true };
                SQLitePCL.raw.sqlite3_create_function(_connection.Handle, "REGEXP", 2, null, MatchRegex);
            }
        }
        private static void MatchRegex(sqlite3_context ctx, object user_data, sqlite3_value[] args) {
            string input = SQLitePCL.raw.sqlite3_value_text(args[1]).utf8_to_string();
            input = input == null ? string.Empty : input;
            string pattern = SQLitePCL.raw.sqlite3_value_text(args[0]).utf8_to_string();
            pattern = pattern == null ? string.Empty : pattern;

            bool isMatched = System.Text.RegularExpressions.Regex.IsMatch(
                input,
                pattern,
                RegexOptions.IgnoreCase);

            if (isMatched)
                SQLitePCL.raw.sqlite3_result_int(ctx, 1);
            else
                SQLitePCL.raw.sqlite3_result_int(ctx, 0);
        }

        private static async Task InitTablesAsync() {
            await _connectionAsync.CreateTableAsync<MpAction>();
            await _connectionAsync.CreateTableAsync<MpPluginPresetParameterValue>();
            await _connectionAsync.CreateTableAsync<MpApp>();
            await _connectionAsync.CreateTableAsync<MpAppClipboardFormatInfo>();
            await _connectionAsync.CreateTableAsync<MpAppPasteShortcut>();
            await _connectionAsync.CreateTableAsync<MpBillableItem>();
            await _connectionAsync.CreateTableAsync<MpCliTransaction>();
            await _connectionAsync.CreateTableAsync<MpCopyItem>();
            await _connectionAsync.CreateTableAsync<MpCopyItemTag>();
            await _connectionAsync.CreateTableAsync<MpCopyItemTransaction>();
            await _connectionAsync.CreateTableAsync<MpDataObject>();
            await _connectionAsync.CreateTableAsync<MpDataObjectItem>();
            await _connectionAsync.CreateTableAsync<MpDbImage>();
            await _connectionAsync.CreateTableAsync<MpDbLog>();
            await _connectionAsync.CreateTableAsync<MpImageAnnotation>();
            await _connectionAsync.CreateTableAsync<MpDllTransaction>();
            await _connectionAsync.CreateTableAsync<MpHttpTransaction>();
            await _connectionAsync.CreateTableAsync<MpIcon>();
            await _connectionAsync.CreateTableAsync<MpPasteHistory>();
            await _connectionAsync.CreateTableAsync<MpPasteToAppPath>();
            await _connectionAsync.CreateTableAsync<MpPluginPreset>();
            await _connectionAsync.CreateTableAsync<MpSearchCriteriaItem>();
            await _connectionAsync.CreateTableAsync<MpShortcut>();
            await _connectionAsync.CreateTableAsync<MpSource>();
            await _connectionAsync.CreateTableAsync<MpSyncHistory>();
            await _connectionAsync.CreateTableAsync<MpTag>();
            await _connectionAsync.CreateTableAsync<MpTextAnnotation>();
            await _connectionAsync.CreateTableAsync<MpTextTemplate>();
            await _connectionAsync.CreateTableAsync<MpContentToken>();
            await _connectionAsync.CreateTableAsync<MpUrl>();
            await _connectionAsync.CreateTableAsync<MpUserDevice>();
            await _connectionAsync.CreateTableAsync<MpUserSearch>();
            await _connectionAsync.CreateTableAsync<MpUserTransaction>();
        }

        private static async Task CreateViewsAsync() {
            await _connectionAsync.ExecuteAsync(@"CREATE VIEW MpSortableCopyItem_View as
                                                    SELECT 
	                                                    pk_MpCopyItemId as RootId,
	                                                    fk_MpCopyItemTypeId,
	                                                    Title,
	                                                    ItemData,
	                                                    ItemWidth,
	                                                    ItemHeight,
	                                                    ItemDescription,
	                                                    CopyDateTime,
	                                                    (select PasteDateTime from MpPasteHistory where fk_MpCopyItemId=pk_MpCopyItemId order by PasteDateTime desc limit 1) AS LastPasteDateTime,
	                                                    CopyCount,
	                                                    PasteCount,
	                                                    CopyCount + PasteCount as UsageScore,
	                                                    MpSource.pk_MpSourceId AS SourceId,
	                                                    case MpSource.fk_MpUrlId
		                                                    when 0
			                                                    then MpApp.SourcePath
		                                                    ELSE
			                                                    MpUrl.UrlPath
	                                                    end as SourcePath,
	                                                    MpApp.AppName,
	                                                    MpApp.SourcePath as AppPath,
	                                                    MpApp.pk_MpAppId AS AppId,
	                                                    MpUrl.pk_MpUrlId AS UrlId,
	                                                    MpUrl.UrlPath,
	                                                    MpUrl.UrlDomainPath,
	                                                    MpUrl.UrlTitle,
	                                                    MpUserDevice.MachineName AS SourceDeviceName,
	                                                    MpUserDevice.PlatformTypeId AS SourceDeviceType
                                                    FROM
	                                                    MpCopyItem
                                                    INNER JOIN MpUserDevice ON MpUserDevice.pk_MpUserDeviceId = MpApp.fk_MpUserDeviceId
                                                    INNER JOIN MpSource ON MpSource.pk_MpSourceId = MpCopyItem.fk_MpSourceId
                                                    INNER JOIN MpApp ON MpApp.pk_MpAppId = MpSource.fk_MpAppId
                                                    LEFT JOIN MpUrl ON MpUrl.pk_MpUrlId = MpSource.fk_MpUrlId");

            //await _connectionAsync.ExecuteAsync(@"CREATE VIEW MpSortableCopyItem_View as
            //                                        SELECT 
	           //                                         case fk_ParentCopyItemId
		          //                                          when 0
			         //                                           then pk_MpCopyItemId
		          //                                          ELSE
			         //                                           fk_ParentCopyItemId
	           //                                         end as RootId,
	           //                                         pk_MpCopyItemId as RootId,
	           //                                         fk_MpCopyItemTypeId,
	           //                                         CompositeSortOrderIdx,
	           //                                         Title,
	           //                                         ItemData,
	           //                                         ItemDescription,
	           //                                         CopyDateTime,
	           //                                         (select PasteDateTime from MpPasteHistory where fk_MpCopyItemId=pk_MpCopyItemId order by PasteDateTime desc limit 1) AS LastPasteDateTime,
	           //                                         CopyCount,
	           //                                         PasteCount,
	           //                                         CopyCount + PasteCount as UsageScore,
	           //                                         MpSource.pk_MpSourceId AS SourceId,
	           //                                         case MpSource.fk_MpUrlId
		          //                                          when 0
			         //                                           then MpApp.SourcePath
		          //                                          ELSE
			         //                                           MpUrl.UrlPath
	           //                                         end as SourcePath,
	           //                                         MpApp.AppName,
	           //                                         MpApp.SourcePath as AppPath,
	           //                                         MpApp.pk_MpAppId AS AppId,
	           //                                         MpUrl.pk_MpUrlId AS UrlId,
	           //                                         MpUrl.UrlPath,
	           //                                         MpUrl.UrlDomainPath,
	           //                                         MpUrl.UrlTitle,
	           //                                         MpUserDevice.MachineName AS SourceDeviceName,
	           //                                         MpUserDevice.PlatformTypeId AS SourceDeviceType
            //                                        FROM
	           //                                         MpCopyItem
            //                                        INNER JOIN MpUserDevice ON MpUserDevice.pk_MpUserDeviceId = MpApp.fk_MpUserDeviceId
            //                                        INNER JOIN MpSource ON MpSource.pk_MpSourceId = MpCopyItem.fk_MpSourceId
            //                                        INNER JOIN MpApp ON MpApp.pk_MpAppId = MpSource.fk_MpAppId
            //                                        LEFT JOIN MpUrl ON MpUrl.pk_MpUrlId = MpSource.fk_MpUrlId");
        }

        public static async Task<bool> ResetPreferenceDefaultsAsync(MpIDbInfo dbInfo, MpIOsInfo osInfo) {
            bool wouldBeNewDb = await InitDbConnectionAsync(dbInfo, false);
            if (wouldBeNewDb) {
                //this should be caught in pref init so somethings wrong
                Debugger.Break();
                return false;
            }
            await _connectionAsync.CreateTableAsync<MpUserDevice>();
            MpUserDevice this_device = await MpDataModelProvider.GetUserDeviceByMachineNameAndDeviceTypeAsync(osInfo.OsMachineName,osInfo.OsType);
            MpPrefViewModel.Instance.ThisDeviceGuid = this_device.Guid;
            MpPrefViewModel.Instance.ThisDeviceType = osInfo.OsType;

            await _connectionAsync.CreateTableAsync<MpApp>();
            var process = Process.GetCurrentProcess();
            string thisAppPath = process.MainModule.FileName;
            var thisApp = await MpDataModelProvider.GetAppByPathAsync(thisAppPath, this_device.Id);

            await _connectionAsync.CreateTableAsync<MpSource>();
            var thisAppSource = await MpSource.Create(appId: thisApp.Id, urlId: 0);
            MpPrefViewModel.Instance.ThisAppSourceId = thisAppSource.Id;


            var osApp = await MpDataModelProvider.GetAppByPathAsync(osInfo.OsFileManagerPath, this_device.Id);
            var osAppSource = await MpDataModelProvider.GetSourceByMembersAsync(osApp.Id,0);
            MpPrefViewModel.Instance.ThisOsFileManagerSourceId = osAppSource.Id;

            await _connectionAsync.CloseAsync();
            _connectionAsync = null;
            return true;
        }

        private static async Task InitDefaultDataAsync() {
            // NOTE! MpTag.AllTagId needs to be changed to 1 not 2 since recent was removed

            #region User Device

            MpPrefViewModel.Instance.ThisDeviceType = MpPlatformWrapper.Services.OsInfo.OsType;

            MpPrefViewModel.Instance.ThisDeviceGuid = Guid.NewGuid().ToString();

            var thisDevice = new MpUserDevice() {
                UserDeviceGuid = Guid.Parse(MpPrefViewModel.Instance.ThisDeviceGuid),
                PlatformType = MpPrefViewModel.Instance.ThisDeviceType,
                MachineName = Environment.MachineName
            };

            await AddItemAsync<MpUserDevice>(thisDevice);
            MpPrefViewModel.Instance.ThisUserDevice = thisDevice;

            #endregion

            #region Tags

            await AddItemAsync<MpTag>(new MpTag() {
                TagGuid = Guid.Parse("df388ecd-f717-4905-a35c-a8491da9c0e3"),
                TagName = "All",
                HexColor = Color.Blue.ToHex(),
                TagSortIdx = 1
            }, "", true, true);

            await AddItemAsync<MpTag>(new MpTag() {
                TagGuid = Guid.Parse("54b61353-b031-4029-9bda-07f7ca55c123"),
                ParentTagId = 1,
                TagName = "Favorites",
                HexColor = Color.Yellow.ToHex(),
                TagSortIdx = 2
            }, "", true, true);

            var helpTag = new MpTag() {
                TagGuid = Guid.Parse("a0567976-dba6-48fc-9a7d-cbd306a4eaf3"),
                TagName = "Help",
                HexColor = Color.Orange.ToHex(),
                TagSortIdx = 3
            };
            await AddItemAsync<MpTag>(helpTag, "", true, true);

            #endregion

            #region Shortcuts

            await InitDefaultShortcutsAsync();

            #endregion


            #region Icon

            var sourceIcon = await MpIcon.Create(MpBase64Images.AppIcon);

            #endregion

            #region Source

            var process = Process.GetCurrentProcess();
            string thisAppPath = process.MainModule.FileName;
            string thisAppName = MpPrefViewModel.Instance.ApplicationName;
            var thisApp = await MpApp.CreateAsync(thisAppPath, thisAppName, sourceIcon.Id);
            var thisAppSource = await MpSource.Create(appId: thisApp.Id, urlId: 0);
            MpPrefViewModel.Instance.ThisAppSourceId = thisAppSource.Id;

            var osApp = await MpApp.CreateAsync(MpPlatformWrapper.Services.OsInfo.OsFileManagerPath, MpPlatformWrapper.Services.OsInfo.OsFileManagerName);
            var osAppSource = await MpSource.Create(osApp.Id, 0);
            MpPrefViewModel.Instance.ThisOsFileManagerSourceId = osAppSource.Id;

            #endregion

            MpConsole.WriteTraceLine(@"Created all default tables");
        }

        private static async Task InitDefaultShortcutsAsync() {
            List<string[]> defaultShortcutDefinitions = new List<string[]>() {
                // ORDER:
                // guid,keyString,shortcutType,routeType
                 new string[] {"5dff238e-770e-4665-93f5-419e48326f01","Control+Shift+D", "ShowMainWindow", "Direct"},
                 new string[] {"cb807500-9121-4e41-80d3-8c3682ce90d9","Escape", "HideMainWindow", "Internal"},
                 new string[] {"a41aeed8-d4f3-47de-86c5-f9ca296fb103","Control+Shift+A", "ToggleAppendMode", "Direct"},
                 new string[] {"892bf7d7-ba8e-4db1-b2ca-62b41ff6614c","Control+Shift+C", "ToggleAutoCopyMode", "Direct"},
                 new string[] {"a12c4211-ab1f-4b97-98ff-fbeb514e9a1c","Control+Shift+R", "ToggleRightClickPasteMode", "Direct"},
                 new string[] {"1d212ca5-fb2a-4962-8f58-24ed9a5d007d","Enter", "PasteSelectedItems", "Internal"},
                 new string[] {"e94ca4f3-4c6e-40dc-8941-c476a81543c7","Delete", "DeleteSelectedItems", "Internal"},
                 new string[] {"7fe24929-6c9e-49c0-a880-2f49780dfb3a","Right", "SelectNextItem", "Internal"},
                 new string[] {"ee657845-f1dc-40cf-848d-6768c0081670","Left", "SelectPreviousItem", "Internal"},
                 new string[] {"5480f103-eabd-4e40-983c-ebae81645a10","Control+A", "SelectAll", "Internal"},
                 new string[] {"39a6b8b5-a585-455b-af83-015fd97ac3fa","Control+Shift+Alt+A", "InvertSelection", "Internal"},
                 new string[] {"166abd7e-7295-47f2-bbae-c96c03aa6082","Control+Home", "BringSelectedToFront", "Internal"},
                 new string[] {"84c11b86-3acc-4d22-b8e9-3bd785446f72","Control+End", "SendSelectedToBack", "Internal"},
                 new string[] {"6487f6ff-da0c-475b-a2ae-ef1484233de0","Control+Shift+H", "AssignShortcut", "Internal"},
                 new string[] {"837e0c20-04b8-4211-ada0-3b4236da0821","Control+Shift+Alt+C", "ChangeColor", "Internal"},
                 new string[] {"4a567aff-33a8-4a1f-8484-038196812849","Control+Shift+S", "SpeakSelectedItem", "Internal"},
                 new string[] {"330afa20-25c3-425c-8e18-f1423eda9066","Control+Shift+M", "MergeSelectedItems", "Internal"},
                 new string[] {"118a2ca6-7021-47a0-8458-7ebc31094329","Control+Z", "Undo", "Internal"},
                 new string[] {"3980efcc-933b-423f-9cad-09e455c6824a","Control+Y", "Redo", "Internal"},
                 new string[] {"7a7580d1-4129-432d-a623-2fff0dc21408","Control+E", "EditContent", "Internal"},
                 new string[] {"085338fb-f297-497a-abb7-eeb7310dc6f3","F2", "EditTitle", "Internal"},
                 new string[] {"e22faafd-4313-441a-b361-16910fc7e9d3","Control+D", "Duplicate", "Internal"},
                 new string[] {"4906a01e-b2f7-43f0-af1e-fb99d55c9778","Control+E", "SendToEmail", "Internal"},
                 new string[] {"c7248087-2031-406d-b4ab-a9007fbd4bc4","Control+Shift+Q", "CreateQrCode", "Internal"},
                 new string[] {"777367e6-c161-4e93-93e0-9bf12221f7ff","Control+Shift+B", "ToggleAppendLineMode", "Direct"},
                 new string[] {"97e29b06-0ec4-4c55-a393-8442d7695038","Control+Shift+P", "ToggleListenToClipboard", "Direct"},
                 new string[] {"ee74dd92-d18b-46cf-91b7-3946ab55427c","Control+C", "CopySelection", "Internal"},
                 new string[] {"ac8abe92-82c3-46fb-9bd5-39d74b100b23","Home", "ScrollToHome", "Internal"},
                 new string[] {"ac8abe92-82c3-46fb-9bd5-39d74b100b23","End", "ScrollToEnd", "Internal"},
                 new string[] {"9b0ca09a-5724-4004-98d2-f5ef8ae02055","Control+Up", "WindowSizeUp", "Internal"},
                 new string[] {"39a6194e-37e3-4d37-a9f4-254ed83157f2","Control+Down", "WindowSizeDown", "Internal"},
                 new string[] {"6cc03ef0-3b33-4b94-9191-0d751e6b7fb6","Control+Left", "WindowSizeLeft", "Internal"},
                 new string[] {"c4ac1629-cdf0-4075-94af-8f934b014452","Control+Right", "WindowSizeRight", "Internal"},
                 new string[] {"94e81589-fe2f-4e80-8940-ed066f0d9c27","Control+V", "PasteHere", "Internal"},
                 new string[] {"30c813a0-d466-4ae7-b75e-82680b4542fc","PageUp", "PreviousPage", "Internal"},
                 new string[] {"09df97ea-f786-48d9-9112-a60266df6586","PageDown", "NextPage", "Internal"},
                 new string[] {"a39ac0cb-41e4-47b5-b963-70e388dc156a","Control+H", "FindAndReplaceSelectedItem", "Internal"},
                 new string[] {"cb1ac03b-a20f-4911-bf4f-bc1a858590e3", "Control+L", "ToggleMainWindowLocked", "Internal"},
                 new string[] {"d73204f5-fbed-4d87-9dca-6dfa8d8cba82", "Control+K", "ToggleFilterMenuVisible", "Internal"},
                 new string[] { "49f44a89-e381-4d6a-bf8c-1090eb443f17", "Control+Q", "ExitApplication", "Internal"}
            };

            foreach (var defaultShortcut in defaultShortcutDefinitions) {
                await MpShortcut.CreateAsync(
                    guid: defaultShortcut[0],
                    keyString: defaultShortcut[1],
                    shortcutType: defaultShortcut[2].ToEnum<MpShortcutType>(),
                    routeType: defaultShortcut[3].ToEnum<MpRoutingType>());
            }
        }

        private static void NotifyRemoteUpdate(MpDbLogActionType actionType, object dbo, string sourceClientGuid) {
            var eventArgs = new MpDbSyncEventArgs() {
                DbObject = dbo,
                EventType = actionType,
                SourceGuid = sourceClientGuid
            };
            switch (actionType) {
                case MpDbLogActionType.Create:
                    SyncAdd?.Invoke(dbo, eventArgs);
                    break;
                case MpDbLogActionType.Modify:
                    SyncUpdate?.Invoke(dbo, eventArgs);
                    break;
                case MpDbLogActionType.Delete:
                    SyncDelete?.Invoke(dbo, eventArgs);
                    break;
            }
        }

        #endregion

        #region Sync Data
        public static bool IsWpf() {
            return false;
        }

        public static bool IsConnectedToNetwork() {
            return MpNetworkHelpers.IsConnectedToNetwork();
        }

        public static bool IsConnectedToInternet() {
            return MpNetworkHelpers.IsConnectedToInternet();
        }
        public static int GetSyncPort() {
            return 44381;
        }
        public static string GetThisClientGuid() {
            return MpPrefViewModel.Instance.ThisDeviceGuid;
        }
        public static string GetPrimaryLocalIp4Address() {
            if (!IsConnectedToNetwork()) {
                return "0.0.0.0";
            }
            return MpNetworkHelpers.GetLocalIp4Address();
        }

        public static string[] GetAllLocalIp4Addresses() {
            if (!IsConnectedToNetwork()) {
                return new string[] { "0.0.0.0" };
            }
            return MpNetworkHelpers.GetAllLocalIPv4();
        }

        public static string GetExternalIp4Address() {
            if (!IsConnectedToInternet()) {
                return "0.0.0.0";
            }
            return MpNetworkHelpers.GetExternalIp4Address();
        }

        public static async Task<List<MpDbLog>> GetDbObjectLogsAsync(string dboGuid, DateTime fromDtUtc) {
            var logs = await MpDataModelProvider.GetDbLogsByGuidAsync(dboGuid, fromDtUtc);
            return logs;
        }

        public static async Task<DateTime> GetLastSyncForRemoteDeviceAsync(string otherDeviceGuid) {
            var shl = await GetItemsAsync<MpSyncHistory>();
            if(shl.Count == 0) {
                return DateTime.MinValue;
            }
            var lsh = shl
                        .Where(x=>x.OtherClientGuid.ToString() == otherDeviceGuid)
                        .OrderByDescending(x=>x.SyncDateTime)
                        .FirstOrDefault();
            if (lsh != null) {
                return lsh.SyncDateTime;
            }
            return DateTime.MinValue;
        }

        public static async Task<string> GetLocalLogFromSyncDateAsync(DateTime fromDateTime, string ignoreGuid = "") {
            var logItems = await GetItemsAsync<MpDbLog>();
            var matchLogItems = logItems.Where(x => x.LogActionDateTime > fromDateTime && x.SourceClientGuid.ToString() != ignoreGuid).ToList();

            var dbol = new List<MpISyncableDbObject>();
            foreach (var li in matchLogItems) {
                dbol.Add(li as MpISyncableDbObject);
            }
            if(dbol.Count == 0) {
                return string.Empty;
            }
            var dbMsgStr = MpDbMessage.Create(dbol);
            return dbMsgStr;
        }

        public static async Task<Dictionary<Guid, List<MpDbLog>>> PrepareRemoteLogForSyncingAsync(string dbLogMessageStr) {
            var dbLogMessage = MpDbMessage.Parse(dbLogMessageStr, GetTypeConverter());

            var remoteDbLogs = new List<MpDbLog>();
            var dbLogWorker = new MpDbLog();

            //deserialize logs and put into guid buckets
            var remoteItemChangeLookup = new Dictionary<Guid, List<MpDbLog>>();
            foreach (var remoteLogRow in dbLogMessage.DbObjects) {
                var logItem = await dbLogWorker.DeserializeDbObjectAsync(remoteLogRow.ObjStr) as MpDbLog;
                if (remoteItemChangeLookup.ContainsKey(logItem.DbObjectGuid)) {
                    remoteItemChangeLookup[logItem.DbObjectGuid].Add(logItem);
                } else {
                    remoteItemChangeLookup.Add(logItem.DbObjectGuid, new List<MpDbLog>() { logItem });
                }
            }

            return remoteItemChangeLookup;
        }

        public static async Task PerformSyncAsync(
            Dictionary<Guid, List<MpDbLog>> changeLookup,
            string remoteClientGuid) {            
            var lastSyncDt = await GetLastSyncForRemoteDeviceAsync(remoteClientGuid);
            //filter & separate remote logs w/ local updates after remote action dt 
            var addChanges = new Dictionary<Guid, List<MpDbLog>>();
            var updateChanges = new Dictionary<Guid, List<MpDbLog>>();
            var deleteChanges = new Dictionary<Guid, List<MpDbLog>>();
            foreach (var ckvp in changeLookup) {
                if (ckvp.Value == null || ckvp.Value.Count == 0) {
                    continue;
                }
                //filter changes by > local action date time
                var rlogs = ckvp.Value;//await MpDbLog.FilterOutdatedRemoteLogs(ckvp.Key.ToString(), ckvp.Value, lastSyncDt); //
                if (rlogs.Count > 0) {
                    //seperate changes into 3 types
                    foreach(var l in rlogs.OrderBy(x => x.LogActionDateTime).ToList()) {
                        switch(l.LogActionType) {
                            case MpDbLogActionType.Create:
                                if(!addChanges.ContainsKey(ckvp.Key)) {
                                    addChanges.Add(ckvp.Key, new List<MpDbLog>() { l });
                                } else {
                                    addChanges[ckvp.Key].Add(l);
                                }                                
                                break;
                            case MpDbLogActionType.Modify:
                                if (!updateChanges.ContainsKey(ckvp.Key)) {
                                    updateChanges.Add(ckvp.Key, new List<MpDbLog>() { l });
                                } else {
                                    updateChanges[ckvp.Key].Add(l);
                                }
                                break;
                            case MpDbLogActionType.Delete:
                                if (!deleteChanges.ContainsKey(ckvp.Key)) {
                                    deleteChanges.Add(ckvp.Key, new List<MpDbLog>() { l });
                                } else {
                                    deleteChanges[ckvp.Key].Add(l);
                                }
                                break;
                        }
                    }                    
                }                
            }

            //ditch adds or modifies when a delete exists
            foreach (var dc in deleteChanges) {
                if (addChanges.ContainsKey(dc.Key)) {
                    addChanges.Remove(dc.Key);
                }
                if (updateChanges.ContainsKey(dc.Key)) {
                    updateChanges.Remove(dc.Key);
                }
            }

            //move updates to adds when dbo doesn't exist
            //foreach(var uc in updateChanges) {
            //    var result = await GetDbObjectByTableGuidAsync(uc.Value[0].DbTableName, uc.Key.ToString());
            //    if(result == null) {

            //    }
            //}

            //sort 3 types by key references
            addChanges = OrderByPrecedence(addChanges);
            deleteChanges = OrderByPrecedence(deleteChanges);
            updateChanges = OrderByPrecedence(updateChanges);

            MpConsole.WriteLine(
                string.Format(
                    @"{0} Received {1} adds {2} updates {3} deletes",
                    DateTime.Now.ToString(),
                    addChanges.Count,
                    updateChanges.Count,
                    deleteChanges.Count));

            MpConsole.WriteLine(@"Deletes: ");
            // in delete, add, update order
            foreach (var ckvp in deleteChanges) {
                foreach (var dbl in ckvp.Value) {
                    dbl.PrintLog();
                }
                var dbot = new MpXamStringToSyncObjectTypeConverter().Convert(ckvp.Value[0].DbTableName);
                var deleteMethod = typeof(MpDb).GetMethod(nameof(DeleteItemAsync));
                var deleteByDboTypeMethod = deleteMethod.MakeGenericMethod(new[] { dbot });
                var dbo = await GetDbObjectByTableGuidAsync(ckvp.Value[0].DbTableName, ckvp.Key.ToString());
                //var dbo = MpDbModelBase.CreateOrUpdateFromLogs(ckvp.Value, remoteClientGuid);
                var deleteTask = (Task)deleteByDboTypeMethod.Invoke(nameof(MpDb), new object[] { dbo,remoteClientGuid,false,true });
                await deleteTask;
            }

            MpConsole.WriteLine(@"Adds: ");
            foreach (var ckvp in addChanges) {
                foreach (var dbl in ckvp.Value) {
                    dbl.PrintLog();
                }
                var dbot = new MpXamStringToSyncObjectTypeConverter().Convert(ckvp.Value[0].DbTableName);
                var dbo = Activator.CreateInstance(dbot);
                dbo = await (dbo as MpISyncableDbObject).CreateFromLogsAsync(ckvp.Key.ToString(), ckvp.Value, remoteClientGuid);
                //var dbo = MpDbModelBase.CreateOrUpdateFromLogs(ckvp.Value, remoteClientGuid);
                var addMethod = typeof(MpDb).GetMethod(nameof(AddOrUpdateAsync));
                var addByDboTypeMethod = addMethod.MakeGenericMethod(new[] { dbot });
                var addTask = (Task)addByDboTypeMethod.Invoke(nameof(MpDb), new object[] { dbo,remoteClientGuid,false,true });
                await addTask;
            }

            MpConsole.WriteLine(@"Updates: ");
            foreach (var ckvp in updateChanges) {
                foreach (var dbl in ckvp.Value) {
                    dbl.PrintLog();
                }
                var dbot = new MpXamStringToSyncObjectTypeConverter().Convert(ckvp.Value[0].DbTableName);
                var dbo = Activator.CreateInstance(dbot);
                dbo = await (dbo as MpISyncableDbObject).CreateFromLogsAsync(ckvp.Key.ToString(), ckvp.Value, remoteClientGuid);                
                //var dbo = MpDbModelBase.CreateOrUpdateFromLogs(ckvp.Value, remoteClientGuid);
                var updateMethod = typeof(MpDb).GetMethod(nameof(AddOrUpdateAsync));
                var updateByDboTypeMethod = updateMethod.MakeGenericMethod(new[] { dbot });
                var updateTask = (Task)updateByDboTypeMethod.Invoke(nameof(MpDb), new object[] { dbo,remoteClientGuid,false,true });
                await updateTask;
            }

            return;
        }

        public static async Task UpdateSyncHistoryAsync(string otherDeviceGuid, DateTime utcDtSentLocalChanges) {
            MpSyncHistory sh = await MpDataModelProvider.GetSyncHistoryByDeviceGuidAsync(otherDeviceGuid);

            if (sh == null) {
                sh = new MpSyncHistory() {
                    OtherClientGuid = otherDeviceGuid,
                    SyncDateTime = utcDtSentLocalChanges
                };
            } else {
                sh.SyncDateTime = utcDtSentLocalChanges;
            }

            await AddOrUpdateAsync<MpSyncHistory>(sh);
        }

        private static Dictionary<Guid,List<MpDbLog>> OrderByPrecedence(Dictionary<Guid,List<MpDbLog>> dict) {
            if(dict.Count == 0) {
                return dict;
            }
            var items = from pair in dict
                        orderby GetDbTableOrder(pair.Value[0]) ascending
                        select pair;
            var customSortedValues = new Dictionary<Guid, List<MpDbLog>>();

            foreach(var i in items) {
                customSortedValues.Add(i.Key, i.Value);
            }
            return customSortedValues;
        }

        private static int GetDbTableOrder(MpDbLog log) {
            var orderedLogs = MpSyncManager.DbTableSyncOrder.ToList();
            var idx = orderedLogs.IndexOf(log.DbTableName);
            if (idx < 0) {
                throw new Exception(@"Unknown dblog table type: " + log.DbTableName);
            }
            return idx;
        }

        public static object GetMainThreadObj() {
            return Application.Current.MainPage;
        }

        public static MpIStringToSyncObjectTypeConverter GetTypeConverter() {
            return new MpXamStringToSyncObjectTypeConverter();
        }

        public static ObservableCollection<MpRemoteDevice> GetRemoteDevices() {
            _rdLock = new object();
            var rdoc = new ObservableCollection<MpRemoteDevice>();
            Xamarin.Forms.BindingBase.EnableCollectionSynchronization(rdoc, null, ObservableCollectionCallback);
            return rdoc;
        }

        private static void ObservableCollectionCallback(IEnumerable collection, object context, Action accessMethod, bool writeAccess) {
            // `lock` ensures that only one thread access the collection at a time
            lock (collection) {
                accessMethod?.Invoke();
            }
        }
        #endregion
    }
}
