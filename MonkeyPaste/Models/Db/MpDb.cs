using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using SQLiteNetExtensions;
using SQLiteNetExtensions.Extensions;
using SQLiteNetExtensions.Exceptions;
using SQLiteNetExtensions.Attributes;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.IO;
using Newtonsoft.Json;
using Xamarin.Forms.PlatformConfiguration;
using System.Collections;
using System.Reflection;
using SkiaSharp;
using System.Diagnostics;
using System.Linq.Expressions;

namespace MonkeyPaste {    
    public class MpDb : MpISync {
        #region Singleton
        private static readonly Lazy<MpDb> _Lazy = new Lazy<MpDb>(() => new MpDb());
        public static MpDb Instance { get { return _Lazy.Value; } }

        private MpDb() {
            //CreateConnection();
            //Init();
        }
        #endregion

        #region Private Variables
        private MpIDbInfo _dbInfo;
        private object _rdLock;
        private SQLiteAsyncConnection _connectionAsync;
        private SQLiteConnection _connection;
        #endregion

        #region Properties
        public bool UseWAL { get; set; } = true;
        public string IdentityToken { get; set; }
        public string AccessToken { get; set; }
        public bool IsLoaded { get; set; } = false;
        #endregion

        #region Events
        public event EventHandler<MpDbModelBase> OnItemAdded;
        public event EventHandler<MpDbModelBase> OnItemUpdated;
        public event EventHandler<MpDbModelBase> OnItemDeleted;
        public event EventHandler<object> OnSyncableChange;

        public event EventHandler<MpDbSyncEventArgs> SyncAdd;
        public event EventHandler<MpDbSyncEventArgs> SyncUpdate;
        public event EventHandler<MpDbSyncEventArgs> SyncDelete;
        #endregion

        #region Public Methods
        public async Task Init(MpIDbInfo dbInfo) {
            await Task.Run(async () => {
                var sw = new Stopwatch();
                sw.Start();
                _dbInfo = dbInfo;
                MpPreferences.Instance.StartupDateTime = DateTime.Now;
                await InitDb();
                IsLoaded = true;
                sw.Stop();
                MpConsole.WriteLine($"Db loading: {sw.ElapsedMilliseconds} ms");
            });
        }

        #region Queries

        public SQLite.TableMapping GetTableMapping(string tableName) {
            if (_connection == null) {
                CreateConnection();
            }
            return _connection
                    .TableMappings
                    .Where(x => x.TableName.ToLower() == tableName.ToLower()).FirstOrDefault();
        }

        public async Task<List<T>> QueryAsync<T>(string query, params object[] args) where T : new() {
            if(_connectionAsync == null) {
                CreateConnection();
            }
            var result = await _connectionAsync.QueryAsync<T>(query, args);
            return result;
        }

        public List<T> Query<T>(string query, params object[] args) where T : new() {
            if (_connection == null) {
                CreateConnection();
            }
            var result = _connection.Query<T>(query, args);
            return result;
        }

