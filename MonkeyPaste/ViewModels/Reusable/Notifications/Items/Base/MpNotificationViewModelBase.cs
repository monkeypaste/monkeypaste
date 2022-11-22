using System;
using System.Collections.Generic;
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
        CenterActiveScreen
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
        Loader,
        InvalidPlugin,
        InvalidClipboardFormatHandler,
        InvalidAction,
        BadHttpRequest,
        AnalyzerTimeout,
        InvalidRequest,
        InvalidResponse,
        DbError,
        LoadComplete,
        Help,
        PluginUpdated,
        Message,
        UserTriggerEnabled,
        UserTriggerDisabled,
        AppModeChange,
        AppendBuffer,
        ContentFormatDegradation,
        TrialExpired,
        PluginResponseError,
        PluginResponseWarning,
        PluginResponseWarningWithOption,
        PluginResponseOther
    }

    public enum MpNotificationLayoutType {
        Default = 0,
        Message,
        Loader,
        Warning, //confirm
        WarningWithOption, //retry/ignore/quit
        Error, //confirm
        ErrorWithOption, //retry/ignore/quit
        ErrorAndShutdown //confirm
    }
    public abstract class MpNotificationViewModelBase : MpViewModelBase {
        #region Statics

        public static MpNotificationLayoutType GetLayoutTypeFromNotificationType(MpNotificationType ndt) {
            switch(ndt) {
                case MpNotificationType.Loader:
                    return MpNotificationLayoutType.Loader;
                case MpNotificationType.ContentFormatDegradation:
                case MpNotificationType.InvalidPlugin:
                case MpNotificationType.InvalidAction:
                case MpNotificationType.InvalidClipboardFormatHandler:
                case MpNotificationType.PluginResponseWarningWithOption:
                    return MpNotificationLayoutType.WarningWithOption;
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
                default:
                    return MpNotificationLayoutType.Default;
            }
        }
        public static MpNotificationButtonsType GetNotificationButtonsType(MpNotificationType ndt) {
            switch (ndt) {
                case MpNotificationType.ContentFormatDegradation:
                    return MpNotificationButtonsType.OkCancel;
                default:
                    MpNotificationLayoutType layoutType = GetLayoutTypeFromNotificationType(ndt);
                    switch (layoutType) {
                        case MpNotificationLayoutType.WarningWithOption:
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
                case MpNotificationType.ContentFormatDegradation:
                    return MpNotificationPlacementType.CenterActiveScreen;
                default:
                    return MpNotificationPlacementType.SystemTray;
            }
        }

        public static bool GetNotificationTypeModality(MpNotificationType ndt) {
            switch(ndt) {
                case MpNotificationType.ContentFormatDegradation:
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        #region Properties

        #region Appearance
        public object IconSourceStr {
            get {
                if (NotificationFormat == null) {
                    return string.Empty;
                }
                return NotificationFormat.IconSourceStr;
            }
        }
        public string NotificationTextForegroundColor {
            get {
                if (LayoutType == MpNotificationLayoutType.Warning ||
                    LayoutType == MpNotificationLayoutType.WarningWithOption) {
                    return MpSystemColors.Yellow;
                }
                if (LayoutType == MpNotificationLayoutType.ErrorAndShutdown ||
                    LayoutType == MpNotificationLayoutType.ErrorWithOption) {
                    return MpSystemColors.Red;
                }
                if (LayoutType != MpNotificationLayoutType.Default) {
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

        public bool IsModal => GetNotificationTypeModality(NotificationType);

        #endregion

        #region State

        //public bool IsVisible { get; set; } = false;

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
                    LayoutType == MpNotificationLayoutType.WarningWithOption;
            }
        }


        public bool IsHovering { get; set; } = false;

        public bool IsVisible { get; set; } = false;

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

        public virtual string Title {
            get {
                if(NotificationFormat == null) {
                    return string.Empty;
                }
                return NotificationFormat.Title;
            }
        }

        public virtual string Body {
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

            if (string.IsNullOrEmpty(nf.IconSourceStr)) {
                nf.IconSourceStr = MpBase64Images.AppIcon;
            }
            NotificationFormat = nf;

            IsBusy = wasBusy;
        }

        public virtual async Task<MpNotificationDialogResultType> ShowNotificationAsync() {
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
            MpPlatformWrapper.Services.NotificationView.ShowWindow(this);
        }

        protected void HideBalloon() {
            //_nbv.HideWindow(nvmb);
            MpPlatformWrapper.Services.NotificationView.HideWindow(this);
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
            }
        }
        #endregion

        #region Commands
        public ICommand ResetAllNotificationsCommand => new MpCommand(
             () => {
                 // TODO this should be moved to somewhere in preferences
                 MpPrefViewModel.Instance.DoNotShowAgainNotificationIdCsvStr = string.Empty;

             }, () => MpBootstrapperViewModelBase.IsCoreLoaded);

        #endregion
    }
}
