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

        #region Wpf compatability

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

            if (_connection == null) {
                try {
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
                _connection.CreateTable<MpAnalyticItem>();
                _connection.CreateTable<MpAnalyticItemParameter>();
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

            await _connectionAsync.CreateTableAsync<MpAnalyticItem>();
            await _connectionAsync.CreateTableAsync<MpAnalyticItemParameter>();
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
            var icon = await MpIcon.Create(MpBase64Images.Instance.AppIcon);
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
                    , CopyItemSortIdx integer default -1
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

                    CREATE TABLE MpAnalyticItem (
                      pk_MpAnalyticItemId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpAnalyticItemGuid text not null
                    , fk_MpIconId integer                    
                    , InputFormatTypeId integer NOT NULL default 0
                    , Title text NOT NULL 
                    , Description text
                    , ApiKey text 
                    , EndPoint text);   

                    CREATE TABLE MpAnalyticItemParameter (
                      pk_MpAnalyticItemParameterId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpAnalyticItemParameterGuid text not null
                    , fk_MpAnalyticItemId integer not null
                    , ParameterTypeId integer not null default 0
                    , Label text
                    , SortOrderIdx integer,
                    , IsRequired integer not null default 0
                    , IsReadOnly integer not null default 0
                    , FormatInfo text); 

                    CREATE TABLE MpAnalyticItemParameterValue (
                      pk_MpAnalyticItemParameterValueId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpAnalyticItemParameterValueGuid text not null
                    , fk_MpAnalyticItemParameterId integer not null
                    , ParameterValueTypeId integer not null default 0
                    , Value text
                    , Label text
                    , IsDefault integer not null default 0   
                    , IsMinimum integer not null default 0
                    , IsMaximum integer not null default 0); 
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
