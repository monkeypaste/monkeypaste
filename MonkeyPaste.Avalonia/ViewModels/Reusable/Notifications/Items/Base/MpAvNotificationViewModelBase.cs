using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public abstract class MpAvNotificationViewModelBase :
        MpAvViewModelBase,
        MpIWantsTopmostWindowViewModel,
        MpICloseWindowViewModel,
        MpIPopupMenuViewModel {
        #region Constants
        #endregion

        #region Statics


        public static MpNotificationLayoutType GetLayoutTypeFromNotificationType(MpNotificationType ndt) {
            switch (ndt) {
                case MpNotificationType.Loader:
                    return MpNotificationLayoutType.Loader;
                case MpNotificationType.Welcome:
                    return MpNotificationLayoutType.Welcome;
                case MpNotificationType.ConfirmEndAppend:
                case MpNotificationType.ModalOkCancelMessageBox:
                case MpNotificationType.ModalOkMessageBox:
                case MpNotificationType.ModalYesNoCancelMessageBox:
                case MpNotificationType.ModalYesNoMessageBox:
                case MpNotificationType.ModalContentFormatDegradation:
                case MpNotificationType.ModalRememberableTextBoxOkCancelMessageBox:
                case MpNotificationType.ModalTextBoxOkCancelMessageBox:
                case MpNotificationType.ModalProgressCancelMessageBox:
                case MpNotificationType.ExecuteParametersRequest:
                case MpNotificationType.ContentCapReached:
                case MpNotificationType.TrashCapReached:
                case MpNotificationType.ContentAddBlockedByAccount:
                case MpNotificationType.ContentRestoreBlockedByAccount:
                case MpNotificationType.DbPasswordInput:
                    return MpNotificationLayoutType.UserAction;
                case MpNotificationType.AnalyzerTimeout:
                case MpNotificationType.InvalidRequest:
                case MpNotificationType.InvalidResponse:
                case MpNotificationType.AccountLoginFailed:
                case MpNotificationType.SubscriptionExpired:
                case MpNotificationType.PluginResponseWarning:
                case MpNotificationType.FileIoWarning:
                    return MpNotificationLayoutType.Warning;

                case MpNotificationType.InvalidPlugin:
                case MpNotificationType.InvalidAction:
                case MpNotificationType.InvalidClipboardFormatHandler:
                case MpNotificationType.PluginResponseWarningWithOption:
                    return MpNotificationLayoutType.ErrorWithOption;
                case MpNotificationType.BadHttpRequest:
                case MpNotificationType.DbError:
                case MpNotificationType.PluginResponseError:
                    return MpNotificationLayoutType.Error;
                //case MpNotificationType.AppendChanged:
                //    return MpNotificationLayoutType.Append;
                default:
                    return MpNotificationLayoutType.Message;
            }
        }
        public static MpNotificationButtonsType GetNotificationButtonsType(MpNotificationType ndt) {
            switch (ndt) {
                case MpNotificationType.ModalYesNoCancelMessageBox:
                    return MpNotificationButtonsType.YesNoCancel;
                case MpNotificationType.ModalYesNoMessageBox:
                    return MpNotificationButtonsType.YesNo;
                case MpNotificationType.ModalRememberableTextBoxOkCancelMessageBox:
                case MpNotificationType.ModalTextBoxOkCancelMessageBox:
                case MpNotificationType.DbPasswordInput:
                    return MpNotificationButtonsType.TextBoxOkCancel;
                case MpNotificationType.ConfirmEndAppend:
                case MpNotificationType.ModalOkCancelMessageBox:
                case MpNotificationType.ModalContentFormatDegradation:
                    return MpNotificationButtonsType.OkCancel;
                case MpNotificationType.ModalProgressCancelMessageBox:
                    return MpNotificationButtonsType.ProgressCancel;
                case MpNotificationType.ExecuteParametersRequest:
                    return MpNotificationButtonsType.SubmitCancel;
                case MpNotificationType.ModalOkMessageBox:
                    return MpNotificationButtonsType.Ok;
                case MpNotificationType.ContentCapReached:
                case MpNotificationType.TrashCapReached:
                case MpNotificationType.ContentAddBlockedByAccount:
                case MpNotificationType.ContentRestoreBlockedByAccount:
                    return MpNotificationButtonsType.UpgradeLearnMore;
                default:
                    MpNotificationLayoutType layoutType = GetLayoutTypeFromNotificationType(ndt);
                    switch (layoutType) {
                        case MpNotificationLayoutType.ErrorWithOption:
                        case MpNotificationLayoutType.UserAction:
                            return MpNotificationButtonsType.IgnoreRetryFix;
                        case MpNotificationLayoutType.Warning:
                            return MpNotificationButtonsType.Ok;
                        case MpNotificationLayoutType.Error:
                            return MpNotificationButtonsType.IgnoreRetryShutdown;
                        default:
                            return MpNotificationButtonsType.None;
                    }
            }
        }

        public static MpNotificationPlacementType GetNotificationPlacementType(MpNotificationType ndt) {
            switch (ndt) {
                case MpNotificationType.ModalYesNoCancelMessageBox:
                case MpNotificationType.ModalYesNoMessageBox:
                case MpNotificationType.ConfirmEndAppend:
                case MpNotificationType.ModalOkCancelMessageBox:
                case MpNotificationType.ModalOkMessageBox:
                case MpNotificationType.ModalContentFormatDegradation:
                case MpNotificationType.ModalRememberableTextBoxOkCancelMessageBox:
                case MpNotificationType.ModalTextBoxOkCancelMessageBox:
                case MpNotificationType.ModalProgressCancelMessageBox:
                case MpNotificationType.Welcome:
                    return MpNotificationPlacementType.ModalAnchor;
                default:
                    return MpNotificationPlacementType.SystemTray;
            }
        }

        public static bool IsNotificationTypeModal(MpNotificationType ndt) {
            switch (ndt) {
                case MpNotificationType.ModalYesNoMessageBox:
                case MpNotificationType.ModalYesNoCancelMessageBox:
                case MpNotificationType.ConfirmEndAppend:
                case MpNotificationType.ModalOkCancelMessageBox:
                case MpNotificationType.ModalOkMessageBox:
                case MpNotificationType.ModalContentFormatDegradation:
                case MpNotificationType.ModalTextBoxOkCancelMessageBox:
                case MpNotificationType.ModalProgressCancelMessageBox:
                case MpNotificationType.ModalRememberableTextBoxOkCancelMessageBox:
                case MpNotificationType.Welcome:
                    return true;
                default:
                    return false;
            }
        }

        public static bool GetIsErrorNotification(MpNotificationFormat nf) {
            var lt = GetLayoutTypeFromNotificationType(nf.NotificationType);
            return lt == MpNotificationLayoutType.Error ||
                   lt == MpNotificationLayoutType.ErrorAndShutdown ||
                   lt == MpNotificationLayoutType.ErrorWithOption;
        }

        public static bool GetIsWarningNotification(MpNotificationFormat nf) {
            var lt = GetLayoutTypeFromNotificationType(nf.NotificationType);
            return lt == MpNotificationLayoutType.Error ||
                   lt == MpNotificationLayoutType.ErrorAndShutdown ||
                   lt == MpNotificationLayoutType.ErrorWithOption;
        }

        #endregion

        #region Interfaces

        #region MpIWantsTopmostWindowViewModel Implementation

        public virtual bool WantsTopmost =>
            true;

        #endregion

        #region MpIChildWindowViewModel Implementation
        public MpWindowType WindowType =>
            IsModal ? MpWindowType.Modal : MpWindowType.Toast;

        public bool IsWindowOpen {
            get => IsVisible;
            set => IsVisible = value;
        }
        #endregion

        #region MpIPopupMenuViewModel Implementation

        public virtual MpAvMenuItemViewModel PopupMenuViewModel {
            get {
                return new MpAvMenuItemViewModel() {
                    SubItems = new List<MpAvMenuItemViewModel>() {
                        new MpAvMenuItemViewModel() {
                            Header = UiStrings.NotificationOptionHideLabel,
                            IconResourceKey = "ErrorImage",
                            Command = CloseNotificationCommand,
                            CommandParameter = "User"
                        },
                        new MpAvMenuItemViewModel() {
                            Header = string.Format(UiStrings.NotificationOptionHideAllLabel,NotificationType.EnumToUiString()),
                            IconResourceKey = "ClosedEyeImage",
                            Command = CheckDoNotShowAgainCommand
                        },
                        new MpAvMenuItemViewModel() {IsSeparator = true, IsVisible = CanPin},
                        new MpAvMenuItemViewModel() {
                            IsVisible = CanPin,
                            Header = IsPinned ? UiStrings.CommonUnpinItemLabel:UiStrings.CommonPinItemLabel,
                            IconResourceKey = "PinImage",
                            Command = ToggleIsPinnedCommand
                        }
                    }
                };
            }
        }

        public bool IsPopupMenuOpen { get; set; }
        #endregion

        #endregion

        #region Properties

        #region Appearance
        public virtual string ForegroundHexColor {
            get {
                if (LayoutType == MpNotificationLayoutType.Warning ||
                    LayoutType == MpNotificationLayoutType.UserAction) {
                    return Mp.Services.PlatformResource.GetResource<string>(MpThemeResourceKey.ThemeAccent1Color.ToString());
                }
                if (LayoutType == MpNotificationLayoutType.ErrorAndShutdown ||
                    LayoutType == MpNotificationLayoutType.ErrorWithOption) {
                    return Mp.Services.PlatformResource.GetResource<string>(MpThemeResourceKey.ThemeAccent2Color.ToString());
                }
                if (LayoutType != MpNotificationLayoutType.Message) {
                    return Mp.Services.PlatformResource.GetResource<string>(MpThemeResourceKey.ThemeAccent5Color.ToString());
                }
                return Mp.Services.PlatformResource.GetResource<string>(MpThemeResourceKey.ThemeInteractiveBgColor.ToString());
            }
        }

        public virtual string BorderHexColor {
            get {
                if (IsWarningNotification) {
                    return Mp.Services.PlatformResource.GetResource<string>(MpThemeResourceKey.ThemeAccent1Color.ToString());
                }
                if (IsErrorNotification) {
                    return Mp.Services.PlatformResource.GetResource<string>(MpThemeResourceKey.ThemeAccent2Color.ToString());
                }
                return Mp.Services.PlatformResource.GetResource<string>(MpThemeResourceKey.ThemeInteractiveBgColor.ToString());
            }
        }

        public virtual string BackgroundHexColor =>
            Mp.Services.PlatformResource.GetResource<string>(MpThemeResourceKey.ThemeColor.ToString());

        #endregion

        #region Layout

        public MpNotificationButtonsType ButtonsType => GetNotificationButtonsType(NotificationType);
        public MpNotificationLayoutType LayoutType => GetLayoutTypeFromNotificationType(NotificationType);

        public MpNotificationPlacementType PlacementType => GetNotificationPlacementType(NotificationType);

        public bool IsModal => IsNotificationTypeModal(NotificationType);

        #endregion

        #region State
        public bool IsLoader =>
            this is MpAvLoaderNotificationViewModel;
        public bool CanMoveWindow =>
            true;

        public bool IsDoNotShowType =>
            !ForceShow &&
            MpAvPrefViewModel.Instance != null &&
            MpAvPrefViewModel.Instance.DoNotShowAgainNotificationIdCsvStr
                    .SplitNoEmpty(",")
                    .Select(x => x.ToEnum<MpNotificationType>())
                    .Any(x => x == NotificationType);
        public bool IsFadeDelayFrozen =>
            MaxShowTimeMs > 0 &&
            (IsHovering || IsPinned || IsPopupMenuOpen);
        public virtual bool ShowOptionsButton => true;
        public virtual bool CanPin => false;
        public bool IsPinned { get; set; } = false;


        public virtual bool CanChooseNotShowAgain => true;


        public bool IsErrorNotification =>
            GetIsErrorNotification(NotificationFormat);

        public bool IsWarningNotification =>
            GetIsWarningNotification(NotificationFormat);

        public virtual bool IsShowOnceNotification =>
            false;


        public bool IsHovering { get; set; } = false;

        public bool IsVisible { get; set; } = false;

        //public bool IsOpening { get; set; }
        //public bool IsClosing { get; set; }
        public string NotifierGuid { get; private set; }
        public bool DoNotShowAgain { get; set; } = false;

        public virtual int MaxShowTimeMs {
            get {
                if (NotificationFormat == null) {
                    return 0;
                }
                return NotificationFormat.MaxShowTimeMs;
            }
        }

        #endregion

        #region Model

        public object IconSourceObj {
            get {
                if (NotificationFormat == null) {
                    return null;
                }
                return NotificationFormat.IconSourceObj;
            }
        }
        public object OtherArgs {
            get {
                if (NotificationFormat == null) {
                    return null;
                }
                // when null default is center of active screen
                return NotificationFormat.OtherArgs;
            }
            set {
                if (OtherArgs != value) {
                    NotificationFormat.OtherArgs = value;
                    OnPropertyChanged(nameof(OtherArgs));
                }
            }
        }

        public object Owner {
            get {
                if (NotificationFormat == null) {
                    return null;
                }
                return NotificationFormat.Owner;
            }
            set {
                if (Owner != value) {
                    NotificationFormat.Owner = value;
                    OnPropertyChanged(nameof(Owner));
                }
            }
        }
        public object AnchorTarget {
            get {
                if (NotificationFormat == null ||
                    !IsModal) {
                    return null;
                }
                // when null default is center of active screen
                return NotificationFormat.AnchorTarget;
            }
            set {
                if (AnchorTarget != value) {
                    NotificationFormat.AnchorTarget = value;
                    OnPropertyChanged(nameof(AnchorTarget));
                }
            }
        }
        public virtual string Title {
            get {
                if (NotificationFormat == null) {
                    return string.Empty;
                }
                return NotificationFormat.Title;
            }
        }

        public virtual object Body {
            get {
                if (NotificationFormat == null) {
                    return string.Empty;
                }
                return NotificationFormat.Body;
            }
        }

        public virtual string Detail {
            get {
                if (NotificationFormat == null) {
                    return string.Empty;
                }
                return NotificationFormat.Detail;
            }
        }
        public virtual bool ForceShow {
            get {
                if (NotificationFormat == null) {
                    return false;
                }
                return NotificationFormat.ForceShow;
            }
        }

        public virtual MpNotificationType NotificationType {
            get {
                if (NotificationFormat == null) {
                    return MpNotificationType.None;
                }
                return NotificationFormat.NotificationType;
            }
        }

        public int NotificationId => (int)NotificationType;

        public MpNotificationFormat NotificationFormat { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpAvNotificationViewModelBase() : base(null) {
            PropertyChanged += MpNotificationViewModelBase_PropertyChanged;
            NotifierGuid = System.Guid.NewGuid().ToString();
        }

        #endregion

        #region Public Methods

        public virtual async Task InitializeAsync(MpNotificationFormat nf) {
            bool wasBusy = IsBusy;

            IsBusy = true;
            await Task.Delay(1);

            nf.IconSourceObj ??= MpBase64Images.AppIcon;
            NotificationFormat = nf;

            IsBusy = wasBusy;
        }

        public virtual async Task<MpNotificationDialogResultType> ShowNotificationAsync() {
            if (IsDoNotShowType) {
                MpConsole.WriteTraceLine($"Notification: {NotificationType} marked as hidden");
                return MpNotificationDialogResultType.DoNotShow;
            }
            await Task.Delay(1);
            MpAvNotificationWindowManager.Instance.ShowNotification(this);

            return MpNotificationDialogResultType.None;
        }

        public virtual void HideNotification(bool force = false) {
            if (IsShowOnceNotification &&
                !IsDoNotShowType) {
                // this was initial show, add to do not show
                DoNotShowAgain = true;
            }
            IsWindowOpen = false;
        }

        #endregion

        #region Protected Methods

        protected virtual void MpNotificationViewModelBase_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(DoNotShowAgain):
                    if (DoNotShowAgain) {
                        MpAvPrefViewModel.Instance.DoNotShowAgainNotificationIdCsvStr += $"{NotificationType},";
                        // show loop checks if DoNotShowAgain is true to hide
                    }
                    break;
                case nameof(NotificationType):
                    OnPropertyChanged(nameof(LayoutType));
                    break;
                case nameof(IsPopupMenuOpen):
                    break;
                case nameof(IsVisible):
                    if (IsVisible) {
                        //UpdatePosition();
                    }
                    break;
                case nameof(IsWindowOpen):
                    //if (IsWindowOpen) {
                    //    WindowOpenDateTime = DateTime.Now;
                    //    UpdatePosition();
                    //} else {
                    //    WindowOpenDateTime = DateTime.MaxValue;
                    //}
                    break;
            }
        }
        protected async Task WaitForFullVisibilityAsync() {

            // wait a tid for anim selectors..
            await Task.Delay(100);
            if (MpAvWindowManager.LocateWindow(this) is not MpAvWindow w) {
                return;
            }
            while (true) {
                bool needs_waiting = false;
                if (w.RenderTransform is TranslateTransform tt) {
                    // sliding in
                    needs_waiting = tt.X > 0;
                }
                if (!needs_waiting) {
                    // fading in
                    needs_waiting = w.Opacity < 1.0d;
                }
                if (needs_waiting) {
                    await Task.Delay(100);
                } else {
                    return;
                }
            }

        }

        #endregion

        #region Private Methods

        //        private static void MpAvWindowManager_NotificationWindowsChanged(object sender, EventArgs e) {
        //            UpdateToastPositions();
        //        }
        //        private static void UpdateToastPositions() {
        //            MpAvWindowManager.ToastNotifications
        //                .OrderBy(x => x.OpenDateTime)
        //                .Select(x => x.DataContext)
        //                .OfType<MpAvNotificationViewModelBase>()
        //                .ForEach(x => x.UpdatePosition());
        //        }
        //        private MpAvWindow CreateNotificationWindow() {
        //            string window_classes = string.Empty;
        //            // MpAvUserControl content_view = null;

        //            if (this is MpAvWelcomeNotificationViewModel) {
        //                window_classes = "notificationWindow fadeOut welcome welcomeGrow transparent";
        //                //content_view = new MpAvWelcomeView();
        //            } else if (this is MpAvLoaderNotificationViewModel) {
        //                window_classes = "notificationWindow loader toast";
        //                //content_view = new MpAvLoaderView();
        //            } else if (this is MpAvUserActionNotificationViewModel) {
        //                window_classes = $"notificationWindow resizable fadeIn fadeOut toast";
        //                // content_view = new MpAvUserActionNotificationView();
        //            } else if (this is MpAvMessageNotificationViewModel) {
        //                window_classes = "notificationWindow msg slideIn resizable fadeIn fadeOut toast";
        //                // content_view = new MpAvMessageNotificationView();
        //            } else {
        //                MpDebug.Break($"Unhandled ntf type");
        //            }
        //            if (IsModal) {
        //                window_classes = window_classes.Replace("toast", "modal");
        //                if (Owner != null) {
        //                    window_classes += " owned";
        //                }
        //                if (AnchorTarget != null) {
        //                    window_classes += " anchored";
        //                }
        //            }
        //            if (CanMoveWindow) {
        //                window_classes += " movable";
        //            }
        //            MpAvWindow nw = Owner == null ? new MpAvWindow() : new MpAvWindow(Owner as Window);
        //            nw.Classes.AddRange(window_classes.Split(" "));
        //            nw.DataContext = this;
        //            if (this is MpAvWelcomeNotificationViewModel) {
        //                nw.Content = new MpAvWelcomeView();
        //                return nw;
        //            }

        //            void OnSizeChanged() {
        //                if (IsModal) {
        //                    UpdatePosition();
        //                    return;
        //                }
        //                UpdateToastPositions();
        //            }
        //            nw.GetObservable(Window.BoundsProperty).Subscribe(x => OnSizeChanged());
        //            nw.EffectiveViewportChanged += (s, e) => OnSizeChanged();

        //            MpConsole.WriteLine($"Ntf '{this}' Window classes: '{window_classes}'");
        //            MpAvNotificationContainerView cv = new MpAvNotificationContainerView();

        //            nw.Content = cv;

        //#if WINDOWS
        //            MpAvToolWindow_Win32.InitToolWindow(nw.TryGetPlatformHandle().Handle);
        //#endif
        //            return nw;
        //        }

        //        private void UpdatePosition() {
        //            if (IsModal) {
        //                SetAnchorPosition();
        //            } else {
        //                SetToastPosition();
        //            }
        //        }

        //        private void SetToastPosition() {
        //            if (MpAvWindowManager.LocateWindow(this) is not MpAvWindow nw ||
        //                nw.Screens.ScreenFromVisual(nw) is not { } nw_screen) {
        //                return;
        //            }
        //            double pd = nw_screen.Scaling;
        //            int pad = (int)(5 * pd);
        //            PixelSize ws = nw.Bounds.Size.ToAvPixelSize(pd);// GetNotificationSize().ToAvPixelSize(pd);
        //            PixelPoint pos = nw.GetScreen().GetSystemTrayWindowPosition(ws, pad);
        //            int x = pos.X;
        //            int y = pos.Y;

        //            var pre_w_vm =
        //                MpAvWindowManager.ToastNotifications
        //                .Select(x => x.DataContext)
        //                .OfType<MpAvNotificationViewModelBase>()
        //                .Where(x => x.WindowOpenDateTime < WindowOpenDateTime)
        //                .OrderByDescending(x => x.WindowOpenDateTime)
        //                .FirstOrDefault();

        //            if (pre_w_vm != default && MpAvWindowManager.LocateWindow(pre_w_vm) is { } pre_w) {
        //#if MAC
        //                y = pre_ntfl.WindowPosition.Y + (int)(pre_ntfl.WindowHeight * pd) + pad;
        //#else

        //                y = pre_w.Position.Y - ws.Height - pad;
        //#endif
        //            }

        //            nw.Position = new PixelPoint(x, y);
        //            MpConsole.WriteLine($"Ntf '{this}' pos: {nw.Position}");
        //        }

        //        private void SetAnchorPosition() {
        //            if (AnchorTarget is not Control owner_c ||
        //                Owner is not MpAvWindow owner_w ||
        //                MpAvWindowManager.LocateWindow(this) is not { } nw) {
        //                return;
        //            }

        //            var anchor_s_origin = owner_c.PointToScreen(new Point());
        //            var anchor_s_size = owner_c.Bounds.Size.ToAvPixelSize(owner_c.VisualPixelDensity());
        //            //var win_size = nw.Bounds.Size.ToAvPixelSize(nw.VisualPixelDensity()); //GetNotificationSize();
        //            var nw_s_size = nw.Bounds.Size.ToAvPixelSize(owner_c.VisualPixelDensity());
        //            double nw_x = anchor_s_origin.X + (anchor_s_size.Width / 2) - (nw_s_size.Width / 2);
        //            double nw_y = anchor_s_origin.Y + (anchor_s_size.Height / 2) - (nw_s_size.Height / 2);
        //            var s_size = owner_w.Screens.ScreenFromVisual(owner_w).WorkingArea.Size;
        //            nw_x = Math.Clamp(nw_x, 0, s_size.Width - nw_s_size.Width);
        //            nw_y = Math.Clamp(nw_y, 0, s_size.Height - nw_s_size.Height);

        //            nw.Position = new PixelPoint((int)nw_x, (int)nw_y);
        //        }

        //        //private MpSize GetNotificationSize() {
        //        //    // NOTE default dims should match window.axaml (to avoid services not initialized)
        //        //    var s = new MpSize(WindowWidth, WindowHeight);
        //        //    s.Width = !s.Width.IsNumber() || s.Width == 0 ? 350 : s.Width;
        //        //    s.Height = !s.Height.IsNumber() || s.Height == 0 ? 150 : s.Height;
        //        //    return s;
        //        //}
        #endregion

        #region Commands
        public ICommand ShowOptionsPopupCommand => new MpCommand<object>(
            (args) => {

                MpAvMenuView.ShowMenu(
                    target: args as Control,
                    dc: PopupMenuViewModel,
                    showByPointer: false,
                    placementMode: PlacementMode.Left,
                    popupAnchor: PopupAnchor.None);
            });
        public ICommand ResetAllNotificationsCommand => new MpCommand(
             () => {
                 // TODO this should be moved to somewhere in preferences
                 MpAvPrefViewModel.Instance.DoNotShowAgainNotificationIdCsvStr = string.Empty;

             }, () => {
                 return
                     Mp.Services != null &&
                     Mp.Services.StartupState != null &&
                     Mp.Services.StartupState.IsCoreLoaded;
             });

        public ICommand CheckDoNotShowAgainCommand => new MpCommand(
            () => {
                DoNotShowAgain = true;
                CloseNotificationCommand.Execute(null);
            });

        public ICommand CloseNotificationCommand => new MpCommand<object>(
            (args) => {
                bool force = args is string argStr && argStr == "User";
                HideNotification(force);
                //IsClosing = false;
            });

        public ICommand ToggleIsPinnedCommand => new MpCommand(
            () => {
                IsPinned = !IsPinned;
            }, () => CanPin);
        #endregion
    }
}
