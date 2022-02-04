using System.Collections.Generic;

namespace MpWpfApp {
    public class MpEmptyActionViewModel : MpActionViewModelBase {
        #region Properties

        public bool IsVisible => ParentActionViewModel == null ? false : IsSelected || ParentActionViewModel.IsSelected;

        #endregion
        #region Constructors

        public MpEmptyActionViewModel(MpActionCollectionViewModel parent) : base(parent) {

        }
        #endregion

        #region Public Methods

        #endregion
    }
}
