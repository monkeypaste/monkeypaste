using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
        InvalidAction,
        BadHttpRequest,
        DbError,
        LoadComplete,
        Help
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
                return MpSystemColors.Black;
            }
        }

        #endregion

        #region State

        public bool IsVisible { get; set; } = false;

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

        #endregion

        #region Model

        public bool DoNotShowAgain { get; set; } = false;

        public string IconImageBase64 { 
            get {
                if(IsErrorNotification) {
                    return MpBase64Images.Error;
                }
                if (IsWarningNotification) {
                    return MpBase64Images.Warning;
                }
                return MpBase64Images.AppIcon;
            }
        }

        public virtual string Title { get; set; }

        public virtual string Body { get; set; }

        public virtual string Detail { get; set; }

        public MpNotificationDialogType DialogType { get; set; }

        public MpNotificationExceptionSeverityType ExceptionType { get; set; }

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
            }
        }
        #endregion

        #region Commands

        #endregion
    }
}
