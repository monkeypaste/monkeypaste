﻿using MonkeyPaste.Common;
using System;
using System.ComponentModel;
using System.Globalization;
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
        public override bool WantsTopmost =>
            false;
        //CurOptGroupViewModel == null ||
        //!CurOptGroupViewModel.IsGestureGroup;
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
            }
        }

        #endregion

        #region Private Methods
        private void InitWelcomeItems() {
            GreetingViewModel = new MpAvWelcomeOptionGroupViewModel(this, MpWelcomePageType.Greeting) {
                SplashIconSourceObj = "AppImage",
                Title = UiStrings.WelcomeGreetingTitle,
                Caption = UiStrings.WelcomeGreetingCaption,
            };
            string account_help_url = @"https://www.monkeypaste.com/help#recycling";
            string monthly_color = MpSystemColors.yellowgreen.RemoveHexAlpha();
            string yearly_color = MpSystemColors.olivedrab.RemoveHexAlpha();
            string line_break = Environment.NewLine;
            AccountViewModel = new MpAvWelcomeOptionGroupViewModel(this, MpWelcomePageType.Account) {
                Title = UiStrings.WelcomeAccountTitle,
                Caption = UiStrings.WelcomeAccountCaption,
                Items = new[] {
                    new MpAvWelcomeOptionItemViewModel(this,null) {
                        IconSourceObj = "LoginImage",
                        LabelText = UiStrings.WelcomeAccountLabel1,
                        DescriptionText = UiStrings.WelcomeAccountDescription1
                    },
                    new MpAvWelcomeOptionItemViewModel(this,MpUserAccountType.Free) {
                        IconSourceObj = "StarOutlineImage",
                        LabelText = UiStrings.WelcomeAccountLabel2,
                        DescriptionText =
                            string.Format(
                                UiStrings.WelcomeAccountDescription2,
                                Mp.Services.AccountTools.GetContentCapacity(MpUserAccountType.Free),
                                Mp.Services.AccountTools.GetTrashCapacity(MpUserAccountType.Free),
                                account_help_url)
                    },
                    new MpAvWelcomeOptionItemViewModel(this,MpUserAccountType.Standard) {
                        IconSourceObj = "StarYellowImage",
                        LabelText = UiStrings.WelcomeAccountLabel3,
                        DescriptionText =
                            string.Format(
                                UiStrings.WelcomeAccountDescription3,
                                new RegionInfo(System.Threading.Thread.CurrentThread.CurrentUICulture.LCID).CurrencySymbol,
                                Mp.Services.AccountTools.GetAccountRate(MpUserAccountType.Standard,true),
                                Mp.Services.AccountTools.GetAccountRate(MpUserAccountType.Standard,false),
                                Mp.Services.AccountTools.GetTrashCapacity(MpUserAccountType.Standard),
                                line_break,
                                account_help_url,
                                monthly_color,
                                yearly_color)
                    },
                    new MpAvWelcomeOptionItemViewModel(this,MpUserAccountType.Unlimited) {
                        IsChecked = true,
                        IconSourceObj = "TrophyImage",
                        LabelText = UiStrings.WelcomeAccountLabel4,
                        DescriptionText =
                            string.Format(
                                UiStrings.WelcomeAccountDescription4,
                                new RegionInfo(System.Threading.Thread.CurrentThread.CurrentUICulture.LCID).CurrencySymbol,
                                Mp.Services.AccountTools.GetAccountRate(MpUserAccountType.Unlimited,true),
                                Mp.Services.AccountTools.GetAccountRate(MpUserAccountType.Unlimited,false),
                                line_break,
                                account_help_url,
                                monthly_color,
                                yearly_color)
                    },
                }
            };

            LoginLoadViewModel = new MpAvWelcomeOptionGroupViewModel(this, MpWelcomePageType.LoginLoad) {
                Title = UiStrings.WelcomeLoginLoadTitle,
                Caption = UiStrings.WelcomeLoginLoadCaption,
                Items = new[] {
                    new MpAvWelcomeOptionItemViewModel(this,null) {
                        IsChecked = true,
                        IconSourceObj = "UserImage",
                        LabelText = UiStrings.WelcomeLoginLoadLabel1,
                        DescriptionText = UiStrings.WelcomeLoginLoadDescription1
                    }
                }
            };

            GestureProfilesViewModel = new MpAvWelcomeOptionGroupViewModel(this, MpWelcomePageType.GestureProfile) {
                Title = UiStrings.WelcomeGestureProfileTitle,
                Caption = UiStrings.WelcomeGestureProfileCaption,
                Items = new[] {
                    new MpAvWelcomeOptionItemViewModel(this,null) {
                        IsChecked = MpAvPrefViewModel.Instance.DefaultRoutingProfileType != MpShortcutRoutingProfileType.Internal,
                        UncheckedIconSourceObj = "PrivateImage",
                        CheckedIconSourceObj = "GlobeImage",
                        CheckedLabelText = UiStrings.WelcomeGestureProfileLabel2,
                        CheckedDescriptionText = UiStrings.WelcomeGestureProfileDescription2,
                        UncheckedLabelText = UiStrings.WelcomeGestureProfileLabel1,
                        UncheckedDescriptionText = UiStrings.WelcomeGestureProfileDescription1
                    },
                }
            };

            ScrollWheelBehaviorViewModel = new MpAvWelcomeOptionGroupViewModel(this, MpWelcomePageType.ScrollWheel) {
                Title = UiStrings.WelcomeScrollToOpenTitle,
                Caption = UiStrings.WelcomeScrollToOpenCaption,
                Items = new[] {
                    new MpAvWelcomeOptionItemViewModel(this,null) {
                        IsChecked = MpAvPrefViewModel.Instance.DoShowMainWindowWithMouseEdgeAndScrollDelta,
                        IconSourceObj = "MouseWheelImage",
                        CheckedLabelText = UiStrings.WelcomeScrollToOpenLabel2,
                        CheckedDescriptionText = UiStrings.WelcomeScrollToOpenDescription2,
                        UncheckedLabelText = UiStrings.WelcomeScrollToOpenLabel1,
                        UncheckedDescriptionText = UiStrings.WelcomeScrollToOpenDescription1
                    }
                }
            };

            DragToOpenBehaviorViewModel = new MpAvWelcomeOptionGroupViewModel(this, MpWelcomePageType.DragToOpen) {
                Title = UiStrings.WelcomeDragToOpenTitle,
                Caption = UiStrings.WelcomeDragToOpenCaption,
                Items = new[] {
                    new MpAvWelcomeOptionItemViewModel(this,null) {
                        IsChecked = MpAvPrefViewModel.Instance.ShowMainWindowOnDragToScreenTop,
                        UncheckedIconSourceObj = "CloseWindowImage",
                        CheckedIconSourceObj = "AppFrameImage",
                        CheckedLabelText = UiStrings.WelcomeDragToOpenLabel2,
                        CheckedDescriptionText = UiStrings.WelcomeDragToOpenDescription2,
                        UncheckedLabelText = UiStrings.WelcomeDragToOpenLabel1,
                        UncheckedDescriptionText = UiStrings.WelcomeDragToOpenDescription1
                    }
                }
            };

            DbPasswordViewModel = new MpAvWelcomeOptionGroupViewModel(this, MpWelcomePageType.DbPassword) {
                SplashIconSourceObj = "LockImage",
                Title = UiStrings.WelcomeDbPasswordTitle,
                Caption = UiStrings.WelcomeDbPasswordCaption,
            };
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
                LoginLoadViewModel.Items.FirstOrDefault().IsChecked;
            Mp.Services.LoadOnLoginTools.SetLoadOnLogin(loadOnLogin);
            MpAvPrefViewModel.Instance.LoadOnLogin = Mp.Services.LoadOnLoginTools.IsLoadOnLoginEnabled;

            // SHORTCUT PROFILE
            MpAvPrefViewModel.Instance.DefaultRoutingProfileType =
                GestureProfilesViewModel.Items.FirstOrDefault().IsChecked ?
                    MpShortcutRoutingProfileType.Global :
                    MpShortcutRoutingProfileType.Internal;

            // SCROLL-TO-OPEN
            if (ScrollWheelBehaviorViewModel.WasVisited) {
                MpAvPrefViewModel.Instance.DoShowMainWindowWithMouseEdgeAndScrollDelta =
                    ScrollWheelBehaviorViewModel.Items.FirstOrDefault().IsChecked;
            } else {
                // when skipped, default to true
                MpAvPrefViewModel.Instance.DoShowMainWindowWithMouseEdgeAndScrollDelta = true;
            }


            // DRAG-TO-SHOW
            if (DragToOpenBehaviorViewModel.WasVisited) {
                MpAvPrefViewModel.Instance.ShowMainWindowOnDragToScreenTop =
                    DragToOpenBehaviorViewModel.Items.FirstOrDefault().IsChecked;
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