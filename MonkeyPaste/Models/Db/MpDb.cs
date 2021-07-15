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
                await CreateConnectionAsync();
            }
            
            IsLoaded = true;
        }

        public async Task<List<T>> QueryAsync<T>(string query, params object[] args) where T : new() {
            if(_connectionAsync == null) {
                await Init();
            }
            var result = await _connectionAsync.QueryAsync<T>(query, args);
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

        public async Task<List<T>> GetItems<T>() where T : new() {
            if (_connectionAsync == null) {
                await Init();
            }
            return await _connectionAsync.Table<T>().ToListAsync();
        }

        public async Task AddItem<T>(T item, bool ignoreTracking = false, string sourceClientGuid = "") where T : new() {
            if (_connectionAsync == null) {
                await Init();
            }
            if (item is MpISyncableDbObject && item is not MpDbLog && item is not MpSyncHistory && !ignoreTracking) {
                if(string.IsNullOrEmpty((item as MpDbModelBase).Guid)) {
                    (item as MpDbModelBase).Guid = System.Guid.NewGuid().ToString();
                }
                MpDbLogTracker.TrackDbWrite(MpDbLogActionType.Create, item as MpDbModelBase, sourceClientGuid);
                OnSyncableChange?.Invoke(this, item);
            }            
            await _connectionAsync.InsertAsync(item);
            OnItemAdded?.Invoke(this, item as MpDbModelBase);
        }

        public async Task UpdateItem<T>(T item, bool ignoreTracking = false, string sourceClientGuid = "") where T : new() {
            if (_connectionAsync == null) {
                await Init();
            }
            if (item is MpISyncableDbObject && item is not MpDbLog && item is not MpSyncHistory && !ignoreTracking) {                
                MpDbLogTracker.TrackDbWrite(MpDbLogActionType.Modify, item as MpDbModelBase, sourceClientGuid);
                OnSyncableChange?.Invoke(this, item);
            }           

            await _connectionAsync.UpdateAsync(item);
            OnItemUpdated?.Invoke(this, item as MpDbModelBase);
        }

        public async Task AddOrUpdate<T>(T item,  string sourceClientGuid = "", bool ignoreTracking = false) where T : new() {
            sourceClientGuid = string.IsNullOrEmpty(sourceClientGuid) ? MpPreferences.Instance.ThisClientGuidStr : sourceClientGuid;
            if ((item as MpDbModelBase).Id == 0 || 
                string.IsNullOrEmpty((item as MpDbModelBase).Guid)) {
                await AddItem(item,ignoreTracking, sourceClientGuid);
            } else {
                await UpdateItem(item,ignoreTracking, sourceClientGuid);
            }
        }

        public async Task DeleteItem<T>(T item, string sourceClientGuid = "", bool ignoreTracking = false) where T: new() {
            if (_connectionAsync == null) {
                await Init();
            }
            if (item is MpISyncableDbObject && item is not MpDbLog && item is not MpSyncHistory && !ignoreTracking) {
                MpDbLogTracker.TrackDbWrite(MpDbLogActionType.Delete, item as MpDbModelBase, sourceClientGuid);
                OnSyncableChange?.Invoke(this, item);
            }            

            await _connectionAsync.DeleteAsync<T>((item as MpDbModelBase).Id);
            OnItemDeleted?.Invoke(this, item as MpDbModelBase);
        }
        public async Task<object> GetObjDbRow(string tableName, string objGuid) {
            var dt = await QueryAsync(
                tableName,
                string.Format("select * from {0} where {1}=?",tableName,tableName+"Guid"),
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
            if (_connectionAsync != null) {
                return;
            }

            var dbPath = DependencyService.Get<MpIDbFilePath>().DbFilePath();

            File.Delete(dbPath);

            bool isNewDb = !File.Exists(dbPath);

            var connStr = new SQLiteConnectionString(
                databasePath: dbPath,//MpPreferences.Instance.DbPath, 
                storeDateTimeAsTicks: true,
                //key: MpPreferences.Instance.DbPassword,
                openFlags: MpPreferences.DbFlags
                );


            _connectionAsync = new SQLiteAsyncConnection(connStr);

            await InitTablesAsync();

            if (isNewDb) {
                await InitDefaultDataAsync();
            }

            if (_connectionAsync != null && UseWAL) {
                // On sqlite-net v1.6.0+, enabling write-ahead logging allows for faster database execution
                await _connectionAsync.EnableWriteAheadLoggingAsync().ConfigureAwait(false);
            }
            MpConsole.WriteTraceLine("Write ahead logging: " + (UseWAL ? "ENABLED" : "DISABLED"));
        }
        private async Task InitTablesAsync() {
            await _connectionAsync.CreateTableAsync<MpApp>();
            await _connectionAsync.CreateTableAsync<MpClient>();
            await _connectionAsync.CreateTableAsync<MpClientPlatform>();
            await _connectionAsync.CreateTableAsync<MpClip>();
            await _connectionAsync.CreateTableAsync<MpClipComposite>();
            await _connectionAsync.CreateTableAsync<MpClipTag>();
            await _connectionAsync.CreateTableAsync<MpClipTemplate>();
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
            await AddItem<MpColor>(green,true);
            await AddItem<MpColor>(blue, true);
            await AddItem<MpColor>(yellow, true);
            await AddItem<MpColor>(orange, true);

            await AddItem<MpTag>(new MpTag() {
                TagGuid = Guid.Parse("310ba30b-c541-4914-bd13-684a5e00a2d3"),
                TagName = "Recent",
                ColorId = green.Id,
                TagColor = green,
                TagSortIdx = 0
            }, true);
            await AddItem<MpTag>(new MpTag() {
                TagGuid = Guid.Parse("df388ecd-f717-4905-a35c-a8491da9c0e3"),
                TagName = "All",
                ColorId = blue.Id,
                TagColor = blue,
                TagSortIdx = 1
            }, true);

            await AddItem<MpTag>(new MpTag() {
                TagGuid = Guid.Parse("54b61353-b031-4029-9bda-07f7ca55c123"),
                TagName = "Favorites",
                ColorId = yellow.Id,
                TagColor = yellow,
                TagSortIdx = 2
            }, true);
            await AddItem<MpTag>(new MpTag() {
                TagGuid = Guid.Parse("a0567976-dba6-48fc-9a7d-cbd306a4eaf3"),
                TagName = "Help",
                ColorId = orange.Id,
                TagColor = orange,
                TagSortIdx = 3
            }, true);

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
        public string GetLocalIp4Address() {
            if (!IsConnectedToNetwork()) {
                return "0.0.0.0";
            }
            return MpHelpers.Instance.GetLocalIp4Address();
        }

        public string GetExternalIp4Address() {
            if (!IsConnectedToInternet()) {
                return "0.0.0.0";
            }
            return MpHelpers.Instance.GetExternalIp4Address();
        }

        public async Task<DateTime> GetLastSyncForRemoteDevice(string otherDeviceGuid) {
            var shl = await GetItems<MpSyncHistory>();
            var sh = shl.Where(x => x.OtherClientGuid.ToString() == otherDeviceGuid)
                        .OrderByDescending(x => x.SyncDateTime)
                        .FirstOrDefault();
            if (sh != null) {
                return sh.SyncDateTime;
            }
            return DateTime.MinValue;
        }

        public async Task<string> GetLocalLogFromSyncDate(DateTime fromDateTime) {
            var logItems = await MpDb.Instance.GetItems<MpDbLog>();
            var matchLogItems = logItems.Where(x => x.LogActionDateTime > fromDateTime).ToList();

            var dbol = new List<MpISyncableDbObject>();
            foreach (var li in matchLogItems) {
                dbol.Add(li as MpISyncableDbObject);
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
            DateTime newSyncDate,
            string remoteClientGuid) {

            var relevantChanges = new Dictionary<Guid,List<MpDbLog>>();
            foreach (var ckvp in changeLookup) {
                if (ckvp.Value == null || ckvp.Value.Count == 0) {
                    continue;
                }
                var rlogs = await MpDbLog.FilterOutdatedRemoteLogs(ckvp.Key.ToString(), ckvp.Value);
                if (rlogs.Count > 0) {
                    relevantChanges.Add(ckvp.Key, rlogs.OrderBy(x => x.LogActionDateTime).ToList());
                }
            }
            // process leaf tables first
            var colorChanges = new List<MpColor>();
            foreach(var ckvp in relevantChanges) {
                if(ckvp.Value[0].DbTableName == "MpColor") {
                    var color = await new MpColor().CreateFromLogs(ckvp.Key.ToString(), ckvp.Value,remoteClientGuid);
                    colorChanges.Add(color);
                } else {
                    continue;
                }
            }

            //process tags
            var tagChanges = new List<MpTag>();
            foreach (var ckvp in relevantChanges) {
                if (ckvp.Value[0].DbTableName == "MpTag") {
                    var tag = await new MpTag().CreateFromLogs(ckvp.Key.ToString(), ckvp.Value, remoteClientGuid);
                    tagChanges.Add(tag);
                } else {
                    continue;
                }
            }            
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
}
