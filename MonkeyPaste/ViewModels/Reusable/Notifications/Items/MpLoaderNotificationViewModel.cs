using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste {
    public class MpLoaderNotificationViewModel : MpNotificationViewModelBase {
        #region Properties

        #region Appearance

        public double ProgressTotalBarWidth { get; set; }

        public double ProgressBarCurrentWidth => ProgressTotalBarWidth * PercentLoaded;

        public string LoadingLabel { get; set; }

        public string PercentLabel {
            get {
                int percent = (int)(PercentLoaded * 100);
                return $"{percent} %";
            }
        }

        #endregion

        #region State

        public bool IsLoaded => PercentLoaded >= 1.0;

        public double PercentLoaded { get; set; }

        #endregion

        #region Model

        #endregion

        #endregion
        #region Constructors

        public MpLoaderNotificationViewModel() : base(null) { }

        public MpLoaderNotificationViewModel(MpNotificationBalloonViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        #endregion
    }
}
