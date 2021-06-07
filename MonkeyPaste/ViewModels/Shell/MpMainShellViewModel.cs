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
            Task.Run(() => {
                //MpSocketClient.Instance.Init();
            });

            TagCollectionViewModel = new MpTagTileCollectionViewModel();            
        }        
    }
}
