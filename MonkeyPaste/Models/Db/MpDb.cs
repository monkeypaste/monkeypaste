using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using SQLite;
using SQLitePCL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
#if ANDROID 
                return false;
#else
                return true;
#endif
            }
        }

        public static string IdentityToken { get; set; }
        public static string AccessToken { get; set; }
        public static bool IsLoaded { get; set; } = false;

        public static bool IgnoreLogging { get; set; } = false;

        static bool IsInitialDbCreate { get; set; } = false;

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
            if (IsLoaded) {
                return;
            }
            var sw = new Stopwatch();
            sw.Start();

            bool? init_result = await InitDbConnectionAsync(Mp.Services.DbInfo, true);
            if (!init_result.HasValue) {
                // connection failed, shutdown triggered
                return;
            }
            IsInitialDbCreate = init_result.Value;

            await InitTablesAsync();

            if (IsInitialDbCreate) {
                await MpDefaultDataModelTools.CreateAsync(Mp.Services.ThisDeviceInfo.ThisDeviceGuid);
                await CreateViewsAsync();
                await Mp.Services.DefaultDataCreator.CreateDefaultDataAsync();
            } else {
                await MpDefaultDataModelTools.InitializeAsync();
            }


            IsLoaded = true;

            sw.Stop();
            MpConsole.WriteLine($"Db loading: {sw.ElapsedMilliseconds} ms");
        }

        public static async Task PerformDbOptimizationAsync() {
            await Task.Delay(1);
            ExecuteCommand<string>("PRAGMA analysis_limit=1000;");
            ExecuteCommand<string>("PRAGMA optimize;");
            MpConsole.WriteLine($"Db optimization completed.");
        }

        public static async Task<bool> CheckIsUserPasswordSetAsync() {
            if (!Mp.Services.DbInfo.DbPath.IsFile()) {
                return false;
            }
            if (Mp.Services.DbInfo.HasUserDefinedPassword) {
                return true;
            }

            // if password is set Test should fail with default pwd
            bool does_default_connect = await TestDbConnectionAsync();
            return !does_default_connect;
        }
        public static string GetParameterizedQueryString(string query, object[] args) {
            if (args == null || args.Length == 0) {
                return query;
            }
            query = query.Replace(Environment.NewLine, " ");
            var argList = new Stack(args.Reverse().ToArray());

            var sb = new StringBuilder();
            for (int i = 0; i < query.Length; i++) {
                if (query[i] == '?') {
                    if (argList.Count == 0) {
                        MpDebug.Break("Param count mismatch");
                    }
                    object arg = argList.Pop();
                    string arg_str = arg.ToString();
                    if (arg is string || arg is char) {
                        arg_str = $"'{arg_str}'";
                    }
                    sb.Append(arg_str);
                } else {
                    sb.Append(query[i]);
                }
            }
            if (argList.Count > 0) {
                MpDebug.Break("Param count mismatch");
            }
            return sb.ToString();
        }

        public static async Task<bool> ChangeDbPasswordAsync(string new_password, bool remember) {
            // run test in case this is before db initialized
            bool can_change = await TestDbConnectionAsync();
            if (!can_change) {
                MpDebug.Break($"Cannot change password.Connection test failed.");
                return false;
            }
            string old_password = Mp.Services.DbInfo.DbPassword;
            try {
                string key_query = $"PRAGMA key = '{old_password}'";
                var key_result = await QueryScalarAsync<object>(key_query);
                bool is_clearing_pwd = string.IsNullOrEmpty(new_password);
                if (is_clearing_pwd) {
                    // user cleared db password
                    // Null DbInfo pwd to activate default
                    Mp.Services.DbInfo.SetPassword(null, remember);
                    new_password = Mp.Services.DbInfo.DbPassword;
                }
                string rekey_query = $"PRAGMA rekey = '{new_password}'";
                var rekey_result = await QueryScalarAsync<object>(rekey_query);
                if (!is_clearing_pwd) {
                    // set new pwd
                    Mp.Services.DbInfo.SetPassword(new_password, remember);
                }

                var success = await TestDbConnectionAsync();
                MpDebug.Assert(success, $"Password change failed");
                return success;

            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Error changing db password: ", ex);
                return false;
            }

        }
        #region Queries

        #region Async Query

        public static AsyncTableQuery<T> GetAsyncTable<T>() where T : new() {
            return _connectionAsync.Table<T>();
        }
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

        public static async Task<int> ExeucuteAsync(string query, params object[] args) {
            if (_connectionAsync == null) {
                await CreateConnectionAsync();
            }
            try {
                var result = await _connectionAsync.ExecuteAsync(query, args);
                return result;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Db Error executing query '{GetParameterizedQueryString(query, args)}'", ex);
                return 0;
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

        private static async Task<int> AddItemAsync<T>(T item, string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {
            int item_id = 0;
            if (_connectionAsync == null) {
                await CreateConnectionAsync();
            }
            sourceClientGuid = GetSourceClientGuid(sourceClientGuid);


            if (item == null) {
                MpConsole.WriteTraceLine(@"Cannot add null item, ignoring...");
                return item_id;
            }

            await LogWriteAsync(MpDbLogActionType.Create, item as MpDbModelBase, sourceClientGuid, ignoreTracking);

            try {
                item_id = await _connectionAsync.InsertAsync(item);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Db Error inserting type '{typeof(T)}' with data {item}", ex);
            }

            NotifyWrite(MpDbLogActionType.Create, item as MpDbModelBase, ignoreSyncing);
            return item_id;
        }

        private static async Task<int> UpdateItemAsync<T>(T item, string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {
            int item_id = 0;
            if (_connectionAsync == null) {
                await CreateConnectionAsync();
            }
            sourceClientGuid = GetSourceClientGuid(sourceClientGuid);


            if (item == null) {
                MpConsole.WriteTraceLine(@"Cannot update null item, ignoring...");
                return item_id;
            }

            await LogWriteAsync(MpDbLogActionType.Modify, item as MpDbModelBase, sourceClientGuid, ignoreTracking);

            try {
                item_id = await _connectionAsync.UpdateAsync(item);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Db Error updating type '{typeof(T)}' with data {item}", ex);
            }

            NotifyWrite(MpDbLogActionType.Modify, item as MpDbModelBase, ignoreSyncing);
            return item_id;
        }

        public static async Task<int> AddOrUpdateAsync<T>(T item, string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {
            int item_id = 0;
            if ((item as MpDbModelBase).Id == 0) {
                item_id = await AddItemAsync(item, sourceClientGuid, ignoreTracking, ignoreSyncing);
            } else {
                item_id = await UpdateItemAsync(item, sourceClientGuid, ignoreTracking, ignoreSyncing);
            }
            return item_id;
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

        public static T ExecuteCommand<T>(string cmdText, params object[] args) {
            if (_connection == null) {
                CreateConnection();
            }
            var cmd = _connection.CreateCommand(cmdText, args);//use db.CreateCommand
            var result = cmd.ExecuteScalar<T>();//execute the command
            return result;
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

        #endregion

        #region Private Methods  

        public static async Task<bool?> InitDbConnectionAsync(MpIDbInfo dbInfo, bool allowCreate) {
            string dbPath = dbInfo.DbPath;
            bool isNewDb = !File.Exists(dbPath);

            if (isNewDb && allowCreate) {
                using (File.Create(dbPath)) { }
                dbInfo.DbCreateDateTime = new FileInfo(dbPath).CreationTimeUtc;
            }
            string dbPass = dbInfo.DbPassword;

            await CreateConnectionAsync(dbPath);

            bool connect_success = await TestDbConnectionAsync();
            if (!connect_success) {
                int curAttemptNum = 0;
                int maxAttempts = 3;
                while (curAttemptNum < maxAttempts) {
                    var result = await GetDbPasswordAsync(curAttemptNum, maxAttempts, dbInfo.EnterPasswordTitle, $"{(maxAttempts - curAttemptNum)} {dbInfo.EnterPasswordText}");
                    if (result.IsDefault()) {
                        // user canceled
                        Mp.Services.ShutdownHelper.ShutdownApp(MpShutdownType.DbAuthFailed, "canceled db password");
                        return null;
                    }
                    dbInfo.SetPassword(result.Item1, result.Item2);
                    await _connectionAsync.CloseAsync();
                    _connectionAsync = null;
                    await CreateConnectionAsync(dbPath);
                    connect_success = await TestDbConnectionAsync();
                    if (connect_success) {
                        break;
                    }
                    // wait for pw box to hide (so new one doesn't reposition)
                    await Task.Delay(3000);
                    curAttemptNum++;
                }
            }

            MpConsole.WriteLine($"Db {(isNewDb ? "CREATED" : "CONNECTED")} at '{dbPath}'");
            bool? success = connect_success ? isNewDb : null;
            if (success.IsTrue()) {
#if WINDOWS
                await InitDbSettingsAsync();
#endif
            }
            return success;
        }

        private static async Task InitDbSettingsAsync() {
            // from https://gist.github.com/phiresky/978d8e204f77feaa0ab5cca08d2d5b27
            var settings = new List<string>() {
                $"pragma synchronous = normal;",
                $"pragma temp_store = memory;",
                //$"pragma mmap_size = 30000000000;",
                $"pragma page_size = 32768;"
            };

            for (int i = 0; i < settings.Count; i++) {
                try {
                    await _connectionAsync.ExecuteAsync(settings[i]);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Error setting db pref: '{settings[i]}'.", ex);
                }
            }
        }

        private static SQLiteConnectionString GetConnectionString(string dbPath, string dbPass) {

            if (!dbPath.IsFile()) {
                MpConsole.WriteLine($"Db Error cannot create connection string. Db file does not exist at '{dbPath}'");
                return null;
            }
            SQLiteConnectionString connStr = null;
            try {
                connStr =
                    new SQLiteConnectionString(
                        databasePath: dbPath,
                        key: dbPass,
                        //preKeyAction: db => db.Execute("PRAGMA cipher_default_use_hmac = ON;"),
                        //postKeyAction: db => db.Execute("PRAGMA kdf_iter = 128000;"),
                        //preKeyAction: db => db.Execute("PRAGMA page_size = 8192;"),
                        //postKeyAction: db => db.Execute("PRAGMA page_size = 512;"),
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

        private static async Task<bool> TestDbConnectionAsync() {
            if (_connectionAsync == null) {
                await CreateConnectionAsync();
                if (_connectionAsync == null) {
                    return false;
                }
            }
            try {
                await _connectionAsync.CreateTableAsync<MpTag>();
                return true;
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"DB connect test failed (couldn't create tag table).", ex);
                return false;
            }
        }
        private static async Task CreateConnectionAsync(
            string dbPath = "",
            string dbPass = null) {
            if (_connectionAsync != null) {
                return;
            }
            //#if !ANDROID
            Batteries_V2.Init();
            //#endif
            if (_connectionAsync == null) {
                try {
                    if (string.IsNullOrEmpty(dbPath)) {
                        dbPath = Mp.Services.DbInfo.DbPath;
                    }
                    if (string.IsNullOrEmpty(dbPass)) {
                        dbPass = Mp.Services.DbInfo.DbPassword;
                    }
                    var cs = GetConnectionString(dbPath, dbPass);
                    if (cs == null) {
                    }
                    _connectionAsync = new SQLiteAsyncConnection(cs);
                    var conn = _connectionAsync.GetConnection();
                    if (conn != null) {
                        MpCustomDbFunctions.AddCustomFunctions(conn.Handle);
                    }

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

        private static void CreateConnection(
            string dbPath = "",
            string dbPass = null) {
            if (_connection != null) {
                return;
            }

            Batteries_V2.Init();
            if (_connection == null) {
                try {

                    if (string.IsNullOrEmpty(dbPath)) {
                        dbPath = Mp.Services.DbInfo.DbPath;
                    }
                    if (string.IsNullOrEmpty(dbPass)) {
                        dbPass = Mp.Services.DbInfo.DbPassword;
                    }
                    _connection = new SQLiteConnection(GetConnectionString(dbPath, dbPass));
                    MpCustomDbFunctions.AddCustomFunctions(_connection.Handle);
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
            return
                string.IsNullOrEmpty(providedSourceClientGuid) ?
                    Mp.Services.ThisDeviceInfo.ThisDeviceGuid :
                    providedSourceClientGuid;
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


        private static async Task InitTablesAsync() {
            await _connectionAsync.CreateTableAsync<MpAction>();
            await _connectionAsync.CreateTableAsync<MpParameterValue>();
            await _connectionAsync.CreateTableAsync<MpApp>();
            await _connectionAsync.CreateTableAsync<MpAppOlePreset>();
            await _connectionAsync.CreateTableAsync<MpAppClipboardShortcuts>();
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
            await _connectionAsync.CreateTableAsync<MpPreset>();
            await _connectionAsync.CreateTableAsync<MpSearchCriteriaItem>();
            await _connectionAsync.CreateTableAsync<MpShortcut>();
            await _connectionAsync.CreateTableAsync<MpSyncHistory>();
            await _connectionAsync.CreateTableAsync<MpTag>();
            await _connectionAsync.CreateTableAsync<MpTextTemplate>();
            await _connectionAsync.CreateTableAsync<MpContentToken>();
            await _connectionAsync.CreateTableAsync<MpUrl>();
            await _connectionAsync.CreateTableAsync<MpUser>();
            await _connectionAsync.CreateTableAsync<MpUserDevice>();
        }

        private static async Task CreateViewsAsync() {
            await _connectionAsync.ExecuteAsync(@"
CREATE VIEW MpContentQueryView as
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
				(select MpUserDeviceGuid from MpUserDevice where pk_MpUserDeviceId in (select fk_MpUserDeviceId from MpApp where pk_MpAppId == MpTransactionSource.fk_SourceObjId limit 1))
		else NULL
	end as DeviceGuid,
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
		when MpCopyItem.e_MpCopyItemType == 'Text' and MpDataObjectItem.ItemFormat = 'Text'
			then 
				MpDataObjectItem.ItemData
		when MpCopyItem.e_MpCopyItemType == 'FileList' and MpDataObjectItem.ItemFormat = 'Files'
			then 
				MpDataObjectItem.ItemData	
		else NULL
	end as ItemData,	
	case
		when MpCopyItem.e_MpCopyItemType == 'Image'
			then 
				MpCopyItem.ItemData
		else NULL
	end as ItemImageData,
	CopyDateTime,
	CopyCount,
	PasteCount,
	HexColor as ItemColor,
	CopyCount + PasteCount as UsageScore
FROM
	MpCopyItem
LEFT JOIN MpDataObjectItem ON MpDataObjectItem.fk_MpDataObjectId = MpCopyItem.fk_MpDataObjectId
LEFT JOIN MpCopyItemTransaction ON MpCopyItemTransaction.fk_MpCopyItemId = MpCopyItem.pk_MpCopyItemId
LEFT JOIN MpTransactionSource ON MpTransactionSource.fk_MpCopyItemTransactionId = MpCopyItemTransaction.pk_MpCopyItemTransactionId");

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

        private static async Task<(string, bool)> GetDbPasswordAsync(int attemptNum, int maxAttempts, string title, string text) {
            if (attemptNum >= maxAttempts) {
                return default;
            }
            var result = await Mp.Services.PlatformMessageBox.ShowRememberableTextBoxMessageBoxAsync(
                title: title,
                passwordChar: '●',
                message: text,
                iconResourceObj: "LockImage",
                ntfType: MpNotificationType.DbPasswordInput);
            return result;
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
        //    return Mp.Services.ThisDeviceInfo.ThisDeviceGuid;
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