        public async Task<List<object>> QueryAsync(string tableName,string query, params object[] args) {
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

        public List<object> Query(string tableName, string query, params object[] args) {
            if (_connection == null) {
                CreateConnection();
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
            var result = _connection.Query(qtm, query, args);
            return result;
        }

        public async Task<T> QueryScalarAsync<T>(string query, params object[] args) {
            if(_connectionAsync == null) {
                CreateConnection();
            }
            var result = await _connectionAsync.ExecuteScalarAsync<T>(query, args);
            return result;
        }

        public T QueryScalar<T>(string query, params object[] args) {
            if (_connection == null) {
                CreateConnection();
            }
            var result = _connection.ExecuteScalar<T>(query, args);
            return result;
        }

        public async Task<List<T>> QueryScalarsAsync<T>(string query, params object[] args) {
            if (_connectionAsync == null) {
                CreateConnection();
            }
            var result = await _connectionAsync.QueryScalarsAsync<T>(query, args);
            return result;
        }

        public List<T> QueryScalars<T>(string query, params object[] args) {
            if (_connection == null) {
                CreateConnection();
            }
            var result = _connection.QueryScalars<T>(query, args);
            return result;
        }

        public async Task<List<T>> GetItemsAsync<T>() where T : new() {
            if (_connectionAsync == null) {
                await InitDb ();
            }
            //return await _connectionAsync.Table<T>().ToListAsync();
            var dbol = await _connectionAsync.GetAllWithChildrenAsync<T>(recursive: true);
            return dbol;
        }

        public List<T> GetItems<T>() where T : new() {
            if (_connection == null) {
                CreateConnection();
            }
            return _connection.GetAllWithChildren<T>(recursive: true);
        }

        public async Task<T> GetItemAsync<T>(int id) where T : new() {
            if (_connectionAsync == null) {
                await InitDb();
            }
            var dbo = await _connectionAsync.GetWithChildrenAsync<T>(id, true);
            return dbo;
        }

        public T GetItem<T>(int id) where T : new() {
            if (_connection == null) {
                CreateConnection();
            }
            return _connection.GetWithChildren<T>(id,true);
        }

        public List<T> GetAllWithChildren<T>(Expression<Func<T,bool>> exp, bool recursive = true) where T:new() {
            if(_connection == null) {
                CreateConnection();
            }
            return _connection.GetAllWithChildren<T>(exp,recursive);
        }

        public async Task<List<T>> GetAllWithChildrenAsync<T>(Expression<Func<T, bool>> exp, bool recursive = true) where T : new() {
            if (_connectionAsync == null) {
                CreateConnection();
            }
            var result = await _connectionAsync.GetAllWithChildrenAsync<T>(exp, recursive);
            return result;
        }

        public async Task AddItemAsync<T>(T item, string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {
            sourceClientGuid = string.IsNullOrEmpty(sourceClientGuid) ? MpPreferences.Instance.ThisDeviceGuid : sourceClientGuid;
            if (_connectionAsync == null) {
                await InitDb();
            }
            if (item == null) {
                MpConsole.WriteTraceLine(@"Cannot add null item, ignoring...");
                return;
            }
            if (item is MpISyncableDbObject && item is not MpDbLog && item is not MpSyncHistory) {
                if (string.IsNullOrEmpty((item as MpDbModelBase).Guid)) {
                    (item as MpDbModelBase).Guid = System.Guid.NewGuid().ToString();
                }
                if (!ignoreTracking) {
                    await MpDbLogTracker.TrackDbWriteAsync(MpDbLogActionType.Create, item as MpDbModelBase, sourceClientGuid);
                }                
            }
            if(item is MpCopyItemTag cit) {
                if(cit.CopyItemId == 0 && cit.TagId == 0) {
                    return;
                }
            } 
            await _connectionAsync.InsertOrReplaceWithChildrenAsync(item, recursive: true);
            OnItemAdded?.Invoke(this, item as MpDbModelBase);
            if (!ignoreSyncing && item is MpISyncableDbObject) {
                OnSyncableChange?.Invoke(item, (item as MpDbModelBase).Guid);
            }
        }

        public void AddItem<T>(T item, string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {
            sourceClientGuid = string.IsNullOrEmpty(sourceClientGuid) ? MpPreferences.Instance.ThisDeviceGuid : sourceClientGuid;
            if (_connection == null) {
                CreateConnection();
            }
            if (item == null) {
                MpConsole.WriteTraceLine(@"Cannot add null item, ignoring...");
                return;
            }
            if (item is MpISyncableDbObject && item is not MpDbLog && item is not MpSyncHistory) {
                if (string.IsNullOrEmpty((item as MpDbModelBase).Guid)) {
                    (item as MpDbModelBase).Guid = System.Guid.NewGuid().ToString();
                }
                if (!ignoreTracking) {
                    MpDbLogTracker.TrackDbWrite(MpDbLogActionType.Create, item as MpDbModelBase, sourceClientGuid);
                }

            }
            _connection.InsertOrReplaceWithChildren(item,true);
            OnItemAdded?.Invoke(this, item as MpDbModelBase);
            if (!ignoreSyncing && item is MpISyncableDbObject) {
                OnSyncableChange?.Invoke(item, (item as MpDbModelBase).Guid);
            }
        }

        public async Task UpdateItemAsync<T>(T item,string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {
            sourceClientGuid = string.IsNullOrEmpty(sourceClientGuid) ? MpPreferences.Instance.ThisDeviceGuid : sourceClientGuid;
            if (_connectionAsync == null) {
                await InitDb();
            }

            if (item == null) {
                MpConsole.WriteTraceLine(@"Cannot update null item, ignoring...");
                return;
            }
            if (item is MpISyncableDbObject && item is not MpDbLog && item is not MpSyncHistory) {   
                if(!ignoreTracking) {
                    await MpDbLogTracker.TrackDbWriteAsync(MpDbLogActionType.Modify, item as MpDbModelBase, sourceClientGuid);
                }
            }

            await _connectionAsync.UpdateWithChildrenAsync(item);
            OnItemUpdated?.Invoke(this, item as MpDbModelBase);
            if (!ignoreSyncing && item is MpISyncableDbObject) {
                OnSyncableChange?.Invoke(item, (item as MpDbModelBase).Guid);
            }
        }

        public void UpdateItem<T>(T item, string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {
            sourceClientGuid = string.IsNullOrEmpty(sourceClientGuid) ? MpPreferences.Instance.ThisDeviceGuid : sourceClientGuid;
            if (_connection == null) {
                CreateConnection();
            }

            if (item == null) {
                MpConsole.WriteTraceLine(@"Cannot update null item, ignoring...");
                return;
            }
            if (item is MpISyncableDbObject && item is not MpDbLog && item is not MpSyncHistory) {
                if (!ignoreTracking) {
                    MpDbLogTracker.TrackDbWrite(MpDbLogActionType.Modify, item as MpDbModelBase, sourceClientGuid);
                }
            }
            _connection.UpdateWithChildren(item);
            OnItemUpdated?.Invoke(this, item as MpDbModelBase);
            if (!ignoreSyncing && item is MpISyncableDbObject) {
                OnSyncableChange?.Invoke(item, (item as MpDbModelBase).Guid);
            }
        }

        public async Task DeleteItemAsync<T>(T item, string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {
            sourceClientGuid = string.IsNullOrEmpty(sourceClientGuid) ? MpPreferences.Instance.ThisDeviceGuid : sourceClientGuid;
            if (_connectionAsync == null) {
                await InitDb();
            }
            if (item == null) {
                MpConsole.WriteTraceLine(@"Cannot delete null item, ignoring...");
                return;
            }
            if (item is MpISyncableDbObject && item is not MpDbLog && item is not MpSyncHistory) {
                if (!ignoreTracking) {
                    MpDbLogTracker.TrackDbWrite(MpDbLogActionType.Delete, item as MpDbModelBase, sourceClientGuid);
                }
            }

            await _connectionAsync.DeleteAsync(item, true);
            OnItemDeleted?.Invoke(this, item as MpDbModelBase);
            if (!ignoreSyncing && item is MpISyncableDbObject) {
                OnSyncableChange?.Invoke(item, (item as MpDbModelBase).Guid);
            }
        }

        public void DeleteItem<T>(T item, string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {
            sourceClientGuid = string.IsNullOrEmpty(sourceClientGuid) ? MpPreferences.Instance.ThisDeviceGuid : sourceClientGuid;
            if (_connection == null) {
                CreateConnection();
            }
            if (item == null) {
                MpConsole.WriteTraceLine(@"Cannot delete null item, ignoring...");
                return;
            }
            if (item is MpISyncableDbObject && item is not MpDbLog && item is not MpSyncHistory) {
                if (!ignoreTracking) {
                    MpDbLogTracker.TrackDbWrite(MpDbLogActionType.Delete, item as MpDbModelBase, sourceClientGuid);
                }
            }

            _connection.Delete(item, true);
            OnItemDeleted?.Invoke(this, item as MpDbModelBase);
            if (!ignoreSyncing && item is MpISyncableDbObject) {
                OnSyncableChange?.Invoke(item, (item as MpDbModelBase).Guid);
            }
        }

        public async Task AddOrUpdateAsync<T>(T item,  string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {            
            if ((item as MpDbModelBase).Id == 0) {
                await AddItemAsync(item, sourceClientGuid,ignoreTracking,ignoreSyncing);
            } else {
                await UpdateItemAsync(item, sourceClientGuid,ignoreTracking,ignoreSyncing);
            }
        }

        public void AddOrUpdate<T>(T item, string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {            
            if ((item as MpDbModelBase).Id == 0) {
                AddItem(item, sourceClientGuid, ignoreTracking, ignoreSyncing);
            } else {
                UpdateItem(item, sourceClientGuid, ignoreTracking, ignoreSyncing);
            }
        }
        
        public async Task<object> GetDbObjectByTableGuidAsync(string tableName, string objGuid) {
            var dt = await QueryAsync(
                tableName,
                string.Format("select * from {0} where {1}=?",tableName,tableName+"Guid"),
                objGuid);

            if (dt != null && dt.Count > 0) {
                return dt[0];
            }
            var dbot = new MpXamStringToSyncObjectTypeConverter().Convert(tableName);

            var dbo = Activator.CreateInstance(dbot);
            return dbo;
        }

        public async Task<T> GetDbObjectByTableGuidAsync<T>(string objGuid) where T : new() {
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

        public object GetDbObjectByTableGuid(string tableName, string objGuid) {
            var dt = Query(
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

        public T GetDbObjectByTableGuid<T>(string objGuid) where T : new() {
            string tableName = typeof(T).ToString().Replace("MonkeyPaste.", string.Empty);
            var dt = Query(
                tableName,
                string.Format("select * from {0} where {1}=?", tableName, tableName + "Guid"),
                objGuid);

            if (dt != null && dt.Count > 0) {
                return GetItem<T>((dt[0] as MpDbModelBase).Id);
                //return dbo;
                //return dt[0];
            }

            var dbo = Activator.CreateInstance(typeof(T));
            return (T)dbo;
        }
        #endregion

        public byte[] GetDbFileBytes() {
            var dbPath = _dbInfo.GetDbFilePath();
            return File.ReadAllBytes(dbPath);
        }

        #region Wpf backwards compatability
        private string GetTableName(string query) {
            string tableName = "UnknownTableName";

            if (query.ToLower().StartsWith("select")) {
                string preStr = "from ";
                int tableNameStartIdx = query.IndexOf(preStr) + preStr.Length;
                int tableNameEndIdx = query.Substring(tableNameStartIdx).IndexOf(" ");
                int tableNameLength = query.Length - tableNameStartIdx;
                if(tableNameEndIdx >= 0) {
                    tableNameEndIdx += tableNameStartIdx;
                    tableNameLength = tableNameEndIdx - tableNameStartIdx;
                }
                tableName = query.Substring(tableNameStartIdx, tableNameLength);
            } else if (query.ToLower().StartsWith("insert")) {
                string preStr = "insert into ";
                int tableNameLength = query.IndexOf(@"(") - preStr.Length;
                tableName = query.Substring(preStr.Length, tableNameLength);
            } else if (query.ToLower().StartsWith("update")) {
                string preStr = "update ";
                int tableNameLength = query.Substring(preStr.Length).IndexOf(@" ");
                tableName = query.Substring(preStr.Length, tableNameLength);
            } else if (query.ToLower().StartsWith("delete")) {
                string preStr = "delete from ";
                int tableNameLength = query.Substring(preStr.Length).IndexOf(@" ");
                tableName = query.Substring(preStr.Length, tableNameLength);
            } else {
                throw new Exception(@"Unknown query format: " + query);
            }

            return tableName;
        }
        private Tuple<string,object[]> PrepareQuery(string query, Dictionary<string, object> args) {
            MpConsole.WriteLine("Prepare @ " + DateTime.Now);
            if (string.IsNullOrEmpty(query.Trim())) {
                return null;
            }

            string newQuery = query;
            object[] newArgs = new object[] { };
            //var sb = new StringBuilder();

            if (args != null) {
                var newArgDict = new Dictionary<int, object>();
                foreach (var arg in args) {
                    int argStartIdx = query.IndexOf(arg.Key);
                    if (argStartIdx < 0) {
                        throw new Exception(@"Error with query: " + query);
                    }
                    newArgDict.Add(argStartIdx, arg.Value);
                    newQuery = newQuery.Replace(arg.Key, "?");
                }
                var sortedArgs = from pair in newArgDict
                                 orderby pair.Key ascending
                                 select pair.Value;
                newArgs = sortedArgs.ToArray();
            }
            return new Tuple<string, object[]>(newQuery, newArgs);
        }

        public DataTable Execute(string query, Dictionary<string, object> args) {
            if (string.IsNullOrEmpty(query.Trim())) {
                return null;
            }
            if (_connection == null) {
                CreateConnection();
            }

            string tn = GetTableName(query);
            var dbot = new MpXamStringToSyncObjectTypeConverter().Convert(tn);

            var tuple = PrepareQuery(query, args);

            var queryMethod = _connection.GetType().GetMethod("Query",new Type[] {typeof(string), typeof(object[]) });
            var queryByDboTypeMethod = queryMethod.MakeGenericMethod(new[] { dbot });
            var resultObj = queryByDboTypeMethod.Invoke(_connection, new object[] { tuple.Item1, tuple.Item2 });

            var result = Activator.CreateInstance(typeof(List<>).MakeGenericType(dbot), resultObj);
            var dt = new DataTable();

            var tm = GetTableMapping(tn);
            foreach(var row in result as IList) {
                var dr = new DataRow(); 
                foreach (var rowProp in row.GetType().GetProperties()) {
                    if(rowProp.GetAttribute<SQLite.IgnoreAttribute>() != null) {
                        continue;
                    }
                    string cn = tm.FindColumnWithPropertyName(rowProp.Name).Name;
                    dr.AddColumn(cn, rowProp.GetValue(row));
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }

        public async Task<DataTable> ExecuteAsync(string query, Dictionary<string, object> args) {
            if (string.IsNullOrEmpty(query.Trim())) {
                return null;
            }
            if (_connectionAsync == null) {
                CreateConnection();
            }

            string tn = GetTableName(query);
            var dbot = new MpXamStringToSyncObjectTypeConverter().Convert(tn);

            var tuple = PrepareQuery(query, args);

            var queryMethod = _connectionAsync.GetType().GetMethod("QueryAsync", new Type[] { typeof(string), typeof(object[]) });
            var queryByDboTypeMethod = queryMethod.MakeGenericMethod(new[] { dbot });
            var resultObj = queryByDboTypeMethod.InvokeAsync(_connectionAsync, new object[] { tuple.Item1, tuple.Item2 });
            await resultObj;
            var result = Activator.CreateInstance(typeof(List<>).MakeGenericType(dbot), resultObj);
            var dt = new DataTable();

            var tm = GetTableMapping(tn);
            foreach (var row in result as IList) {
                var dr = new DataRow();
                foreach (var rowProp in row.GetType().GetProperties()) {
                    if (rowProp.GetAttribute<SQLite.IgnoreAttribute>() != null) {
                        continue;
                    }
                    string cn = tm.FindColumnWithPropertyName(rowProp.Name).Name;
                    dr.AddColumn(cn, rowProp.GetValue(row));
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }

        public int ExecuteWrite(string query, Dictionary<string, object> args, string dbObjectGuid = "", string sourceClientGuid = "", object dbObject = null, bool ignoreTracking = false, bool ignoreSyncing = false) {
            if (_connection == null) {
                CreateConnection();
            }
            MpDbLogActionType actionType = MonkeyPaste.MpDbLogActionType.None;
            if (!string.IsNullOrEmpty(dbObjectGuid) && !ignoreTracking && dbObject != null && dbObject is MonkeyPaste.MpISyncableDbObject) {
                //only track objects providing a guid
                actionType = MpDbLogTracker.TrackDbWrite(query, args, dbObjectGuid, sourceClientGuid, dbObject);
            }

            var tuple = PrepareQuery(query, args);

            if(tuple.Item2 == null) {
                return _connection.Execute(tuple.Item1);
            }
            int rowsAffected = _connection.Execute(tuple.Item1, tuple.Item2);

            if (actionType != MonkeyPaste.MpDbLogActionType.None &&
                    !ignoreSyncing &&
                    dbObject is MonkeyPaste.MpISyncableDbObject) {
                OnSyncableChange?.Invoke(dbObject, dbObjectGuid);
            } else if (dbObject != null) {
                //dbObject will only be non-null when this write is coming from perform sync
                string tableName = dbObject.GetType().ToString().Replace("MpWpfApp.", string.Empty);
                string pkPropName = dbObject.GetType().ToString().Replace("MpWpfApp.Mp", string.Empty) + "Id";
                int pk = GetLastRowId(tableName, "pk_Mp" + pkPropName);
                dbObject.GetType().GetProperty(pkPropName).SetValue(dbObject, pk);

                NotifyRemoteUpdate(actionType, dbObject, sourceClientGuid);
            }
            return rowsAffected;
        }
        public int GetLastRowId(string tableName, string pkName) {
            if (_connection == null) {
                CreateConnection();
            }

            DataTable dt = Execute("select * from " + tableName + " ORDER BY " + pkName + " DESC LIMIT 1;", null);
            if (dt.Rows.Count > 0) {
                return Convert.ToInt32(dt.Rows[0][0].ToString());
            }
            return -1;
        }

        public DataRow GetDbDataRowByTableGuid(string tableName, string objGuid) {
            var dt = MpDb.Instance.Execute(
                "select * from " + tableName + " where " + tableName + "Guid='" + objGuid + "'", null);
            if (dt != null && dt.Rows.Count > 0) {
                return dt.Rows[0];
            }
            return null;
        }
        #endregion

        #endregion

        #region Private Methods  
        private void CreateConnection() {
            SQLiteConnectionString connStr = null;
            connStr = new SQLiteConnectionString(
                    databasePath: _dbInfo.GetDbFilePath(),
                    storeDateTimeAsTicks: false,
                    openFlags: SQLiteOpenFlags.ReadWrite |
                               SQLiteOpenFlags.Create |
                               SQLiteOpenFlags.SharedCache |
                               SQLiteOpenFlags.FullMutex
                    );
            //if (string.IsNullOrEmpty(_dbInfo.GetDbPassword())) {
            //    connStr = new SQLiteConnectionString(
            //        databasePath: _dbInfo.GetDbFilePath(),
            //        storeDateTimeAsTicks: false,
            //        openFlags: SQLiteOpenFlags.ReadWrite |
            //                   SQLiteOpenFlags.Create |
            //                   SQLiteOpenFlags.SharedCache |
            //                   SQLiteOpenFlags.FullMutex
            //        );
            //} else {
            //    connStr = new SQLiteConnectionString(
            //        databasePath: _dbInfo.GetDbPassword(),
            //        storeDateTimeAsTicks: false,
            //        key: MpPreferences.Instance.DbPassword,
            //        openFlags: SQLiteOpenFlags.ReadWrite |
            //                   SQLiteOpenFlags.Create |
            //                   SQLiteOpenFlags.SharedCache |
            //                   SQLiteOpenFlags.FullMutex
            //        );
            //}
            if (_connection == null) {
                try {
                    //SQLitePCL.Batteries.Init();
                    _connection = new SQLiteConnection(connStr) { Trace = true };
                }catch(Exception ex) {
                    Console.WriteLine(ex);
                }
            }

            if (_connectionAsync == null) {
                _connectionAsync = new SQLiteAsyncConnection(connStr) { Trace = true };
            }            
        }

        private async Task InitDb() {
            var dbPath = _dbInfo.GetDbFilePath();
            
            //File.Delete(dbPath);

            bool isNewDb = !File.Exists(dbPath);

            if(isNewDb) {
                using (File.Create(dbPath));
            }
            CreateConnection();


            if (isNewDb) {
                foreach (var c in GetCreateString().Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries)) {
                    if (string.IsNullOrEmpty(c.Trim().Replace(Environment.NewLine, string.Empty))) {
                        continue;
                    }
                    ExecuteWrite(c + ";", null);
                }
            }

            if (UseWAL) {
                // On sqlite-net v1.6.0+, enabling write-ahead logging allows for faster database execution
                if (_connection != null) {
                    _connection.EnableWriteAheadLogging();
                }

                if (_connectionAsync != null) {
                    await _connectionAsync.EnableWriteAheadLoggingAsync().ConfigureAwait(false);
                }
            }

            await InitTables();
            
            if (isNewDb) {
                await InitDefaultData();
            }


            MpConsole.WriteLine(@"Db file located: " + dbPath);
            MpConsole.WriteLine(@"This Client Guid: " + MpPreferences.Instance.ThisDeviceGuid);

            MpConsole.WriteLine("Write ahead logging: " + (UseWAL ? "ENABLED" : "DISABLED"));
        }

        private async Task InitTables() {
            await Task.Run(() => {
                _connection.CreateTable<MpApp>();
                _connection.CreateTable<MpCopyItem>();
                _connection.CreateTable<MpCopyItemTag>();
                _connection.CreateTable<MpCopyItemTemplate>();
                _connection.CreateTable<MpDbImage>();
                _connection.CreateTable<MpDbLog>();
                _connection.CreateTable<MpIcon>();
                _connection.CreateTable<MpPasteHistory>();
                _connection.CreateTable<MpPasteToAppPath>();
                _connection.CreateTable<MpShortcut>();
                _connection.CreateTable<MpSource>();
                _connection.CreateTable<MpSyncHistory>();
                _connection.CreateTable<MpTag>();
                _connection.CreateTable<MpUrl>();
                _connection.CreateTable<MpUserDevice>();
            });

            await _connectionAsync.CreateTableAsync<MpApp>();
            await _connectionAsync.CreateTableAsync<MpCopyItem>();
            await _connectionAsync.CreateTableAsync<MpCopyItemTag>();
            await _connectionAsync.CreateTableAsync<MpCopyItemTemplate>();
            await _connectionAsync.CreateTableAsync<MpDbImage>();
            await _connectionAsync.CreateTableAsync<MpDbLog>();
            await _connectionAsync.CreateTableAsync<MpIcon>();
            await _connectionAsync.CreateTableAsync<MpPasteHistory>();
            await _connectionAsync.CreateTableAsync<MpPasteToAppPath>();
            await _connectionAsync.CreateTableAsync<MpShortcut>();
            await _connectionAsync.CreateTableAsync<MpSource>();
            await _connectionAsync.CreateTableAsync<MpSyncHistory>();
            await _connectionAsync.CreateTableAsync<MpTag>();
            await _connectionAsync.CreateTableAsync<MpUrl>();
            await _connectionAsync.CreateTableAsync<MpUserDevice>();
        }

        private async Task InitDefaultData() {
            MpPreferences.Instance.ThisDeviceGuid = Guid.NewGuid().ToString();

            var thisDevice = new MpUserDevice() {
                UserDeviceGuid = Guid.Parse(MpPreferences.Instance.ThisDeviceGuid),
                PlatformType = MpPreferences.Instance.ThisDeviceType
            };
            await AddItemAsync<MpUserDevice>(thisDevice);

            var process = Process.GetCurrentProcess();
            string appPath = process.MainModule.FileName;
            string appName = MpPreferences.Instance.ApplicationName;
            var iconStr = @"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";// new MpImageConverter().Convert(MpHelpers.Instance.LoadBitmapResource(@"MonkeyPaste.Resources.Icons.monkey.png"), typeof(string)) as string;
            var icon = await MpIcon.Create(iconStr);
            var app = await MpApp.Create(appPath, appName, icon);
            var source = await MpSource.Create(app, null);
            MpPreferences.Instance.ThisDeviceSourceId = source.Id;

            await AddItemAsync<MpTag>(new MpTag() {
                TagGuid = Guid.Parse("310ba30b-c541-4914-bd13-684a5e00a2d3"),
                TagName = "Recent",
                HexColor = Color.Green.ToHex(),
                TagSortIdx = 0
            }, "", true, true);
            await AddItemAsync<MpTag>(new MpTag() {
                TagGuid = Guid.Parse("df388ecd-f717-4905-a35c-a8491da9c0e3"),
                TagName = "All",
                HexColor = Color.Blue.ToHex(),
                TagSortIdx = 1
            }, "", true, true);

            await AddItemAsync<MpTag>(new MpTag() {
                TagGuid = Guid.Parse("54b61353-b031-4029-9bda-07f7ca55c123"),
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

            MpConsole.WriteTraceLine(@"Created all default tables");
        }

        private void NotifyRemoteUpdate(MpDbLogActionType actionType, object dbo, string sourceClientGuid) {
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

        private string GetCreateString() {
            return @"                    
                    CREATE TABLE MpSyncHistory (
                      pk_MpSyncHistoryId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , OtherClientGuid text not null
                    , SyncDateTime datetime not null
                    );
                    
                    CREATE TABLE MpDbLog (
                      pk_MpDbLogId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , DbObjectGuid text not null
                    , DbTableName text not null
                    , AffectedColumnName text not null
                    , AffectedColumnValue text not null default ''
                    , LogActionType integer default 0
                    , LogActionDateTime datetime not null
                    , SourceClientGuid text not null
                    );
                    
                    CREATE TABLE MpDbImage (
                      pk_MpDbImageId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpDbImageGuid text not null
                    , ImageBase64 text not null
                    );
                                        
                    CREATE TABLE MpTag (
                      pk_MpTagId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpTagGuid text not null
                    , fk_ParentTagId integer default 0
                    , TagName text
                    , SortIdx integer
                    , HexColor text not null default '#FFFF0000'
                    );
                    
                    CREATE TABLE MpIcon (
                      pk_MpIconId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpIconGuid text not null
                    , fk_IconDbImageId integer not null
                    , fk_IconBorderDbImageId integer 
                    , fk_IconSelectedHighlightBorderDbImageId integer
                    , fk_IconHighlightBorderDbImageId integer 
                    , HexColor1 text 
                    , HexColor2 text 
                    , HexColor3 text 
                    , HexColor4 text
                    , HexColor5 text);                                       
                    
                    
                    CREATE TABLE MpPasteToAppPath (
                      pk_MpPasteToAppPathId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpPasteToAppPathGuid text not null
                    , AppPath text NOT NULL
                    , AppName text default ''
                    , Args text default ''
                    , Label text default ''
                    , fk_MpDbImageId integer 
                    , WindowState integer default 1
                    , IsSilent integer NOT NULL default 0
                    , IsAdmin integer NOT NULL default 0
                    , PressEnter integer NOT NULL default 0
                    );
                    INSERT INTO MpPasteToAppPath(AppName,MpPasteToAppPathGuid,AppPath,IsAdmin) VALUES ('Command Prompt','0b9d1b30-abce-4407-b745-95f9cde57135','%windir%\System32\cmd.exe',0);
                    
                    CREATE TABLE MpUserDevice (
                      pk_MpUserDeviceId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpUserDeviceGuid text not null
                    , PlatformTypeId integer NOT NULL
                    );
                    
                    CREATE TABLE MpApp (
                      pk_MpAppId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpAppGuid text not null
                    , SourcePath text NOT NULL 
                    , AppName text 
                    , IsAppRejected integer NOT NULL   
                    , fk_MpUserDeviceId integer not null
                    , fk_MpIconId integer);   
                    
                    CREATE TABLE MpUrlDomain (
                      pk_MpUrlDomainId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpUrlDomainGuid text not null
                    , UrlDomainPath text NOT NULL 
                    , UrlDomainTitle text
                    , IsUrlDomainRejected integer NOT NULL DEFAULT 0   
                    , fk_MpIconId integer);  
                    
                    CREATE TABLE MpUrl (
                      pk_MpUrlId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpUrlGuid text not null
                    , UrlPath text NOT NULL 
                    , UrlTitle text
                    , fk_MpUrlDomainId int 
                    ); 
                    
                    CREATE TABLE MpSource (
                      pk_MpSourceId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpSourceGuid text not null
                    , fk_MpUrlId integer default 0
                    , fk_MpAppId integer NOT NULL
                    ); 

                    CREATE TABLE MpCopyItem (
                      pk_MpCopyItemId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpCopyItemGuid text not null
                    , fk_ParentCopyItemId integer default 0
                    , fk_MpCopyItemTypeId integer NOT NULL default 0
                    , fk_MpSourceId integer NOT NULL
                    , CompositeSortOrderIdx integer default 0
                    , HexColor text 
                    , Title text NULL default ''
                    , CopyCount integer not null default 1
                    , PasteCount integer not null default 0
                    , fk_MpDbImageId integer
                    , fk_SsMpDbImageId integer
                    , ItemData text default ''
                    , ItemDescription text default ''
                    , CopyDateTime datetime DEFAULT (current_timestamp) NOT NULL  
                    , ModifiedDateTime datetime DEFAULT (current_timestamp) NOT NULL  
                    );
                    
                    CREATE TABLE MpCopyItemContent (
                      pk_MpCopyItemContentId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpCopyItemContentGuid text not null
                    , fk_MpCopyItemId integer not null
                    , ContentText text
                    , ContentTypeId integer default 0
                    , CONSTRAINT FK_MpCopyItemContent_0_0 FOREIGN KEY (fk_MpCopyItemId) REFERENCES MpCopyItem (pk_MpCopyItemId)   
                    );

                    CREATE TABLE MpCopyItemTag (
                      pk_MpCopyItemTagId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpCopyItemTagGuid text not null
                    , fk_MpCopyItemId integer NOT NULL
                    , fk_MpTagId integer NOT NULL
                    , CopyItemSortIdx integer default 0
                    );

                    CREATE TABLE MpShortcut (
                      pk_MpShortcutId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpShortcutGuid text not null
                    , fk_MpCopyItemId INTEGER DEFAULT 0
                    , fk_MpTagId INTEGER DEFAULT 0
                    , ShortcutName text NOT NULL                    
                    , KeyString text NULL       
                    , DefaultKeyString text NULL
                    , RoutingType integer NOT NULL DEFAULT 0 
                    );
                    INSERT INTO MpShortcut(MpShortcutGuid,ShortcutName,RoutingType,KeyString,DefaultKeyString) VALUES
                    ('5dff238e-770e-4665-93f5-419e48326f01','Show Window',2,'Control+Shift+D','Control+Shift+D')
                    ,('cb807500-9121-4e41-80d3-8c3682ce90d9','Hide Window',1,'Escape','Escape')
                    ,('a41aeed8-d4f3-47de-86c5-f9ca296fb103','Append Mode',2,'Control+Shift+A','Control+Shift+A')
                    ,('892bf7d7-ba8e-4db1-b2ca-62b41ff6614c','Auto-Copy Mode',2,'Control+Shift+C','Control+Shift+C')
                    ,('a12c4211-ab1f-4b97-98ff-fbeb514e9a1c','Right-Click Paste Mode',2,'Control+Shift+R','Control+Shift+R')
                    ,('1d212ca5-fb2a-4962-8f58-24ed9a5d007d','Paste Selected Clip',1,'Enter','Enter')
                    ,('e94ca4f3-4c6e-40dc-8941-c476a81543c7','Delete Selected Clip',1,'Delete','Delete')
                    ,('7fe24929-6c9e-49c0-a880-2f49780dfb3a','Select Next',1,'Right','Right')
                    ,('ee657845-f1dc-40cf-848d-6768c0081670','Select Previous',1,'Left','Left')
                    ,('5480f103-eabd-4e40-983c-ebae81645a10','Select All',1,'Control+A','Control+A')
                    ,('39a6b8b5-a585-455b-af83-015fd97ac3fa','Invert Selection',1,'Control+Shift+Alt+A','Control+Shift+Alt+A')
                    ,('166abd7e-7295-47f2-bbae-c96c03aa6082','Bring to front',1,'Control+Home','Control+Home')
                    ,('84c11b86-3acc-4d22-b8e9-3bd785446f72','Send to back',1,'Control+End','Control+End')
                    ,('6487f6ff-da0c-475b-a2ae-ef1484233de0','Assign Hotkey',1,'Control+Shift+H','Control+Shift+H')
                    ,('837e0c20-04b8-4211-ada0-3b4236da0821','Change Color',1,'Control+Shift+Alt+C','Control+Shift+Alt+C')
                    ,('4a567aff-33a8-4a1f-8484-038196812849','Say',1,'Control+Shift+S','Control+Shift+S')
                    ,('330afa20-25c3-425c-8e18-f1423eda9066','Merge',1,'Control+Shift+M','Control+Shift+M')
                    ,('118a2ca6-7021-47a0-8458-7ebc31094329','Undo',1,'Control+Z','Control+Z')
                    ,('3980efcc-933b-423f-9cad-09e455c6824a','Redo',1,'Control+Y','Control+Y')
                    ,('7a7580d1-4129-432d-a623-2fff0dc21408','Edit',1,'Control+E','Control+E')
                    ,('085338fb-f297-497a-abb7-eeb7310dc6f3','Rename',1,'F2','F2')
                    ,('e22faafd-4313-441a-b361-16910fc7e9d3','Duplicate',1,'Control+D','Control+D')
                    ,('4906a01e-b2f7-43f0-af1e-fb99d55c9778','Email',1,'Control+E','Control+E')
                    ,('c7248087-2031-406d-b4ab-a9007fbd4bc4','Qr Code',1,'Control+Shift+Q','Control+Shift+Q')
                    ,('777367e6-c161-4e93-93e0-9bf12221f7ff','Toggle Auto-Analyze Mode',2,'Control+Shift+B','Control+Shift+B')
                    ,('97e29b06-0ec4-4c55-a393-8442d7695038','Toggle Is App Paused',2,'Control+Shift+P','Control+Shift+P')
                    ,('ee74dd92-d18b-46cf-91b7-3946ab55427c','Copy Selection',1,'Control+C','Control+C');
                    
                    CREATE TABLE MpDetectedImageObject (
                      pk_MpDetectedImageObjectId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpDetectedImageObjectGuid text not null
                    , fk_MpCopyItemId integer NOT NULL
                    , Confidence real NOT NULL
                    , X real NOT NULL
                    , Y real NOT NULL
                    , Width real NOT NULL
                    , Height real NOT NULL                    
                    , ObjectTypeName text
                    );
                    
                    CREATE TABLE MpCopyItemTemplate (
                      pk_MpCopyItemTemplateId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpCopyItemTemplateGuid text not null
                    , fk_MpCopyItemId integer NOT NULL
                    , HexColor text default '#0000FF'
                    , TemplateName text NOT NULL 
                    );       
                    
                    CREATE TABLE MpPasteHistory (
                      pk_MpPasteHistoryId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpPasteHistoryGuid text not null
                    , fk_MpCopyItemId integer NOT NULL
                    , fk_MpUserDeviceId integer NOT NULL
                    , fk_MpAppId integer default 0                    
                    , fk_MpUrlId integer default 0
                    , PasteDateTime datetime NOT NULL
                    );
                                        
            ";
        }
        #endregion

        #region Sync Data
        public bool IsWpf() {
            return false;
        }

        public bool IsConnectedToNetwork() {
            return MpHelpers.Instance.IsConnectedToNetwork();
        }

        public bool IsConnectedToInternet() {
            return MpHelpers.Instance.IsConnectedToInternet();
        }
        public int GetSyncPort() {
            return 44381;
        }
        public string GetThisClientGuid() {
            return MpPreferences.Instance.ThisDeviceGuid;
        }
        public string GetPrimaryLocalIp4Address() {
            if (!IsConnectedToNetwork()) {
                return "0.0.0.0";
            }
            return MpHelpers.Instance.GetLocalIp4Address();
        }

        public string[] GetAllLocalIp4Addresses() {
            if (!IsConnectedToNetwork()) {
                return new string[] { "0.0.0.0" };
            }
            return MpHelpers.Instance.GetAllLocalIPv4();
        }

        public string GetExternalIp4Address() {
            if (!IsConnectedToInternet()) {
                return "0.0.0.0";
            }
            return MpHelpers.Instance.GetExternalIp4Address();
        }

        public async Task<List<MonkeyPaste.MpDbLog>> GetDbObjectLogs(string dboGuid, DateTime fromDtUtc) {
            var logs = await MpDbLog.GetDbLogsByGuidAsync(dboGuid, fromDtUtc);
            return logs;
        }

        public async Task<DateTime> GetLastSyncForRemoteDevice(string otherDeviceGuid) {
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

        public async Task<string> GetLocalLogFromSyncDate(DateTime fromDateTime, string ignoreGuid = "") {
            var logItems = await MpDb.Instance.GetItemsAsync<MpDbLog>();
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

        public async Task<Dictionary<Guid, List<MpDbLog>>> PrepareRemoteLogForSyncing(string dbLogMessageStr) {
            var dbLogMessage = MpDbMessage.Parse(dbLogMessageStr, GetTypeConverter());

            var remoteDbLogs = new List<MpDbLog>();
            var dbLogWorker = new MpDbLog();

            //deserialize logs and put into guid buckets
            var remoteItemChangeLookup = new Dictionary<Guid, List<MpDbLog>>();
            foreach (var remoteLogRow in dbLogMessage.DbObjects) {
                var logItem = await dbLogWorker.DeserializeDbObject(remoteLogRow.ObjStr) as MpDbLog;
                if (remoteItemChangeLookup.ContainsKey(logItem.DbObjectGuid)) {
                    remoteItemChangeLookup[logItem.DbObjectGuid].Add(logItem);
                } else {
                    remoteItemChangeLookup.Add(logItem.DbObjectGuid, new List<MpDbLog>() { logItem });
                }
            }

            return remoteItemChangeLookup;
        }

        public async Task PerformSync(
            Dictionary<Guid, List<MpDbLog>> changeLookup,
            string remoteClientGuid) {            
            var lastSyncDt = await MpDb.Instance.GetLastSyncForRemoteDevice(remoteClientGuid);
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
            //    var result = await MpDb.Instance.GetDbObjectByTableGuidAsync(uc.Value[0].DbTableName, uc.Key.ToString());
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
                var dbo = await MpDb.Instance.GetDbObjectByTableGuidAsync(ckvp.Value[0].DbTableName, ckvp.Key.ToString());
                //var dbo = MpDbModelBase.CreateOrUpdateFromLogs(ckvp.Value, remoteClientGuid);
                var deleteTask = (Task)deleteByDboTypeMethod.Invoke(MpDb.Instance, new object[] { dbo,remoteClientGuid,false,true });
                await deleteTask;
            }

            MpConsole.WriteLine(@"Adds: ");
            foreach (var ckvp in addChanges) {
                foreach (var dbl in ckvp.Value) {
                    dbl.PrintLog();
                }
                var dbot = new MpXamStringToSyncObjectTypeConverter().Convert(ckvp.Value[0].DbTableName);
                var dbo = Activator.CreateInstance(dbot);
                dbo = await (dbo as MpISyncableDbObject).CreateFromLogs(ckvp.Key.ToString(), ckvp.Value, remoteClientGuid);
                //var dbo = MpDbModelBase.CreateOrUpdateFromLogs(ckvp.Value, remoteClientGuid);
                var addMethod = typeof(MpDb).GetMethod(nameof(AddOrUpdateAsync));
                var addByDboTypeMethod = addMethod.MakeGenericMethod(new[] { dbot });
                var addTask = (Task)addByDboTypeMethod.Invoke(MpDb.Instance, new object[] { dbo,remoteClientGuid,false,true });
                await addTask;
            }

            MpConsole.WriteLine(@"Updates: ");
            foreach (var ckvp in updateChanges) {
                foreach (var dbl in ckvp.Value) {
                    dbl.PrintLog();
                }
                var dbot = new MpXamStringToSyncObjectTypeConverter().Convert(ckvp.Value[0].DbTableName);
                var dbo = Activator.CreateInstance(dbot);
                dbo = await (dbo as MpISyncableDbObject).CreateFromLogs(ckvp.Key.ToString(), ckvp.Value, remoteClientGuid);                
                //var dbo = MpDbModelBase.CreateOrUpdateFromLogs(ckvp.Value, remoteClientGuid);
                var updateMethod = typeof(MpDb).GetMethod(nameof(AddOrUpdateAsync));
                var updateByDboTypeMethod = updateMethod.MakeGenericMethod(new[] { dbot });
                var updateTask = (Task)updateByDboTypeMethod.Invoke(MpDb.Instance, new object[] { dbo,remoteClientGuid,false,true });
                await updateTask;
            }

            return;
        }

        public void UpdateSyncHistory(string otherDeviceGuid, DateTime utcDtSentLocalChanges) {
            Task.Run(async () => {
                MpSyncHistory sh = MpSyncHistory.GetSyncHistoryByDeviceGuid(otherDeviceGuid);

                if (sh == null) {
                    sh = new MpSyncHistory() {
                        OtherClientGuid = otherDeviceGuid,
                        SyncDateTime = utcDtSentLocalChanges
                    };
                } else {
                    sh.SyncDateTime = utcDtSentLocalChanges;
                }

                await MpDb.Instance.AddOrUpdateAsync<MpSyncHistory>(sh);
            });
        }

        private Dictionary<Guid,List<MpDbLog>> OrderByPrecedence(Dictionary<Guid,List<MpDbLog>> dict) {
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

        private int GetDbTableOrder(MpDbLog log) {
            var orderedLogs = MpSyncManager.Instance.DbTableSyncOrder.ToList();
            var idx = orderedLogs.IndexOf(log.DbTableName);
            if (idx < 0) {
                throw new Exception(@"Unknown dblog table type: " + log.DbTableName);
            }
            return idx;
        }

        public object GetMainThreadObj() {
            return Application.Current.MainPage;
        }

        public MpIStringToSyncObjectTypeConverter GetTypeConverter() {
            return new MpXamStringToSyncObjectTypeConverter();
        }

        public string GetDbFileAsBase64() {
            var bytes = File.ReadAllBytes(_dbInfo.GetDbFilePath());
            return Convert.ToBase64String(bytes);
        }

        public ObservableCollection<MpRemoteDevice> GetRemoteDevices() {
            _rdLock = new object();
            var rdoc = new ObservableCollection<MpRemoteDevice>();
            Xamarin.Forms.BindingBase.EnableCollectionSynchronization(rdoc, null, ObservableCollectionCallback);
            return rdoc;
        }

        private void ObservableCollectionCallback(IEnumerable collection, object context, Action accessMethod, bool writeAccess) {
            // `lock` ensures that only one thread access the collection at a time
            lock (collection) {
                accessMethod?.Invoke();
            }
        }
        #endregion
    }

    public class MpDbLogTableComparer : IComparer<MpDbLog> {
        public int Compare(MpDbLog a, MpDbLog b) {
            return GetVal(a).CompareTo(GetVal(b));
        }
        private int GetVal(MpDbLog log) {
            var orderedLogs = new List<string>() {
                          "MpColor",
                          "MpDbImage",
                          "MpIcon",
                          "MpUrl",
                          "MpUrlDomain",
                          "MpApp",
                          "MpSource",
                          "MpCopyItem",
                          "MpTag",
                          "MpCompositeCopyItem",
                          "MpCopyItemTag",
                          "MpCopyItemTemplate",
                          "MpUserDevice" };
            var idx = orderedLogs.IndexOf(log.DbTableName);
            if(idx < 0) {
                throw new Exception(@"Unknown dblog table type: " + log.DbTableName);
            }
            return idx;
        }
    }

    public class DataTable {
        public List<DataRow> Rows { get; set; } = new List<DataRow>();
    }

    public class DataRow{
        private Dictionary<string, object> _columns = new Dictionary<string, object>();

        #region Property Reflection Referencer
        public object this[string colName] {
            get {
                if(!_columns.ContainsKey(colName)) {
                    if (colName.StartsWith("pk_") || colName.StartsWith("fk_") || colName.Contains("Id")) {
                        return 0;
                    }
                    return null;
                    //throw new Exception("Unable to find property: " + colName);
                }
                return _columns[colName];
            }
            set {
                if (!_columns.ContainsKey(colName)) {
                    throw new Exception("Unable to find property: " + colName);
                }
                _columns[colName] = value;
            }
        }

        public object this[int idx] {
            get {
                if (idx >= _columns.Count) {
                    throw new Exception("Index out of bounds: "+idx);
                }
                return _columns.ToArray()[idx].Value;
            }
        }
        #endregion        

        public void AddColumn(string colName,object value = null) {
            if (_columns.ContainsKey(colName)) {
                _columns[colName] = value;
            } else {
                _columns.Add(colName, value);
            }
        }
    }
}
