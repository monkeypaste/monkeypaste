using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpMainShellViewModel : MpViewModelBase {
        public MpTagTileCollectionViewModel TagCollectionViewModel { get; set; }
        
        public MpMainShellViewModel() {
            Task.Run(Initialize);
          
        }        

        private async Task Initialize() {
            MpTempFileManager.Instance.Init();
            //MpSocketClient.StartClient("192.168.43.209");
            MpSyncManager.Instance.Init();

            TagCollectionViewModel = new MpTagTileCollectionViewModel();
        }
    }
}
