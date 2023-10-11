using MonkeyPaste.Common;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public enum MpWelcomePageType {
        Greeting,
        Account,
        LoginLoad,
        GestureProfile,
        ScrollWheel,
        DragToOpen,
        DbPassword
    }
    public class MpAvWelcomeNotificationViewModel :
        MpAvNotificationViewModelBase {
        #region Private Variables
        #endregion

        #region Constants
        private const string PRE_ESTABLISHED_USER_DB_PWD_TEXT = "<^&user has already set a password^&>";

        #endregion

        #region Statics
        public static async Task ShowWelcomeNotificationAsync(bool forceShow = false) {
            await MpAvSubcriptionPurchaseViewModel.Instance.InitializeAsync();
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
        public MpAvWelcomeOptionGroupViewModel LoginLoadViewModel { get; set; }
        public MpAvWelcomeOptionGroupViewModel GestureProfilesViewModel { get; set; }
        public MpAvWelcomeOptionGroupViewModel ScrollWheelBehaviorViewModel { get; set; }
        public MpAvWelcomeOptionGroupViewModel DragToOpenBehaviorViewModel { get; set; }
        public MpAvWelcomeOptionGroupViewModel DbPasswordViewModel { get; set; }

        public MpAvPointerGestureWindowViewModel CurPointerGestureWindowViewModel { get; set; }

        private MpAvWelcomeOptionGroupViewModel[] _items;
        public MpAvWelcomeOptionGroupViewModel[] Items {
            get {
                if (_items == null) {
                    _items = new[] {
                        GreetingViewModel,
                        AccountViewModel,
                        LoginLoadViewModel,
                        GestureProfilesViewModel,
                        ScrollWheelBehaviorViewModel,
                        DragToOpenBehaviorViewModel,
                        DbPasswordViewModel
                    }.OrderBy(x => (int)x.WelcomePageType).ToArray();
                }
                return _items;
            }
        }

        public MpAvWelcomeOptionItemViewModel HoverItem =>
            CurOptGroupViewModel == null || CurOptGroupViewModel.Items == null ?
                null :
                CurOptGroupViewModel.Items.FirstOrDefault(x => x.IsHovering);

        public MpAvWelcomeOptionItemViewModel CheckedItem =>
            CurOptGroupViewModel == null || CurOptGroupViewModel.Items == null ?
                null :
                CurOptGroupViewModel.Items.FirstOrDefault(x => x.IsChecked);

        public MpAvWelcomeOptionItemViewModel PrimaryItem =>
            HoverItem == null ? CheckedItem : HoverItem;
        public MpAvWelcomeOptionGroupViewModel CurOptGroupViewModel =>
            Items[CurPageIdx];

        #endregion

        #region State
        public bool IsAccountOptSelected =>
            CurPageType == MpWelcomePageType.Account;
        public bool IsAccountMonthEnabled { get; set; } = false;
        public override bool WantsTopmost =>
            false;

        public bool IsGestureDemoOpen { get; set; }
        public bool IsPrimaryChecked =>
            PrimaryItem == CheckedItem;
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

        public MpWelcomePageType CurPageType { get; set; } = MpWelcomePageType.Greeting;

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
            MpConsole.WriteLine("Greeting vm created");
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
                case nameof(PrimaryItem):
                    OnPropertyChanged(nameof(IsPrimaryChecked));
                    break;
                case nameof(CurPageType):
                    OnPropertyChanged(nameof(IsAccountOptSelected));
                    OnPropertyChanged(nameof(WantsTopmost));
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));
                    CurOptGroupViewModel.OnPropertyChanged(nameof(CurOptGroupViewModel.IsGestureGroup));
                    CloseGestureDemo();
                    switch (CurPageType) {
                        case MpWelcomePageType.Account:
                            var test = Mp.Services.AccountTools;
                            var sel_acct_vm = AccountViewModel.Items.FirstOrDefault(x => x.IsChecked);
                            if (sel_acct_vm.OptionId == null) {
                                // TODO show login/restore thing here

                                return;
                            }
                            // TODO when not free, trigger platform subscription stuff here
                            break;
                        case MpWelcomePageType.ScrollWheel:
                        case MpWelcomePageType.DragToOpen:

                            break;
                    }

                    CurOptGroupViewModel.WasVisited = true;
                    break;
                case nameof(IsAccountMonthEnabled):
                    if (AccountViewModel == null) {
                        break;
                    }
                    AccountViewModel.Items =
                        MpAvSubcriptionPurchaseViewModel.Instance
                        .ToWelcomeOptionGroup(IsAccountMonthEnabled).Items;
                    break;
            }
        }

        #endregion

        #region Private Methods
        private void InitWelcomeItems() {
            #region Greeting
            GreetingViewModel = new MpAvWelcomeOptionGroupViewModel(this, MpWelcomePageType.Greeting) {
                SplashIconSourceObj = "AppImage",
                Title = UiStrings.WelcomeGreetingTitle,
                Caption = UiStrings.WelcomeGreetingCaption,
            };
            #endregion

            #region Account

            AccountViewModel = MpAvSubcriptionPurchaseViewModel.Instance.ToWelcomeOptionGroup(IsAccountMonthEnabled);

            #endregion

            #region LoginLoad
            LoginLoadViewModel = new MpAvWelcomeOptionGroupViewModel(this, MpWelcomePageType.LoginLoad) {
                Title = UiStrings.WelcomeLoginLoadTitle,
                Caption = UiStrings.WelcomeLoginLoadCaption
            };
            LoginLoadViewModel.Items = new[] {
                    new MpAvWelcomeOptionItemViewModel(this,false) {
                        IsChecked = false,
                        IconSourceObj = "NoEntryImage",
                        LabelText = UiStrings.CommonDisableLabel,
                        DescriptionText = UiStrings.WelcomeLoginLoadDescription1
                    },
                    new MpAvWelcomeOptionItemViewModel(this,true) {
                        IsChecked = true,
                        IconSourceObj = "UserImage",
                        LabelText = UiStrings.CommonEnableLabel,
                        DescriptionText = UiStrings.WelcomeLoginLoadDescription2
                    }
                };
            #endregion

            #region Gesture Profile
            GestureProfilesViewModel = new MpAvWelcomeOptionGroupViewModel(this, MpWelcomePageType.GestureProfile) {
                Title = UiStrings.WelcomeGestureProfileTitle,
                Caption = UiStrings.WelcomeGestureProfileCaption
            };
            GestureProfilesViewModel.Items = new[] {
                    new MpAvWelcomeOptionItemViewModel(this,false) {
                        IsChecked = MpAvPrefViewModel.Instance.DefaultRoutingProfileType == MpShortcutRoutingProfileType.Internal,
                        IconSourceObj = "PrivateImage",
                        LabelText = UiStrings.WelcomeGestureProfileLabel1,
                        DescriptionText = UiStrings.WelcomeGestureProfileDescription1
                    },
                    new MpAvWelcomeOptionItemViewModel(this,true) {
                        IsChecked = MpAvPrefViewModel.Instance.DefaultRoutingProfileType == MpShortcutRoutingProfileType.Global,
                        IconSourceObj = "GlobeImage",
                        LabelText = UiStrings.WelcomeGestureProfileLabel2,
                        DescriptionText = UiStrings.WelcomeGestureProfileDescription2
                    },
                };
            #endregion

            #region Scroll Wheel
            ScrollWheelBehaviorViewModel = new MpAvWelcomeOptionGroupViewModel(this, MpWelcomePageType.ScrollWheel) {
                Title = UiStrings.WelcomeScrollToOpenTitle,
                Caption = UiStrings.WelcomeScrollToOpenCaption
            };
            ScrollWheelBehaviorViewModel.Items = new[] {
                    new MpAvWelcomeOptionItemViewModel(this,false) {
                        IsChecked = !MpAvPrefViewModel.Instance.DoShowMainWindowWithMouseEdgeAndScrollDelta,
                        IconSourceObj = "NoEntryImage",
                        LabelText = UiStrings.CommonDisableLabel,
                        DescriptionText = UiStrings.WelcomeScrollToOpenDescription1
                    },
                    new MpAvWelcomeOptionItemViewModel(this,true) {
                        IsChecked = MpAvPrefViewModel.Instance.DoShowMainWindowWithMouseEdgeAndScrollDelta,
                        IconSourceObj = "MouseWheelImage",
                        LabelText = UiStrings.CommonEnableLabel,
                        DescriptionText = UiStrings.WelcomeScrollToOpenDescription2,
                    }
                };
            #endregion

            #region Drag To Open
            DragToOpenBehaviorViewModel = new MpAvWelcomeOptionGroupViewModel(this, MpWelcomePageType.DragToOpen) {
                Title = UiStrings.WelcomeDragToOpenTitle,
                Caption = UiStrings.WelcomeDragToOpenCaption
            };
            DragToOpenBehaviorViewModel.Items = new[] {
                    new MpAvWelcomeOptionItemViewModel(this,false) {
                        IsChecked = !MpAvPrefViewModel.Instance.ShowMainWindowOnDragToScreenTop,
                        IconSourceObj = "CloseWindowImage",
                        LabelText = UiStrings.CommonDisableLabel,
                        DescriptionText = UiStrings.WelcomeDragToOpenDescription1
                    },
                    new MpAvWelcomeOptionItemViewModel(this,true) {
                        IsChecked = MpAvPrefViewModel.Instance.ShowMainWindowOnDragToScreenTop,
                        IconSourceObj = "AppFrameImage",
                        LabelText = UiStrings.CommonEnableLabel,
                        DescriptionText = UiStrings.WelcomeDragToOpenDescription2
                    }
                };
            #endregion

            #region Db Password
            DbPasswordViewModel = new MpAvWelcomeOptionGroupViewModel(this, MpWelcomePageType.DbPassword) {
                SplashIconSourceObj = "LockImage",
                Title = UiStrings.WelcomeDbPasswordTitle,
                Caption = UiStrings.WelcomeDbPasswordCaption,
            };
            #endregion
        }
        private async Task BeginWelcomeSetupAsync() {
            IsWelcomeDone = false;
            Mp.Services.LoadOnLoginTools.SetLoadOnLogin(false);
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
            // NOTE this isn't flagged until DONE is clicked and assumes
            // there's per-page validation (only needed for accounts atm I think)
            MpAvPrefViewModel.Instance.IsWelcomeComplete = true;

            if (CurPointerGestureWindowViewModel != null) {
                CurPointerGestureWindowViewModel.Destroy();
            }

            // ACCOUNT TYPE

            MpUserAccountType acct_type = MpUserAccountType.Free;
            if (AccountViewModel.Items.FirstOrDefault(x => x.IsChecked) is MpAvWelcomeOptionItemViewModel sel_acct_vm &&
                sel_acct_vm.OptionId is MpUserAccountType sel_acct_type) {
                // NOTE to work around login failures or no selection, just default to free i guess
                acct_type = sel_acct_type;
            }

#if DEBUG
            MpAvPrefViewModel.Instance.TestAccountType = acct_type;
#endif

            Mp.Services.AccountTools.SetAccountType(acct_type);

            // LOGIN LOAD
            bool loadOnLogin =
                LoginLoadViewModel.Items.FirstOrDefault(x => x.OptionId is bool boolVal && boolVal).IsChecked;
            Mp.Services.LoadOnLoginTools.SetLoadOnLogin(loadOnLogin);
            MpAvPrefViewModel.Instance.LoadOnLogin = Mp.Services.LoadOnLoginTools.IsLoadOnLoginEnabled;

            // SHORTCUT PROFILE
            MpAvPrefViewModel.Instance.DefaultRoutingProfileType =
                GestureProfilesViewModel.Items.FirstOrDefault(x => x.OptionId is bool boolVal && boolVal).IsChecked ?
                    MpShortcutRoutingProfileType.Global :
                    MpShortcutRoutingProfileType.Internal;

            // SCROLL-TO-OPEN
            if (ScrollWheelBehaviorViewModel.WasVisited) {
                MpAvPrefViewModel.Instance.DoShowMainWindowWithMouseEdgeAndScrollDelta =
                    ScrollWheelBehaviorViewModel.Items.FirstOrDefault(x => x.OptionId is bool boolVal && boolVal).IsChecked;
            } else {
                // when skipped, default to true
                MpAvPrefViewModel.Instance.DoShowMainWindowWithMouseEdgeAndScrollDelta = true;
            }


            // DRAG-TO-SHOW
            if (DragToOpenBehaviorViewModel.WasVisited) {
                MpAvPrefViewModel.Instance.ShowMainWindowOnDragToScreenTop =
                    DragToOpenBehaviorViewModel.Items.FirstOrDefault(x => x.OptionId is bool boolVal && boolVal).IsChecked;
            } else {
                // when skipped default to true
                MpAvPrefViewModel.Instance.ShowMainWindowOnDragToScreenTop = true;
            }

            // DB PASSWORD
            if (!IsDbPasswordIgnored) {
                Mp.Services.DbInfo.DbPassword = DbPassword;
            }
        }

        private void CloseGestureDemo() {

            IsGestureDemoOpen = false;
            // close/reset gesture vm
            if (CurPointerGestureWindowViewModel != null) {
                CurPointerGestureWindowViewModel.Destroy();
            }
            CurPointerGestureWindowViewModel = null;
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

        public ICommand SelectPageByMarkerCommand => new MpCommand<object>(
            (args) => {
                if (args is not MpWelcomePageType wpt) {
                    return;
                }
                CurPageType = wpt;
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


        public ICommand ToggleGestureDemoCommand => new MpCommand(
            () => {
                if (IsGestureDemoOpen) {
                    CloseGestureDemo();
                    return;
                }
                IsGestureDemoOpen = true;

                CurPointerGestureWindowViewModel =
                    CurPageType == MpWelcomePageType.ScrollWheel ?
                        new MpAvPointerGestureWindowViewModel(CurOptGroupViewModel.Items.First(), MpPointGestureType.ScrollToOpen) :
                        new MpAvPointerGestureWindowViewModel(CurOptGroupViewModel.Items.First(), MpPointGestureType.DragToOpen);

                if (!CurOptGroupViewModel.WasVisited) {
                    // for first visit set to unchecked
                    CurOptGroupViewModel.Items.First().IsChecked = false;
                }
                CurPointerGestureWindowViewModel.ShowGestureWindowCommand.Execute(null);
            });

        #endregion

    }
}
