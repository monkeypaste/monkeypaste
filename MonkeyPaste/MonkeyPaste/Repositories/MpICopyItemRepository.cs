using MonkeyPaste.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste.Repositories {
    public interface MpICopyItemRepository {
        event EventHandler<MpCopyItem> OnItemAdded;
        event EventHandler<MpCopyItem> OnItemUpdated;
        event EventHandler<MpCopyItem> OnItemDeleted;

        Task<List<MpCopyItem>> GetItems();
       
        Task AddItem(MpCopyItem item);
        Task UpdateItem(MpCopyItem item);
        Task AddOrUpdate(MpCopyItem item);
        Task DeleteItem(MpCopyItem item);
    }
}

