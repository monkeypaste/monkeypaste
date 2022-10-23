using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste.Common;

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

        public double ValueLoaded {
            get {
                if (ProgressLoader == null) {
                    return 0;
                }
                return ProgressLoader.PercentLoaded * 100;
            }
        }
        #endregion

        #region Model

        public override MpNotificationType NotificationType {
            get {
                if(ProgressLoader == null) {
                    return MpNotificationType.None;
                }
                return ProgressLoader.DialogType;
            }
        }

        public MpIProgressLoader ProgressLoader { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpLoaderNotificationViewModel() : base() { }

        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpNotificationFormat nf, object nfArgs) {            
            IsBusy = true;
            await base.InitializeAsync(nf, nfArgs);
            ProgressLoader = (MpIProgressLoader)nfArgs;
            IsBusy = false;
        }

        public override async Task<MpNotificationDialogResultType> ShowNotificationAsync() {
            var base_result = await base.ShowNotificationAsync();
            // NOTE returning loading notifies builder not to hide loader
            return MpNotificationDialogResultType.Loading;
        }
        #endregion

        #region Private Methods

        #endregion
    }
}
