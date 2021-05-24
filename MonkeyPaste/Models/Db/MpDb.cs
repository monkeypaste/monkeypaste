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
    public class MpDb : MpICopyItemImporter {
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
        public bool IsLoaded { get; set; }
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
        private async Task CreateConnectionAsync() {
            if (_connectionAsync != null) {
                return;
            }
            if (File.Exists(MpDbConstants.DbPath)) {
                File.Delete(MpDbConstants.DbPath);
            }
            _connectionAsync = new SQLiteAsyncConnection(MpDbConstants.DbPath, MpDbConstants.Flags);
            
            await _connectionAsync.CreateTableAsync<MpCopyItem>();
            await _connectionAsync.CreateTableAsync<MpTag>();
            await _connectionAsync.CreateTableAsync<MpCopyItemTag>();
            await _connectionAsync.CreateTableAsync<MpApp>();
            await _connectionAsync.CreateTableAsync<MpColor>();
            await _connectionAsync.CreateTableAsync<MpUrl>();
            await _connectionAsync.CreateTableAsync<MpUrlDomain>();
            await _connectionAsync.CreateTableAsync<MpIcon>();

            int colorCount = await _connectionAsync.Table<MpColor>().CountAsync();
            var green = new MpColor(Color.Green);
            if (colorCount == 0) {
                await _connectionAsync.InsertAsync(green);
                await _connectionAsync.InsertAsync(new MpColor(Color.Blue));
                await _connectionAsync.InsertAsync(new MpColor(Color.Yellow));
                await _connectionAsync.InsertAsync(new MpColor(Color.Orange));

                var recentTag = new MpTag() {
                    TagName = "Recent",
                    ColorId = 1,
                    TagSortIdx = 0
                };

                await _connectionAsync.InsertAsync(recentTag);

                var allTag = new MpTag() {
                    TagName = "All",
                    ColorId = 2,
                    TagSortIdx = 1
                };
                await _connectionAsync.InsertAsync(allTag);
                var favTag = new MpTag() {
                    TagName = "Favorites",
                    ColorId = 3,
                    TagSortIdx = 2
                };
                await _connectionAsync.InsertAsync(favTag);
                var helpTag = new MpTag() {
                    TagName = "Help",
                    ColorId = 4,
                    TagSortIdx = 3
                };
                await _connectionAsync.InsertAsync(helpTag);

                MpConsole.WriteLine(@"Create all default tables");
            } 

            if (_connectionAsync != null && UseWAL) {
                // On sqlite-net v1.6.0+, enabling write-ahead logging allows for faster database execution
                await _connectionAsync.EnableWriteAheadLoggingAsync().ConfigureAwait(false);
            }
            MpConsole.WriteLine("Write ahead logging: " + (UseWAL ? "ENABLED" : "DISABLED"));
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
            //await CreateConnection();
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
            await _connectionAsync.DeleteAsync<T>(item);
            OnItemDeleted?.Invoke(this, item as MpDbObject);
        }

        public async Task UpdateWithChildren(MpDbObject dbo) {
            await _connectionAsync.UpdateWithChildrenAsync(dbo);
        }

        public async Task<T> GetWithChildren<T>(T item) where T: new() {
            return await _connectionAsync.GetWithChildrenAsync<T>((item as MpDbObject).Id);
        }

        public async Task<List<T>> GetAllWithChildren<T>() where T : new() {
            return await _connectionAsync.GetAllWithChildrenAsync<T>();
        }
        #region Private Methods
        #endregion


        public void InitUser(string idToken) {
            // User = new MpUser() { IdentityToken = idToken };
        }
        public void InitClient(string accessToken) {
            //Client = new MpClient(0, 3, MpHelpers.Instance.GetCurrentIPAddress()/*.MapToIPv4()*/.ToString(), accessToken, DateTime.Now);
        }

        public async Task<ObservableCollection<MpCopyItem>> Get(int tagId, int start, int count, string sortColumn = "Id", bool isDescending = false)
        {
            //SELECT
            //user_number,
            //user_name
            //FROM user_table
            //WHERE(user_name LIKE '%{1}%' OR user_number LIKE '%{2}%')
            //AND user_category = { 3 } OR user_category = { 4 }
            //ORDER BY user_uid LIMIT { 5}
            //OFFSET { 6}
            //Where { 5} is page size and { 6 } is page number * page size.

            var result = await QueryAsync<MpCopyItem>(
                                string.Format(
                                    @"SELECT * from MpCopyItem
                                      WHERE Id in 
                                        (SELECT CopyItemId FROM MpCopyItemTag 
                                         WHERE TagId=?)
                                      ORDER BY {0} {1} LIMIT ? OFFSET ?",
                                    sortColumn,
                                    (isDescending ? "DESC":"ASC")),
                                tagId,
                                count,
                                start);

            return new ObservableCollection<MpCopyItem>(result);

            //var items = await GetItems<MpCopyItem>();
            //return new ObservableCollection<MpCopyItem>(items);

            //return new ObservableCollection<MpCopyItem>(items.GetRange(start, count));
        }
    }
}
