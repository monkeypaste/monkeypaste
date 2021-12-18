using System.Collections.ObjectModel;
using System;

namespace MpWpfApp {
    public class MpKeyViewModel : MpViewModelBase<MpKeyGestureViewModel> {
        #region Properties

        #region State

        public ObservableCollection<DateTime> UpDownTimes { get; set; } = new ObservableCollection<DateTime>();
                
        #endregion

        #region Model

        public MpKey Key { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpKeyViewModel() : base(null) { }
        public MpKeyViewModel(MpKeyGestureViewModel parent, MpKey key) : base(parent) {
            Key = key;
            UpDownTimes = new ObservableCollection<DateTime>();
        }

        #endregion
    }
}
