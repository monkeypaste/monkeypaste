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

namespace MonkeyPaste {
    public class MpDb {
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
        public bool UseWAL { get; set; } = false;
        public string IdentityToken { get; set; }
        public string AccessToken { get; set; }
        public bool IsLoaded { get; set; } = false;
        #endregion

        #region Events
        public event EventHandler<MpDbObject> OnItemAdded;
        public event EventHandler<MpDbObject> OnItemUpdated;
        public event EventHandler<MpDbObject> OnItemDeleted;
        #endregion


        public async Task Init() {
            InitUser(IdentityToken);
            InitClient(AccessToken);

            await CreateConnectionAsync();
            IsLoaded = true;
        }

        //public void SetDbPassword(string newPassword) {
        //    if(MpPreferences.DbPassword != newPassword) {
        //        MpPreferences.EncryptDb = true;
        //        //if db is not encrypted use rekey otherwise key
        //        string encryptQuery = string.Format(
        //            @"PRAGMA {0}={1}",
        //            string.IsNullOrEmpty(MpPreferences.DbPassword) ?
        //                "key" : "rekey",
        //            newPassword);
        //        _connectionAsync.QueryAsync<int>(encryptQuery);
        //        MpConsole.WriteLine(@"Db Password updated");
        //    }
        //}

        private async Task CreateConnectionAsync() {
            if (_connectionAsync != null) {
                return;
            }
            bool isNewDb = !File.Exists(MpPreferences.Instance.DbPath);

            var connStr = new SQLiteConnectionString(
                databasePath: MpPreferences.Instance.DbPath, 
                storeDateTimeAsTicks: true,
                key: MpPreferences.Instance.DbPassword,
                openFlags: MpPreferences.DbFlags
                );


            _connectionAsync = new SQLiteAsyncConnection(connStr);

            await InitTablesAsync();

            if(isNewDb) {
                await InitDefaultDataAsync();
            }


            if (_connectionAsync != null && UseWAL) {
                // On sqlite-net v1.6.0+, enabling write-ahead logging allows for faster database execution
                await _connectionAsync.EnableWriteAheadLoggingAsync().ConfigureAwait(false);
            }
            MpConsole.WriteTraceLine("Write ahead logging: " + (UseWAL ? "ENABLED" : "DISABLED"));
        }
        private async Task InitTablesAsync() {
            await _connectionAsync.CreateTableAsync<MpCopyItem>();
            await _connectionAsync.CreateTableAsync<MpTag>();
            await _connectionAsync.CreateTableAsync<MpCopyItemTag>();
            await _connectionAsync.CreateTableAsync<MpApp>();
            await _connectionAsync.CreateTableAsync<MpColor>();
            await _connectionAsync.CreateTableAsync<MpUrl>();
            await _connectionAsync.CreateTableAsync<MpUrlDomain>();
            await _connectionAsync.CreateTableAsync<MpIcon>();
            await _connectionAsync.CreateTableAsync<MpDbImage>();
        }
        private async Task InitDefaultDataAsync() {
            await AddItem<MpColor>(new MpColor(Color.Green));
            await AddItem<MpColor>(new MpColor(Color.Blue));
            await AddItem<MpColor>(new MpColor(Color.Yellow));
            await AddItem<MpColor>(new MpColor(Color.Orange));

            await AddItem<MpTag>(new MpTag() {
                TagName = "Recent",
                ColorId = 1,
                TagSortIdx = 0
            });
            await AddItem<MpTag>(new MpTag() {
                TagName = "All",
                ColorId = 2,
                TagSortIdx = 1
            });

            await AddItem<MpTag>(new MpTag() {
                TagName = "Favorites",
                ColorId = 3,
                TagSortIdx = 2
            });
            await AddItem<MpTag>(new MpTag() {
                TagName = "Help",
                ColorId = 4,
                TagSortIdx = 3
            });

            MpConsole.WriteTraceLine(@"Create all default tables");
        }
        public async Task<List<T>> QueryAsync<T>(string query, params object[] args) where T : new() {
            var result = await _connectionAsync.QueryAsync<T>(query, args);
            return result;
        }

        public async Task<List<T>> GetItems<T>() where T : new() {
            return await _connectionAsync.Table<T>().ToListAsync();
        }

        public async Task AddItem<T>(T item) where T : new() {
            await _connectionAsync.InsertAsync(item);
            OnItemAdded?.Invoke(this, item as MpDbObject);
        }

        public async Task UpdateItem<T>(T item) where T : new() {
            await _connectionAsync.UpdateAsync(item);
            OnItemUpdated?.Invoke(this, item as MpDbObject);
        }

        public async Task AddOrUpdate<T>(T item) where T : new() {
            if ((item as MpDbObject).Id == 0) {
                await AddItem(item);
            } else {
                await UpdateItem(item);
            }
        }

        public async Task DeleteItem<T>(T item) where T: new() {
            await _connectionAsync.DeleteAsync<T>((item as MpDbObject).Id);
            OnItemDeleted?.Invoke(this, item as MpDbObject);
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
        #region Private Methods
        #endregion


        public void InitUser(string idToken) {
            // User = new MpUser() { IdentityToken = idToken };
        }
        public void InitClient(string accessToken) {
            //Client = new MpClient(0, 3, MpHelpers.Instance.GetCurrentIPAddress()/*.MapToIPv4()*/.ToString(), accessToken, DateTime.Now);
        }       
    }
}
