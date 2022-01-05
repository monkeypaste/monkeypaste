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

namespace MonkeyPaste {
    public class MpDb : MpSingleton<MpDb>, MpISync {
        #region Private Variables
        private MpIDbInfo _dbInfo;
        private object _rdLock = new object();
        private SQLiteAsyncConnection _connectionAsync;
        #endregion

        #region Properties

        public bool UseWAL { get; set; } = false;
        public string IdentityToken { get; set; }
        public string AccessToken { get; set; }
        public bool IsLoaded { get; set; } = false;

        #endregion

        #region Events
        public event EventHandler OnInitDefaultNativeData;

        public event EventHandler<MpDbModelBase> OnItemAdded;
        public event EventHandler<MpDbModelBase> OnItemUpdated;
        public event EventHandler<MpDbModelBase> OnItemDeleted;
        public event EventHandler<object> OnSyncableChange;

        public event EventHandler<MpDbSyncEventArgs> SyncAdd;
        public event EventHandler<MpDbSyncEventArgs> SyncUpdate;
        public event EventHandler<MpDbSyncEventArgs> SyncDelete;
        #endregion

        #region Constructors

        public MpDb() { }

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

        #endregion

        #region Public Methods

        #region Queries

        public async Task<TableMapping> GetTableMappingAsync(string tableName) {
            await Task.Delay(1);
            if (_connectionAsync == null) {
                CreateConnection();
            }
            return _connectionAsync
                    .TableMappings
                    .Where(x => x.TableName.ToLower() == tableName.ToLower()).FirstOrDefault();
        }

