using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MonkeyPaste
{
    public interface MpICopyItemImporter
    {
        Task<ObservableCollection<MpCopyItem>> Get(int tagId, int start, int count, string sortColumn = "Id", bool isDescending = false);
    }
}
