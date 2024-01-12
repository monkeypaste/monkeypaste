using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
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
        MpAvNotificationViewModelBase,
        MpIWindowStateViewModel {
        #region Private Variables
        #endregion

        #region Constants
        private const string PRE_ESTABLISHED_USER_DB_PWD_TEXT = "<^&user has already set a password^&>";
        #endregion

        #region Statics
        public static async Task ShowWelcomeNotificationAsync(bool forceShow = false) {
            bool will_show =
                forceShow ||
                !MpAvPrefViewModel.Instance.IsWelcomeComplete;
#if !DESKTOP
            will_show = false;
            MpAvPrefViewModel.Instance.IsWelcomeComplete = true;
#endif
            await MpAvAccountViewModel.Instance.InitializeAsync();
            if (will_show) {
                await MpDb.InitAsync();
                await MpAvSubscriptionPurchaseViewModel.Instance.InitializeAsync();
                Instance.InitWelcomeItems();
                await Mp.Services.NotificationBuilder.ShowNotificationAsync(
                    new MpNotificationFormat() {
                        ForceShow = forceShow,
                        NotificationType = MpNotificationType.Welcome,
                        MaxShowTimeMs = -1
                    });
            }

        }

        private static MpAvWelcomeNotificationViewModel _instance;
        public static MpAvWelcomeNotificationViewModel Instance => _instance ?? (_instance = new MpAvWelcomeNotificationViewModel());
        #endregion

        #region Interfaces
        public WindowState WindowState { get; set; }
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
        public bool IsFinishing { get; set; }
        public bool IsTermsWindowOpen { get; set; }
        public bool IsAccountOptSelected { get; set; }
        bool IsExistingSubscriptionDetected { get; set; }
        bool HasShownExistingSubscriptionNtf { get; set; }
        public bool IsAccountMonthlyChecked { get; set; } = false;
        public bool IsAccountMonthToggleEnabled { get; set; } = true;
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
        public bool RememberDbPassword { get; set; }
        public string WelcomeTitle =>
            CurPageType.ToString();

        #endregion

        #endregion

        #region Constructors
        public MpAvWelcomeNotificationViewModel() : base() {
            MpDebug.Assert(_instance == null, "Only 1 welcome vm should be created");
            _instance = this;
            MpConsole.WriteLine("Greeting vm created");
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
            //InitWelcomeItems();
        }

        #endregion

        #region Public Methods
        public override async Task<MpNotificationDialogResultType> ShowNotificationAsync() {
            DialogResult = BeginShow();
            if (DialogResult == MpNotificationDialogResultType.DoNotShow) {
                return DialogResult;
            }
            await BeginWelcomeSetupAsync();

            return MpNotificationDialogResultType.Dismiss;
        }

        public override void HideNotification(bool force = false) {
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
                    IsAccountOptSelected = CurPageType == MpWelcomePageType.Account;
                    OnPropertyChanged(nameof(IsAccountOptSelected));
                    OnPropertyChanged(nameof(WantsTopmost));
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));
                    CurOptGroupViewModel.OnPropertyChanged(nameof(CurOptGroupViewModel.IsGestureGroup));
                    CloseGestureDemo();
                    switch (CurPageType) {
                        case MpWelcomePageType.Account:
                            if (HasShownExistingSubscriptionNtf ||
                                !IsExistingSubscriptionDetected) {
                                // no local existing subscription detected or ntf already shown
                                break;
                            }
                            MpAvAccountViewModel.Instance.ShowAccountNotficationAsync(MpAccountNtfType.ExistingLoginSuccessful).FireAndForgetSafeAsync();
                            HasShownExistingSubscriptionNtf = true;
                            break;
                        case MpWelcomePageType.ScrollWheel:
                        case MpWelcomePageType.DragToOpen:

                            break;
                    }

                    CurOptGroupViewModel.WasVisited = true;
                    break;
                case nameof(IsAccountOptSelected):
                    if (IsAccountOptSelected) {
                        //MpDebug.BreakAll(true, false);
                    }
                    break;
                case nameof(IsAccountMonthlyChecked):
                    if (AccountViewModel == null) {
                        break;
                    }
                    // when changing monthly keep sel type the same
                    var sel_item = AccountViewModel.Items.FirstOrDefault(x => x.IsChecked);
                    MpUserAccountType sel_item_type = sel_item == null ? MpUserAccountType.Unlimited : (MpUserAccountType)sel_item.OptionId;
                    foreach (var (optvm, idx) in AccountViewModel.Items.WithIndex()) {
                        optvm.IsOptionVisible =
                            IsAccountMonthlyChecked ? idx >= 4 : idx < 4;
                        MpUserAccountType uat = (MpUserAccountType)optvm.OptionId;
                        optvm.IsChecked = optvm.IsOptionVisible && uat == sel_item_type;
                    }
                    break;
                case nameof(WindowState):
                    MpAvWindowManager.AllWindows
                    .Where(x => x.DataContext is MpAvPointerGestureWindowViewModel || x.DataContext is MpAvFakeWindowViewModel)
                    .ForEach(x => x.WindowState = WindowState);
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

            AccountViewModel = MpAvSubscriptionPurchaseViewModel.Instance.ToWelcomeOptionGroup();
            if (AccountViewModel.Items.FirstOrDefault(x => !string.IsNullOrEmpty(x.LabelText3))
                is MpAvWelcomeOptionItemViewModel active_woivm &&
                AccountViewModel.Items.IndexOf(active_woivm) is int active_idx &&
                active_idx >= 4 is bool is_active_monthly) {
                // existing subscription detected, toggle monthly to show it
                IsAccountMonthlyChecked = is_active_monthly;
                AccountViewModel.SelectedItem = AccountViewModel.Items[active_idx];
                //IsExistingSubscriptionDetected = true;
            }
            #endregion

            #region LoginLoad
            LoginLoadViewModel = new MpAvWelcomeOptionGroupViewModel(this, MpWelcomePageType.LoginLoad) {
                Title = UiStrings.WelcomeLoginLoadTitle,
                Caption = UiStrings.WelcomeLoginLoadCaption
            };
            LoginLoadViewModel.Items = new[] {
                    new MpAvWelcomeOptionItemViewModel(this,false,LoginLoadViewModel) {
                        IsChecked = false,
                        IconSourceObj = "NoEntryImage",
                        LabelText = UiStrings.CommonDisableLabel,
                        DescriptionText = UiStrings.WelcomeLoginLoadDescription1
                    },
                    new MpAvWelcomeOptionItemViewModel(this,true,LoginLoadViewModel) {
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
                    new MpAvWelcomeOptionItemViewModel(this,false, GestureProfilesViewModel) {
                        IsChecked = MpAvPrefViewModel.Instance.DefaultRoutingProfileType == MpShortcutRoutingProfileType.Internal,
                        IconSourceObj = "PrivateImage",
                        LabelText = UiStrings.WelcomeGestureProfileLabel1,
                        DescriptionText = UiStrings.WelcomeGestureProfileDescription1
                    },
                    new MpAvWelcomeOptionItemViewModel(this,true, GestureProfilesViewModel) {
                        IsChecked = MpAvPrefViewModel.Instance.DefaultRoutingProfileType == MpShortcutRoutingProfileType.Default,
                        IconSourceObj = "GlobeImage",
                        LabelText = UiStrings.CommonDefaultLabel,
                        DescriptionText = UiStrings.WelcomeGestureProfileDescription2
                    }
                };
            #endregion

            #region Scroll Wheel
            ScrollWheelBehaviorViewModel = new MpAvWelcomeOptionGroupViewModel(this, MpWelcomePageType.ScrollWheel) {
                Title = UiStrings.WelcomeScrollToOpenTitle,
                Caption = UiStrings.WelcomeScrollToOpenCaption
            };
            ScrollWheelBehaviorViewModel.Items = new[] {
                    new MpAvWelcomeOptionItemViewModel(this,false,ScrollWheelBehaviorViewModel) {
                        IsChecked = !MpAvPrefViewModel.Instance.ScrollToOpen,
                        IconSourceObj = "NoEntryImage",
                        LabelText = UiStrings.CommonDisableLabel,
                        DescriptionText = UiStrings.WelcomeScrollToOpenDescription1
                    },
                    new MpAvWelcomeOptionItemViewModel(this,true,ScrollWheelBehaviorViewModel) {
                        IsChecked = MpAvPrefViewModel.Instance.ScrollToOpen,
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
                    new MpAvWelcomeOptionItemViewModel(this,false,DragToOpenBehaviorViewModel) {
                        IsChecked = !MpAvPrefViewModel.Instance.DragToOpen,
                        IconSourceObj = "CloseWindowImage",
                        LabelText = UiStrings.CommonDisableLabel,
                        DescriptionText = UiStrings.WelcomeDragToOpenDescription1
                    },
                    new MpAvWelcomeOptionItemViewModel(this,true,DragToOpenBehaviorViewModel) {
                        IsChecked = MpAvPrefViewModel.Instance.DragToOpen,
                        IconSourceObj = "AppFrameImage",
                        LabelText = UiStrings.CommonEnableLabel,
                        DescriptionText = UiStrings.WelcomeDragToOpenDescription2
                    }
                };
            #endregion

            #region Db Password
            DbPasswordViewModel = new MpAvWelcomeOptionGroupViewModel(this, MpWelcomePageType.DbPassword) {
                SplashIconSourceObj = "ShieldImage",
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
        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.AccountStateChanged:
                    if (!IsWindowOpen || MpAvAccountViewModel.Instance.AccountState != MpUserAccountState.Connected) {
                        break;
                    }
                    // after connecting to an existing account toggle monthly and sel account...
                    var AccountType = MpAvAccountViewModel.Instance.AccountType;
                    var monthly = MpAvAccountViewModel.Instance.IsMonthly;

                    int acct_idx = (int)AccountType + (monthly ? 4 : 0);
                    IsAccountMonthlyChecked = monthly;
                    IsAccountMonthToggleEnabled = false;
                    AccountViewModel.SelectedItem = AccountViewModel.Items[acct_idx];
                    AccountViewModel.Items.ForEach(x => x.IsEnabled = false);
                    break;

            }
        }
        private async void FinishWelcomeSetup() {
            IsFinishing = true;
            if (CurPointerGestureWindowViewModel != null) {
                CurPointerGestureWindowViewModel.Destroy();
            }

            // TERMS AGGREEMENT
            IsTermsWindowOpen = true;
            bool agreed = await MpAvTermsView.ShowTermsAgreementWindowAsync(
                new MpAvTermsAgreementCollectionViewModel() {
                    IntroText = UiStrings.TermsIntroAppText,
                    OutroText = UiStrings.TermsOutroAppText,
                    Items = new[] {
                        new MpAvTermsAgreementViewModel() {
                            Author = Mp.Services.ThisAppInfo.ThisAppCompanyName,
                            PackageName = Mp.Services.ThisAppInfo.ThisAppProductName,
                            LicenseUri = Mp.Services.PlatformInfo.TermsPath.ToFileSystemUriFromPath()
                        }
                    }.ToList()
                });
            IsTermsWindowOpen = false;
            if (!agreed) {
                Mp.Services.ShutdownHelper.ShutdownApp(MpShutdownType.TermsDeclined, "declined terms");
                return;
            }
            IsWindowOpen = false;

            // ACCOUNT TYPE
            if (!AccountViewModel.NeedsSkip) {
                bool is_monthly = false;
                MpUserAccountType acct_type = MpUserAccountType.Free;
                MpDebug.Assert(AccountViewModel.Items.Where(x => x.IsChecked).Count() <= 1, $"Account selection not single");
                if (AccountViewModel.Items.FirstOrDefault(x => x.IsChecked) is MpAvWelcomeOptionItemViewModel sel_acct_vm &&
                    sel_acct_vm.OptionId is MpUserAccountType sel_acct_type) {
                    acct_type = sel_acct_type;
                    is_monthly = AccountViewModel.Items.IndexOf(sel_acct_vm) >= 4;
                }
                await MpAvSubscriptionPurchaseViewModel.Instance.PurchaseSubscriptionCommand
                    .ExecuteAsync(new object[] { acct_type, is_monthly });
            }


            // LOGIN LOAD
            bool loadOnLogin =
                LoginLoadViewModel.Items.FirstOrDefault(x => x.OptionId is bool boolVal && boolVal).IsChecked;
            Mp.Services.LoadOnLoginTools.SetLoadOnLogin(loadOnLogin);
            MpAvPrefViewModel.Instance.LoadOnLogin = Mp.Services.LoadOnLoginTools.IsLoadOnLoginEnabled;

            // SHORTCUT PROFILE
            MpAvPrefViewModel.Instance.DefaultRoutingProfileType =
                GestureProfilesViewModel.Items.FirstOrDefault(x => x.OptionId is bool boolVal && boolVal).IsChecked ?
                    MpShortcutRoutingProfileType.Default :
                    MpShortcutRoutingProfileType.Internal;
            await Mp.Services.DefaultDataCreator.ResetShortcutsAsync();

            // SCROLL-TO-OPEN
            if (ScrollWheelBehaviorViewModel.WasVisited) {
                MpAvPrefViewModel.Instance.ScrollToOpen =
                    ScrollWheelBehaviorViewModel.Items.FirstOrDefault(x => x.OptionId is bool boolVal && boolVal).IsChecked;
            } else {
                // when skipped, default to true
                MpAvPrefViewModel.Instance.ScrollToOpen = true;
            }


            // DRAG-TO-SHOW
            if (DragToOpenBehaviorViewModel.WasVisited) {
                MpAvPrefViewModel.Instance.DragToOpen =
                    DragToOpenBehaviorViewModel.Items.FirstOrDefault(x => x.OptionId is bool boolVal && boolVal).IsChecked;
            } else {
                // when skipped default to true
                MpAvPrefViewModel.Instance.DragToOpen = true;
            }

            // DB PASSWORD
            if (!IsDbPasswordIgnored) {
                await MpDb.ChangeDbPasswordAsync(DbPassword, RememberDbPassword);
            }

            IsWelcomeDone = true;
            // NOTE this isn't flagged until DONE is clicked and assumes
            // there's per-page validation (only needed for accounts atm I think)
            MpAvPrefViewModel.Instance.IsWelcomeComplete = true;
            IsFinishing = false;
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
                if (CurOptGroupViewModel.NeedsSkip) {
                    SelectNextPageCommand.Execute(null);
                }
            },
            () => {
                return CanSelectNext;
            });
        public ICommand SelectPrevPageCommand => new MpCommand(
            () => {
                CurPageType = (MpWelcomePageType)((int)CurPageType - 1);
                if (CurOptGroupViewModel.NeedsSkip) {
                    SelectPrevPageCommand.Execute(null);
                }
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
            },
            () => {
                return IsDbPasswordValid;
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

        public ICommand MinimizeWindowCommand => new MpCommand(
            () => {
                WindowState = WindowState.Minimized;
            });

        #endregion

    }
}
