using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpCopyItemDetailViewModel : MpViewModelBase {
        #region Properties
        public MpCopyItem CopyItem { get; set; }
        #endregion

        //public void ApplyQueryAttributes(IDictionary<string, string> query) {
        //    // The query parameter requires URL decoding.
        //    int ciid = Convert.ToInt32(HttpUtility.UrlDecode(query["CopyItemId"]));
        //    LoadCopyItem(ciid);
        //}

        //void LoadCopyItem(int id) {
        //    try {
        //        Task.Run(async () => {
        //            CopyItem = await MpCopyItem.GetCopyItemById(id);
        //            OnPropertyChanged(nameof(CopyItem));
        //        });                
        //    } catch (Exception) {
        //        Console.WriteLine("Failed to load animal.");
        //    }
        //}
    }
}
