using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MonkeyPaste.Common;

namespace MonkeyPaste {
    //public enum MpNotifierType {
    //    None = 0,
    //    Startup,
    //    Dialog
    //}

    public enum MpNotificationDialogType {
        None = 0,
        StartupLoader,
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
        TrialExpired
    }

    public enum MpNotificationExceptionSeverityType {
        None = 0,
        Warning, //confirm
        WarningWithOption, //retry/ignore/quit
        Error, //confirm
        ErrorWithOption, //retry/ignore/quit
        ErrorAndShutdown //confirm
    }
    public abstract class MpNotificationViewModelBase : MpViewModelBase<MpNotificationCollectionViewModel> {
        #region Statics

        public static MpNotificationExceptionSeverityType GetExceptionFromNotificationType(MpNotificationDialogType ndt) {
            switch(ndt) {
                case MpNotificationDialogType.InvalidPlugin:
                case MpNotificationDialogType.InvalidAction:
                case MpNotificationDialogType.InvalidClipboardFormatHandler:
                    return MpNotificationExceptionSeverityType.WarningWithOption;
                case MpNotificationDialogType.AnalyzerTimeout:
                case MpNotificationDialogType.InvalidRequest:
                case MpNotificationDialogType.InvalidResponse:
                case MpNotificationDialogType.TrialExpired:
                    return MpNotificationExceptionSeverityType.Warning;
                case MpNotificationDialogType.BadHttpRequest:
                case MpNotificationDialogType.DbError:
                    return MpNotificationExceptionSeverityType.Error;
                default:
                    return MpNotificationExceptionSeverityType.None;
            }
        }


        #endregion

        #region Properties

        #region Appearance

        public string NotificationTextForegroundColor {
            get {
                if (ExceptionType == MpNotificationExceptionSeverityType.Warning ||
                    ExceptionType == MpNotificationExceptionSeverityType.WarningWithOption) {
                    return MpSystemColors.Yellow;
                }
                if (ExceptionType == MpNotificationExceptionSeverityType.ErrorAndShutdown ||
                    ExceptionType == MpNotificationExceptionSeverityType.ErrorWithOption) {
                    return MpSystemColors.Red;
                }
                if (ExceptionType != MpNotificationExceptionSeverityType.None) {
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

        #region Icon


        public int IconId { get; set; } = 0;

        public string IconResourceKey { get; set; } = string.Empty;

        public string IconHexStr { get; set; } = string.Empty;

        private string _iconImageBase64;
        public string IconImageBase64 {
            get {
                if (string.IsNullOrEmpty(_iconImageBase64)) {
                    if (IsErrorNotification) {
                        return MpBase64Images.Error;
                    }
                    if (IsWarningNotification) {
                        return MpBase64Images.Warning;
                    }
                    return MpBase64Images.AppIcon;
                }
                return _iconImageBase64;
            }
            set {
                if (IconImageBase64 != value) {
                    _iconImageBase64 = value;
                    OnPropertyChanged(nameof(IconImageBase64));
                }
            }
        }

        public object IconSourceObj {
            get {
                if (IconId > 0) {
                    return IconId;
                }
                if (IconHexStr.IsStringHexColor()) {
                    return IconHexStr;
                }
                return IconResourceKey;
            }
        }

        #endregion


        #endregion

        #region State

        //public bool IsVisible { get; set; } = false;

        public virtual bool CanChooseNotShowAgain => true;

        public bool IsErrorNotification {
            get {
                return ExceptionType == MpNotificationExceptionSeverityType.Error ||
                    ExceptionType == MpNotificationExceptionSeverityType.ErrorAndShutdown ||
                    ExceptionType == MpNotificationExceptionSeverityType.ErrorWithOption;
            }
        }

        public bool IsWarningNotification {
            get {
                return ExceptionType == MpNotificationExceptionSeverityType.Warning ||
                    ExceptionType == MpNotificationExceptionSeverityType.WarningWithOption;
            }
        }

        public bool IsStartupNotification => DialogType == MpNotificationDialogType.StartupLoader;

        public bool IsLoaderNotification => DialogType == MpNotificationDialogType.Loader;

        public bool IsIconBase64 => string.IsNullOrEmpty(IconResourceKey);

        public bool IsHovering { get; set; } = false;

        public bool IsVisible { get; set; } = false;
        #endregion

        #region Model

        

        public bool DoNotShowAgain { get; set; } = false;

        


        public virtual string Title { get; set; }

        public virtual string Body { get; set; }

        public virtual string Detail { get; set; }

        public virtual MpNotificationDialogType DialogType { get; set; }

        public MpNotificationExceptionSeverityType ExceptionType => GetExceptionFromNotificationType(DialogType);

        public int NotificationId => (int)DialogType;

        #endregion

        #endregion

        #region Constructors

        public MpNotificationViewModelBase() : base(null) { }

        public MpNotificationViewModelBase(MpNotificationCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpNotificationViewModelBase_PropertyChanged;
        }



        #endregion

        #region Public Methods
        #endregion

        #region Private Methods
        private void MpNotificationViewModelBase_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(DoNotShowAgain):
                    if(DoNotShowAgain) {
                        Parent.DoNotShowAgainCommand.Execute((int)DialogType);
                    }
                    break;
                case nameof(DialogType):
                    OnPropertyChanged(nameof(ExceptionType));
                    break;
            }
        }
        #endregion

        #region Commands

        #endregion
    }
}
