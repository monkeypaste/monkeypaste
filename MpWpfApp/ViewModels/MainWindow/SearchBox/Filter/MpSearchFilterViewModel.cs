using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpSearchFilterViewModel : MpViewModelBase<MpSearchBoxViewModel> {
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

        public MpSearchFilterViewModel() : base(null) { }

        public MpSearchFilterViewModel(MpSearchBoxViewModel parent, bool isSeperator) : base(parent) {
            IsSeperator = isSeperator;
        }

        public MpSearchFilterViewModel(MpSearchBoxViewModel parent,string label, string prefName, MpContentFilterType filterType) : base(parent) {
            PropertyChanged += MpSearchFilterViewModel_PropertyChanged;
            _filterType = filterType;
            Label = label;
            PreferenceName = prefName;
            IsChecked = (bool)MpPreferences.Instance[PreferenceName];
        }

        #endregion

        #region Private Methods

        private void MpSearchFilterViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsChecked):
                    MpPreferences.Instance[PreferenceName] = IsChecked;
                    break;
            }
        }

        #endregion
    }
}
