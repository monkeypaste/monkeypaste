using System.Collections.Generic;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvEmptyActionViewModel : MpAvActionViewModelBase {
        #region Properties

        public bool IsDesignerVisible {
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
                    return MpSystemColors.lightgray;
                }
                return MpSystemColors.DarkGray;
            }
        }

        #endregion
        #region Constructors

        public MpAvEmptyActionViewModel(MpAvActionCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpEmptyActionViewModel_PropertyChanged;
        }

        private void MpEmptyActionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsDesignerVisible):
                    if(IsDesignerVisible && ParentActionViewModel != null) {
                        X = ParentActionViewModel.X;
                        Y = ParentActionViewModel.Y - (Height * 1.75);
                    }
                    break;
            }
        }
        #endregion

        #region Protected Methods

        protected override async Task Enable() {
            await Task.Delay(1);
            IsEnabled = true;
        }

        protected override async Task Disable() {
            await Task.Delay(1);
            IsEnabled = false;
        }

        #endregion
    }
}
