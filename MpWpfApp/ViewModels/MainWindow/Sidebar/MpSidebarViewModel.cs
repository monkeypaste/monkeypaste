using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpSidebarViewModel : 
        MpViewModelBase, 
        MpISingletonViewModel<MpSidebarViewModel> {
        #region Properties

        #region View Models

        public MpAnalyticItemCollectionViewModel AnalyticItemCollectionViewModel => MpAnalyticItemCollectionViewModel.Instance;

        public MpClipboardHandlerCollectionViewModel ClipboardHandlerCollectionViewModel => MpClipboardHandlerCollectionViewModel.Instance;

        public MpTagTrayViewModel TagTrayViewModel => MpTagTrayViewModel.Instance;

        public MpISelectableViewModel SelectedItem { get; set; }

        #endregion

        #endregion

        #region Constructors
        private static MpSidebarViewModel _instance;
        public static MpSidebarViewModel Instance => _instance ?? (_instance = new MpSidebarViewModel());

        public MpSidebarViewModel() : base() { }

        #endregion

        #region Public Methods

        public async Task Init() {
            IsBusy = true;

            await Task.Delay(1);
            IsBusy = false;
        }

        #endregion
    }
}
