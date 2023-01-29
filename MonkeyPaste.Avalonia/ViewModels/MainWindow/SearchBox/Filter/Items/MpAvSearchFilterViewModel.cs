using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MonkeyPaste;
using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvSearchFilterViewModel : MpViewModelBase<MpAvSearchFilterCollectionViewModel> {
        #region Private Variables

        private MpContentQueryBitFlags _filterType = MpContentQueryBitFlags.None;

        #endregion

        #region Properties

        public MpMenuItemViewModel MenuItemViewModel {
            get {
                if (IsSeperator) {
                    return new MpMenuItemViewModel() {
                        IsSeparator = true
                    };
                }
                return new MpMenuItemViewModel() {
                    IsChecked = IsChecked,
                    Command = ToggleIsCheckedCommand,
                    Header = Label,
                    IconBorderHexColor = MpSystemColors.Black,
                    IconHexStr = MpSystemColors.White,
                };
            }
        }

        #region Appearance

        public string Label { get; set; }

        #endregion

        #region Model

        public bool IsSeperator { get; set; } = false;

        public bool? IsChecked { get; set; }

        public bool IsEnabled => !IsChecked.IsNull();

        public string PreferenceName { get; set; }

        public MpContentQueryBitFlags FilterType => _filterType;

        public MpContentQueryBitFlags FilterValue => IsChecked.IsTrue() ? _filterType : MpContentQueryBitFlags.None;

        #endregion

        #endregion

        #region Constructors

        public MpAvSearchFilterViewModel() : base(null) { }

        public MpAvSearchFilterViewModel(MpAvSearchFilterCollectionViewModel parent, bool isSeperator) : base(parent) {
            IsSeperator = isSeperator;
        }

        public MpAvSearchFilterViewModel(MpAvSearchFilterCollectionViewModel parent, string label, string prefName, MpContentQueryBitFlags filterType) : base(parent) {
            PropertyChanged += MpSearchFilterViewModel_PropertyChanged;
            _filterType = filterType;
            Label = label;
            PreferenceName = prefName;
            if (MpPrefViewModel.Instance[PreferenceName] == null) {
                IsChecked = false;
            } else {
                IsChecked = (bool)MpPrefViewModel.Instance[PreferenceName];
            }

        }

        #endregion

        #region Private Methods

        private void MpSearchFilterViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsChecked):
                    MpPrefViewModel.Instance[PreferenceName] = IsChecked;
                    break;
            }
        }

        #endregion

        #region Commands

        public ICommand ToggleIsCheckedCommand => new MpCommand(
            () => {
                IsChecked = !IsChecked;
            }, () => {
                //if (FilterType == MpContentFilterType.FileType ||
                //FilterType == MpContentFilterType.ImageType ||
                //FilterType == MpContentFilterType.TextType) {

                //}

                //return true;
                return IsEnabled;
            });
        #endregion
    }
}
