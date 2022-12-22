using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MonkeyPaste.Common;

namespace MonkeyPaste {
    //public enum MpNotifierType {
    //    Default = 0,
    //    Startup,
    //    Dialog
    //}

    public enum MpNotificationPlacementType {
        None = 0,
        SystemTray,
        ModalAnchor
    }

    public enum MpNotificationDialogResultType {
        None = 0,
        Yes,
        No,
        Ok,
        Cancel,
        Ignore,
        Retry,
        Fix,
        Shutdown,
        DoNotShow,
        Loading
    }

    public enum MpNotificationButtonsType {
        None = 0,
        YesNoCancel,
        Ok,
        OkCancel,
        IgnoreRetryFix,
        IgnoreRetryShutdown,
        ProgressBar
    }
    public enum MpNotificationType {
        None = 0,
        // Loader
        Loader,

        // Message
        DbError,
        Help,
        PluginUpdated,
        Message,
        UserTriggerEnabled,
        UserTriggerDisabled,
        AppModeChange,
        TrialExpired,

        // User Action (System Tray)
        InvalidPlugin,
        InvalidClipboardFormatHandler,
        InvalidAction,
        BadHttpRequest,
        AnalyzerTimeout,
        InvalidRequest,
        InvalidResponse,

        // User Action (Modal) 

        ModalContentFormatDegradation,
        ModalYesNoCancelMessageBox,
        ModalOkCancelMessageBox,

        // Append Tile

        AppendChanged,

        // Plugin Wrapper 
        PluginResponseMessage,
        PluginResponseError,
        PluginResponseWarning,
        PluginResponseWarningWithOption,
        PluginResponseOther
    }

    public enum MpNotificationLayoutType {
        //Default = 0,
        Message,
        Append,
        Loader,
        Warning, //confirm
        UserAction, //retry/ignore/quit
        Error, //confirm
        ErrorWithOption, //retry/ignore/quit
        ErrorAndShutdown //confirm
    }
    public abstract class MpNotificationViewModelBase : MpViewModelBase, MpIPopupMenuViewModel {
        #region Constants

        public const int DEFAULT_NOTIFICATION_SHOWTIME_MS = 3000;

        #endregion

        #region Statics

        public static MpNotificationLayoutType GetLayoutTypeFromNotificationType(MpNotificationType ndt) {
            switch (ndt) {
                case MpNotificationType.Loader:
                    return MpNotificationLayoutType.Loader;
                case MpNotificationType.ModalOkCancelMessageBox:
                case MpNotificationType.ModalYesNoCancelMessageBox:
                case MpNotificationType.ModalContentFormatDegradation:
                case MpNotificationType.InvalidPlugin:
                case MpNotificationType.InvalidAction:
                case MpNotificationType.InvalidClipboardFormatHandler:
                case MpNotificationType.PluginResponseWarningWithOption:
                    return MpNotificationLayoutType.UserAction;
                case MpNotificationType.AnalyzerTimeout:
                case MpNotificationType.InvalidRequest:
                case MpNotificationType.InvalidResponse:
                case MpNotificationType.TrialExpired:
                case MpNotificationType.PluginResponseWarning:
                    return MpNotificationLayoutType.Warning;
                case MpNotificationType.BadHttpRequest:
                case MpNotificationType.DbError:
                case MpNotificationType.PluginResponseError:
                    return MpNotificationLayoutType.Error;
                case MpNotificationType.AppendChanged:
                    return MpNotificationLayoutType.Append;
                default:
                    return MpNotificationLayoutType.Message;
            }
        }
        public static MpNotificationButtonsType GetNotificationButtonsType(MpNotificationType ndt) {
            switch (ndt) {
                case MpNotificationType.ModalYesNoCancelMessageBox:
                    return MpNotificationButtonsType.YesNoCancel;
                case MpNotificationType.ModalOkCancelMessageBox:
                case MpNotificationType.ModalContentFormatDegradation:
                    return MpNotificationButtonsType.OkCancel;
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
                case MpNotificationType.ModalOkCancelMessageBox:
                case MpNotificationType.ModalContentFormatDegradation:
                    return MpNotificationPlacementType.ModalAnchor;
                default:
                    return MpNotificationPlacementType.SystemTray;
            }
        }

