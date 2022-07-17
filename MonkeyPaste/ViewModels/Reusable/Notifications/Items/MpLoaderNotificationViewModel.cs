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
            IconResourceKey = MpPlatformWrapper.Services.PlatformResource.GetResource("AppImage") as string;
            OnPropertyChanged(nameof(IconSourceObj));
            await Task.Delay(1);
            ProgressLoader = progressLoader;
            IsBusy = false;
        }
 
        #endregion

        #region Private Methods

        #endregion
    }
}
