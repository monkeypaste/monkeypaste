using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MonkeyPaste;
namespace MonkeyPaste {
    public enum MpNotifierStateType {
        None = 0,
        Startup,
        Warning
    }

    public enum MpNotificationType { 
        None = 0,
        InvalidPlugin,
        InvalidAction,
        DbError,
        LoadComplete
    }

    public enum MpNotificationExceptionSeverityType {
        None = 0,
        Warning,
        WarningWithOption,
        ErrorWithOption,
        ErrorAndShutdown
    }

    public enum MpNotificationUserActionType {
        None = 0,
        Ignore,
        Retry,
        Shutdown
    }

    public class MpNotificationBalloonViewModel : MpViewModelBase {
        #region Static Variables
        #endregion

        #region Private Variables
        private int _updateCount = 0;

        private MpINotificationBalloonView _nbv;

        #endregion


        #region Properties

        #region MpISingletonViewModel Implementation

        private static MpNotificationBalloonViewModel _instance;
        public static MpNotificationBalloonViewModel Instance => _instance ?? (_instance = new MpNotificationBalloonViewModel());

        #endregion

        #region View Models
        #endregion

        #region State

        public bool IsVisible { get; set; } = false;

        public bool IsStartupState => NotifierState == MpNotifierStateType.Startup;

        public bool IsWarningState => NotifierState == MpNotifierStateType.Warning;

        public bool IsLoaded => PercentLoaded >= 1.0;

        public double PercentLoaded { get; set; }

        public bool IsValid => string.IsNullOrEmpty(ValidationMessage);

        public MpNotifierStateType NotifierState { get; set; } = MpNotifierStateType.None;

        public MpNotificationType CurrentNotification { get; set; } = MpNotificationType.None;

        public MpNotificationExceptionSeverityType ExceptionType { get; set; } = MpNotificationExceptionSeverityType.None;

        public bool ShowIgnoreButton {
            get {
                if(IsValid) {
                    return false;
                }
                return ExceptionType != MpNotificationExceptionSeverityType.ErrorAndShutdown;
            }
        }

        public bool ShowRetryButton {
            get {
                if (IsValid) {
                    return false;
                }
                return ExceptionType != MpNotificationExceptionSeverityType.ErrorAndShutdown;
            }
        }

        public bool ShowShutdownButton {
            get {
                if (IsValid) {
                    return false;
                }
                return true;
            }
        }

        public MpNotificationUserActionType LastNotificationResult { get; set; } = MpNotificationUserActionType.None;
        #endregion

        #region Appearance

        public string NotificationTextForegroundColor {
            get {
                if(ExceptionType == MpNotificationExceptionSeverityType.Warning || 
                    ExceptionType == MpNotificationExceptionSeverityType.WarningWithOption) {
                    return MpSystemColors.Yellow;
                }
                if(ExceptionType == MpNotificationExceptionSeverityType.ErrorAndShutdown ||
                    ExceptionType == MpNotificationExceptionSeverityType.ErrorWithOption) {
                    return MpSystemColors.Red;
                }
                if(ExceptionType != MpNotificationExceptionSeverityType.None) {
                    return MpSystemColors.royalblue;
                }
                return MpSystemColors.Black;
            }
        }

        public double ProgressTotalBarWidth { get; set; }

        public double ProgressBarCurrentWidth => ProgressTotalBarWidth * PercentLoaded;

        #endregion

        public string ValidationMessage { get; set; }

        public string PostLoadedMessage { get; set; }

        public string Info { get; set; }

        public string LoadingLabel { get; set; }

        public string PercentLabel {
            get {
                int percent = (int)(PercentLoaded * 100);
                return $"{percent} %";
            }
        }

        #endregion

        #region Public Methods
        public MpNotificationBalloonViewModel() : base(null) {
            PropertyChanged += MpStandardBalloonViewModel_PropertyChanged;
        }

        public async Task Init() {
            await Task.Delay(1);
            NotifierState = MpNotifierStateType.Startup;
        }

        public async Task Attach(MpINotificationBalloonView nbv) {
            await Task.Delay(1);
            _nbv = nbv;
            //_nbv.SetDataContext(this);
        }

        public async Task<MpNotificationUserActionType> ShowUserActions(
            MpNotificationType notificationType = MpNotificationType.None,
            MpNotificationExceptionSeverityType exceptionType = MpNotificationExceptionSeverityType.None,
            MpNotificationUserActionType result = MpNotificationUserActionType.None,
            string msg = "",
            double maxShowTimeMs = 100000) {
            SetNotification(notificationType, exceptionType, result, msg);

            DateTime startTime = DateTime.Now;
            while (LastNotificationResult == MpNotificationUserActionType.None &&
                   DateTime.Now - startTime <= TimeSpan.FromMilliseconds(maxShowTimeMs)) {
                await Task.Delay(100);
            }
            return LastNotificationResult;
        }

        public void SetNotification(
            MpNotificationType notificationType = MpNotificationType.None, 
            MpNotificationExceptionSeverityType exceptionType = MpNotificationExceptionSeverityType.None, 
            MpNotificationUserActionType result = MpNotificationUserActionType.None,
            string msg = "") {
            LastNotificationResult = result;

            CurrentNotification = notificationType;
            ExceptionType = exceptionType;
            ValidationMessage = msg;

            if (!IsVisible) {
                ShowBalloon();
            }
        }


        public void ShowBalloon() {
            if (IsVisible) {
                return;
            }
            _nbv.ShowBalloon();
            IsVisible = true;
        }

        public void HideBalloon() {
            if (!IsVisible) {
                return;
            }
            _nbv.HideBalloon();
            IsVisible = false;
        }
        #endregion


        #region Private Methods

        private void MpStandardBalloonViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(PercentLoaded):
                    if(PercentLoaded > 1.0) {
                        PercentLoaded = 1.0;
                    }
                    
                    OnPropertyChanged(nameof(ProgressBarCurrentWidth));
                    OnPropertyChanged(nameof(PercentLabel));
                    OnPropertyChanged(nameof(IsLoaded));

                    int dotCount = _updateCount % 4;
                    LoadingLabel = "LOADING";
                    for(int i = 0;i < dotCount;i++) {
                        LoadingLabel += ".";
                    }
                    _updateCount++;
                    break;
            }
        }

        #endregion

        #region Commands
        public ICommand IgnoreCommand => new MpRelayCommand(
            () => {
                SetNotification(
                    result: MpNotificationUserActionType.Ignore);
            });

        public ICommand RetryCommand => new MpRelayCommand(
            () => {
                SetNotification(
                    result: MpNotificationUserActionType.Retry);
            });

        public ICommand ShutdownCommand => new MpRelayCommand(
            () => {
                System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
            });

        #endregion
    }
}