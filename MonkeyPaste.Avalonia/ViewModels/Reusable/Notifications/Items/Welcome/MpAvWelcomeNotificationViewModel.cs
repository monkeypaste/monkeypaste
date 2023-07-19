using MonkeyPaste.Common;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public enum MpWelcomePageType {
        Welcome,
        Account,
        Keyboard,
        Mouse,
        //Security,
    }
    public class MpAvWelcomeNotificationViewModel :
        MpAvNotificationViewModelBase {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        public static async Task ShowWelcomeNotification(bool forceShow = false) {
            await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                new MpNotificationFormat() {
                    ForceShow = forceShow,
                    NotificationType = MpNotificationType.Welcome,
                    MaxShowTimeMs = -1
                });
        }
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region View Models
        public MpAvGestureProfileCollectionViewModel GestureProfilesViewModel { get; set; }
        #endregion

        #region State
        public bool IsWelcomeDone { get; set; } = false;
        public override bool IsShowOnceNotification =>
            true;

        public int CurPageIdx =>
            (int)CurPageType;

        public MpWelcomePageType CurPageType { get; set; } = MpWelcomePageType.Welcome;

        public bool CanSelectPrevious =>
            (int)CurPageType > 0;
        public bool CanSelectNext =>
            (int)CurPageType + 1 < typeof(MpWelcomePageType).Length();

        public bool CanFinish =>
            (int)CurPageType + 1 >= typeof(MpWelcomePageType).Length();

        #endregion

        #region Appearance

        public string WelcomeTitle =>
            CurPageType.ToString();

        #endregion

        #endregion

        #region Constructors
        public MpAvWelcomeNotificationViewModel() {
            GestureProfilesViewModel = new MpAvGestureProfileCollectionViewModel();
        }
        #endregion

        #region Public Methods
        public override async Task<MpNotificationDialogResultType> ShowNotificationAsync() {
            var base_result = await base.ShowNotificationAsync();
            if (base_result == MpNotificationDialogResultType.DoNotShow) {
                return base_result;
            }
            IsWelcomeDone = false;
            while (!IsWelcomeDone) {
                await Task.Delay(100);
            }
            return MpNotificationDialogResultType.Dismiss;
        }

        public override void HideNotification() {
            base.HideNotification();
            IsWindowOpen = false;
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        private void FinishWelcomeSetup() {
            IsWelcomeDone = true;
            MpPrefViewModel.Instance.InitialStartupRoutingProfileType =
                GestureProfilesViewModel.Items.FirstOrDefault(x => x.IsChecked).ProfileType;
        }
        #endregion

        #region Commands
        public ICommand SelectNextPageCommand => new MpCommand(
            () => {
                CurPageType = (MpWelcomePageType)((int)CurPageType + 1);
            },
            () => {
                return CanSelectNext;
            });
        public ICommand SelectPrevPageCommand => new MpCommand(
            () => {
                CurPageType = (MpWelcomePageType)((int)CurPageType - 1);
            },
            () => {
                return CanSelectPrevious;
            });

        public ICommand SkipWelcomeCommand => new MpCommand(
            () => {
                FinishWelcomeSetup();
            });

        public ICommand FinishWelcomeCommand => new MpCommand(
            () => {
                FinishWelcomeSetup();
            }, () => {
                return CanFinish;
            });

        #endregion

    }
}
