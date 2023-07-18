using MonkeyPaste.Common;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public enum MpWelcomePageType {
        Intro,
        Security,
        Input,
        Account
    }
    public class MpAvWelcomeNotificationViewModel :
        MpAvNotificationViewModelBase {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        #region State
        public bool IsWelcomeDone { get; set; } = false;
        public override bool IsShowOnceNotification =>
            true;
        public MpWelcomePageType CurPageType { get; set; } = MpWelcomePageType.Intro;

        public bool CanSelectPrevious =>
            (int)CurPageType > 0;
        public bool CanSelectNext =>
            (int)CurPageType + 1 < typeof(MpWelcomePageType).Length();

        public bool CanFinish =>
            (int)CurPageType + 1 >= typeof(MpWelcomePageType).Length();

        #endregion
        #endregion

        #region Constructors
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
            //IsWindowOpen = false;
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
                IsWelcomeDone = true;
            });

        public ICommand FinishWelcomeCommand => new MpCommand(
            () => {
                IsWelcomeDone = true;
            }, () => {
                return CanFinish;
            });

        #endregion


    }
}
