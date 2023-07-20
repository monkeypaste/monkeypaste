using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public enum MpWelcomePageType {
        Welcome,
        Account,
        Shortcuts,
        ScrollWheel,
        DbPassword
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

        private static MpAvWelcomeNotificationViewModel _instance;
        public static MpAvWelcomeNotificationViewModel Instance => _instance ?? (_instance = new MpAvWelcomeNotificationViewModel());
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region View Models
        public MpAvWelcomeOptionGroupViewModel GreetingViewModel { get; set; }
        public MpAvWelcomeOptionGroupViewModel AccountViewModel { get; set; }
        public MpAvWelcomeOptionGroupViewModel GestureProfilesViewModel { get; set; }
        public MpAvWelcomeOptionGroupViewModel ScrollWheelBehaviorViewModel { get; set; }
        public MpAvWelcomeOptionGroupViewModel DbPasswordViewModel { get; set; }

        private MpAvWelcomeOptionGroupViewModel[] _items;
        public MpAvWelcomeOptionGroupViewModel[] Items {
            get {
                if (_items == null) {
                    _items = new[] {
                        GreetingViewModel,
                        AccountViewModel,
                        GestureProfilesViewModel,
                        ScrollWheelBehaviorViewModel,
                        DbPasswordViewModel
                    };
                }
                return _items;
            }
        }

        public MpAvWelcomeOptionItemViewModel HoverItem =>
            CurOptGroupViewModel == null || CurOptGroupViewModel.Items == null ?
                null :
                CurOptGroupViewModel.Items.FirstOrDefault(x => x.IsHovering);
        public MpAvWelcomeOptionGroupViewModel CurOptGroupViewModel =>
            Items[CurPageIdx];

        #endregion

        #region State
        public string DbPassword1 { get; set; }
        public string DbPassword2 { get; set; }
        public bool IsDbPasswordValid { get; private set; }

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
        public MpAvWelcomeNotificationViewModel() : base() {
            MpDebug.Assert(_instance == null, "Only 1 welcome vm should be created");
            _instance = this;
            MpConsole.WriteLine("Welcome vm created");
            GreetingViewModel = new MpAvWelcomeOptionGroupViewModel() {
                Title = "Welcome",
                Caption = "Hey! Let's setup a few things to improve your overall experience with MonkeyPaste.",
            };

            // TODO add pricing & capacity values as item description
            AccountViewModel = new MpAvWelcomeOptionGroupViewModel() {
                Title = "Subscription",
                Caption = "No features are limited by subscription, only storage capacity and can be changed at anytime. ",
                Items = new[] {
                    new MpAvWelcomeOptionItemViewModel(this,null) {
                        IconSourceObj = "LoginImage",
                        LabelText = "Restore"
                    },
                    new MpAvWelcomeOptionItemViewModel(this,MpUserAccountType.Free) {
                        IconSourceObj = "StarOutlineImage",
                        LabelText = "Free"
                    },
                    new MpAvWelcomeOptionItemViewModel(this,MpUserAccountType.Basic) {
                        IconSourceObj = "StarYellowImage",
                        LabelText = "Standard"
                    },
                    new MpAvWelcomeOptionItemViewModel(this,MpUserAccountType.Premium) {
                        IsChecked = true,
                        IconSourceObj = "TrophyImage",
                        LabelText = "Unlimited"
                    },
                }
            };

            GestureProfilesViewModel = new MpAvWelcomeOptionGroupViewModel() {
                Title = "Shortcuts",
                Caption = "Keyboard shortcuts can be reviewed or changed at anytime from the 'Settings->Shortcuts' menu. ",
                Items = new[] {
                    new MpAvWelcomeOptionItemViewModel(this,0) {
                        IsChecked = MpPrefViewModel.Instance.InitialStartupRoutingProfileType == MpShortcutRoutingProfileType.Internal,
                        IconSourceObj = "PrivateImage",
                        LabelText = MpShortcutRoutingProfileType.Internal.ToString(),
                        DescriptionText = "No global shortcuts will be enabled by default."
                    },
                    new MpAvWelcomeOptionItemViewModel(this,1) {
                        IsChecked = MpPrefViewModel.Instance.InitialStartupRoutingProfileType != MpShortcutRoutingProfileType.Internal,
                        IconSourceObj = "GlobeImage",
                        LabelText = MpShortcutRoutingProfileType.Global.ToString(),
                        DescriptionText = "MonkeyPaste's clipboard shortcuts will be available in all applications"
                    },
                }
            };
            ScrollWheelBehaviorViewModel = new MpAvWelcomeOptionGroupViewModel() {
                Title = "Scroll-to-Open",
                Caption = "When enabled, a scroll gesture at the top of the screen will reveal MonkeyPaste.",
                Items = new[] {
                    new MpAvWelcomeOptionItemViewModel(this,0) {
                        IsChecked = !MpPrefViewModel.Instance.DoShowMainWindowWithMouseEdgeAndScrollDelta,
                        IconSourceObj = "CloseWindowImage",
                        LabelText = "Disabled"
                    },
                    new MpAvWelcomeOptionItemViewModel(this,1) {
                        IsChecked = MpPrefViewModel.Instance.DoShowMainWindowWithMouseEdgeAndScrollDelta,
                        IconSourceObj = "AppFrameImage",
                        LabelText = "Enabled"
                    }
                }
            };

            DbPasswordViewModel = new MpAvWelcomeOptionGroupViewModel() {
                Title = "Password",
                Caption = "Your privacy is important and clipboard data can be very personal. Storage is always encrypted but you can set a password that will be required in case your device is stolen or someone else is using your device.",
            };
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
        protected override void MpNotificationViewModelBase_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            base.MpNotificationViewModelBase_PropertyChanged(sender, e);
            switch (e.PropertyName) {
                case nameof(CurPageIdx):
                    OnPropertyChanged(nameof(CurOptGroupViewModel));
                    break;
            }
        }

        #endregion

        #region Private Methods

        private void FinishWelcomeSetup() {
            IsWelcomeDone = true;

            // SHORTCUT PROFILE
            MpPrefViewModel.Instance.InitialStartupRoutingProfileType =
                GestureProfilesViewModel.Items.Any(x => x.IsChecked && (int)x.OptionId == 1) ?
                    MpShortcutRoutingProfileType.Global :
                    MpShortcutRoutingProfileType.Internal;

            // SCROLL-TO-SHOW
            MpPrefViewModel.Instance.DoShowMainWindowWithMouseEdgeAndScrollDelta =
                ScrollWheelBehaviorViewModel.Items.Any(x => x.IsChecked && (int)x.OptionId == 1);

            // ACCOUNT TYPE

            MpUserAccountType acct_type = MpUserAccountType.Free;
            if (AccountViewModel.Items.FirstOrDefault(x => x.IsChecked) is MpAvWelcomeOptionItemViewModel sel_acct_vm &&
                sel_acct_vm.OptionId is MpUserAccountType sel_acct_type) {
                // NOTE to work around login failures or no selection, just default to free i guess
                acct_type = sel_acct_type;
            }

            Mp.Services.AccountTools.SetAccountType(acct_type);

            // DB PASSWORD
            if (!string.IsNullOrEmpty(DbPassword1)) {
                Mp.Services.DbInfo.DbPassword = DbPassword1;
            }
        }
        #endregion

        #region Commands
        public ICommand SelectNextPageCommand => new MpAsyncCommand(
            async () => {
                if (CurPageType == MpWelcomePageType.Account) {
                    var test = Mp.Services.AccountTools;
                    var sel_acct_vm = AccountViewModel.Items.FirstOrDefault(x => x.IsChecked);
                    if (sel_acct_vm.OptionId == null) {
                        // TODO show login/restore thing here

                        return;
                    }
                    // TODO when not free, trigger platform subscription stuff here
                }
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
                IsDbPasswordValid = DbPassword1 == DbPassword2;
                if (!IsDbPasswordValid) {
                    return false;
                }
                return CanFinish;
            });

        #endregion

    }
}
