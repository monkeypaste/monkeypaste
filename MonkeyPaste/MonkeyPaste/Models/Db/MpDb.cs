using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpDb {
        #region Singleton
        private static readonly Lazy<MpDb> _Lazy = new Lazy<MpDb>(() => new MpDb());
        public static MpDb Instance { get { return _Lazy.Value; } }

        private MpDb() { }
        #endregion

        #region Private Variables
        public static bool IsLoaded = false;
        private SQLiteAsyncConnection _connection;
        #endregion

        #region Properties
        public string IdentityToken { get; set; }
        public string AccessToken { get; set; }
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

            if (await _connection.Table<MpCopyItem>().CountAsync() == 0) {
                await _connection.InsertAsync(new MpCopyItem() {
                    Title = "First copy item",
                    CopyItemText = "Test first item",
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

        public async Task<List<T>> GetItems<T>() where T : new() {
            return await _connection.Table<T>().ToListAsync();
        }

        public async Task AddItem<T>(T item) where T: new() {
            await _connection.InsertAsync(item);
            //OnItemAdded?.Invoke(this, item);
        }

        public async Task UpdateItem<T>(T item) where T: new() {
            //await CreateConnection();
            await _connection.UpdateAsync(item);
            //OnItemUpdated?.Invoke(this, item);
        }

        public async Task AddOrUpdate<T>(T item) where T: new() {
            if ((item as MpDbObject).Id == 0) {
                await AddItem(item);
            } else {
                await UpdateItem(item);
            }
        }

        public Task DeleteItem(MpCopyItem item) {
            throw new NotImplementedException();
        }

        public void InitUser(string idToken) {
            // User = new MpUser() { IdentityToken = idToken };
        }
        public void InitClient(string accessToken) {
            //Client = new MpClient(0, 3, MpHelpers.Instance.GetCurrentIPAddress()/*.MapToIPv4()*/.ToString(), accessToken, DateTime.Now);
        }
    }
}
