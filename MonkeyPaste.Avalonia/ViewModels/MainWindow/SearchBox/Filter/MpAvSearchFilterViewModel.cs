using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MonkeyPaste.Avalonia {
    public class MpAvSearchFilterViewModel : MpViewModelBase<MpAvSearchBoxViewModel> {
        #region Private Variables

        private MpContentFilterType _filterType = MpContentFilterType.None;
        
        #endregion

        #region Properties

        #region Appearance

        public string Label { get; set; }

        #endregion

        #region Model

        public bool IsSeperator { get; set; } = false;

        public bool IsChecked { get; set; }

        public bool IsEnabled { get; set; } = true;

        public string PreferenceName { get; set; }

        public MpContentFilterType FilterValue => IsChecked ? _filterType : MpContentFilterType.None;

        #endregion

        #endregion

        #region Constructors

        public MpAvSearchFilterViewModel() : base(null) { }

        public MpAvSearchFilterViewModel(MpAvSearchBoxViewModel parent, bool isSeperator) : base(parent) {
            IsSeperator = isSeperator;
        }

        public MpAvSearchFilterViewModel(MpAvSearchBoxViewModel parent,string label, string prefName, MpContentFilterType filterType) : base(parent) {
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
            switch(e.PropertyName) {
                case nameof(IsChecked):
                    MpPrefViewModel.Instance[PreferenceName] = IsChecked;
                    break;
            }
        }

        #endregion
    }
}
