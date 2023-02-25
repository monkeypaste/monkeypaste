using MonkeyPaste.Common;
using SQLite;
using SQLitePCL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public static class MpDb {
        #region Private Variables

        private static object _rdLock = new object();
        private static SQLiteAsyncConnection _connectionAsync;
        private static SQLiteConnection _connection;

        #endregion

        #region Properties

        public static bool IsDateTimeTicks { get; set; } = true;
        public static bool UseWAL {
            get {
                //if (MpPlatform.Services.PlatformInfo.OsType == MpUserDeviceType.Android) {
                //    return false;
                //}
                return true;
            }
        }

        public static string IdentityToken { get; set; }
        public static string AccessToken { get; set; }
        public static bool IsLoaded { get; set; } = false;

        public static bool IgnoreLogging { get; set; } = false;

        #endregion

        #region Events

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
            await InitDbAsync();
            IsLoaded = true;
            sw.Stop();
            MpConsole.WriteLine($"Db loading: {sw.ElapsedMilliseconds} ms");
        }

        public static string GetParameterizedQueryString(string query, object[] args) {
            if (args == null || args.Length == 0) {
                return query;
            }
            //return _connection.CreateCommand(query, args).ToString();
            string query_str = query + Environment.NewLine + (args == null ? string.Empty : string.Join(Environment.NewLine, args));
            return query_str;
        }

        #region Queries

        #region Async Query

        public static async Task<TableMapping> GetTableMappingAsync(string tableName) {
            await Task.Delay(1);
            if (_connectionAsync == null) {
                await CreateConnectionAsync();
            }
            return _connectionAsync
                    .TableMappings
                    .Where(x => x.TableName.ToLower() == tableName.ToLower()).FirstOrDefault();
        }


        public static async Task<List<object>> QueryAsync(string tableName, string query, params object[] args) {
            if (_connectionAsync == null) {
                await CreateConnectionAsync();
            }
            TableMapping qtm = null;
            foreach (var tm in _connectionAsync.TableMappings) {
                if (tm.TableName.ToLower() == tableName.ToLower()) {
                    qtm = tm;
                    break;
                }
            }
            if (qtm == null) {
                return new List<object>();
            }
            try {
                var result = await _connectionAsync.QueryAsync(qtm, query, args);
                return result;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Db Error executing dynamic query on table '{tableName}' with query '{GetParameterizedQueryString(query, args)}'", ex);
                return new List<object>();
            }
        }


        public static async Task<List<T>> QueryAsync<T>(string query, params object[] args) where T : new() {
            if (_connectionAsync == null) {
                await CreateConnectionAsync();
            }
            try {
                var result = await _connectionAsync.QueryAsync<T>(query, args);
                return result;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Db Error executing query '{GetParameterizedQueryString(query, args)}' for type '{typeof(T)}'", ex);
                return new List<T>();
            }
        }

        public static async Task<T> QueryScalarAsync<T>(string query, params object[] args) {
            if (_connectionAsync == null) {
                await CreateConnectionAsync();
            }
            try {
                var result = await _connectionAsync.ExecuteScalarAsync<T>(query, args);
                return result;
            }
            catch (Exception ex) {

                MpConsole.WriteTraceLine($"Db Error executing scalar query '{GetParameterizedQueryString(query, args)}' for type '{typeof(T)}'", ex);
                return default(T);
            }
        }

        public static async Task<List<T>> QueryScalarsAsync<T>(string query, params object[] args) {
            if (_connectionAsync == null) {
                await CreateConnectionAsync();
            }
            try {
                var result = await _connectionAsync.QueryScalarsAsync<T>(query, args);
                return result;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Db Error executing scalars query '{GetParameterizedQueryString(query, args)}' for type '{typeof(T)}'", ex);
                return new List<T>();
            }
        }

        public static async Task CreateTableAsync<T>() where T : new() {
            if (_connectionAsync == null) {
                await CreateConnectionAsync();
            }
            try {
                await _connectionAsync.CreateTableAsync<T>();
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Db Error creating table of type '{typeof(T)}'", ex);
            }
        }

        public static async Task CloseConnectionAsync() {
            if (_connectionAsync == null) {
                return;
            }

            try {
                await _connectionAsync.CloseAsync();
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Db Error closing async db connection.", ex);
            }
            finally {
                _connectionAsync = null;
            }
        }

        private static async Task AddItemAsync<T>(T item, string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {
            if (_connectionAsync == null) {
                await CreateConnectionAsync();
            }
            sourceClientGuid = GetSourceClientGuid(sourceClientGuid);


            if (item == null) {
                MpConsole.WriteTraceLine(@"Cannot add null item, ignoring...");
                return;
            }

            await LogWriteAsync(MpDbLogActionType.Create, item as MpDbModelBase, sourceClientGuid, ignoreTracking);

            try {
                await _connectionAsync.InsertAsync(item);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Db Error inserting type '{typeof(T)}' with data {item}", ex);
            }

            NotifyWrite(MpDbLogActionType.Create, item as MpDbModelBase, ignoreSyncing);
        }

        private static async Task UpdateItemAsync<T>(T item, string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {
            if (_connectionAsync == null) {
                await CreateConnectionAsync();
            }
            sourceClientGuid = GetSourceClientGuid(sourceClientGuid);


            if (item == null) {
                MpConsole.WriteTraceLine(@"Cannot update null item, ignoring...");
                return;
            }

            await LogWriteAsync(MpDbLogActionType.Modify, item as MpDbModelBase, sourceClientGuid, ignoreTracking);

            try {
                await _connectionAsync.UpdateAsync(item);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Db Error updating type '{typeof(T)}' with data {item}", ex);
            }

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
                await CreateConnectionAsync();
            }
            sourceClientGuid = GetSourceClientGuid(sourceClientGuid);

            if (item == null) {
                MpConsole.WriteTraceLine(@"Cannot delete null item, ignoring...");
                return;
            }

            await LogWriteAsync(MpDbLogActionType.Delete, item as MpDbModelBase, sourceClientGuid, ignoreTracking);

            try {
                await _connectionAsync.DeleteAsync(item);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Db Error deleting type '{typeof(T)}' with data {item}", ex);
            }
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
                var item = await MpDataModelProvider.GetItemAsync<T>((dt[0] as MpDbModelBase).Id);
                return item;
                //return dbo;
                //return dt[0];
            }

            var dbo = Activator.CreateInstance(typeof(T));
            return (T)dbo;
        }

        #endregion

        #region Sync Query

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

        #endregion

        #region Private Methods  

        private static async Task InitDbAsync() {
            bool isNewDb = await InitDbConnectionAsync(MpPlatform.Services.DbInfo, true);

            await InitTablesAsync();

            if (isNewDb) {
                await MpDefaultDataModelTools.CreateAsync(MpPrefViewModel.Instance.ThisDeviceGuid);
                await CreateViewsAsync();
                await InitDefaultDataAsync();

                await CreateTestDataAsync();
            } else {
                await MpDefaultDataModelTools.InitializeAsync();
            }
        }
        public static async Task<bool> InitDbConnectionAsync(MpIDbInfo dbInfo, bool allowCreate) {
            string dbPath = dbInfo.DbPath;
            bool isNewDb = !File.Exists(dbPath);

            if (isNewDb && allowCreate) {
                using (File.Create(dbPath)) { }
            }

            await CreateConnectionAsync(dbPath);

            MpConsole.WriteLine($"Db {(isNewDb ? "CREATED" : "CONNECTED")} at '{dbPath}'");
            return isNewDb;
        }

        private static SQLiteConnectionString GetConnectionString(string dbPath = "") {
            if (string.IsNullOrEmpty(dbPath)) {
                dbPath = MpPlatform.Services.DbInfo.DbPath;
            }
            SQLiteConnectionString connStr = null;
            try {
                connStr = new SQLiteConnectionString(
                                   databasePath: dbPath,
                                   storeDateTimeAsTicks: IsDateTimeTicks,
                                   openFlags: SQLiteOpenFlags.ReadWrite |
                                              SQLiteOpenFlags.Create |
                                              SQLiteOpenFlags.SharedCache |
                                              SQLiteOpenFlags.FullMutex);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Db Error creating connection str for db path '{dbPath}'.", ex);
                return null;
            }
            return connStr;
        }
        private static async Task CreateConnectionAsync(string dbPath = "") {
            if (_connectionAsync != null) {
                return;
            }

            Batteries_V2.Init();
            if (_connectionAsync == null) {
                try {
                    _connectionAsync = new SQLiteAsyncConnection(GetConnectionString(dbPath)) { Trace = true };
                    raw.sqlite3_create_function(_connectionAsync.GetConnection().Handle, "REGEXP", 2, null, MatchRegex);

                    if (UseWAL) {
                        await _connectionAsync.EnableWriteAheadLoggingAsync();
                    }
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Db Error creating async connection", ex);
                }
            }
            MpConsole.WriteLine($"Db Async WAL: {(UseWAL ? "ENABLED" : "DISABLED")}");
        }
        private static void CreateConnection(string dbPath = "") {
            if (_connection != null) {
                return;
            }

            Batteries_V2.Init();
            if (_connection == null) {
                try {
                    _connection = new SQLiteConnection(GetConnectionString(dbPath)) { Trace = true };
                    raw.sqlite3_create_function(_connection.Handle, "REGEXP", 2, null, MatchRegex);

                    if (UseWAL) {
                        _connection.EnableWriteAheadLogging();
                    }
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Db Error creating async connection", ex);
                }
            }
            MpConsole.WriteLine($"Db Sync WAL: {(UseWAL ? "ENABLED" : "DISABLED")}");
        }

        private static string GetSourceClientGuid(string providedSourceClientGuid) {
            if (!IsLoaded) {
                return null;
            }
            return string.IsNullOrEmpty(providedSourceClientGuid) ? MpPrefViewModel.Instance.ThisDeviceGuid : providedSourceClientGuid;
        }

        private static async Task LogWriteAsync(MpDbLogActionType actionType, MpDbModelBase item, string sourceClientGuid, bool ignoreTracking) {
            if (!IsLoaded || IgnoreLogging) {
                return;
            }
            if (item is MpISyncableDbObject && item is not MpDbLog && item is not MpSyncHistory) {
                if (string.IsNullOrEmpty(item.Guid)) {
                    if (actionType != MpDbLogActionType.Create) {
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
            switch (actionType) {
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

        private static void MatchRegex(sqlite3_context ctx, object user_data, sqlite3_value[] args) {
            string pattern = raw.sqlite3_value_text(args[0]).utf8_to_string();
            pattern = pattern == null ? string.Empty : pattern;

            string input = raw.sqlite3_value_text(args[1]).utf8_to_string();
            input = input == null ? string.Empty : input;

            if (args.Length > 2) {
                string test = raw.sqlite3_value_text(args[2]).utf8_to_string();
                test = test == null ? string.Empty : test;
                Debugger.Break();
            }
            bool isMatched = false;
            try {
                isMatched = Regex.IsMatch(input, pattern);

            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Regex exception using pattern '{pattern}'", ex);
                isMatched = false;
            }

            if (isMatched) {
                raw.sqlite3_result_int(ctx, 1);
            } else {
                raw.sqlite3_result_int(ctx, 0);
            }
        }

        private static async Task InitTablesAsync() {
            await _connectionAsync.CreateTableAsync<MpAction>();
            await _connectionAsync.CreateTableAsync<MpParameterValue>();
            await _connectionAsync.CreateTableAsync<MpApp>();
            await _connectionAsync.CreateTableAsync<MpAppClipboardFormatInfo>();
            await _connectionAsync.CreateTableAsync<MpAppPasteShortcut>();
            await _connectionAsync.CreateTableAsync<MpBillableItem>();
            await _connectionAsync.CreateTableAsync<MpCopyItem>();
            await _connectionAsync.CreateTableAsync<MpCopyItemAnnotation>();
            await _connectionAsync.CreateTableAsync<MpCopyItemTag>();
            await _connectionAsync.CreateTableAsync<MpTransactionSource>();
            await _connectionAsync.CreateTableAsync<MpCopyItemTransaction>();
            await _connectionAsync.CreateTableAsync<MpDataObject>();
            await _connectionAsync.CreateTableAsync<MpDataObjectItem>();
            await _connectionAsync.CreateTableAsync<MpDbImage>();
            await _connectionAsync.CreateTableAsync<MpDbLog>();
            await _connectionAsync.CreateTableAsync<MpIcon>();
            await _connectionAsync.CreateTableAsync<MpPasteToAppPath>();
            await _connectionAsync.CreateTableAsync<MpPluginPreset>();
            await _connectionAsync.CreateTableAsync<MpSearchCriteriaItem>();
            await _connectionAsync.CreateTableAsync<MpShortcut>();
            await _connectionAsync.CreateTableAsync<MpSyncHistory>();
            await _connectionAsync.CreateTableAsync<MpTag>();
            await _connectionAsync.CreateTableAsync<MpTextTemplate>();
            await _connectionAsync.CreateTableAsync<MpContentToken>();
            await _connectionAsync.CreateTableAsync<MpUrl>();
            await _connectionAsync.CreateTableAsync<MpUserDevice>();
        }

        private static async Task CreateViewsAsync() {
            // ADVANCED 
            await _connectionAsync.ExecuteAsync(@"
CREATE VIEW MpContentQueryView_advanced as
SELECT 
	pk_MpCopyItemId as RootId,
	MpTransactionSource.fk_SourceObjId as SourceObjId,
	MpTransactionSource.e_MpTransactionSourceType as SourceType,
	ItemSize1,
	ItemSize2,
	MpCopyItemTransaction.TransactionDateTime as TransactionDateTime,
	MpCopyItemTransaction.TransactionLabel as TransactionLabel,
	case 
		when MpTransactionSource.e_MpTransactionSourceType == 'App' 
			then 
				(select AppPath from MpApp where pk_MpAppId == MpTransactionSource.fk_SourceObjId limit 1)
		when MpTransactionSource.e_MpTransactionSourceType == 'Url' 
			then 
				(select UrlPath from MpUrl where pk_MpUrlId == MpTransactionSource.fk_SourceObjId limit 1)
		when MpTransactionSource.e_MpTransactionSourceType == 'CopyItem' 
			then 
				'https://localhost?type=copyItem&id=' || MpTransactionSource.fk_SourceObjId
		else NULL
	end as SourcePath,
	case 
		when MpTransactionSource.e_MpTransactionSourceType == 'App' 
			then 
				(select AppPath from MpApp where pk_MpAppId == MpTransactionSource.fk_SourceObjId limit 1)
		when MpTransactionSource.e_MpTransactionSourceType == 'Url' 
			then 
				(select UrlTitle from MpUrl where pk_MpUrlId == MpTransactionSource.fk_SourceObjId limit 1)
		when MpTransactionSource.e_MpTransactionSourceType == 'CopyItem' 
			then 
				(select Title from MpCopyItem where pk_MpCopyItemId == MpTransactionSource.fk_SourceObjId limit 1)
		else NULL
	end as AppPath,
	case
		when MpTransactionSource.e_MpTransactionSourceType == 'App' 
				then 
					(select AppName from MpApp where pk_MpAppId == MpTransactionSource.fk_SourceObjId limit 1)
		else NULL
	end as AppName,
	case
		when MpTransactionSource.e_MpTransactionSourceType == 'Url' 
				then 
					(select UrlPath from MpUrl where pk_MpUrlId == MpTransactionSource.fk_SourceObjId limit 1)
		else NULL
	end as UrlPath,
	case
		when MpTransactionSource.e_MpTransactionSourceType == 'Url' 
				then 
					(select UrlTitle from MpUrl where pk_MpUrlId == MpTransactionSource.fk_SourceObjId limit 1)
		else NULL
	end as UrlTitle,
	case 
		when MpTransactionSource.e_MpTransactionSourceType == 'App' 
			then 
				(select MachineName from MpUserDevice where pk_MpUserDeviceId in (select fk_MpUserDeviceId from MpApp where pk_MpAppId == MpTransactionSource.fk_SourceObjId limit 1))
		else NULL
	end as DeviceName,
	case 
		when MpTransactionSource.e_MpTransactionSourceType == 'App' 
			then 
				(select e_MpUserDeviceType from MpUserDevice where pk_MpUserDeviceId in (select fk_MpUserDeviceId from MpApp where pk_MpAppId == MpTransactionSource.fk_SourceObjId limit 1))
		else NULL
	end as DeviceType,
	e_MpCopyItemType,
	ItemMetaData,
	Title,
	case 
		when MpCopyItem.e_MpCopyItemType == 'Text' or MpCopyItem.e_MpCopyItemType == 'FileList'
			then 
				(select ItemData from MpDataObjectItem where fk_MpDataObjectId = MpCopyItem.fk_MpDataObjectId limit 1)
		else NULL
	end as ItemData,
	CopyDateTime,
	CopyCount,
	PasteCount,
	CopyCount + PasteCount as UsageScore
FROM
	MpCopyItem 
LEFT JOIN MpCopyItemTransaction ON MpCopyItemTransaction.fk_MpCopyItemId = MpCopyItem.pk_MpCopyItemId
LEFT JOIN MpTransactionSource ON MpTransactionSource.fk_MpCopyItemTransactionId = MpCopyItemTransaction.pk_MpCopyItemTransactionId");

            // SIMPLE 
            await _connectionAsync.ExecuteAsync(@"
CREATE VIEW MpContentQueryView_simple as
SELECT 
	pk_MpCopyItemId as RootId,
	MpTransactionSource.fk_SourceObjId as SourceObjId,
	MpTransactionSource.e_MpTransactionSourceType as SourceType,	
	case 
		when MpTransactionSource.e_MpTransactionSourceType == 'App' 
			then 
				(select AppPath from MpApp where pk_MpAppId == MpTransactionSource.fk_SourceObjId limit 1)
		when MpTransactionSource.e_MpTransactionSourceType == 'Url' 
			then 
				(select UrlPath from MpUrl where pk_MpUrlId == MpTransactionSource.fk_SourceObjId limit 1)
		when MpTransactionSource.e_MpTransactionSourceType == 'CopyItem' 
			then 
				'https://localhost?type=copyItem&id=' || MpTransactionSource.fk_SourceObjId
		else NULL
	end as SourcePath,
	case 
		when MpTransactionSource.e_MpTransactionSourceType == 'App' 
			then 
				(select AppPath from MpApp where pk_MpAppId == MpTransactionSource.fk_SourceObjId limit 1)
		when MpTransactionSource.e_MpTransactionSourceType == 'Url' 
			then 
				(select UrlTitle from MpUrl where pk_MpUrlId == MpTransactionSource.fk_SourceObjId limit 1)
		when MpTransactionSource.e_MpTransactionSourceType == 'CopyItem' 
			then 
				(select Title from MpCopyItem where pk_MpCopyItemId == MpTransactionSource.fk_SourceObjId limit 1)
		else NULL
	end as AppPath,
	case
		when MpTransactionSource.e_MpTransactionSourceType == 'App' 
				then 
					(select AppName from MpApp where pk_MpAppId == MpTransactionSource.fk_SourceObjId limit 1)
		else NULL
	end as AppName,
	case
		when MpTransactionSource.e_MpTransactionSourceType == 'Url' 
				then 
					(select UrlPath from MpUrl where pk_MpUrlId == MpTransactionSource.fk_SourceObjId limit 1)
		else NULL
	end as UrlPath,
	case
		when MpTransactionSource.e_MpTransactionSourceType == 'Url' 
				then 
					(select UrlTitle from MpUrl where pk_MpUrlId == MpTransactionSource.fk_SourceObjId limit 1)
		else NULL
	end as UrlTitle,
	case 
		when MpTransactionSource.e_MpTransactionSourceType == 'App' 
			then 
				(select MachineName from MpUserDevice where pk_MpUserDeviceId in (select fk_MpUserDeviceId from MpApp where pk_MpAppId == MpTransactionSource.fk_SourceObjId limit 1))
		else NULL
	end as DeviceName,
	case 
		when MpTransactionSource.e_MpTransactionSourceType == 'App' 
			then 
				(select e_MpUserDeviceType from MpUserDevice where pk_MpUserDeviceId in (select fk_MpUserDeviceId from MpApp where pk_MpAppId == MpTransactionSource.fk_SourceObjId limit 1))
		else NULL
	end as DeviceType,
	e_MpCopyItemType,
	ItemMetaData,
	Title,
	case 
		when MpCopyItem.e_MpCopyItemType == 'Text' or MpCopyItem.e_MpCopyItemType == 'FileList'
			then 
				(select ItemData from MpDataObjectItem where fk_MpDataObjectId = MpCopyItem.fk_MpDataObjectId limit 1)
		else NULL
	end as ItemData,
	CopyDateTime,
	CopyCount,
	PasteCount,
	CopyCount + PasteCount as UsageScore
FROM
	MpCopyItem
INNER JOIN MpCopyItemTransaction ON MpCopyItemTransaction.fk_MpCopyItemId = MpCopyItem.pk_MpCopyItemId
INNER JOIN MpTransactionSource ON MpTransactionSource.fk_MpCopyItemTransactionId = MpCopyItemTransaction.pk_MpCopyItemTransactionId");

        }

        private static async Task InitDefaultDataAsync() {
            // NOTE! MpTag.AllTagId needs to be changed to 1 not 2 since recent was removed

            bool tracked = true;
            bool synced = true;

            #region Tags
            var default_tags = new object[] {
                new object[] { "df388ecd-f717-4905-a35c-a8491da9c0e3", "All", MpSystemColors.blue1, 0,0, tracked,synced, 0, MpTagType.Link},
                new object[] { "54b61353-b031-4029-9bda-07f7ca55c123", "Favorites", MpSystemColors.yellow1, 0,-1,tracked,synced, MpTag.AllTagId, MpTagType.Link},
                new object[] { "e62b8e5d-52a6-46f1-ac51-8f446916dd85", "Searches", MpSystemColors.forestgreen, 1,-1,tracked,synced, 0, MpTagType.Group},
                new object[] { "a0567976-dba6-48fc-9a7d-cbd306a4eaf3", "Help", MpSystemColors.orange1, 2,1,tracked,synced, 0, MpTagType.Link},
            };
            for (int i = 0; i < default_tags.Length; i++) {
                var t = (object[])default_tags[i];
                await MpTag.CreateAsync(
                    guid: t[0].ToString(),
                    tagName: t[1].ToString(),
                    hexColor: t[2].ToString(),
                    treeSortIdx: (int)t[3],
                    pinSortIdx: (int)t[4],
                    ignoreTracking: (bool)t[5],
                    ignoreSyncing: (bool)t[6],
                    parentTagId: (int)t[7],
                    tagType: (MpTagType)t[8]);
            }

            #endregion

            #region Shortcuts

            await InitDefaultShortcutsAsync();

            #endregion

            await InitHelpContentAsync();


            MpConsole.WriteLine(@"Created all default tables");
        }

        private static async Task InitHelpContentAsync() {
            // NOTE called in clip tray init when initial startup is flagged

            var helpContentDefinitions = new List<string[]> {
                new string[] {
                    "Welcome to the jungle!",
                    "<h1>Monkey paste is the <b>best</b> am I <i>right</i>?!</h1>"
                },
                new string[] {
                    "Help Test 1",
                    "<h1>Here at Monkey paste we earn our bananas by aiding you with business logic automation</h1>"
                }
            };

            var thisApp = await MpDataModelProvider.GetItemAsync<MpApp>(MpDefaultDataModelTools.ThisAppId);
            var thisAppRef = MpPlatform.Services.SourceRefBuilder.ConvertToRefUrl(thisApp);

            var hci_idl = new List<int>();
            foreach (var hcd in helpContentDefinitions) {
                var hci_mpdo = new MpPortableDataObject() {
                    DataFormatLookup = new Dictionary<MpPortableDataFormat, object>() {
                            {
                                MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.INTERNAL_CONTENT_TITLE_FORMAT),
                                hcd[0]
                            },
                            {
                                MpPortableDataFormats.GetDataFormat(MpPortableDataFormats.CefHtml),
                                hcd[1]
                            }
                        }
                };

                //var hci_do = await MpDataObject.CreateAsync(pdo: hci_mpdo);

                //var hci = await MpCopyItem.CreateAsync(
                //    title: hcd[0],
                //    data: hcd[1],
                //    dataObjectId: hci_do.Id);

                //await MpPlatform.Services.TransactionBuilder.ReportTransactionAsync(
                //            copyItemId: hci.Id,
                //            reqType: MpJsonMessageFormatType.DataObject,
                //            req: hci_mpdo.SerializeData(),
                //            respType: MpJsonMessageFormatType.None,
                //            resp: null,
                //            ref_urls: new[] { thisAppRef },
                //            transType: MpTransactionType.Created);

                var hci = await MpPlatform.Services.CopyItemBuilder.BuildAsync(
                    pdo: hci_mpdo,
                    transType: MpTransactionType.System,
                    force_ext_sources: false);

                hci_idl.Add(hci.Id);
            }

            await Task.WhenAll(hci_idl.Select((x, idx) => MpCopyItemTag.Create(MpTag.HelpTagId, x, idx)));
        }
        private static async Task CreateTestDataAsync() {
            for (int i = 0; i < 100; i++) {
                var mpdo = new MpPortableDataObject(MpPortableDataFormats.Text, $"This is test {i}.");
                await MpPlatform.Services.CopyItemBuilder.BuildAsync(
                    pdo: mpdo,
                    transType: MpTransactionType.Created,
                    force_ext_sources: false);
            }

        }
        public static async Task ResetShortcutsAsync() {
            var sl = await MpDataModelProvider.GetItemsAsync<MpShortcut>();
            await Task.WhenAll(sl.Select(x => x.DeleteFromDatabaseAsync()));

            await InitDefaultShortcutsAsync();
        }

        private static async Task InitDefaultShortcutsAsync() {
            var ps = MpPlatform.Services.PlatformShorcuts;
            List<string[]> defaultShortcutDefinitions = new List<string[]>() {
                // ORDER:
                // guid,keyString,shortcutType,routeType, readOnly = false

                // GLOBAL

                 new string[] {"5dff238e-770e-4665-93f5-419e48326f01","Caps Lock", "ShowMainWindow", "Direct"},
                 new string[] {"97e29b06-0ec4-4c55-a393-8442d7695038","Control+F1", "ToggleListenToClipboard", "Direct"},
                 new string[] {"777367e6-c161-4e93-93e0-9bf12221f7ff","Control+F2", "ToggleAppendLineMode", "Direct"},
                 new string[] {"a41aeed8-d4f3-47de-86c5-f9ca296fb103","Control+F3", "ToggleAppendMode", "Direct"},
                 new string[] {"892bf7d7-ba8e-4db1-b2ca-62b41ff6614c","Control+F4", "ToggleAutoCopyMode", "Direct"},
                 new string[] {"a12c4211-ab1f-4b97-98ff-fbeb514e9a1c","Control+F5", "ToggleRightClickPasteMode", "Direct"},

                 // APPLICATION
                 new string[] {"94e81589-fe2f-4e80-8940-ed066f0d9c27",ps.PasteKeys, "PasteHere", "Internal","True"},
                 new string[] {"ee74dd92-d18b-46cf-91b7-3946ab55427c",ps.CopyKeys, "CopySelection", "Internal","True"},
                 new string[] {"2acde1cc-c8e4-4675-8895-81712a6f0a36",ps.CutKeys, "CutSelection", "Internal","True"},
                 new string[] {"cb807500-9121-4e41-80d3-8c3682ce90d9","Escape", "HideMainWindow", "Internal","True"},
                 new string[] {"1d212ca5-fb2a-4962-8f58-24ed9a5d007d","Control+Enter", "PasteSelectedItems", "Internal"},
                 new string[] {"e94ca4f3-4c6e-40dc-8941-c476a81543c7","Delete", "DeleteSelectedItems", "Internal"},
                 new string[] {"7fe24929-6c9e-49c0-a880-2f49780dfb3a","Right", "SelectNextColumnItem", "Internal"},
                 new string[] {"ee657845-f1dc-40cf-848d-6768c0081670","Left", "SelectPreviousColumnItem", "Internal"},
                 new string[] {"674bae7f-0a60-4f17-ac2c-81d5c6c3d879","Down", "SelectNextRowItem", "Internal"},
                 new string[] {"b916ab44-d4bd-4d8b-ac4a-de947343bd5a","Up", "SelectPreviousRowItem", "Internal"},
                 new string[] {"6487f6ff-da0c-475b-a2ae-ef1484233de0","Control+I", "AssignShortcut", "Internal"},
                 new string[] {"837e0c20-04b8-4211-ada0-3b4236da0821","Control+P", "ChangeColor", "Internal"},
                 new string[] {"118a2ca6-7021-47a0-8458-7ebc31094329","Control+Z", "Undo", "Internal"},
                 new string[] {"3980efcc-933b-423f-9cad-09e455c6824a","Control+Y", "Redo", "Internal"},
                 new string[] {"7a7580d1-4129-432d-a623-2fff0dc21408","Control+E", "EditContent", "Internal"},
                 new string[] {"085338fb-f297-497a-abb7-eeb7310dc6f3","F2", "EditTitle", "Internal"},
                 new string[] {"e22faafd-4313-441a-b361-16910fc7e9d3","Control+D", "Duplicate", "Internal"},
                 new string[] {"ac8abe92-82c3-46fb-9bd5-39d74b100b23","Home", "ScrollToHome", "Internal"},
                 new string[] {"ac8abe92-82c3-46fb-9bd5-39d74b100b23","End", "ScrollToEnd", "Internal"},
                 new string[] {"9b0ca09a-5724-4004-98d2-f5ef8ae02055","Control+Up", "WindowSizeUp", "Internal"},
                 new string[] {"39a6194e-37e3-4d37-a9f4-254ed83157f2","Control+Down", "WindowSizeDown", "Internal"},
                 new string[] {"6cc03ef0-3b33-4b94-9191-0d751e6b7fb6","Control+Left", "WindowSizeLeft", "Internal"},
                 new string[] {"c4ac1629-cdf0-4075-94af-8f934b014452","Control+Right", "WindowSizeRight", "Internal"},
                 new string[] {"30c813a0-d466-4ae7-b75e-82680b4542fc","PageUp", "PreviousPage", "Internal"},
                 new string[] {"09df97ea-f786-48d9-9112-a60266df6586","PageDown", "NextPage", "Internal"},
                 new string[] {"a39ac0cb-41e4-47b5-b963-70e388dc156a","Control+H", "FindAndReplaceSelectedItem", "Internal"},
                 new string[] {"cb1ac03b-a20f-4911-bf4f-bc1a858590e3","Control+L", "ToggleMainWindowLocked", "Internal"},
                 new string[] {"d73204f5-fbed-4d87-9dca-6dfa8d8cba82","Control+K", "ToggleFilterMenuVisible", "Internal"},
                 new string[] {"49f44a89-e381-4d6a-bf8c-1090eb443f17","Control+Q", "ExitApplication", "Internal"},
                 new string[] { "31e0a078-e80b-4d31-b236-2a585d6352cf", "Control+,", "ShowSettings", "Internal"}
            };

            foreach (var defaultShortcut in defaultShortcutDefinitions) {
                await MpShortcut.CreateAsync(
                    guid: defaultShortcut[0],
                    keyString: defaultShortcut[1],
                    shortcutType: defaultShortcut[2].ToEnum<MpShortcutType>(),
                    routeType: defaultShortcut[3].ToEnum<MpRoutingType>(),
                    isReadOnly: defaultShortcut.Length >= 5 ? bool.Parse(defaultShortcut[4]) : false);
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
        //public static int GetSyncPort() {
        //    return 44381;
        //}
        //public static string GetThisClientGuid() {
        //    return MpPrefViewModel.Instance.ThisDeviceGuid;
        //}
        //public static string GetPrimaryLocalIp4Address() {
        //    if (!IsConnectedToNetwork()) {
        //        return "0.0.0.0";
        //    }
        //    return MpNetworkHelpers.GetLocalIp4Address();
        //}

        //public static string[] GetAllLocalIp4Addresses() {
        //    if (!IsConnectedToNetwork()) {
        //        return new string[] { "0.0.0.0" };
        //    }
        //    return MpNetworkHelpers.GetAllLocalIPv4();
        //}

        //public static string GetExternalIp4Address() {
        //    if (!IsConnectedToInternet()) {
        //        return "0.0.0.0";
        //    }
        //    return MpNetworkHelpers.GetExternalIp4AddressAsync();
        //}

        public static async Task<List<MpDbLog>> GetDbObjectLogsAsync(string dboGuid, DateTime fromDtUtc) {
            var logs = await MpDataModelProvider.GetDbLogsByGuidAsync(dboGuid, fromDtUtc);
            return logs;
        }

        public static async Task<DateTime> GetLastSyncForRemoteDeviceAsync(string otherDeviceGuid) {
            var shl = await MpDataModelProvider.GetItemsAsync<MpSyncHistory>();
            if (shl.Count == 0) {
                return DateTime.MinValue;
            }
            var lsh = shl
                        .Where(x => x.OtherClientGuid.ToString() == otherDeviceGuid)
                        .OrderByDescending(x => x.SyncDateTime)
                        .FirstOrDefault();
            if (lsh != null) {
                return lsh.SyncDateTime;
            }
            return DateTime.MinValue;
        }

        public static async Task<string> GetLocalLogFromSyncDateAsync(DateTime fromDateTime, string ignoreGuid = "") {
            var logItems = await MpDataModelProvider.GetItemsAsync<MpDbLog>();
            var matchLogItems = logItems.Where(x => x.LogActionDateTime > fromDateTime && x.SourceClientGuid.ToString() != ignoreGuid).ToList();

            var dbol = new List<MpISyncableDbObject>();
            foreach (var li in matchLogItems) {
                dbol.Add(li as MpISyncableDbObject);
            }
            if (dbol.Count == 0) {
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
                    foreach (var l in rlogs.OrderBy(x => x.LogActionDateTime).ToList()) {
                        switch (l.LogActionType) {
                            case MpDbLogActionType.Create:
                                if (!addChanges.ContainsKey(ckvp.Key)) {
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
                var deleteTask = (Task)deleteByDboTypeMethod.Invoke(nameof(MpDb), new object[] { dbo, remoteClientGuid, false, true });
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
                var addTask = (Task)addByDboTypeMethod.Invoke(nameof(MpDb), new object[] { dbo, remoteClientGuid, false, true });
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
                var updateTask = (Task)updateByDboTypeMethod.Invoke(nameof(MpDb), new object[] { dbo, remoteClientGuid, false, true });
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

        private static Dictionary<Guid, List<MpDbLog>> OrderByPrecedence(Dictionary<Guid, List<MpDbLog>> dict) {
            if (dict.Count == 0) {
                return dict;
            }
            var items = from pair in dict
                        orderby GetDbTableOrder(pair.Value[0]) ascending
                        select pair;
            var customSortedValues = new Dictionary<Guid, List<MpDbLog>>();

            foreach (var i in items) {
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

        public static MpIStringToSyncObjectTypeConverter GetTypeConverter() {
            return new MpXamStringToSyncObjectTypeConverter();
        }

        public static ObservableCollection<MpRemoteDevice> GetRemoteDevices() {
            _rdLock = new object();
            var rdoc = new ObservableCollection<MpRemoteDevice>();
            //Xamarin.Forms.BindingBase.EnableCollectionSynchronization(rdoc, null, ObservableCollectionCallback);
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
