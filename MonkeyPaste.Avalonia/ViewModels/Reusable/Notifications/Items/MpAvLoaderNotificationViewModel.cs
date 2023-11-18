using MonkeyPaste.Common;
using System.ComponentModel;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvLoaderNotificationViewModel : MpAvNotificationViewModelBase {
        #region Private Variables
        #endregion

        #region Properties

        #region Appearance

        public double ProgressTotalBarWidth { get; set; }

        public double ProgressBarCurrentWidth {
            get {
                if (ProgressLoader == null) {
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
                if (ProgressLoader == null) {
                    return MpNotificationType.None;
                }
                return ProgressLoader.DialogType;
            }
        }

        public MpIProgressLoaderViewModel ProgressLoader {
            get {
                if (NotificationFormat == null) {
                    return null;
                }
                return NotificationFormat.OtherArgs as MpIProgressLoaderViewModel;
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpAvLoaderNotificationViewModel() : base() {
            if (ProgressLoader != null) {
                ProgressLoader.PropertyChanged += ProgressLoader_PropertyChanged;
            }
        }


        #endregion

        #region Public Methods


        public override async Task<MpNotificationDialogResultType> ShowNotificationAsync() {
            var base_result = await base.ShowNotificationAsync();
            if (base_result == MpNotificationDialogResultType.DoNotShow) {
                // when loader is DoNotShow base never shows it (and StartLoader is called from window)
                // so manually perform load
                if (ValueLoaded == 0) {
                    // don't restart loader if user JUST set don't show again 
                    await DoLoaderAsync();
                }

                return base_result;
            }
            // NOTE returning loading notifies builder not to hide loader
            return MpNotificationDialogResultType.Loading;
        }
        #endregion

        #region Private Methods

        private void ProgressLoader_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(MpIProgressLoaderViewModel.PercentLoaded):
                    OnPropertyChanged(nameof(ValueLoaded));
                    break;
            }
        }
        private async Task DoLoaderAsync() {

            await ProgressLoader.BeginLoaderAsync();
            await ProgressLoader.FinishLoaderAsync();
        }
        #endregion
    }
}
