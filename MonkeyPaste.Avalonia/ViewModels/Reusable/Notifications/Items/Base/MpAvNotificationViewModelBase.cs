using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
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

        public const int DEFAULT_NOTIFICATION_SHOWTIME_MS = 3000;

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
                case MpNotificationType.ModalTextBoxOkCancelMessageBox:
                case MpNotificationType.ModalProgressCancelMessageBox:
                case MpNotificationType.InvalidPlugin:
                case MpNotificationType.InvalidAction:
                case MpNotificationType.InvalidClipboardFormatHandler:
                case MpNotificationType.PluginResponseWarningWithOption:
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
                case MpNotificationType.TrialExpired:
                case MpNotificationType.PluginResponseWarning:
                case MpNotificationType.FileIoWarning:
                    return MpNotificationLayoutType.Warning;
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
                case MpNotificationType.Welcome:
                    return true;
                default:
                    return false;
            }
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
                            Header = "Hide",
                            IconResourceKey = "ErrorImage",
                            Command = CloseNotificationCommand
                        },
                        new MpAvMenuItemViewModel() {
                            Header = $"Hide all '{NotificationType.EnumToUiString()}' notifications",
                            IconResourceKey = "ClosedEyeImage",
                            Command = CheckDoNotShowAgainCommand
                        },
                        new MpAvMenuItemViewModel() {IsSeparator = true, IsVisible = CanPin},
                        new MpAvMenuItemViewModel() {
                            IsVisible = CanPin,
                            Header = IsPinned ? "Unpin":"Pin",
                            IconResourceKey = IsPinned ? "PinDownImage" : "PinImage",
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
        public object IconSourceObj {
            get {
                if (NotificationFormat == null) {
                    return string.Empty;
                }
                return NotificationFormat.IconSourceObj;
            }
        }
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

        public bool CanMoveWindow =>
            true;
        public virtual List<Type> RejectedMoveControlTypes { get; set; } // defaults to button,textbox in ext

        public bool IsDoNotShowType =>
            !ForceShow &&
            MpAvPrefViewModel.Instance != null &&
            MpAvPrefViewModel.Instance.DoNotShowAgainNotificationIdCsvStr
                    .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .Any(x => x == (int)NotificationType);
        public bool IsFadeDelayFrozen =>
            MaxShowTimeMs > 0 &&
            (IsHovering || IsPinned || IsPopupMenuOpen);
        public virtual bool ShowOptionsButton => true;
        public virtual bool CanPin => false;
        public bool IsPinned { get; set; } = false;

        public bool IsOverOptionsButton { get; set; }

        public virtual bool CanChooseNotShowAgain => true;

        public bool IsErrorNotification {
            get {
                return LayoutType == MpNotificationLayoutType.Error ||
                    LayoutType == MpNotificationLayoutType.ErrorAndShutdown ||
                    LayoutType == MpNotificationLayoutType.ErrorWithOption;
            }
        }

        public bool IsWarningNotification {
            get {
                return LayoutType == MpNotificationLayoutType.Warning ||
                    LayoutType == MpNotificationLayoutType.UserAction;
            }
        }

        public virtual bool IsShowOnceNotification =>
            false;


        public bool IsHovering { get; set; } = false;

        public bool IsVisible { get; set; } = false;

        public bool IsOpening { get; set; }
        public bool IsClosing { get; set; }
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

            Mp.Services.NotificationManager.ShowNotification(this);

            return MpNotificationDialogResultType.None;
        }

        public virtual void HideNotification() {
            if (IsShowOnceNotification &&
                !IsDoNotShowType) {
                // this was initial show, add to do not show
                DoNotShowAgain = true;
            }
            Mp.Services.NotificationManager.HideNotification(this);
        }

        #endregion

        #region Protected Methods


        protected virtual void MpNotificationViewModelBase_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(DoNotShowAgain):
                    if (DoNotShowAgain) {
                        MpAvPrefViewModel.Instance.DoNotShowAgainNotificationIdCsvStr += NotificationId + ",";
                        // show loop checks if DoNotShowAgain is true to hide
                    }
                    break;
                case nameof(NotificationType):
                    OnPropertyChanged(nameof(LayoutType));
                    break;
                case nameof(IsPopupMenuOpen):
                    break;
                case nameof(IsVisible):
                    if (!IsVisible) {
                        IsClosing = false;
                    }
                    break;
            }
        }
        #endregion

        #region Private Methods
        #endregion

        #region Commands
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

        public ICommand CloseNotificationCommand => new MpCommand(
            () => {
                HideNotification();
                IsClosing = false;
            });

        public ICommand ToggleIsPinnedCommand => new MpCommand(
            () => {
                IsPinned = !IsPinned;
            }, () => CanPin);
        #endregion
    }
}
