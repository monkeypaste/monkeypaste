using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.IO;
using Newtonsoft.Json;
using Xamarin.Forms.PlatformConfiguration;

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
        public async Task Init(bool isWpf = false) {
            if (_connectionAsync != null) {
                return;
            }
            InitUser(IdentityToken);
            InitClient(AccessToken);

            if(isWpf) {
                //CreateConnection();
            } else {
                await InitDbAsync();
            }
            
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
                await Init();
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
                await Init();
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
                await Init();
            }
            return await _connectionAsync.Table<T>().ToListAsync();
        }

        public List<T> GetItems<T>() where T : new() {
            if (_connection == null) {
                CreateConnection();
            }
            return _connection.Table<T>().ToList();
        }

        public T GetItem<T>(int id) where T : new() {
            if (_connection == null) {
                CreateConnection();
            }
            return _connection.Table<T>().Where(x => (x as MpDbModelBase).Id == id).FirstOrDefault();
        }

        public async Task AddItemAsync<T>(T item, string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {
            if (_connectionAsync == null) {
                await Init();
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
            await _connectionAsync.InsertAsync(item);
            OnItemAdded?.Invoke(this, item as MpDbModelBase);
            if (!ignoreSyncing) {
                OnSyncableChange?.Invoke(this, (item as MpDbModelBase).Guid);
            }
        }

        public void AddItem<T>(T item, string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {
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
            _connection.Insert(item);
            OnItemAdded?.Invoke(this, item as MpDbModelBase);
            if (!ignoreSyncing) {
                OnSyncableChange?.Invoke(this, (item as MpDbModelBase).Guid);
            }
        }

        public async Task UpdateItemAsync<T>(T item,string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {
            if (_connectionAsync == null) {
                await Init();
            }

            if (item == null) {
                MpConsole.WriteTraceLine(@"Cannot update null item, ignoring...");
                return;
            }
            //object oldItem = null;
            if (item is MpISyncableDbObject && item is not MpDbLog && item is not MpSyncHistory) {   
                if(!ignoreTracking) {
                    await MpDbLogTracker.TrackDbWriteAsync(MpDbLogActionType.Modify, item as MpDbModelBase, sourceClientGuid);
                }
                //string tableName = new MpXamStringToSyncObjectTypeConverter().Convert(item.GetType().ToString()).ToString().Replace(@"MonkeyPaste.", string.Empty);
                //oldItem = await GetObjDbRowAsync(tableName, (item as MpDbModelBase).Guid);
            }
            

            await _connectionAsync.UpdateAsync(item);
            //var updateEventArg = new MpDbObjectUpdateEventArg() {
            //    DbObject = item as MpDbModelBase
            //};
            //if (oldItem != null) {
            //    updateEventArg.UpdatedPropertyLookup = (item as MpISyncableDbObject).DbDiff(oldItem);
            //}
            //OnItemUpdated?.Invoke(this, updateEventArg); 
            OnItemUpdated?.Invoke(this, item as MpDbModelBase);
            if (!ignoreSyncing) {
                OnSyncableChange?.Invoke(this, (item as MpDbModelBase).Guid);
            }
        }

        public void UpdateItem<T>(T item, string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {
            if (_connection == null) {
                CreateConnection();
            }

            if (item == null) {
                MpConsole.WriteTraceLine(@"Cannot update null item, ignoring...");
                return;
            }
            //object oldItem = null;
            if (item is MpISyncableDbObject && item is not MpDbLog && item is not MpSyncHistory) {
                if (!ignoreTracking) {
                    MpDbLogTracker.TrackDbWrite(MpDbLogActionType.Modify, item as MpDbModelBase, sourceClientGuid);
                }
                //string tableName = new MpXamStringToSyncObjectTypeConverter().Convert(item.GetType().ToString()).ToString().Replace(@"MonkeyPaste.", string.Empty);
                //oldItem = await GetObjDbRowAsync(tableName, (item as MpDbModelBase).Guid);
            }


            _connection.Update(item);
            //var updateEventArg = new MpDbObjectUpdateEventArg() {
            //    DbObject = item as MpDbModelBase
            //};
            //if (oldItem != null) {
            //    updateEventArg.UpdatedPropertyLookup = (item as MpISyncableDbObject).DbDiff(oldItem);
            //}
            //OnItemUpdated?.Invoke(this, updateEventArg); 
            OnItemUpdated?.Invoke(this, item as MpDbModelBase);
            if (!ignoreSyncing) {
                OnSyncableChange?.Invoke(this, (item as MpDbModelBase).Guid);
            }
        }


        public async Task AddOrUpdateAsync<T>(T item,  string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {
            sourceClientGuid = string.IsNullOrEmpty(sourceClientGuid) ? MpPreferences.Instance.ThisClientGuidStr : sourceClientGuid;
            if ((item as MpDbModelBase).Id == 0 || 
                (string.IsNullOrEmpty((item as MpDbModelBase).Guid))) {
                await AddItemAsync(item, sourceClientGuid,ignoreTracking,ignoreSyncing);
            } else {
                await UpdateItemAsync(item, sourceClientGuid,ignoreTracking,ignoreSyncing);
            }
        }

        public void AddOrUpdate<T>(T item, string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {
            sourceClientGuid = string.IsNullOrEmpty(sourceClientGuid) ? MpPreferences.Instance.ThisClientGuidStr : sourceClientGuid;
            if ((item as MpDbModelBase).Id == 0 ||
                (string.IsNullOrEmpty((item as MpDbModelBase).Guid))) {
                AddItem(item, sourceClientGuid, ignoreTracking, ignoreSyncing);
            } else {
                UpdateItem(item, sourceClientGuid, ignoreTracking, ignoreSyncing);
            }
        }

        public async Task DeleteItemAsync<T>(T item, string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T: new() {
            if (_connectionAsync == null) {
                await Init();
            }
            if(item == null) {
                MpConsole.WriteTraceLine(@"Cannot delete null item, ignoring...");
                return;
            }
            if (item is MpISyncableDbObject && item is not MpDbLog && item is not MpSyncHistory) {
                if(!ignoreTracking) {
                   await MpDbLogTracker.TrackDbWriteAsync(MpDbLogActionType.Delete, item as MpDbModelBase, sourceClientGuid);
                }
            }            

            await _connectionAsync.DeleteAsync<T>((item as MpDbModelBase).Id);
            OnItemDeleted?.Invoke(this, item as MpDbModelBase); 
            if (!ignoreSyncing) {
                OnSyncableChange?.Invoke(this, (item as MpDbModelBase).Guid);
            }
        }

        public void DeleteItem<T>(T item, string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {
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

            _connection.Delete<T>((item as MpDbModelBase).Id);
            OnItemDeleted?.Invoke(this, item as MpDbModelBase);
            if (!ignoreSyncing) {
                OnSyncableChange?.Invoke(this, (item as MpDbModelBase).Guid);
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
            return null;
        }

        public object GetDbObjectByTableGuid(string tableName, string objGuid) {
            var dt = Query(
                tableName,
                string.Format("select * from {0} where {1}=?", tableName, tableName + "Guid"),
                objGuid);

            if (dt != null && dt.Count > 0) {
                return dt[0];
            }
            return null;
        }

        //public async Task UpdateWithChildren(MpDbObject dbo) {
        //    await _connectionAsync.UpdateWithChildrenAsync(dbo);
        //}

        //public async Task<T> GetWithChildren<T>(T item) where T: new() {
        //    return await _connectionAsync.GetWithChildrenAsync<T>((item as MpDbObject).Id);
        //}

        //public async Task<List<T>> GetAllWithChildren<T>() where T : new() {
        //    return await _connectionAsync.GetAllWithChildrenAsync<T>();
        //}

        public void InitUser(string idToken) {
            // User = new MpUser() { IdentityToken = idToken };
        }
        public void InitClient(string accessToken) {
            //Client = new MpClient(0, 3, MpHelpers.Instance.GetCurrentIPAddress()/*.MapToIPv4()*/.ToString(), accessToken, DateTime.Now);
        }

        public byte[] GetDbFileBytes() {
            var dbPath = DependencyService.Get<MpIDbFilePath>().DbFilePath();
            return File.ReadAllBytes(dbPath);
        }
        #endregion

        #region Private Methods  
        private async Task CreateConnectionAsync() {
            await Task.Delay(1);
            var dbPath = DependencyService.Get<MpIDbFilePath>().DbFilePath();

            var connStr = new SQLiteConnectionString(
                databasePath: dbPath,
                storeDateTimeAsTicks: false,
                //key: MpPreferences.Instance.DbPassword,
                openFlags: SQLiteOpenFlags.ReadWrite |
                           SQLiteOpenFlags.Create |
                           SQLiteOpenFlags.SharedCache |
                           SQLiteOpenFlags.FullMutex
                );


            _connectionAsync = new SQLiteAsyncConnection(connStr) { Trace = true };
        }

        private void CreateConnection() {
            var dbPath = DependencyService.Get<MpIDbFilePath>().DbFilePath();

            var connStr = new SQLiteConnectionString(
                databasePath: dbPath,
                storeDateTimeAsTicks: false,
                //key: MpPreferences.Instance.DbPassword,
                openFlags: SQLiteOpenFlags.ReadWrite |
                           SQLiteOpenFlags.Create |
                           SQLiteOpenFlags.SharedCache |
                           SQLiteOpenFlags.FullMutex
                );


            _connection = new SQLiteConnection(connStr) { Trace = true };
        }

        private async Task InitDbAsync() {
            if (_connectionAsync != null) {
                return;
            }

            var dbPath = DependencyService.Get<MpIDbFilePath>().DbFilePath();
            
            File.Delete(dbPath);

            bool isNewDb = !File.Exists(dbPath);

            await CreateConnectionAsync();

            await InitTablesAsync();

            if (isNewDb) {
                await InitDefaultDataAsync();
            }

            if (_connectionAsync != null && UseWAL) {
                // On sqlite-net v1.6.0+, enabling write-ahead logging allows for faster database execution
                await _connectionAsync.EnableWriteAheadLoggingAsync().ConfigureAwait(false);
            }

            MpConsole.WriteLine(@"Db file located: " + dbPath);
            MpConsole.WriteLine(@"This Client Guid: " + MpPreferences.Instance.ThisClientGuidStr);

            MpConsole.WriteLine("Write ahead logging: " + (UseWAL ? "ENABLED" : "DISABLED"));
        }
        private async Task InitTablesAsync() {
            await _connectionAsync.CreateTableAsync<MpApp>();
            await _connectionAsync.CreateTableAsync<MpClient>();
            await _connectionAsync.CreateTableAsync<MpClientPlatform>();
            await _connectionAsync.CreateTableAsync<MpCopyItem>();
            await _connectionAsync.CreateTableAsync<MpCompositeCopyItem>();
            await _connectionAsync.CreateTableAsync<MpCopyItemTag>();
            await _connectionAsync.CreateTableAsync<MpCopyItemTemplate>();
            await _connectionAsync.CreateTableAsync<MpColor>();
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

        private async Task InitDefaultDataAsync() {
            if(string.IsNullOrEmpty(MpPreferences.Instance.ThisClientGuidStr)) {
                MpPreferences.Instance.ThisClientGuidStr = System.Guid.NewGuid().ToString();
            }
            var green = new MpColor(0, 255/255, 0, 255 / 255) {
                ColorGuid = Guid.Parse("fec9579b-a580-4b02-af2f-d1b275812392")
            };
            var blue = new MpColor(0, 0, 255 / 255, 255 / 255) {
                ColorGuid = Guid.Parse("8b30650f-c616-4972-b4a7-a88d1022ae15")
            };
            var yellow = new MpColor(255 / 255, 255 / 255, 0, 255 / 255) {
                ColorGuid = Guid.Parse("bb666db2-1762-4b18-a1da-dd678a458f7a")
            };
            var orange = new MpColor(255 / 255, 165 / 255, 0, 255 / 255) {
                ColorGuid = Guid.Parse("2c5a7c6f-042c-4890-92e5-5ccf088ee698")
            };
            await AddItemAsync<MpColor>(green,"",true,true);
            await AddItemAsync<MpColor>(blue, "", true, true);
            await AddItemAsync<MpColor>(yellow, "", true, true);
            await AddItemAsync<MpColor>(orange, "", true, true);

            await AddItemAsync<MpTag>(new MpTag() {
                TagGuid = Guid.Parse("310ba30b-c541-4914-bd13-684a5e00a2d3"),
                TagName = "Recent",
                ColorId = green.Id,
                //Color = green,
                TagSortIdx = 0
            }, "", true, true);
            await AddItemAsync<MpTag>(new MpTag() {
                TagGuid = Guid.Parse("df388ecd-f717-4905-a35c-a8491da9c0e3"),
                TagName = "All",
                ColorId = blue.Id,
                //Color = blue,
                TagSortIdx = 1
            }, "", true, true);

            await AddItemAsync<MpTag>(new MpTag() {
                TagGuid = Guid.Parse("54b61353-b031-4029-9bda-07f7ca55c123"),
                TagName = "Favorites",
                ColorId = yellow.Id,
                //Color = yellow,
                TagSortIdx = 2
            }, "", true, true);
            await AddItemAsync<MpTag>(new MpTag() {
                TagGuid = Guid.Parse("a0567976-dba6-48fc-9a7d-cbd306a4eaf3"),
                TagName = "Help",
                ColorId = orange.Id,
                //Color = orange,
                TagSortIdx = 3
            }, "", true, true);

            MpConsole.WriteTraceLine(@"Create all default tables");
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
            return MpPreferences.Instance.ThisClientGuidStr;
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

        public DateTime GetLastSyncForRemoteDevice(string otherDeviceGuid) {
            var shl = GetItems<MpSyncHistory>();
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
            var lastSyncDt = MpDb.Instance.GetLastSyncForRemoteDevice(remoteClientGuid);
            //filter & separate remote logs w/ local updates after remote action dt 
            var addChanges = new Dictionary<Guid, List<MpDbLog>>();
            var updateChanges = new Dictionary<Guid, List<MpDbLog>>();
            var deleteChanges = new Dictionary<Guid, List<MpDbLog>>();
            foreach (var ckvp in changeLookup) {
                if (ckvp.Value == null || ckvp.Value.Count == 0) {
                    continue;
                }
                //filter changes by > local action date time
                var rlogs = ckvp.Value;//await MpDbLog.FilterOutdatedRemoteLogs(ckvp.Key.ToString(), ckvp.Value,lastSyncDt);
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
                //ditch adds or modifies when a delete exists
                foreach (var dc in deleteChanges) {
                    if (addChanges.ContainsKey(dc.Key)) {
                        addChanges.Remove(dc.Key);
                    }
                    if (updateChanges.ContainsKey(dc.Key)) {
                        updateChanges.Remove(dc.Key);
                    }
                }

                //sort 3 types by key references
                addChanges = OrderByPrecedence(addChanges);
                deleteChanges = OrderByPrecedence(deleteChanges);
                updateChanges = OrderByPrecedence(updateChanges);
            }

            // in delete, add, update order
            foreach(var ckvp in deleteChanges) {
                var dbot = new MpXamStringToSyncObjectTypeConverter().Convert(ckvp.Value[0].DbTableName);
                var deleteMethod = typeof(MpDb).GetMethod("DeleteItemAsync");
                var deleteByDboTypeMethod = deleteMethod.MakeGenericMethod(new[] { dbot });
                //var dbo = await MpDb.Instance.GetObjDbRowAsync(ckvp.Value[0].DbTableName, ckvp.Key.ToString());
                var dbo = MpDbModelBase.CreateOrUpdateFromLogs(ckvp.Value, remoteClientGuid);
                Task deleteItem = (Task)deleteByDboTypeMethod.Invoke(MpDb.Instance, new object[] { dbo,remoteClientGuid,false,true });
                await deleteItem;
            }

            foreach (var ckvp in addChanges) {
                var dbot = new MpXamStringToSyncObjectTypeConverter().Convert(ckvp.Value[0].DbTableName);
                //var dbo = Activator.CreateInstance(dbot);
                //dbo = await (dbo as MpISyncableDbObject).CreateFromLogs(ckvp.Key.ToString(), ckvp.Value, remoteClientGuid);
                var dbo = MpDbModelBase.CreateOrUpdateFromLogs(ckvp.Value, remoteClientGuid);
                var addMethod = typeof(MpDb).GetMethod("AddOrUpdateAsync");
                var addByDboTypeMethod = addMethod.MakeGenericMethod(new[] { dbot });
                Task addItem = (Task)addByDboTypeMethod.Invoke(MpDb.Instance, new object[] { dbo,remoteClientGuid,false,true });
                await addItem;
            }

            foreach (var ckvp in updateChanges) {
                var dbot = new MpXamStringToSyncObjectTypeConverter().Convert(ckvp.Value[0].DbTableName);
                //var dbo = Activator.CreateInstance(dbot);
                //dbo = await (dbo as MpISyncableDbObject).CreateFromLogs(ckvp.Key.ToString(), ckvp.Value, remoteClientGuid);                
                var dbo = MpDbModelBase.CreateOrUpdateFromLogs(ckvp.Value, remoteClientGuid);
                var updateMethod = typeof(MpDb).GetMethod("UpdateItemAsync");
                var updateByDboTypeMethod = updateMethod.MakeGenericMethod(new[] { dbot });
                Task updateItem = (Task)updateByDboTypeMethod.Invoke(MpDb.Instance, new object[] { dbo,remoteClientGuid,false,true });
                await updateItem;
            }


            var newSyncHistory = new MpSyncHistory() {
                OtherClientGuid = System.Guid.Parse(remoteClientGuid),
                SyncDateTime = DateTime.UtcNow
            };
            await MpDb.Instance.AddItemAsync<MpSyncHistory>(newSyncHistory,"",true,true);
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
            var orderedLogs = new List<string>() {
                          "MpColor",
                          "MpDbImage",
                          "MpIcon",
                          "MpUrl",
                          "MpUrlDomain",
                          "MpApp",
                          "MpSource",
                          "MpCompositeCopyItem",
                          "MpCopyItemTag",
                          "MpCopyItemTemplate",
                          "MpCopyItem",
                          "MpTag",
                          "MpClient" };
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
            var bytes = File.ReadAllBytes(DependencyService.Get<MpIDbFilePath>().DbFilePath());
            return Convert.ToBase64String(bytes);
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
                          "MpCompositeCopyItem",
                          "MpCopyItemTag",
                          "MpCopyItemTemplate",
                          "MpCopyItem",
                          "MpTag",
                          "MpClient" };
            var idx = orderedLogs.IndexOf(log.DbTableName);
            if(idx < 0) {
                throw new Exception(@"Unknown dblog table type: " + log.DbTableName);
            }
            return idx;
        }
    }
}
