using System;
using System.Collections.Generic;
using MonkeyPaste;

namespace MonkeyPaste {
    public class MpColorPalletePopupMenuViewModel : MpViewModelBase, MpIPopupMenuViewModel, MpIUserColorViewModel {
        public event EventHandler<string> OnColorChanged;

        private string _userHexColor;
        public string UserHexColor {
            get => _userHexColor;
            set {
                if(_userHexColor != value) {
                    _userHexColor = value;
                    OnColorChanged?.Invoke(this, UserHexColor);
                    OnPropertyChanged(nameof(UserHexColor));
                }
            }
        }

        public MpMenuItemViewModel PopupMenuViewModel => new MpMenuItemViewModel() {
            SubItems = new List<MpMenuItemViewModel>() {
                MpMenuItemViewModel.GetColorPalleteMenuItemViewModel(this)
            }
        };

        public MpColorPalletePopupMenuViewModel() : base(null) { }
    }
}
