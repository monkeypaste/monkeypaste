using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpMainShellViewModel : MpViewModelBase {
        public MpTagCollectionViewModel TagCollectionViewModel { get; set; }
        public static bool IsContextMenuOpen { get; set; }

        public MpMainShellViewModel() {
            //Device.BeginInvokeOnMainThread(async () => await Initialize());
            Task.Run(Initialize);
            
        }

        private async Task Initialize() {
            await MpDb.Instance.Init();
            TagCollectionViewModel = new MpTagCollectionViewModel();
            
        }
    }
}
