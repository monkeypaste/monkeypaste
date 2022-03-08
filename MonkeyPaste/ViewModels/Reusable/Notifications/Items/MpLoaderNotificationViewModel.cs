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

        public double ProgressBarCurrentWidth {
            get {
                if(ProgressLoader == null) {
                    return 0;
                }
                return ProgressTotalBarWidth * ProgressLoader.PercentLoaded;
            }
        }

        #endregion

        #region State

        #endregion

        #region Model

        public override MpNotificationDialogType DialogType {
            get {
                if(ProgressLoader == null) {
                    return MpNotificationDialogType.None;
                }
                return ProgressLoader.DialogType;
            }
        }

        public MpIProgressLoader ProgressLoader { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpLoaderNotificationViewModel() : base(null) { }

        public MpLoaderNotificationViewModel(MpNotificationCollectionViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpIProgressLoader progressLoader) {
            IsBusy = true;
            await Task.Delay(1);
            ProgressLoader = progressLoader;
            IsBusy = false;
        }
 
        #endregion

        #region Private Methods

        #endregion
    }
}
