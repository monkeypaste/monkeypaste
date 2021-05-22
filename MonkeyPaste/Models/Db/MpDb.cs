using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using SQLite;
using SQLiteNetExtensionsAsync.Extensions;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpDb : MpICopyItemImporter {
        #region Singleton
        private static readonly Lazy<MpDb> _Lazy = new Lazy<MpDb>(() => new MpDb());
        public static MpDb Instance { get { return _Lazy.Value; } }

        public MpDb() {
            Init();
        }
        #endregion

        #region Private Variables
        public static bool IsLoaded = false;
        private SQLiteAsyncConnection _connection;
        #endregion

        #region Properties
        public string IdentityToken { get; set; }
        public string AccessToken { get; set; }
        #endregion

        #region Events
        public event EventHandler<MpDbObject> OnItemAdded;
        public event EventHandler<MpDbObject> OnItemUpdated;
        public event EventHandler<MpDbObject> OnItemDeleted;
        #endregion


        private async Task Init() {
            InitUser(IdentityToken);
            InitClient(AccessToken);
            
            await CreateConnection();
            IsLoaded = true;
        }


        private async Task CreateConnection() {
            if (_connection != null) {
                return;
            }
            _connection = new SQLiteAsyncConnection(MpDbConstants.DbPath);
            
            await _connection.CreateTableAsync<MpCopyItem>();
            await _connection.CreateTableAsync<MpTag>();
            await _connection.CreateTableAsync<MpCopyItemTag>();
            await _connection.CreateTableAsync<MpApp>();
            await _connection.CreateTableAsync<MpColor>();
            await _connection.CreateTableAsync<MpUrl>();
            await _connection.CreateTableAsync<MpUrlDomain>();
            await _connection.CreateTableAsync<MpIcon>();

            ..await _connection.DeleteAllAsync<MpTag>();

            if (await _connection.Table<MpTag>().CountAsync() == 0) {
                await _connection.InsertAsync(new MpTag() {
                    TagName = "Recent",
                    TagColor = Color.Green.ToHex(),
                    TagSortIdx = 0
                });

                await _connection.InsertAsync(new MpTag() {
                    TagName = "All",
                    TagColor = Color.Blue.ToHex(),
                    TagSortIdx = 1
                });

                await _connection.InsertAsync(new MpTag() {
                    TagName = "Favorites",
                    TagColor = Color.Yellow.ToHex(),
                    TagSortIdx = 2
                });

                await _connection.InsertAsync(new MpTag() {
                    TagName = "Help",
                    TagColor = Color.Orange.ToHex(),
                    TagSortIdx = 3
                });
            }

            if (await _connection.Table<MpCopyItem>().CountAsync() == 0) {
                await _connection.InsertAsync(new MpCopyItem() {
                    Title = "Test Title 1",
                    ItemText = "Test item 1",
                    CopyDateTime = DateTime.Now
                });


                await _connection.InsertAsync(new MpCopyItem() {
                    Title = "Test Title 2",
                    ItemText = "Test item 2",
                    CopyDateTime = DateTime.Now
                });


                await _connection.InsertAsync(new MpCopyItem() {
                    Title = "Test Title 3",
                    ItemText = "Test item 3",
                    CopyDateTime = DateTime.Now
                });
            }
        }

        public async Task<int> ExecuteWriteAsync<T>(string query, Dictionary<string, object> args) where T : new() {
            if (string.IsNullOrEmpty(query.Trim())) {
                return 0;
            }
            return await _connection.ExecuteAsync(query, args);
        }

        public async Task<List<T>> ExecuteAsync<T>(string query, Dictionary<string, object> args) where T : new() {
            if (string.IsNullOrEmpty(query.Trim())) {
                return null;
            }
            
            return await _connection.QueryAsync<T>(query, args);
        }
        
        public async Task<int> GetLastRowIdAsync<T>() where T: new() {
            var result = await _connection.QueryAsync<T>("select * from " + typeof(T) + " ORDER BY Id DESC LIMIT 1;", null);
             if (result != null && result.Count > 0) {
                return (result as MpDbObject).Id;
            }
            return -1;
        }

        public async Task<List<T>> Query<T>(string query, params object[] args) where T: new() {
            var result = await _connection.QueryAsync<T>(query, args);
            return result;
        }

        public async Task<List<T>> GetItems<T>() where T : new() {
            return await _connection.Table<T>().ToListAsync();
        }

        public async Task AddItem<T>(T item) where T: new() {
            await _connection.InsertAsync(item);
            OnItemAdded?.Invoke(this, item as MpDbObject);
        }

        public async Task UpdateItem<T>(T item) where T: new() {
            //await CreateConnection();
            await _connection.UpdateAsync(item);
            OnItemUpdated?.Invoke(this, item as MpDbObject);
        }

        public async Task AddOrUpdate<T>(T item) where T: new() {
            if ((item as MpDbObject).Id == 0) {
                await AddItem(item);
            } else {
                await UpdateItem(item);
            }
        }

        public async Task DeleteItem(MpCopyItem item) {
            await _connection.DeleteAsync(item);
            OnItemDeleted?.Invoke(this, item as MpDbObject);
        }

        public void InitUser(string idToken) {
            // User = new MpUser() { IdentityToken = idToken };
        }
        public void InitClient(string accessToken) {
            //Client = new MpClient(0, 3, MpHelpers.Instance.GetCurrentIPAddress()/*.MapToIPv4()*/.ToString(), accessToken, DateTime.Now);
        }

        public async Task<ObservableCollection<MpCopyItem>> Get(int tagId, int start, int count, Quality quality = Quality.Low)
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

            //var itemList = await ExecuteAsync<MpCopyItem>(
            //    @"SELECT * from MpCopyItem
            //      WHERE Id in (
            //        SELECT CopyItemId FROM MpCopyItemTag WHERE TagId=@tid)
            //      ORDER BY Id LIMIT @limit OFFSET @offset",
            //    new Dictionary<string, object>()
            //    {
            //        {"@tid",tagId },
            //        {"@limit",count },
            //        {"@offset",start }
            //    }
            //);

            //return new ObservableCollection<MpCopyItem>(itemList);

            var items = await GetItems<MpCopyItem>();
            return new ObservableCollection<MpCopyItem>(items);

            //return new ObservableCollection<MpCopyItem>(items.GetRange(start, count));
        }
    }
}
