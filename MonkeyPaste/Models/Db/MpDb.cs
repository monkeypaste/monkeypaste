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
        #endregion

        #region Public Methods
        public void Init(MpIDbInfo dbInfo) {
            _dbInfo = dbInfo;
            InitDb();            
            IsLoaded = true;            
        }

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
                InitDb();
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
                InitDb();
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

        public async Task<List<T>> GetItemsAsync<T>() where T : new() {
            if (_connectionAsync == null) {
                InitDb();
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
                InitDb();
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

        public async Task AddItemAsync<T>(T item, string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {
            sourceClientGuid = string.IsNullOrEmpty(sourceClientGuid) ? MpPreferences.Instance.ThisDeviceGuid : sourceClientGuid;
            if (_connectionAsync == null) {
                InitDb();
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
            await _connectionAsync.InsertWithChildrenAsync(item, recursive: true);
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
            _connection.InsertWithChildren(item,true);
            OnItemAdded?.Invoke(this, item as MpDbModelBase);
            if (!ignoreSyncing && item is MpISyncableDbObject) {
                OnSyncableChange?.Invoke(item, (item as MpDbModelBase).Guid);
            }
        }

        public async Task UpdateItemAsync<T>(T item,string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {
            sourceClientGuid = string.IsNullOrEmpty(sourceClientGuid) ? MpPreferences.Instance.ThisDeviceGuid : sourceClientGuid;
            if (_connectionAsync == null) {
                InitDb();
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
                InitDb();
            }
            if (item == null) {
                MpConsole.WriteTraceLine(@"Cannot delete null item, ignoring...");
                return;
            }
            if (item is MpISyncableDbObject && item is not MpDbLog && item is not MpSyncHistory) {
                if (!ignoreTracking) {
                    await MpDbLogTracker.TrackDbWriteAsync(MpDbLogActionType.Delete, item as MpDbModelBase, sourceClientGuid);
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
                int tableNameLength = tableNameEndIdx > 0 ?
                    query.Substring(tableNameStartIdx).Length - tableNameEndIdx :
                    query.Length - tableNameStartIdx;
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
            if (string.IsNullOrEmpty(query.Trim())) {
                return null;
            }

            string newQuery = query;
            object[] newArgs = null;
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
        public MpDataTable Execute(string query, Dictionary<string, object> args) {
            if(_connection == null) {
                CreateConnection();
            }

            string tn = GetTableName(query);
            var dbot = new MpXamStringToSyncObjectTypeConverter().Convert(tn);

            var tuple = PrepareQuery(query, args);

            var queryMethod = _connection.GetType().GetMethod("Query");
            var queryByDboTypeMethod = queryMethod.MakeGenericMethod(new[] { dbot });
            var result = queryByDboTypeMethod.Invoke(_connection, new object[] { tuple.Item1, tuple.Item2 }) as IList;

            var dt = new MpDataTable();

            foreach(var row in result) {
                var dr = new MpDataRow();
                foreach (var rowProp in row.GetType().GetProperties()) {
                    dr.AddColumn(rowProp.Name, rowProp.GetValue(row));
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }

        public int ExecuteWrite(string query, Dictionary<string, object> args, string dbObjectGuid = "", string sourceClientGuid = "", object dbObject = null, bool ignoreTracking = false, bool ignoreSyncing = false) {
            if (_connection == null) {
                CreateConnection();
            }

            var tuple = PrepareQuery(query, args);

            if(tuple.Item2 == null) {
                return _connection.Execute(tuple.Item1);
            }
            return _connection.Execute(tuple.Item1, tuple.Item2);
        }
        public int GetLastRowId(string tableName, string pkName) {
            if (_connection == null) {
                CreateConnection();
            }

            MpDataTable dt = Execute("select * from " + tableName + " ORDER BY " + pkName + " DESC LIMIT 1;", null);
            if (dt.Rows.Count > 0) {
                return Convert.ToInt32(dt.Rows[0][0].ToString());
            }
            return -1;
        }
        #endregion

        #endregion

        #region Private Methods  
        private void CreateConnection() {

            SQLiteConnectionString connStr = null;

            if(string.IsNullOrEmpty(_dbInfo.GetDbPassword())) {
                connStr = new SQLiteConnectionString(
                    databasePath: _dbInfo.GetDbFilePath(),
                    storeDateTimeAsTicks: false,
                    openFlags: SQLiteOpenFlags.ReadWrite |
                               SQLiteOpenFlags.Create |
                               SQLiteOpenFlags.SharedCache |
                               SQLiteOpenFlags.FullMutex
                    );
            } else {
                connStr = new SQLiteConnectionString(
                    databasePath: _dbInfo.GetDbPassword(),
                    storeDateTimeAsTicks: false,
                    key: MpPreferences.Instance.DbPassword,
                    openFlags: SQLiteOpenFlags.ReadWrite |
                               SQLiteOpenFlags.Create |
                               SQLiteOpenFlags.SharedCache |
                               SQLiteOpenFlags.FullMutex
                    );
            }
            if(_connection == null) {
                _connection = new SQLiteConnection(connStr) { Trace = true };
            }
            
            if(_connectionAsync == null) {
                _connectionAsync = new SQLiteAsyncConnection(connStr) { Trace = true };
            }
            
        }

        private void InitDb() {
            var dbPath = _dbInfo.GetDbFilePath();
            
            File.Delete(dbPath);

            bool isNewDb = !File.Exists(dbPath);

            CreateConnection();

            if(isNewDb) {
                foreach (var c in GetCreateString().Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries)) {
                    if (string.IsNullOrEmpty(c.Trim().Replace(Environment.NewLine, string.Empty))) {
                        continue;
                    }
                    ExecuteWrite(c + ";", null);
                }
            }


            if (UseWAL) {
                // On sqlite-net v1.6.0+, enabling write-ahead logging allows for faster database execution
                if(_connection != null) {
                    _connection.EnableWriteAheadLogging();
                }

                if (_connectionAsync != null) {
                    Task.Run(async () => {
                        await _connectionAsync.EnableWriteAheadLoggingAsync().ConfigureAwait(false);
                    });
                }
            }
            InitTables();

            if(isNewDb) {
                InitDefaultData();
            }

            //if (isNewDb) {
            //    await InitDefaultDataAsync();
            //}

            //if (_connectionAsync != null && UseWAL) {
            //    // On sqlite-net v1.6.0+, enabling write-ahead logging allows for faster database execution
            //    await _connectionAsync.EnableWriteAheadLoggingAsync().ConfigureAwait(false);
            //}

            MpConsole.WriteLine(@"Db file located: " + dbPath);
            MpConsole.WriteLine(@"This Client Guid: " + MpPreferences.Instance.ThisDeviceGuid);

            MpConsole.WriteLine("Write ahead logging: " + (UseWAL ? "ENABLED" : "DISABLED"));
        }

        private void InitTables() {
            _connection.CreateTable<MpApp>();
            _connection.CreateTable<MpUserDevice>();
            _connection.CreateTable<MpCopyItem>();
            _connection.CreateTable<MpCopyItemTag>();
            _connection.CreateTable<MpCopyItemTemplate>();
            _connection.CreateTable<MpDbImage>();
            _connection.CreateTable<MpIcon>();
            _connection.CreateTable<MpPasteHistory>();
            _connection.CreateTable<MpSource>();
            _connection.CreateTable<MpTag>();
            _connection.CreateTable<MpUrl>();
            _connection.CreateTable<MpUrlDomain>();
            _connection.CreateTable<MpDbLog>();
            _connection.CreateTable<MpSyncHistory>();
        }

        private async Task InitTablesAsync() {
            await _connectionAsync.CreateTableAsync<MpApp>();
            await _connectionAsync.CreateTableAsync<MpUserDevice>();
            await _connectionAsync.CreateTableAsync<MpCopyItem>();
            await _connectionAsync.CreateTableAsync<MpCopyItemTag>();
            await _connectionAsync.CreateTableAsync<MpCopyItemTemplate>();
            await _connectionAsync.CreateTableAsync<MpDbImage>();
            await _connectionAsync.CreateTableAsync<MpIcon>();
            await _connectionAsync.CreateTableAsync<MpPasteHistory>();
            await _connectionAsync.CreateTableAsync<MpSource>();
            await _connectionAsync.CreateTableAsync<MpTag>();
            await _connectionAsync.CreateTableAsync<MpUrl>();
            await _connectionAsync.CreateTableAsync<MpUrlDomain>();
            await _connectionAsync.CreateTableAsync<MpDbLog>();
            await _connectionAsync.CreateTableAsync<MpSyncHistory>();
        }

        private void InitDefaultData() {
            if(string.IsNullOrEmpty(MpPreferences.Instance.ThisDeviceGuid)) {
                MpPreferences.Instance.ThisDeviceGuid = System.Guid.NewGuid().ToString();
            }
            if(MpUserDevice.GetUserDeviceByGuid(MpPreferences.Instance.ThisDeviceGuid) == null) {
                var thisDevice = new MpUserDevice() {
                    UserDeviceGuid = Guid.Parse(MpPreferences.Instance.ThisDeviceGuid),
                    PlatformType = MpPreferences.Instance.ThisDeviceType
                };
                AddItem<MpUserDevice>(thisDevice);
            }
            AddItem<MpTag>(new MpTag() {
                TagGuid = Guid.Parse("310ba30b-c541-4914-bd13-684a5e00a2d3"),
                TagName = "Recent",
                HexColor = Color.Green.ToHex(),
                TagSortIdx = 0
            }, "", true, true);
            AddItem<MpTag>(new MpTag() {
                TagGuid = Guid.Parse("df388ecd-f717-4905-a35c-a8491da9c0e3"),
                TagName = "All",
                HexColor = Color.Blue.ToHex(),
                TagSortIdx = 1
            }, "", true, true);

            AddItem<MpTag>(new MpTag() {
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
            AddItem<MpTag>(helpTag, "", true, true);

            MpConsole.WriteTraceLine(@"Create all default tables");
        }


        private string GetCreateString() {
            return @"                    
                    CREATE TABLE MpSyncHistory (
                      pk_MpSyncHistoryId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , OtherClientGuid text
                    , SyncDateTime datetime
                    );
                    
                    CREATE TABLE MpDbLog (
                      pk_MpDbLogId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , DbObjectGuid text
                    , DbTableName text
                    , AffectedColumnName text
                    , AffectedColumnValue text
                    , LogActionType integer default 0
                    , LogActionDateTime datetime
                    , SourceClientGuid text
                    );
                    
                    CREATE TABLE MpDbImage (
                      pk_MpDbImageId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpDbImageGuid text
                    , ImageBase64 text
                    );
                                        
                    CREATE TABLE MpTag (
                      pk_MpTagId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpTagGuid text
                    , fk_ParentTagId integer default 0
                    , TagName text
                    , SortIdx integer
                    , HexColor text
                    );
                    
                    CREATE TABLE MpIcon (
                      pk_MpIconId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpIconGuid text
                    , fk_IconDbImageId integer
                    , fk_IconBorderDbImageId integer
                    , fk_IconSelectedHighlightBorderDbImageId integer
                    , fk_IconHighlightBorderDbImageId integer
                    , HexColor1 text '#FFFF0000'
                    , HexColor2 text '#FFFF0000'
                    , HexColor3 text '#FFFF0000'
                    , HexColor4 text '#FFFF0000'
                    , HexColor5 text '#FFFF0000'
                    , CONSTRAINT FK_MpIcon_0_0 FOREIGN KEY (fk_IconDbImageId) REFERENCES MpDbImage (pk_MpDbImageId)   
                    , CONSTRAINT FK_MpIcon_1_0 FOREIGN KEY (fk_IconBorderDbImageId) REFERENCES MpDbImage (pk_MpDbImageId)                       
                    , CONSTRAINT FK_MpIcon_0_0 FOREIGN KEY (fk_IconSelectedHighlightBorderDbImageId) REFERENCES MpDbImage (pk_MpDbImageId)   
                    , CONSTRAINT FK_MpIcon_1_0 FOREIGN KEY (fk_IconHighlightBorderDbImageId) REFERENCES MpDbImage (pk_MpDbImageId)   
                    );
                                        
                    
                    
                    CREATE TABLE MpPasteToAppPath (
                      pk_MpPasteToAppPathId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , AppPath text NOT NULL
                    , AppName text
                    , Args text
                    , Label text
                    , fk_MpDbImageId integer 
                    , WindowState integer default 1
                    , IsSilent integer NOT NULL default 0
                    , IsAdmin integer NOT NULL default 0
                    , PressEnter integer NOT NULL default 0
                    , CONSTRAINT FK_MpPasteToAppPath_0_0 FOREIGN KEY (fk_MpDbImageId) REFERENCES MpDbImage (pk_MpDbImageId)                    
                    );
                    INSERT INTO MpPasteToAppPath(AppPath,IsAdmin) VALUES ('%windir%\System32\cmd.exe',0);
                    
                    CREATE TABLE MpUserDevice (
                      pk_MpUserDeviceId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpUserDeviceGuid text 
                    , PlatformTypeId integer NOT NULL
                    );
                    
                    CREATE TABLE MpApp (
                      pk_MpAppId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpAppGuid text
                    , SourcePath text NOT NULL 
                    , AppName text 
                    , IsAppRejected integer NOT NULL   
                    , fk_MpUserDeviceId integer
                    , fk_MpIconId integer
                    , CONSTRAINT FK_MpApp_0_0 FOREIGN KEY (fk_MpUserDeviceId) REFERENCES MpUserDevice (pk_MpUserDeviceId)
                    , CONSTRAINT FK_MpApp_1_0 FOREIGN KEY (fk_MpIconId) REFERENCES MpIcon (pk_MpIconId)
                    );   
                    
                    CREATE TABLE MpUrlDomain (
                      pk_MpUrlDomainId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpUrlDomainGuid text
                    , UrlDomainPath text NOT NULL 
                    , UrlDomainTitle text
                    , IsUrlDomainRejected integer NOT NULL DEFAULT 0   
                    , fk_MpIconId integer
                    , CONSTRAINT FK_MpUrlDomain_0_0 FOREIGN KEY (fk_MpIconId) REFERENCES MpIcon (pk_MpIconId)
                    );  
                    
                    CREATE TABLE MpUrl (
                      pk_MpUrlId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpUrlGuid text
                    , UrlPath text NOT NULL 
                    , UrlTitle text
                    , fk_MpUrlDomainId int NOT NULL
                    , CONSTRAINT FK_MpUrl_0_0 FOREIGN KEY (fk_MpUrlDomainId) REFERENCES MpUrlDomain (pk_MpUrlDomainId)
                    ); 
                    
                    CREATE TABLE MpSource (
                      pk_MpSourceId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpSourceGuid text
                    , fk_MpUrlId integer
                    , fk_MpAppId integer NOT NULL
                    , CONSTRAINT FK_MpUrl_0_0 FOREIGN KEY (fk_MpUrlId) REFERENCES MpUrl (pk_MpUrlId)
                    , CONSTRAINT FK_MpApp_1_0 FOREIGN KEY (fk_MpAppId) REFERENCES MpApp (pk_MpAppId)
                    ); 
                    
                    CREATE TABLE MpCopyItem (
                      pk_MpCopyItemId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpCopyItemGuid text
                    , fk_ParentCopyItemId integer
                    , fk_MpCopyItemTypeId integer NOT NULL default 0
                    , fk_MpAppId integer NOT NULL
                    , fk_MpUrlId integer
                    , CompositeSortOrderIdx integer default 0
                    , HexColor text
                    , Title text NULL 
                    , CopyCount integer not null default 1
                    , PasteCount integer not null default 0
                    , fk_MpDbImageId integer
                    , fk_SsMpDbImageId integer
                    , ItemText text 
                    , ItemRtf text 
                    , ItemHtml text 
                    , ItemDescription text
                    , ItemCsv text
                    , CopyDateTime datetime DEFAULT (current_timestamp) NOT NULL    
                    , CONSTRAINT FK_MpCopyItem_0_0 FOREIGN KEY (fk_MpAppId) REFERENCES MpApp (pk_MpAppId)   
                    );
                    
                    CREATE TABLE MpCopyItemTag (
                      pk_MpCopyItemTagId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpCopyItemTagGuid text
                    , fk_MpCopyItemId integer NOT NULL
                    , fk_MpTagId integer NOT NULL
                    , CONSTRAINT FK_MpCopyItemTag_0_0 FOREIGN KEY (fk_MpCopyItemId) REFERENCES MpCopyItem (pk_MpCopyItemId)
                    , CONSTRAINT FK_MpCopyItemTag_1_0 FOREIGN KEY (fk_MpTagId) REFERENCES MpTag (pk_MpTagId)
                    );

                    CREATE TABLE MpShortcut (
                      pk_MpShortcutId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpCopyItemId INTEGER DEFAULT 0
                    , fk_MpTagId INTEGER DEFAULT 0
                    , ShortcutName text NOT NULL                    
                    , KeyString text NULL       
                    , DefaultKeyString text NULL
                    , RoutingType integer NOT NULL DEFAULT 0 
                    );
                    INSERT INTO MpShortcut(ShortcutName,RoutingType,KeyString,DefaultKeyString) VALUES
                    ('Show Window',2,'Control+Shift+D','Control+Shift+D')
                    ,('Hide Window',1,'Escape','Escape')
                    ,('Append Mode',2,'Control+Shift+A','Control+Shift+A')
                    ,('Auto-Copy Mode',2,'Control+Shift+C','Control+Shift+C')
                    ,('Right-Click Paste Mode',2,'Control+Shift+R','Control+Shift+R')
                    ,('Paste Selected Clip',1,'Enter','Enter')
                    ,('Delete Selected Clip',1,'Delete','Delete')
                    ,('Select Next',1,'Right','Right')
                    ,('Select Previous',1,'Left','Left')
                    ,('Select All',1,'Control+A','Control+A')
                    ,('Invert Selection',1,'Control+Shift+Alt+A','Control+Shift+Alt+A')
                    ,('Bring to front',1,'Control+Home','Control+Home')
                    ,('Send to back',1,'Control+End','Control+End')
                    ,('Assign Hotkey',1,'A','A')
                    ,('Change Color',1,'C','C')
                    ,('Say',1,'S','S')
                    ,('Merge',1,'M','M')
                    ,('Undo',1,'Control+Z','Control+Z')
                    ,('Redo',1,'Control+Y','Control+Y')
                    ,('Edit',1,'Control+E','Control+E')
                    ,('Rename',1,'F2','F2')
                    ,('Duplicate',1,'Control+D','Control+D')
                    ,('Email',1,'Control+E','Control+E')
                    ,('Qr Code',1,'Control+Shift+Q','Control+Shift+Q')
                    ,('Toggle Auto-Analyze Mode',2,'Control+Shift+B','Control+Shift+B')
                    ,('Toggle Is App Paused',2,'Control+Shift+P','Control+Shift+P')
                    ,('Copy Selection',1,'Control+C','Control+C');
                    
                    CREATE TABLE MpDetectedImageObject (
                      pk_MpDetectedImageObjectId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , fk_MpCopyItemId integer NOT NULL
                    , Confidence real NOT NULL
                    , X real NOT NULL
                    , Y real NOT NULL
                    , Width real NOT NULL
                    , Height real NOT NULL                    
                    , ObjectTypeName text
                    , CONSTRAINT FK_MpDetectedImageObject_0_0 FOREIGN KEY (fk_MpCopyItemId) REFERENCES MpCopyItem (pk_MpCopyItemId)
                    );
                    
                    CREATE TABLE MpCopyItemTemplate (
                      pk_MpCopyItemTemplateId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpCopyItemTemplateGuid text
                    , fk_MpCopyItemId integer NOT NULL
                    , HexColor text default '#0000FF'
                    , TemplateName text NOT NULL 
                    , CONSTRAINT FK_MpCopyItemTemplate_0_0 FOREIGN KEY (fk_MpCopyItemId) REFERENCES MpCopyItem (pk_MpCopyItemId)                    
                    );       
                    
                    CREATE TABLE MpPasteHistory (
                      pk_MpPasteHistoryId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpPasteHistoryGuid text
                    , fk_MpCopyItemId integer NOT NULL
                    , fk_MpUserDeviceId integer NOT NULL
                    , fk_MpAppId integer default 0                    
                    , fk_MpUrlId integer default 0
                    , PasteDateTime datetime NOT NULL
                    , CONSTRAINT FK_MpPasteHistory_0_0 FOREIGN KEY (fk_MpAppId) REFERENCES MpApp (pk_MpAppId)
                    , CONSTRAINT FK_MpPasteHistory_1_0 FOREIGN KEY (fk_MpUserDeviceId) REFERENCES MpUserDevice (pk_MpUserDeviceId)
                    , CONSTRAINT FK_MpPasteHistory_2_0 FOREIGN KEY (fk_MpCopyItemId) REFERENCES MpCopyItem (pk_MpCopyItemId)
                    , CONSTRAINT FK_MpPasteHistory_3_0 FOREIGN KEY (fk_MpUrlId) REFERENCES MpUrl (pk_MpUrlId)
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

    public class MpDataTable {
        public List<MpDataRow> Rows { get; set; } = new List<MpDataRow>();
    }

    public class MpDataRow {
        private Dictionary<string, object> _columns = new Dictionary<string, object>();

        #region Property Reflection Referencer
        public object this[string colName] {
            get {
                if(!_columns.ContainsKey(colName)) {
                    throw new Exception("Unable to find property: " + colName);
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
                if (_columns.Count >= idx) {
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
