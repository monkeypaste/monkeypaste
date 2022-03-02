using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpLoaderNotificationViewModel : MpNotificationViewModelBase {
        #region Private Variables
        private int _updateCount = 0;
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

        public override string IconImageBase64 => MpBase64Images.AppIcon;

        #endregion

        #endregion

        #region Constructors

        public MpLoaderNotificationViewModel() : base(null) { }

        public MpLoaderNotificationViewModel(MpNotificationBalloonViewModel parent) : base(parent) { }

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
