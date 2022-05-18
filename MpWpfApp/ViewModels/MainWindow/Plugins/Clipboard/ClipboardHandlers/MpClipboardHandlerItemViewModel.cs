using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpClipboardHandlerItemViewModel :
        MpSelectorViewModelBase<MpClipboardHandlerCollectionViewModel, MpHandledClipboardFormatViewModel>,
        MpISelectableViewModel,
        MpIHoverableViewModel,
        MpIMenuItemViewModel {

        #region Properties

        #region View Models


        #endregion

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }

        #endregion

        #region MpIHoverableViewModel Implementation

        public bool IsHovering { get; set; }

        #endregion

        #region MpIMenuItemViewModel Implementation

        public MpMenuItemViewModel MenuItemViewModel { get; }

        #endregion

        #region Model

        public MpPluginFormat PluginFormat { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpClipboardHandlerItemViewModel(MpClipboardHandlerCollectionViewModel parent) : base(parent) { }



        #endregion

        #region Public Methods

        

        #endregion
    }
}