        public static bool IsNotificationTypeModal(MpNotificationType ndt) {
            switch (ndt) {
                case MpNotificationType.ModalYesNoCancelMessageBox:
                case MpNotificationType.ModalOkCancelMessageBox:
                case MpNotificationType.ModalContentFormatDegradation:
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        #region Properties

        #region MpIPopupMenuViewModel Implementation

        public virtual MpMenuItemViewModel PopupMenuViewModel {
            get {
                return new MpMenuItemViewModel() {
                    SubItems = new List<MpMenuItemViewModel>() {
                        new MpMenuItemViewModel() {
                            Header = "Hide",
                            IconResourceKey = "ErrorImage",
                            Command = CloseNotificationCommand
                        },
                        new MpMenuItemViewModel() {
                            Header = $"Hide all '{NotificationType.EnumToLabel()}' notifications",
                            IconResourceKey = "ClosedEyeImage",
                            Command = CheckDoNotShowAgainCommand
                        },
                        new MpMenuItemViewModel() {IsSeparator = true, IsVisible = CanPin},
                        new MpMenuItemViewModel() {
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

        #region Appearance
        public object IconSourceObj {
            get {
                if (NotificationFormat == null) {
                    return string.Empty;
                }
                return NotificationFormat.IconSourceObj;
            }
        }
        public string NotificationTextForegroundColor {
            get {
                if (LayoutType == MpNotificationLayoutType.Warning ||
                    LayoutType == MpNotificationLayoutType.UserAction) {
                    return MpSystemColors.Yellow;
                }
                if (LayoutType == MpNotificationLayoutType.ErrorAndShutdown ||
                    LayoutType == MpNotificationLayoutType.ErrorWithOption) {
                    return MpSystemColors.Red;
                }
                if (LayoutType != MpNotificationLayoutType.Message) {
                    return MpSystemColors.royalblue;
                }
                return MpSystemColors.White;
            }
        }

        private string _borderHexColor;
        public string BorderHexColor {
            get {
                if (_borderHexColor == null) {
                    if (IsErrorNotification) {
                        return MpSystemColors.red1;
                    }
                    if (IsWarningNotification) {
                        return MpSystemColors.yellow1;
                    }
                    return MpSystemColors.oldlace;
                }
                return _borderHexColor;
            }
            set {
                if (_borderHexColor != value) {
                    _borderHexColor = value;
                    OnPropertyChanged(nameof(BorderHexColor));
                }
            }
        }

        public string BackgroundHexColor { get; set; } = MpSystemColors.mediumpurple;

        #endregion

        #region Layout

        public MpNotificationButtonsType ButtonsType => GetNotificationButtonsType(NotificationType);
        public MpNotificationLayoutType LayoutType => GetLayoutTypeFromNotificationType(NotificationType);

        public MpNotificationPlacementType PlacementType => GetNotificationPlacementType(NotificationType);

        public bool IsModal => IsNotificationTypeModal(NotificationType);


        #endregion

        #region State

        public virtual bool CanPin => false;
        public bool IsPinned { get; set; } = false;

        public bool IsOverOptionsButton {get;set;}

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

        //public MpTextContentFormat BodyFormat {
        //    get {
        //        if(NotificationFormat == null) {
        //            return MpTextContentFormat.PlainText;
        //        }
        //        return NotificationFormat.BodyFormat;
        //    }
        //}

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
                if(AnchorTarget != value) {
                    NotificationFormat.AnchorTarget = value;
                    OnPropertyChanged(nameof(AnchorTarget));
                }
            }
        }
        public virtual string Title {
            get {
                if(NotificationFormat == null) {
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

        public virtual MpNotificationType NotificationType {
            get {
                if (NotificationFormat == null) {
                    return MpNotificationType.None;
                }
                return NotificationFormat.NotificationType;
            }
        }        

        public int NotificationId => (int)NotificationType;

        public MpNotificationFormat NotificationFormat { get; private set; }
        #endregion

        #endregion

        #region Constructors

        public MpNotificationViewModelBase() : base(null) {
            PropertyChanged += MpNotificationViewModelBase_PropertyChanged;
            NotifierGuid = System.Guid.NewGuid().ToString();
        }
        #endregion

        #region Public Methods

        public virtual async Task InitializeAsync(MpNotificationFormat nf) {
            bool wasBusy = IsBusy;

            IsBusy = true;
            await Task.Delay(1);

            if (nf.IconSourceObj == null) {
                nf.IconSourceObj = MpBase64Images.AppIcon;
            }
            NotificationFormat = nf;

            IsBusy = wasBusy;
        }

        public virtual async Task<MpNotificationDialogResultType> ShowNotificationAsync() {

            bool isDoNotShowType = MpPrefViewModel.Instance.DoNotShowAgainNotificationIdCsvStr
                    .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x)).Any(x => x == (int)NotificationType);

            if (isDoNotShowType) {
                MpConsole.WriteTraceLine($"Notification: {NotificationType.ToString()} marked as hidden");
                return MpNotificationDialogResultType.DoNotShow;
            }
            await Task.Delay(1);
            ShowBalloon();
            return MpNotificationDialogResultType.None;
        }

        public virtual void HideNotification() {
            HideBalloon();
        }

        #endregion

        #region Protected Methods
        protected void ShowBalloon() {
            //_nbv.ShowWindow(nvmb);
            //IsVisible = true;
            MpPlatformWrapper.Services.NotificationManager.ShowNotification(this);
        }

        protected void HideBalloon() {
            //_nbv.HideWindow(nvmb);
            MpPlatformWrapper.Services.NotificationManager.HideNotification(this);
            //Parent.RemoveNotificationCommand.Execute(this);
            //Notifications.Remove(nvmb);
            //IsVisible = false;
        }

        #endregion

        #region Private Methods
        private void MpNotificationViewModelBase_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(DoNotShowAgain):
                    if(DoNotShowAgain) {
                        MpPrefViewModel.Instance.DoNotShowAgainNotificationIdCsvStr += NotificationId + ",";
                        // show loop checks if DoNotShowAgain is true to hide
                    }
                    break;
                case nameof(NotificationType):
                    OnPropertyChanged(nameof(LayoutType));
                    break;
                case nameof(IsPopupMenuOpen):
                    break;
                case nameof(IsVisible):
                    if(!IsVisible) {
                        IsClosing = false;
                    }
                    break;
            }
        }
        #endregion

        #region Commands
        public ICommand ResetAllNotificationsCommand => new MpCommand(
             () => {
                 // TODO this should be moved to somewhere in preferences
                 MpPrefViewModel.Instance.DoNotShowAgainNotificationIdCsvStr = string.Empty;

             }, () => MpBootstrapperViewModelBase.IsCoreLoaded);

        public ICommand CheckDoNotShowAgainCommand => new MpCommand(
            () => {
                DoNotShowAgain = true;
                CloseNotificationCommand.Execute(null);
            });

        public ICommand CloseNotificationCommand => new MpCommand(
            () => {
                MpPlatformWrapper.Services.NotificationManager.HideNotification(this);
            });

        public ICommand ToggleIsPinnedCommand => new MpCommand(
            () => {
                IsPinned = !IsPinned;
            },()=>CanPin);
        #endregion
    }
}
