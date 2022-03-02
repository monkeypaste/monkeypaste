using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste {
    public abstract class MpNotificationViewModelBase : MpViewModelBase<MpNotificationBalloonViewModel> {
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

        #endregion

        #region Model

        public abstract string IconImageBase64 { get; }

        public virtual string Title {
            get {
                if(Notification == null) {
                    return string.Empty;
                }
                return Notification.Title;
            }
            set {
                if(Title != value) {
                    Notification.Title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public virtual string Body {
            get {
                if (Notification == null) {
                    return string.Empty;
                }
                return Notification.Body;
            }
            set {
                if(Body != value) {
                    Notification.Body = value;
                    OnPropertyChanged(nameof(Body));
                }
            }
        }

        public virtual string Detail {
            get {
                if (Notification == null) {
                    return string.Empty;
                }
                return Notification.Detail;
            }
            set {
                if (Detail != value) {
                    Notification.Detail = value;
                    OnPropertyChanged(nameof(Detail));
                }
            }
        }

        public MpNotifierType NotifierType {
            get {
                if(Notification == null) {
                    return MpNotifierType.None;
                }
                return Notification.NotifierType;
            }
        }

        public MpNotificationDialogType DialogType {
            get {
                if(Notification == null) {
                    return MpNotificationDialogType.None;
                }
                return Notification.DialogType;
            }
        }

        public MpNotificationExceptionSeverityType ExceptionType {
            get {
                if(Notification == null) {
                    return MpNotificationExceptionSeverityType.None;
                }
                return Notification.SeverityType;
            }
        }

        public bool DoNotShowAgain {
            get {
                if(Notification == null) {
                    return false;
                }
                return Notification.DoNotShowAgain;
            }
            set {
                if(DoNotShowAgain != value) {
                    Notification.DoNotShowAgain = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(DoNotShowAgain));
                }
            }
        }

        public MpNotification Notification { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpNotificationViewModelBase() : base(null) { }

        public MpNotificationViewModelBase(MpNotificationBalloonViewModel parent) : base(parent) {
            PropertyChanged += MpNotificationViewModelBase_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpNotification notification) {
            //IsBusy = true;

            await Task.Delay(1);
            Notification = notification;

            //IsBusy = false;
        }

        #endregion

        #region Private Methods

        private void MpNotificationViewModelBase_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(HasModelChanged):
                    if(HasModelChanged) {
                        WriteModel(Notification);
                    }
                    break;
            }
        }
        #endregion

        #region Commands

        #endregion
    }
}
