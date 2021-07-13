using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpMainShellViewModel : MpViewModelBase {
        #region Properties
        public MpTagTileCollectionViewModel TagCollectionViewModel { get; set; }
        #endregion

        #region Public Methods
        public MpMainShellViewModel() {
            Task.Run(async () => {
                await MpDb.Instance.Init();

                MpTempFileManager.Instance.Init();
                //MpSocketClient.StartClient("192.168.43.209");

                MpSyncManager.Instance.Init(MpDb.Instance);

                TagCollectionViewModel = new MpTagTileCollectionViewModel();
            });
        }
        #endregion

        #region Commands
        public ICommand SyncCommand => new Command<object>(async (args) => {
            MpDbLogTracker.PrintDbLog();
            //var ms = Application.Current.MainPage as MpMainShell;
            //var curDbBytes = MpDb.Instance.GetDbFileBytes();
            //ms.StorageService.CreateFile(@"mp_clone", curDbBytes, @".db");
        });
        #endregion
    }
}