        public async Task<List<T>> QueryAsync<T>(string query, params object[] args) where T : new() {
            if (_connectionAsync == null) {
                CreateConnection();
            }
            var result = await _connectionAsync.QueryAsync<T>(query, args);
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

        public async Task<T> QueryScalarAsync<T>(string query, params object[] args) {
            if(_connectionAsync == null) {
                CreateConnection();
            }
            var result = await _connectionAsync.ExecuteScalarAsync<T>(query, args);
            return result;
        }

        public async Task<List<T>> QueryScalarsAsync<T>(string query, params object[] args) {
            if (_connectionAsync == null) {
                CreateConnection();
            }
            var result = await _connectionAsync.QueryScalarsAsync<T>(query, args);
            return result;
        }

        public async Task<List<T>> GetItemsAsync<T>() where T : new() {
            if (_connectionAsync == null) {
                await InitDb ();
            }
            var dbol = await _connectionAsync.GetAllWithChildrenAsync<T>(recursive: true);
            return dbol;
        }
        public async Task<T> GetItemAsync<T>(int id) where T : new() {
            if (_connectionAsync == null) {
                await InitDb();
            }
            var dbo = await _connectionAsync.GetWithChildrenAsync<T>(id, true);
            return dbo;
        }

        public async Task<List<T>> GetAllWithChildrenAsync<T>(Expression<Func<T, bool>> exp, bool recursive = true) where T : new() {
            if (_connectionAsync == null) {
                CreateConnection();
            }
            var result = await _connectionAsync.GetAllWithChildrenAsync<T>(exp, recursive);
            return result;
        }

        //public async Task RunInMutex(Func<Task> action) {
        //    try {
        //        await mutex.WaitAsync();
        //        await action.Invoke();
        //    }
        //    finally {
        //        mutex.Release();
        //    }
        //}

        //public async Task<T> RunInMutex<T>(Func<Task<T>> action) {
        //    try {
        //        await mutex.WaitAsync();
        //        return await action.Invoke();
        //    }
        //    finally {
        //        mutex.Release();
        //    }
        //}

        private async Task AddItemAsync<T>(T item, string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {            
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

            if (item is MpCopyItemTag cit) {
                if(cit.CopyItemId == 0 && cit.TagId == 0) {
                    return;
                }
            }

            //await RunInMutex(async () => {
                await _connectionAsync.InsertOrReplaceWithChildrenAsync(item, recursive: true);
            //});            
            
            OnItemAdded?.Invoke(this, item as MpDbModelBase);

            if (!ignoreSyncing && item is MpISyncableDbObject) {
                OnSyncableChange?.Invoke(item, (item as MpDbModelBase).Guid);
            }
        }

        private async Task UpdateItemAsync<T>(T item,string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {
            sourceClientGuid = string.IsNullOrEmpty(sourceClientGuid) ? MpPreferences.Instance.ThisDeviceGuid : sourceClientGuid;
            if (_connectionAsync == null) {
                await InitDb();
            }

            if (item == null) {
                MpConsole.WriteTraceLine(@"Cannot update null item, ignoring...");
                return;
            }
            if (item is MpISyncableDbObject && item is not MpDbLog && item is not MpSyncHistory) {
                if (!ignoreTracking) {
                    await MpDbLogTracker.TrackDbWriteAsync(MpDbLogActionType.Modify, item as MpDbModelBase, sourceClientGuid);
                }
            }
            
            //await RunInMutex(async () => {
                await _connectionAsync.UpdateWithChildrenAsync(item);
            //});
            OnItemUpdated?.Invoke(this, item as MpDbModelBase);
            if (!ignoreSyncing && item is MpISyncableDbObject) {
                OnSyncableChange?.Invoke(item, (item as MpDbModelBase).Guid);
            }
        }

        public async Task AddOrUpdateAsync<T>(T item, string sourceClientGuid = "", bool ignoreTracking = false, bool ignoreSyncing = false) where T : new() {
            if ((item as MpDbModelBase).Id == 0) {
                await AddItemAsync(item, sourceClientGuid, ignoreTracking, ignoreSyncing);
            } else {
                await UpdateItemAsync(item, sourceClientGuid, ignoreTracking, ignoreSyncing);
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
                    //MpDbLogTracker.TrackDbWrite(MpDbLogActionType.Delete, item as MpDbModelBase, sourceClientGuid);
                    await MpDbLogTracker.TrackDbWriteAsync(MpDbLogActionType.Delete, item as MpDbModelBase, sourceClientGuid);
                }
            }

            //await RunInMutex( async()=> {
                await _connectionAsync.DeleteAsync(item, true);
            //});

            OnItemDeleted?.Invoke(this, item as MpDbModelBase);
            if (!ignoreSyncing && item is MpISyncableDbObject) {
                OnSyncableChange?.Invoke(item, (item as MpDbModelBase).Guid);
            }
        }
                
        public async Task<object> GetDbObjectByTableGuidAsync(string tableName, string objGuid) {
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
        
        #endregion

        public byte[] GetDbFileBytes() {
            var dbPath = _dbInfo.GetDbFilePath();
            return File.ReadAllBytes(dbPath);
        }

        #endregion

        #region Private Methods  

        private async Task InitDb() {

            //SQLitePCL.Batteries.Init();

            var dbPath = _dbInfo.GetDbFilePath();
            
            //File.Delete(dbPath);

            bool isNewDb = !File.Exists(dbPath);

            if(isNewDb) {
                using (File.Create(dbPath));
            }

            CreateConnection();

            //CreateFunctions();

            if (UseWAL) {
                if (_connectionAsync != null) {
                    await _connectionAsync.EnableWriteAheadLoggingAsync().ConfigureAwait(false);
                }
            }

            await InitTables();
            
            if (isNewDb) {
                await CreateViews();
                await InitDefaultPortableData();

                OnInitDefaultNativeData?.Invoke(this, null);
            }

            MpPreferences.Instance.ThisUserDevice = await MpDataModelProvider.Instance.GetUserDeviceByGuid(MpPreferences.Instance.ThisDeviceGuid);

            MpPreferences.Instance.ThisAppSource = await GetItemAsync<MpSource>(MpPreferences.Instance.ThisDeviceSourceId);

            if(isNewDb) {
                OnInitDefaultNativeData?.Invoke(this, null);
            }

            MpConsole.WriteLine(@"Db file located: " + dbPath);
            MpConsole.WriteLine(@"This Client Guid: " + MpPreferences.Instance.ThisDeviceGuid);
            MpConsole.WriteLine("Write ahead logging: " + (UseWAL ? "ENABLED" : "DISABLED"));
        }

        private void CreateConnection() {
            if (_connectionAsync == null) {
                SQLitePCL.Batteries.Init();
                var _connStr = new SQLiteConnectionString(
                                databasePath: _dbInfo.GetDbFilePath(),
                                storeDateTimeAsTicks: true,
                                openFlags: SQLiteOpenFlags.ReadWrite |
                                           SQLiteOpenFlags.Create |
                                           SQLiteOpenFlags.SharedCache |
                                           SQLiteOpenFlags.FullMutex
                                );
                _connectionAsync = new SQLiteAsyncConnection(_connStr) { Trace = true };
                SQLitePCL.raw.sqlite3_create_function(
                    _connectionAsync.GetConnection().Handle, 
                    "REGEXP", 2, null, MatchRegex);                
            }
        }

        private void MatchRegex(sqlite3_context ctx, object user_data, sqlite3_value[] args) {
            bool isMatched = System.Text.RegularExpressions.Regex.IsMatch(
                SQLitePCL.raw.sqlite3_value_text(args[1]).utf8_to_string(),
                SQLitePCL.raw.sqlite3_value_text(args[0]).utf8_to_string(),
                RegexOptions.IgnoreCase);

            if (isMatched)
                SQLitePCL.raw.sqlite3_result_int(ctx, 1);
            else
                SQLitePCL.raw.sqlite3_result_int(ctx, 0);
        }

        private async Task InitTables() {
            await _connectionAsync.CreateTableAsync<MpAnalyticItem>();
            await _connectionAsync.CreateTableAsync<MpAnalyticItemPreset>();
            await _connectionAsync.CreateTableAsync<MpAnalyticItemPresetParameterValue>();
            await _connectionAsync.CreateTableAsync<MpApp>();
            await _connectionAsync.CreateTableAsync<MpBillableItem>();
            await _connectionAsync.CreateTableAsync<MpCopyItem>();
            await _connectionAsync.CreateTableAsync<MpCopyItemTag>();
            await _connectionAsync.CreateTableAsync<MpCopyItemTemplate>();
            await _connectionAsync.CreateTableAsync<MpDbImage>();
            await _connectionAsync.CreateTableAsync<MpDbLog>();
            await _connectionAsync.CreateTableAsync<MpDetectedImageObject>();
            await _connectionAsync.CreateTableAsync<MpIcon>();
            await _connectionAsync.CreateTableAsync<MpMatcher>();
            await _connectionAsync.CreateTableAsync<MpMatchCommand>();
            await _connectionAsync.CreateTableAsync<MpMatchableEvent>();
            await _connectionAsync.CreateTableAsync<MpPasteHistory>();
            await _connectionAsync.CreateTableAsync<MpPasteToAppPath>();
            await _connectionAsync.CreateTableAsync<MpSearchCriteriaItem>();
            await _connectionAsync.CreateTableAsync<MpShortcut>();
            await _connectionAsync.CreateTableAsync<MpSource>();
            await _connectionAsync.CreateTableAsync<MpSyncHistory>();
            await _connectionAsync.CreateTableAsync<MpTag>();
            await _connectionAsync.CreateTableAsync<MpTagProperty>();
            await _connectionAsync.CreateTableAsync<MpUrl>();
            await _connectionAsync.CreateTableAsync<MpUserDevice>();
            await _connectionAsync.CreateTableAsync<MpUserSearch>();
        }

        private async Task CreateViews() {
            await _connectionAsync.ExecuteAsync(@"CREATE VIEW MpSortableCopyItem_View as
                                                    SELECT 
	                                                    case fk_ParentCopyItemId
		                                                    when 0
			                                                    then pk_MpCopyItemId
		                                                    ELSE
			                                                    fk_ParentCopyItemId
	                                                    end as RootId,
	                                                    pk_MpCopyItemId,
	                                                    fk_MpCopyItemTypeId,
	                                                    CompositeSortOrderIdx,
	                                                    Title,
	                                                    ItemData,
	                                                    ItemDescription,
	                                                    CopyDateTime,
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
	                                                    MpUrl.UrlTitle
                                                    FROM
	                                                    MpCopyItem
                                                    INNER JOIN MpSource ON MpSource.pk_MpSourceId = MpCopyItem.fk_MpSourceId
                                                    INNER JOIN MpApp ON MpApp.pk_MpAppId = MpSource.fk_MpAppId
                                                    LEFT JOIN MpUrl ON MpUrl.pk_MpUrlId = MpSource.fk_MpUrlId");
        }
        
        private async Task InitDefaultPortableData() {
            #region User Device

            MpPreferences.Instance.ThisDeviceGuid = Guid.NewGuid().ToString();

            var thisDevice = new MpUserDevice() {
                UserDeviceGuid = Guid.Parse(MpPreferences.Instance.ThisDeviceGuid),
                PlatformType = MpPreferences.Instance.ThisDeviceType
            };
            await AddItemAsync<MpUserDevice>(thisDevice);

            #endregion

            #region Source

            var process = Process.GetCurrentProcess();
            string appPath = process.MainModule.FileName;
            string appName = MpPreferences.Instance.ApplicationName;
            var icon = await MpIcon.Create(MpBase64Images.Instance.AppIcon);
            var app = await MpApp.Create(appPath, appName, icon);
            var source = await MpSource.Create(app, null);
            MpPreferences.Instance.ThisDeviceSourceId = source.Id;

            #endregion

            #region Tags

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

            #endregion

            #region Shortcuts

            var sh1 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("5dff238e-770e-4665-93f5-419e48326f01"),
                ShortcutName = "Show Window",
                RouteType = 2,
                KeyString = "Control+Shift+D",
                DefaultKeyString = "Control+Shift+D"
            };
            await AddItemAsync<MpShortcut>(sh1);

            var sh2 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("cb807500-9121-4e41-80d3-8c3682ce90d9"),
                ShortcutName = "Hide Window",
                RouteType = 1,
                KeyString = "Escape",
                DefaultKeyString = "Escape"
            };
            await AddItemAsync<MpShortcut>(sh2);

            var sh3 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("a41aeed8-d4f3-47de-86c5-f9ca296fb103"),
                ShortcutName = "Append Mode",
                RouteType = 2,
                KeyString = "Control+Shift+A",
                DefaultKeyString = "Control+Shift+A"
            };
            await AddItemAsync<MpShortcut>(sh3);

            var sh4 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("892bf7d7-ba8e-4db1-b2ca-62b41ff6614c"),
                ShortcutName = "Auto-Copy Mode",
                RouteType = 2,
                KeyString = "Control+Shift+C",
                DefaultKeyString = "Control+Shift+C"
            };
            await AddItemAsync<MpShortcut>(sh4);

            var sh5 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("a12c4211-ab1f-4b97-98ff-fbeb514e9a1c"),
                ShortcutName = "Right-Click Paste Mode",
                RouteType = 2,
                KeyString = "Control+Shift+R",
                DefaultKeyString = "Control+Shift+R"
            };
            await AddItemAsync<MpShortcut>(sh5);

