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
        //private SQLiteConnection _connectionAsync;
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
        #endregion

        #region Public Methods
        public async Task Init() {
            if (_connectionAsync != null) {
                return;
            }
            InitUser(IdentityToken);
            InitClient(AccessToken);

            await CreateConnectionAsync();
            IsLoaded = true;
        }

        public async Task<List<T>> QueryAsync<T>(string query, params object[] args) where T : new() {
            if(_connectionAsync == null) {
                await Init();
            }
            var result = await _connectionAsync.QueryAsync<T>(query, args);
            return result;
        }

        public async Task<List<T>> GetItems<T>() where T : new() {
            if (_connectionAsync == null) {
                await Init();
            }
            return await _connectionAsync.Table<T>().ToListAsync();
        }

        public async Task AddItem<T>(T item) where T : new() {
            if (_connectionAsync == null) {
                await Init();
            }
            MpDbLogTracker.TrackDbWrite(MpDbLogActionType.Create, item as MpDbModelBase);

            await _connectionAsync.InsertAsync(item);
            OnItemAdded?.Invoke(this, item as MpDbModelBase);
        }

        public async Task UpdateItem<T>(T item) where T : new() {
            if (_connectionAsync == null) {
                await Init();
            }
            MpDbLogTracker.TrackDbWrite(MpDbLogActionType.Modify, item as MpDbModelBase);

            await _connectionAsync.UpdateAsync(item);
            OnItemUpdated?.Invoke(this, item as MpDbModelBase);
        }

        public async Task AddOrUpdate<T>(T item) where T : new() {
            if ((item as MpDbModelBase).Id == 0) {
                await AddItem(item);
            } else {
                await UpdateItem(item);
            }
        }

        public async Task DeleteItem<T>(T item) where T: new() {
            if (_connectionAsync == null) {
                await Init();
            }
            MpDbLogTracker.TrackDbWrite(MpDbLogActionType.Delete, item as MpDbModelBase);

            await _connectionAsync.DeleteAsync<T>((item as MpDbModelBase).Id);
            OnItemDeleted?.Invoke(this, item as MpDbModelBase);
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
            await AddItem<MpColor>(green);
            await AddItem<MpColor>(blue);
            await AddItem<MpColor>(yellow);
            await AddItem<MpColor>(orange);

            await AddItem<MpTag>(new MpTag() {
                TagGuid = Guid.Parse("310ba30b-c541-4914-bd13-684a5e00a2d3"),
                TagName = "Recent",
                ColorId = green.Id,
                TagSortIdx = 0
            });
            await AddItem<MpTag>(new MpTag() {
                TagGuid = Guid.Parse("df388ecd-f717-4905-a35c-a8491da9c0e3"),
                TagName = "All",
                ColorId = blue.Id,
                TagSortIdx = 1
            });

            await AddItem<MpTag>(new MpTag() {
                TagGuid = Guid.Parse("54b61353-b031-4029-9bda-07f7ca55c123"),
                TagName = "Favorites",
                ColorId = yellow.Id,
                TagSortIdx = 2
            });
            await AddItem<MpTag>(new MpTag() {
                TagGuid = Guid.Parse("a0567976-dba6-48fc-9a7d-cbd306a4eaf3"),
                TagName = "Help",
                ColorId = orange.Id,
                TagSortIdx = 3
            });

            MpConsole.WriteTraceLine(@"Create all default tables");
        }
        #endregion

        #region Sync Data
        public async Task<List<object>> GetLocalData() {
            var ld = new List<object>();
            var cil = await GetItems<MpClip>();
            foreach(var c in cil) {
                //MpApp app = await MpApp.GetAppById(c.AppId);
                //app.Icon = await MpIcon.GetIconById(app.IconId);
                //app.Icon.IconImage = await MpDbImage.GetDbImageById(app.Icon.IconImageId);
                //c.App = app;

                //var color = await MpColor.GetColorById(c.ColorId);
                //if (color != null) {
                //    c.ItemColor = color;
                //}
                ld.Add(c);
            }
            return ld;
        }

        public async Task ProcessRemoteData(List<object> remoteData) {
            foreach(var rdi in remoteData) {
                Console.WriteLine(rdi.ToString());
            }
            await Task.Delay(10);
        }

        public string ConvertToJson(List<object> objList) {
            var sb = new StringBuilder();
            foreach(var obj in objList) {
                sb.Append(JsonConvert.SerializeObject(obj));
            }
            
            return sb.ToString();
        }

        public string GetLocalIp4Address() {
            return MpHelpers.Instance.GetLocalIp4Address();
        }

        public string GetExternalIp4Address() {
            return MpHelpers.Instance.GetExternalIp4Address();
        }

        public async Task<string> GetLocalLog() {
            var logItems = await MpDb.Instance.GetItems<MpDbLog>();
            var dbol = new List<MpISyncableDbObject>();
            foreach (var li in logItems) {
                dbol.Add(li as MpISyncableDbObject);
            }
            var dbMsgStr = MpDbMessage.Create(dbol);
            //var streamMsg = MpStreamMessage.
            return dbMsgStr;
        }

        public Task<MpStreamMessage> ProcessRemoteDbLog(MpDbMessage dbLogMessage) {
            foreach(var jdbo in dbLogMessage.JsonDbObjects) {
                string objTypeStr = jdbo.DbObjectType.ToString().ToLower();
                dynamic obj = JsonConvert.DeserializeObject(jdbo.DbObjectJson);
                if(objTypeStr.Contains("mptag")) {
                    //var tag = 
                }
            }
            return null;
        }

        public string GetThisClientGuid() {
            return MpPreferences.Instance.ThisClientGuidStr;
        }

        public bool IsWpf() {
            return false;
        }
        #endregion
    }
}
