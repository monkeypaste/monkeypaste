
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MonkeyPaste.Repositories {
    public class MpCopyItemRepository : MpICopyItemRepository {
        public event EventHandler<MpCopyItem> OnItemAdded;
        public event EventHandler<MpCopyItem> OnItemUpdated;
        public event EventHandler<MpCopyItem> OnItemDeleted;

        private SQLiteAsyncConnection _connection;

        private async Task CreateConnection() {
            if (_connection != null) {
                return;
            }
            var documentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var databasePath = Path.Combine(documentPath, "Mp.db");
            _connection = new SQLiteAsyncConnection(databasePath);
            await _connection.CreateTableAsync<MpCopyItem>();

            if (await _connection.Table<MpCopyItem>().CountAsync() == 0) {
                await _connection.InsertAsync(new MpCopyItem() {
                    Title = "First copy item",
                    CopyItemText = "Test first item",
                    CopyDateTime = DateTime.Now
                });
            }
        }

        public async Task<List<MpCopyItem>> GetItems() {
            await CreateConnection();
            return await _connection.Table<MpCopyItem>().ToListAsync();
        }

        public async Task AddItem(MpCopyItem item) {
            await CreateConnection();
            await _connection.InsertAsync(item);
            OnItemAdded?.Invoke(this, item);
        }

        public async Task UpdateItem(MpCopyItem item) {
            await CreateConnection();
            await _connection.UpdateAsync(item);
            OnItemUpdated?.Invoke(this, item);
        }

        public async Task AddOrUpdate(MpCopyItem item) {
            if (item.Id == 0) {
                await AddItem(item);
            } else {
                await UpdateItem(item);
            }
        }

        public Task DeleteItem(MpCopyItem item) {
            throw new NotImplementedException();
        }
    }
}



        