            var sh6 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("1d212ca5-fb2a-4962-8f58-24ed9a5d007d"),
                ShortcutName = "Paste Selected Clip",
                RouteType = 1,
                KeyString = "Enter",
                DefaultKeyString = "Enter"
            };
            await AddItemAsync<MpShortcut>(sh6);

            var sh7 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("e94ca4f3-4c6e-40dc-8941-c476a81543c7"),
                ShortcutName = "Delete Selected Clip",
                RouteType = 1,
                KeyString = "Delete",
                DefaultKeyString = "Delete"
            };
            await AddItemAsync<MpShortcut>(sh7);

            var sh8 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("7fe24929-6c9e-49c0-a880-2f49780dfb3a"),
                ShortcutName = "Select Next",
                RouteType = 1,
                KeyString = "Right",
                DefaultKeyString = "Right"
            };
            await AddItemAsync<MpShortcut>(sh8);

            var sh9 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("ee657845-f1dc-40cf-848d-6768c0081670"),
                ShortcutName = "Select Previous",
                RouteType = 1,
                KeyString = "Left",
                DefaultKeyString = "Left"
            };
            await AddItemAsync<MpShortcut>(sh9);

            var sh10 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("5480f103-eabd-4e40-983c-ebae81645a10"),
                ShortcutName = "Select All",
                RouteType = 1,
                KeyString = "Control+A",
                DefaultKeyString = "Control+A"
            };
            await AddItemAsync<MpShortcut>(sh10);

            var sh11 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("39a6b8b5-a585-455b-af83-015fd97ac3fa"),
                ShortcutName = "Invert Selection",
                RouteType = 1,
                KeyString = "Control+Shift+Alt+A",
                DefaultKeyString = "Control+Shift+Alt+A"
            };
            await AddItemAsync<MpShortcut>(sh11);

            var sh12 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("166abd7e-7295-47f2-bbae-c96c03aa6082"),
                ShortcutName = "Bring to front",
                RouteType = 1,
                KeyString = "Control+Home",
                DefaultKeyString = "Control+Home"
            };
            await AddItemAsync<MpShortcut>(sh12);

            var sh13 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("84c11b86-3acc-4d22-b8e9-3bd785446f72"),
                ShortcutName = "Send to back",
                RouteType = 1,
                KeyString = "Control+End",
                DefaultKeyString = "Control+End"
            };
            await AddItemAsync<MpShortcut>(sh13);

            var sh14 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("6487f6ff-da0c-475b-a2ae-ef1484233de0"),
                ShortcutName = "Assign Hotkey",
                RouteType = 1,
                KeyString = "Control+Shift+H",
                DefaultKeyString = "Control+Shift+H"
            };
            await AddItemAsync<MpShortcut>(sh14);

            var sh15 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("837e0c20-04b8-4211-ada0-3b4236da0821"),
                ShortcutName = "Change Color",
                RouteType = 1,
                KeyString = "Control+Shift+Alt+C",
                DefaultKeyString = "Control+Shift+Alt+C"
            };
            await AddItemAsync<MpShortcut>(sh15);

            var sh16 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("4a567aff-33a8-4a1f-8484-038196812849"),
                ShortcutName = "Say",
                RouteType = 1,
                KeyString = "Control+Shift+S",
                DefaultKeyString = "Control+Shift+S"
            };
            await AddItemAsync<MpShortcut>(sh16);

            var sh17 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("330afa20-25c3-425c-8e18-f1423eda9066"),
                ShortcutName = "Merge",
                RouteType = 1,
                KeyString = "Control+Shift+M",
                DefaultKeyString = "Control+Shift+M"
            };
            await AddItemAsync<MpShortcut>(sh17);

            var sh18 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("118a2ca6-7021-47a0-8458-7ebc31094329"),
                ShortcutName = "Undo",
                RouteType = 1,
                KeyString = "Control+Z",
                DefaultKeyString = "Control+Z"
            };
            await AddItemAsync<MpShortcut>(sh18);

            var sh19 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("3980efcc-933b-423f-9cad-09e455c6824a"),
                ShortcutName = "Redo",
                RouteType = 1,
                KeyString = "Control+Y",
                DefaultKeyString = "Control+Y"
            };
            await AddItemAsync<MpShortcut>(sh19);

            var sh20 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("7a7580d1-4129-432d-a623-2fff0dc21408"),
                ShortcutName = "Edit",
                RouteType = 1,
                KeyString = "Control+E",
                DefaultKeyString = "Control+E"
            };
            await AddItemAsync<MpShortcut>(sh20);

            var sh21 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("085338fb-f297-497a-abb7-eeb7310dc6f3"),
                ShortcutName = "Rename",
                RouteType = 1,
                KeyString = "F2",
                DefaultKeyString = "F2"
            };
            await AddItemAsync<MpShortcut>(sh21);

            var sh22 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("e22faafd-4313-441a-b361-16910fc7e9d3"),
                ShortcutName = "Duplicate",
                RouteType = 1,
                KeyString = "Control+D",
                DefaultKeyString = "Control+D"
            };
            await AddItemAsync<MpShortcut>(sh22);

            var sh23 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("4906a01e-b2f7-43f0-af1e-fb99d55c9778"),
                ShortcutName = "Email",
                RouteType = 1,
                KeyString = "Control+E",
                DefaultKeyString = "Control+E"
            };
            await AddItemAsync<MpShortcut>(sh23);

            var sh24 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("c7248087-2031-406d-b4ab-a9007fbd4bc4"),
                ShortcutName = "Qr Code",
                RouteType = 1,
                KeyString = "Control+Shift+Q",
                DefaultKeyString = "Control+Shift+Q"
            };
            await AddItemAsync<MpShortcut>(sh24);

            var sh25 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("777367e6-c161-4e93-93e0-9bf12221f7ff"),
                ShortcutName = "Toggle Append Line Mode",
                RouteType = 2,
                KeyString = "Control+Shift+B",
                DefaultKeyString = "Control+Shift+B"
            };
            await AddItemAsync<MpShortcut>(sh25);

            var sh26 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("97e29b06-0ec4-4c55-a393-8442d7695038"),
                ShortcutName = "Toggle Is App Paused",
                RouteType = 2,
                KeyString = "Control+Shift+P",
                DefaultKeyString = "Control+Shift+P"
            };
            await AddItemAsync<MpShortcut>(sh26);

            var sh27 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("ee74dd92-d18b-46cf-91b7-3946ab55427c"),
                ShortcutName = "Copy Selection",
                RouteType = 1,
                KeyString = "Control+C",
                DefaultKeyString = "Control+C"
            };
            await AddItemAsync<MpShortcut>(sh27);

            var sh28 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("ac8abe92-82c3-46fb-9bd5-39d74b100b23"),
                ShortcutName = "Scroll Home",
                RouteType = 1,
                KeyString = "Home",
                DefaultKeyString = "Home"
            };
            await AddItemAsync<MpShortcut>(sh28);

            var sh29 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("ac8abe92-82c3-46fb-9bd5-39d74b100b23"),
                ShortcutName = "Scroll End",
                RouteType = 1,
                KeyString = "End",
                DefaultKeyString = "End"
            };
            await AddItemAsync<MpShortcut>(sh28);

            var sh30 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("9b0ca09a-5724-4004-98d2-f5ef8ae02055"),
                ShortcutName = "Scroll Up",
                RouteType = 1,
                KeyString = "Up",
                DefaultKeyString = "Up"
            };
            await AddItemAsync<MpShortcut>(sh28);

            var sh31 = new MpShortcut() {
                ShortcutGuid = Guid.Parse("39a6194e-37e3-4d37-a9f4-254ed83157f2"),
                ShortcutName = "Scroll Down",
                RouteType = 1,
                KeyString = "Down",
                DefaultKeyString = "Down"
            };
            await AddItemAsync<MpShortcut>(sh28);
            #endregion

            #region Anayltic Items

            var ai1 = await MpAnalyticItem.Create(
                        "https://api.cognitive.microsofttranslator.com/{0}",
                        MpPreferences.Instance.AzureCognitiveServicesKey,
                        MpCopyItemType.RichText,
                        MpOutputFormatType.Text,
                        "Language Translator",
                        "Azure Cognitive-Services Language Translator",
                        MpHelpers.Instance.ReadTextFromResource(
                            "MonkeyPaste.Resources.Data.Analytics.Formats.LanguageTranslator.azuretranslator.json", 
                            GetType().Assembly));

            var ai2 = await MpAnalyticItem.Create(
                        "https://api.openai.com/v1/",
                        MpPreferences.Instance.RestfulOpenAiApiKey,
                        MpCopyItemType.RichText,
                        MpOutputFormatType.Text,
                        "Open Ai",
                        "OpenAI is an artificial intelligence research laboratory consisting of the for-profit corporation OpenAI LP and its parent company, the non-profit OpenAI Inc.",
                        MpHelpers.Instance.ReadTextFromResource(
                            "MonkeyPaste.Resources.Data.Analytics.Formats.OpenAi.openai.json",
                            GetType().Assembly));

            // TODO add other analyzers here or better load w/ json

            #endregion

            #region Matcher

            /*
            -on end of startup MpMatchManager loads matchers,events and commands and registers for events
            -on copy item create invoke event w/ item from tray
            -in manager run all event commands with copy item
            -compare content w/ all clipboard matchers

            */

            var mr1 = await MpMatcher.Create(
                MpMatcherType.Contains,
                "cat",

                MpMatchTriggerType.ContentItemAdded,
                MpMatchActionType.Analyzer,
                24,

                MpMatchActionType.Classifier,
                32
                );

            #endregion

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
                    , ProcessName text
                    , AppName text 
                    , IsAppRejected integer NOT NULL   
                    , fk_MpUserDeviceId integer not null
                    , fk_MpIconId integer);   
                    
                    CREATE TABLE MpUrl (
                      pk_MpUrlId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpUrlGuid text not null
                    , UrlPath text NOT NULL 
                    , UrlDomainPath text 
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
                    , fk_MpAnalyticItemPresetId INTEGER DEFAULT 0
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
                    , SortOrderIdx integer
                    , ApiKey text 
                    , EndPoint text);   

                    CREATE TABLE MpAnalyticItemParameter (
                      pk_MpAnalyticItemParameterId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpAnalyticItemParameterGuid text not null
                    , fk_MpAnalyticItemId integer not null
                    , ParameterTypeId integer not null default 0
                    , ParameterValueTypeId integer not null default 0
                    , Label text
                    , EnumId integer default 0
                    , SortOrderIdx integer
                    , Description text
                    , IsRequired integer not null default 0
                    , IsReadOnly integer not null default 0
                    , FormatInfo text); 

                    CREATE TABLE MpAnalyticItemParameterValue (
                      pk_MpAnalyticItemParameterValueId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpAnalyticItemParameterValueGuid text not null
                    , fk_MpAnalyticItemParameterId integer not null
                    , Value text
                    , Label text
                    , Description text
                    , IsDefault integer not null default 0   
                    , IsMinimum integer not null default 0
                    , IsMaximum integer not null default 0); 
                    
                    CREATE TABLE MpAnalyticItemPreset (
                      pk_MpAnalyticItemPresetId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpAnalyticItemPresetGuid text not null
                    , fk_MpAnalyticItemId integer not null
                    , fk_MpIconId integer              
                    , Label text
                    , Description text
                    , SortOrderIdx integer
                    , IsReadOnly integer not null default 0                    
                    , IsQuickAction integer not null default 0); 

                    CREATE TABLE MpAnalyticItemPresetParameterValue (
                      pk_MpAnalyticItemPresetParameterValueId INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT
                    , MpAnalyticItemPresetParameterValueGuid text not null
                    , fk_MpAnalyticItemPresetId integer not null
                    , ParameterEnumId integer
                    , Value text
                    , DefaultValue text); 

                    CREATE VIEW MpSortableCopyItem_View
                    AS 
                    SELECT
	                    pk_MpCopyItemId,
	                    fk_ParentCopyItemId,
	                    fk_MpCopyItemTypeId,
	                    CompositeSortOrderIdx,
	                    Title,
	                    ItemData,
	                    ItemDescription,
	                    CopyDateTime,
	                    CopyCount,
	                    PasteCount,
	                    MpSource.pk_MpSourceId AS SourceId,
	                    MpApp.AppName,
	                    MpApp.SourcePath as AppPath,
	                    MpApp.pk_MpAppId AS AppId,
	                    MpUrl.pk_MpUrlId AS UrlId,
	                    MpUrl.UrlPath,
	                    MpUrl.UrlTitle
                    FROM
	                    MpCopyItem
                    INNER JOIN MpSource ON MpSource.pk_MpSourceId = MpCopyItem.fk_MpSourceId
                    INNER JOIN MpApp ON MpApp.pk_MpAppId = MpSource.fk_MpAppId
                    LEFT JOIN MpUrl ON MpUrl.pk_MpUrlId = MpSource.fk_MpUrlId;
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

        public async Task<List<MpDbLog>> GetDbObjectLogs(string dboGuid, DateTime fromDtUtc) {
            var logs = await MpDataModelProvider.Instance.GetDbLogsByGuidAsync(dboGuid, fromDtUtc);
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

        public async Task UpdateSyncHistory(string otherDeviceGuid, DateTime utcDtSentLocalChanges) {
            MpSyncHistory sh = await MpDataModelProvider.Instance.GetSyncHistoryByDeviceGuid(otherDeviceGuid);

            if (sh == null) {
                sh = new MpSyncHistory() {
                    OtherClientGuid = otherDeviceGuid,
                    SyncDateTime = utcDtSentLocalChanges
                };
            } else {
                sh.SyncDateTime = utcDtSentLocalChanges;
            }

            await MpDb.Instance.AddOrUpdateAsync<MpSyncHistory>(sh);
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
