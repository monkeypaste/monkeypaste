using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public static class MpDataModelCleanupHelpers {

        public static async Task<List<MpCopyItemTag>> GetUnlinkedCopyItemTagIds() {
            var citl = await MpDataModelProvider.GetItemsAsync<MpCopyItemTag>();
            var cil = await MpDataModelProvider.GetItemsAsync<MpCopyItem>();
            return citl.Where(x => cil.All(y => y.Id != x.CopyItemId)).ToList();
        }


    }
}
