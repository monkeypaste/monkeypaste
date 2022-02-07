using System.Collections.Generic;

namespace MpWpfApp {
    public class MpEmptyActionViewModel : MpActionViewModelBase {
        #region Properties

        public bool IsVisible {
            get {
                if(ParentActionViewModel == null ||
                    Parent.PrimaryAction == null) {
                    return false;
                }
                return IsSelected || ParentActionViewModel.ActionId == Parent.PrimaryAction.ActionId;
            }
        }

        public string AddNewButtonBorderBrushHexColor {
            get {
                if (IsHovering) {
                    return MpSystemColors.LightGray;
                }
                return MpSystemColors.DarkGray;
            }
        }

        #endregion
        #region Constructors

        public MpEmptyActionViewModel(MpActionCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpEmptyActionViewModel_PropertyChanged;
        }

        private void MpEmptyActionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsVisible):
                    if(IsVisible && ParentActionViewModel != null) {
                        X = ParentActionViewModel.X;
                        Y = ParentActionViewModel.Y - (Height * 1.75);
                    }
                    break;
            }
        }
        #endregion

        #region Public Methods

        #endregion
    }
}
