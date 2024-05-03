using MonkeyPaste.Common;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvLoaderNotificationViewModel : MpAvNotificationViewModelBase {
        #region Private Variables
        #endregion

        #region Properties

        #region View Models
        public override MpAvMenuItemViewModel PopupMenuViewModel {
            get {
                var cmvm = base.PopupMenuViewModel;
                var cmvm_sil = cmvm.SubItems.ToList();
                cmvm_sil.Add(new MpAvMenuItemViewModel() {
                    HasLeadingSeparator = true,
                    Header = UiStrings.CommonCancelLabel,
                    IconResourceKey = "SignOutImage",
                    Command = MpAvSystemTrayViewModel.Instance.ExitApplicationCommand,
                    CommandParameter = "loader cancel click"
                });
                cmvm.SubItems = cmvm_sil;
                return cmvm;
            }
        }
        #endregion

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
            if(IsDoNotShowType) {
                await DoLoaderAsync();
                return MpNotificationDialogResultType.DoNotShow;
            }
            return BeginShow();
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
