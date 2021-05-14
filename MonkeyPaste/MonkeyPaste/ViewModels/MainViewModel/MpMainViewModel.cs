
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpMainViewModel : MpViewModelBase {
        #region Private Variables

        #endregion

        #region Properties
        public MpCopyItemCollectionViewModel CopyItemCollectionViewModel { get; set; } = new MpCopyItemCollectionViewModel();
        #endregion

        #region Public Methods
        public MpMainViewModel() : base() {
            CopyItemCollectionViewModel = new MpCopyItemCollectionViewModel();
        }
        #endregion

        #region Commands

        #endregion        
    }
}
