using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
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
        private const string PRE_ESTABLISHED_USER_DB_PWD_TEXT = "<^&user has already set a password^&>";
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
        public bool IsDbPasswordValid { get; set; } = true;
        public bool IsDbPasswordProvided =>
            IsDbPasswordValid && !string.IsNullOrEmpty(DbPassword);
        public bool IsDbPasswordIgnored =>
            !IsDbPasswordProvided ||
            (IsDbPasswordProvided && DbPassword == PRE_ESTABLISHED_USER_DB_PWD_TEXT);

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

        public string AutoFillPassword { get; set; }
        public string DbPassword { get; set; }
        public string WelcomeTitle =>
            CurPageType.ToString();

        #endregion

        #endregion

        #region Constructors
        public MpAvWelcomeNotificationViewModel() : base() {
            MpDebug.Assert(_instance == null, "Only 1 welcome vm should be created");
            _instance = this;
            MpConsole.WriteLine("Welcome vm created");
            InitWelcomeItems();
        }

        #endregion

        #region Public Methods
        public override async Task<MpNotificationDialogResultType> ShowNotificationAsync() {
            var base_result = await base.ShowNotificationAsync();
            if (base_result == MpNotificationDialogResultType.DoNotShow) {
                return base_result;
            }
            await BeginWelcomeSetupAsync();

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
        private void InitWelcomeItems() {
            GreetingViewModel = new MpAvWelcomeOptionGroupViewModel() {
                Title = "Welcome",
                SplashIconSourceObj = "AppImage",
                Caption = "Hey! Let's setup a few things to improve your overall experience with MonkeyPaste.",
            };
            int free_ccap = Mp.Services.AccountTools.GetContentCapacity(MpUserAccountType.Free);
            int free_tcap = Mp.Services.AccountTools.GetTrashCapacity(MpUserAccountType.Free);
            int standard_ccap = Mp.Services.AccountTools.GetTrashCapacity(MpUserAccountType.Standard);
            // TODO add pricing & capacity values as item description
            AccountViewModel = new MpAvWelcomeOptionGroupViewModel() {
                Title = "Subscription",
                Caption = "No features are limited by subscription, only storage capacity and can be changed at anytime. ",
                Items = new[] {
                    new MpAvWelcomeOptionItemViewModel(this,null) {
                        IconSourceObj = "LoginImage",
                        LabelText = "Restore",
                        DescriptionText = "Restore your existing account.."
                    },
                    new MpAvWelcomeOptionItemViewModel(this,MpUserAccountType.Free) {
                        IconSourceObj = "StarOutlineImage",
                        LabelText = "Free",
                        DescriptionText = $"Content and archive is limited to {free_ccap} and {free_tcap} clips respectively. No syncing capabilities are enabled. More info here."
                    },
                    new MpAvWelcomeOptionItemViewModel(this,MpUserAccountType.Standard) {
                        IconSourceObj = "StarYellowImage",
                        LabelText = "Standard",
                        DescriptionText = $"$0.99/$9.99 (monthly/annually) {Environment.NewLine} Content is limited to {standard_ccap} clips with an unlimited archive and syncing across all devices. More info here."
                    },
                    new MpAvWelcomeOptionItemViewModel(this,MpUserAccountType.Unlimited) {
                        IsChecked = true,
                        IconSourceObj = "TrophyImage",
                        LabelText = "Unlimited",
                        DescriptionText = $"$2.99/$29.99 (monthly/annually) {Environment.NewLine} Unrestricted, unlimited storage (optimized for efficiency with millions of items) with syncing across all devices. More info here."
                    },
                }
            };

            GestureProfilesViewModel = new MpAvWelcomeOptionGroupViewModel() {
                Title = "Shortcuts",
                Caption = "Keyboard shortcuts can be reviewed or changed at anytime from the 'Settings->Shortcuts' menu.",
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
                        DescriptionText = "MonkeyPaste's clipboard shortcuts will be available in all applications."
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
                        LabelText = "Disabled",
                        DescriptionText = "Left-clicking the taskbar icon will still open MonkeyPaste."
                    },
                    new MpAvWelcomeOptionItemViewModel(this,1) {
                        IsChecked = MpPrefViewModel.Instance.DoShowMainWindowWithMouseEdgeAndScrollDelta,
                        IconSourceObj = "AppFrameImage",
                        LabelText = "Enabled",
                        DescriptionText = "More window preferences are available from the 'Settings->Preferences->Window' menu."
                    }
                }
            };

            DbPasswordViewModel = new MpAvWelcomeOptionGroupViewModel() {
                Title = "Password",
                SplashIconSourceObj = "LockImage",
                Caption = "Your privacy is important and clipboard data can be very personal. Storage is always encrypted but you can set a password that will be required in case your device is stolen or someone else is using your device.",
            };
        }
        private async Task BeginWelcomeSetupAsync() {
            IsWelcomeDone = false;
            bool is_pwd_already_set = await MpDb.CheckIsUserPasswordSetAsync();
            if (is_pwd_already_set) {
                // this is not initial startup, user has reset ntf
                // fill pwd with special const so finish knows not to use if still same
                AutoFillPassword = PRE_ESTABLISHED_USER_DB_PWD_TEXT;
            }
            while (!IsWelcomeDone) {
                await Task.Delay(100);
            }
        }

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
            if (!IsDbPasswordIgnored) {
                Mp.Services.DbInfo.DbPassword = DbPassword;
            }
        }
        #endregion

        #region Commands
        public ICommand SelectNextPageCommand => new MpAsyncCommand(
            async () => {
                await Task.Delay(1);
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
                if (!IsDbPasswordValid) {
                    return false;
                }
                return CanFinish;
            });

        #endregion

    }
}
