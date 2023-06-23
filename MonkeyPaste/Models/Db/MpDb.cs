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
using System.Text;
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

        public static bool IsInitialDbCreate { get; set; } = false;

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

            IsInitialDbCreate = await InitDbConnectionAsync(Mp.Services.DbInfo, true);

            await InitTablesAsync();

            if (IsInitialDbCreate) {
                await MpDefaultDataModelTools.CreateAsync(MpPrefViewModel.Instance.ThisDeviceGuid);
                await CreateViewsAsync();
                await InitDefaultDataAsync();
            } else {
                await MpDefaultDataModelTools.InitializeAsync();
            }

            IsLoaded = true;

            sw.Stop();
            MpConsole.WriteLine($"Db loading: {sw.ElapsedMilliseconds} ms");
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

        public static async Task<bool> InitDbConnectionAsync(MpIDbInfo dbInfo, bool allowCreate) {
            string dbPath = dbInfo.DbPath;
            string dbPass = dbInfo.DbPassword;
            bool isNewDb = !File.Exists(dbPath);

            if (isNewDb && allowCreate) {
                using (File.Create(dbPath)) { }
            }

            await CreateConnectionAsync(dbPath);

            MpConsole.WriteLine($"Db {(isNewDb ? "CREATED" : "CONNECTED")} at '{dbPath}'");
#if DEBUG
            MpConsole.WriteLine($"Db Password: '{dbPass}'");
#endif
            return isNewDb;
        }

        private static SQLiteConnectionString GetConnectionString(
            string dbPath = "",
            string dbPass = null) {
            if (string.IsNullOrEmpty(dbPath)) {
                dbPath = Mp.Services.DbInfo.DbPath;
            }
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
        private static async Task CreateConnectionAsync(
            string dbPath = "",
            string dbPass = null) {
            if (_connectionAsync != null) {
                return;
            }

            Batteries_V2.Init();
            if (_connectionAsync == null) {
                try {
                    _connectionAsync = new SQLiteAsyncConnection(GetConnectionString(dbPath, dbPass));
                    MpCustomDbFunctions.AddCustomFunctions(_connectionAsync.GetConnection().Handle);
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
            await _connectionAsync.CreateTableAsync<MpPluginPreset>();
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

        private static async Task InitDefaultDataAsync() {
            await InitDefaultTagsAsync();
            await InitDefaultShortcutsAsync();
#if DEBUG
            if (MpPrefViewModel.Instance.IsInitialLoad) {
                await CreateTestContentAsync();
            }
#endif
            MpConsole.WriteLine(@"Created all default tables");
        }

        private static async Task InitDefaultTagsAsync() {
            bool tracked = true;
            bool synced = true;

            var default_tags = new object[] {
                // guid,name,color,treeIdx,pinIdx,track,sync,parentId,type
                new object[] { "df388ecd-f717-4905-a35c-a8491da9c0e3", "Collections", MpSystemColors.lemonchiffon2, 1,-1, tracked,synced, 0, MpTagType.Group},
                new object[] { "287140cc-2f9a-4bc6-a88d-c5b836f1a340", "All", MpSystemColors.blue1, 0,-1, tracked,synced, MpTag.FiltersTagId, MpTagType.Link},
                new object[] { "54b61353-b031-4029-9bda-07f7ca55c123", "Favorites", MpSystemColors.yellow1, 1,-1,tracked,synced, MpTag.CollectionsTagId, MpTagType.Link},
                new object[] { "e62b8e5d-52a6-46f1-ac51-8f446916dd85", "Filters", MpSystemColors.forestgreen, 0,-1,tracked,synced, 0, MpTagType.Group},
                new object[] { "70db0f5c-a717-4bca-af2f-a7581aecc24d", "Trash", MpSystemColors.lightsalmon1, 2,-1,tracked,synced, 0, MpTagType.Link},
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

            #region Recent

            var recent_tag = await MpTag.CreateAsync(
                    tagName: "Recent",
                    hexColor: MpSystemColors.pink,
                    parentTagId: MpTag.FiltersTagId,
                    sortType: MpContentSortType.CopyDateTime,
                    pinSortIdx: 0,
                    isSortDescending: true,
                    tagType: MpTagType.Query);

            var recent_tag_simple_cri = await MpSearchCriteriaItem.CreateAsync(
                tagId: recent_tag.Id,
                sortOrderIdx: 0,
                joinType: MpLogicalQueryType.And,
                queryType: MpQueryType.Simple,
                options:
                    ((long)(
                    MpContentQueryBitFlags.Content |
                    MpContentQueryBitFlags.TextType |
                    MpContentQueryBitFlags.ImageType |
                    MpContentQueryBitFlags.FileType)).ToString());

            var recent_tag_datetime_cri = await MpSearchCriteriaItem.CreateAsync(
                tagId: recent_tag.Id,
                sortOrderIdx: 1,
                joinType: MpLogicalQueryType.And,
                queryType: MpQueryType.Advanced,
                options:
                    string.Join(
                        ",",
                        new[] {
                            (int)MpRootOptionType.History, //5
                            (int)MpHistoryTypeOptionType.Created, //1
                            (int)MpDateTimeOptionType.After, //3 
                            (int)MpDateAfterUnitType.Yesterday} //1
                        .Select(x => x.ToString())),
                matchValue: 0.ToString());

            #endregion

            #region Item Types

            var item_type_group_tag = await MpTag.CreateAsync(
                    tagName: "Formats",
                    hexColor: MpSystemColors.peachpuff4,
                    parentTagId: MpTag.FiltersTagId,
                    tagType: MpTagType.Group);

            #region Text

            var text_type_tag = await MpTag.CreateAsync(
                    tagName: "Text",
                    hexColor: MpSystemColors.darkgoldenrod3,
                    parentTagId: item_type_group_tag.Id,
                    sortType: MpContentSortType.CopyDateTime,
                    isSortDescending: true,
                    tagType: MpTagType.Query);


            var text_def_simple_cri = await MpSearchCriteriaItem.CreateAsync(
                tagId: text_type_tag.Id,
                queryType: MpQueryType.Simple,
                options:
                    ((long)(MpSearchCriteriaItem.DefaultSimpleFilters)).ToString());

            var text_type_tag_cri = await MpSearchCriteriaItem.CreateAsync(
                tagId: text_type_tag.Id,
                queryType: MpQueryType.Advanced,
                options:
                    string.Join(",",
                    new[] {
                            (int)MpRootOptionType.Type,
                            (int)MpContentTypeOptionType.Text }));

            #endregion

            #region Image

            var image_type_tag = await MpTag.CreateAsync(
                    tagName: "Image",
                    hexColor: MpSystemColors.sienna2,
                    parentTagId: item_type_group_tag.Id,
                    sortType: MpContentSortType.CopyDateTime,
                    isSortDescending: true,
                    tagType: MpTagType.Query);


            var image_def_simple_cri = await MpSearchCriteriaItem.CreateAsync(
                tagId: image_type_tag.Id,
                queryType: MpQueryType.Simple,
                options:
                    ((long)(MpSearchCriteriaItem.DefaultSimpleFilters)).ToString());

            var image_type_tag_cri = await MpSearchCriteriaItem.CreateAsync(
                tagId: image_type_tag.Id,
                queryType: MpQueryType.Advanced,
                options:
                    string.Join(",",
                    new[] {
                            (int)MpRootOptionType.Type,
                            (int)MpContentTypeOptionType.Image }));

            #endregion

            #region Files

            var file_type_tag = await MpTag.CreateAsync(
                    tagName: "Files",
                    hexColor: MpSystemColors.mediumorchid3,
                    parentTagId: item_type_group_tag.Id,
                    sortType: MpContentSortType.CopyDateTime,
                    isSortDescending: true,
                    tagType: MpTagType.Query);


            var file_def_simple_cri = await MpSearchCriteriaItem.CreateAsync(
                tagId: file_type_tag.Id,
                queryType: MpQueryType.Simple,
                options:
                    ((long)(MpSearchCriteriaItem.DefaultSimpleFilters)).ToString());

            var file_type_tag_cri = await MpSearchCriteriaItem.CreateAsync(
                tagId: file_type_tag.Id,
                queryType: MpQueryType.Advanced,
                options:
                    string.Join(",",
                    new[] {
                            (int)MpRootOptionType.Type,
                            (int)MpContentTypeOptionType.Files }));

            #endregion

            #endregion
        }

        public static async Task ResetShortcutsAsync(MpShortcutRoutingProfileType routingProfile) {
            var sl = await MpDataModelProvider.GetItemsAsync<MpShortcut>();
            await Task.WhenAll(sl.Select(x => x.DeleteFromDatabaseAsync()));

            await InitDefaultShortcutsAsync(routingProfile);
        }
        private static async Task CreateTestContentAsync() {
            return;
            var this_app = await MpDataModelProvider.GetItemAsync<MpApp>(MpDefaultDataModelTools.ThisAppId);
            string this_app_url = Mp.Services.SourceRefTools.ConvertToInternalUrl(this_app);

            int content_cap = Mp.Services.AccountTools.GetContentCapacity(MpUser.TEST_ACCOUNT_TYPE);
            // NOTE subtracting 2 for 1 the loading item and 2 
            int test_items = content_cap < 0 ? 300 : content_cap - 2;
            for (int i = 0; i < 300; i++) {
                string data = $"<p>This is test {i + 1}.</p>";
                var mpdo = new MpPortableDataObject(MpPortableDataFormats.Text, data);
                var dobj = await MpDataObject.CreateAsync(pdo: mpdo);
                var ci = await MpCopyItem.CreateAsync(
                    data: data,
                    itemType: MpCopyItemType.Text,
                    title: $"Test {i + 1}",
                    dataObjectId: dobj.Id);

                await Mp.Services.TransactionBuilder.ReportTransactionAsync(
                    copyItemId: ci.Id,
                    reqType: MpJsonMessageFormatType.DataObject,
                    respType: MpJsonMessageFormatType.Delta,
                    transType: MpTransactionType.Created,
                    ref_uris: new[] { this_app_url });

            }

        }
        private static async Task InitDefaultShortcutsAsync(MpShortcutRoutingProfileType routingProfile = MpShortcutRoutingProfileType.Internal) {
            MpRoutingType mw_routing = routingProfile.GetProfileBasedRoutingType(MpShortcutType.ToggleMainWindow);
            MpRoutingType globalRouting = routingProfile.GetProfileBasedRoutingType(MpShortcutType.ToggleListenToClipboard);


            var ps = Mp.Services.PlatformShorcuts;
            List<string[]> defaultShortcutDefinitions = new List<string[]>() {
                // ORDER:
                // guid,keyString,shortcutType,routeType, readOnly = false

                // GLOBAL
                
                 new string[] {"5dff238e-770e-4665-93f5-419e48326f01","Caps Lock", MpShortcutType.ToggleMainWindow.ToString(), mw_routing.ToString(),"False","False"},
                 new string[] {"97e29b06-0ec4-4c55-a393-8442d7695038","Control+Shift+F1", MpShortcutType.ToggleListenToClipboard.ToString(), globalRouting.ToString(),"False","False"},
                 new string[] {"892bf7d7-ba8e-4db1-b2ca-62b41ff6614c", "Control+Shift+F2", MpShortcutType.ToggleAutoCopyMode.ToString(), globalRouting.ToString(),"False","False"},
                 new string[] {"a12c4211-ab1f-4b97-98ff-fbeb514e9a1c", "Control+Shift+F3", MpShortcutType.ToggleRightClickPasteMode.ToString(), globalRouting.ToString(),"False","False"},
                 new string[] {"501b4f91-706a-4c9d-a536-f7f8871c0a54", "Control+Shift+F4", MpShortcutType.ToggleDropWidgetEnabled.ToString(), globalRouting.ToString(),"False","False"},
                 new string[] {"777367e6-c161-4e93-93e0-9bf12221f7ff", "Control+Shift+F5", MpShortcutType.ToggleAppendLineMode.ToString(), globalRouting.ToString(),"False","False"},
                 new string[] {"a41aeed8-d4f3-47de-86c5-f9ca296fb103", "Control+Shift+F6", MpShortcutType.ToggleAppendInsertMode.ToString(), globalRouting.ToString(),"False","False"},
                 new string[] {"9fa72a1b-2286-4907-bf70-37686aad009a", "Control+Shift+F7", MpShortcutType.ToggleAppendPreMode.ToString(), globalRouting.ToString(),"False","False"},
                 new string[] {"9fa72a1b-2286-4907-bf70-37686aad009a", "Control+Shift+F8", MpShortcutType.ToggleAppendPaused.ToString(), globalRouting.ToString(),"False","False"},

                 // APPLICATION
                 new string[] {"94e81589-fe2f-4e80-8940-ed066f0d9c27",ps.PasteKeys, MpShortcutType.PasteHere.ToString(), MpRoutingType.Internal.ToString(),"True"},
                 new string[] {"ee74dd92-d18b-46cf-91b7-3946ab55427c",ps.CopyKeys, MpShortcutType.CopySelection.ToString(), MpRoutingType.Internal.ToString(),"True"},
                 new string[] {"2acde1cc-c8e4-4675-8895-81712a6f0a36",ps.CutKeys, MpShortcutType.CutSelection.ToString(), MpRoutingType.Internal.ToString(),"True"},
                 new string[] {"cb807500-9121-4e41-80d3-8c3682ce90d9","Escape", MpShortcutType.HideMainWindow.ToString(), MpRoutingType.Internal.ToString(),"True"},
                 new string[] {"1d212ca5-fb2a-4962-8f58-24ed9a5d007d","Control+Enter", MpShortcutType.PasteSelectedItems.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"e94ca4f3-4c6e-40dc-8941-c476a81543c7","Delete", MpShortcutType.DeleteSelectedItems.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"7fe24929-6c9e-49c0-a880-2f49780dfb3a","Right", MpShortcutType.SelectNextColumnItem.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"ee657845-f1dc-40cf-848d-6768c0081670","Left", MpShortcutType.SelectPreviousColumnItem.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"674bae7f-0a60-4f17-ac2c-81d5c6c3d879","Down", MpShortcutType.SelectNextRowItem.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"b916ab44-d4bd-4d8b-ac4a-de947343bd5a","Up", MpShortcutType.SelectPreviousRowItem.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"6487f6ff-da0c-475b-a2ae-ef1484233de0","Control+I", MpShortcutType.AssignShortcut.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"837e0c20-04b8-4211-ada0-3b4236da0821","Control+M", MpShortcutType.ChangeColor.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"118a2ca6-7021-47a0-8458-7ebc31094329","Control+Z", MpShortcutType.Undo.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"3980efcc-933b-423f-9cad-09e455c6824a","Control+Y", MpShortcutType.Redo.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"7a7580d1-4129-432d-a623-2fff0dc21408","Control+E", MpShortcutType.ToggleContentReadOnly.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"085338fb-f297-497a-abb7-eeb7310dc6f3","F2", MpShortcutType.Rename.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"e22faafd-4313-441a-b361-16910fc7e9d3","Control+D", MpShortcutType.Duplicate.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"ac8abe92-82c3-46fb-9bd5-39d74b100b23","Home", MpShortcutType.ScrollToHome.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"ac8abe92-82c3-46fb-9bd5-39d74b100b23","End", MpShortcutType.ScrollToEnd.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"9b0ca09a-5724-4004-98d2-f5ef8ae02055","Control+Up", MpShortcutType.WindowSizeUp.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"39a6194e-37e3-4d37-a9f4-254ed83157f2","Control+Down", MpShortcutType.WindowSizeDown.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"6cc03ef0-3b33-4b94-9191-0d751e6b7fb6","Control+Left", MpShortcutType.WindowSizeLeft.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"c4ac1629-cdf0-4075-94af-8f934b014452","Control+Right", MpShortcutType.WindowSizeRight.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"30c813a0-d466-4ae7-b75e-82680b4542fc","PageUp", MpShortcutType.PreviousPage.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"09df97ea-f786-48d9-9112-a60266df6586","PageDown", MpShortcutType.NextPage.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"a39ac0cb-41e4-47b5-b963-70e388dc156a","Control+H", MpShortcutType.FindAndReplaceSelectedItem.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"cb1ac03b-a20f-4911-bf4f-bc1a858590e3","Control+L", MpShortcutType.ToggleMainWindowLocked.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"d73204f5-fbed-4d87-9dca-6dfa8d8cba82","Control+K", MpShortcutType.ToggleFilterMenuVisible.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"49f44a89-e381-4d6a-bf8c-1090eb443f17","Control+Q", MpShortcutType.ExitApplication.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"31e0a078-e80b-4d31-b236-2a585d6352cf", "Control+,", MpShortcutType.ShowSettings.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"5143ed80-a50a-41b9-9979-5e00c084560d", "Control+P", MpShortcutType.TogglePinned.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"755a4d0e-d26a-42cf-89a3-6c5710bd2e4c", "Control+O", MpShortcutType.OpenInWindow.ToString(), MpRoutingType.Internal.ToString()},
                 new string[] {"728bfb06-1d96-441c-b710-efee383138be", "Control+G", MpShortcutType.ToggleAppendManualMode.ToString(), MpRoutingType.Internal.ToString()},
            };

            foreach (var defaultShortcut in defaultShortcutDefinitions) {
                await MpShortcut.CreateAsync(
                    guid: defaultShortcut[0],
                    keyString: defaultShortcut[1],
                    shortcutType: defaultShortcut[2].ToEnum<MpShortcutType>(),
                    routeType: defaultShortcut[3].ToEnum<MpRoutingType>(),
                    isReadOnly: defaultShortcut.Length >= 5 ? bool.Parse(defaultShortcut[4]) : false,
                    isInternalOnly: defaultShortcut.Length >= 6 ? bool.Parse(defaultShortcut[5]) : true);
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
