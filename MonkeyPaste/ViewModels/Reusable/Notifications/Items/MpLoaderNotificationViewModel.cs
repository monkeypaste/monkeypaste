using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpLoaderNotificationViewModel : MpNotificationViewModelBase {
        #region Private Variables
        #endregion

        #region Properties

        #region Appearance

        public double ProgressTotalBarWidth { get; set; }

        public double ProgressBarCurrentWidth => ProgressTotalBarWidth * PercentLoaded;

        #endregion

        #region State
        
        public bool IsLoaded => PercentLoaded >= 1.0;

        public double PercentLoaded { get; set; } = 0.0;

        #endregion

        #region Model

        #endregion

        #endregion

        #region Constructors

        public MpLoaderNotificationViewModel() : base(null) { }

        public MpLoaderNotificationViewModel(MpNotificationCollectionViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        private void MpLoaderNotificationViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(PercentLoaded):
                    if (PercentLoaded > 1.0) {
                        PercentLoaded = 1.0;
                    }

                    OnPropertyChanged(nameof(ProgressBarCurrentWidth));
                    OnPropertyChanged(nameof(Detail));
                    OnPropertyChanged(nameof(IsLoaded));
                    break;
            }
        }

        #endregion
    }
}
