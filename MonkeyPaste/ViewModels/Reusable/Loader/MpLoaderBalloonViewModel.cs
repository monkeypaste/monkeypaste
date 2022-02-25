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
    public enum MpLoaderNotificationType { 
        None = 0,
        InvalidPlugin,
        DbError,
        LoadComplete
    }

    public enum MpLoaderExceptionSeverityType {
        None = 0,
        Warning,
        WarningWithOption,
        ErrorWithOption,
        ErrorAndShutdown
    }

    public enum MpLoaderNotificationResultType {
        None = 0,
        Ignore,
        Retry,
        Shutdown
    }

    public class MpLoaderBalloonViewModel : MpViewModelBase, MpISingletonViewModel<MpLoaderBalloonViewModel> {
        #region Static Variables
        #endregion

        #region Private Variables
        private int _updateCount = 0;
        #endregion


        #region Properties

        #region MpISingletonViewModel Implementation

        private static MpLoaderBalloonViewModel _instance;
        public static MpLoaderBalloonViewModel Instance => _instance ?? (_instance = new MpLoaderBalloonViewModel());

        #endregion

        #region View Models
        #endregion

        #region State

        public bool IsLoaded => PercentLoaded >= 1.0;

        public double PercentLoaded { get; set; }

        public bool IsValid => string.IsNullOrEmpty(ValidationMessage);                

        public MpLoaderNotificationType CurrentNotification { get; set; } = MpLoaderNotificationType.None;

        public MpLoaderExceptionSeverityType ExceptionType { get; set; } = MpLoaderExceptionSeverityType.None;

        public bool ShowIgnoreButton {
            get {
                if(IsValid) {
                    return false;
                }
                return ExceptionType != MpLoaderExceptionSeverityType.ErrorAndShutdown;
            }
        }

        public bool ShowRetryButton {
            get {
                if (IsValid) {
                    return false;
                }
                return ExceptionType != MpLoaderExceptionSeverityType.ErrorAndShutdown;
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

        public MpLoaderNotificationResultType LastNotificationResult { get; set; } = MpLoaderNotificationResultType.None;
        #endregion

        #region Appearance

        public string NotificationTextForegroundColor {
            get {
                if(ExceptionType == MpLoaderExceptionSeverityType.Warning || 
                    ExceptionType == MpLoaderExceptionSeverityType.WarningWithOption) {
                    return MpSystemColors.Yellow;
                }
                if(ExceptionType == MpLoaderExceptionSeverityType.ErrorAndShutdown ||
                    ExceptionType == MpLoaderExceptionSeverityType.ErrorWithOption) {
                    return MpSystemColors.Red;
                }
                if(ExceptionType != MpLoaderExceptionSeverityType.None) {
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
        public MpLoaderBalloonViewModel() : base(null) {
            PropertyChanged += MpStandardBalloonViewModel_PropertyChanged;
        }


        public async Task Init() {
            await Task.Delay(1);
        }

        public void SetNotification(
            MpLoaderNotificationType notificationType = MpLoaderNotificationType.None, 
            MpLoaderExceptionSeverityType exceptionType = MpLoaderExceptionSeverityType.None, 
            MpLoaderNotificationResultType result = MpLoaderNotificationResultType.None,
            string msg = "") {
            LastNotificationResult = result;

            CurrentNotification = notificationType;
            ExceptionType = exceptionType;
            ValidationMessage = msg;
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
                    result: MpLoaderNotificationResultType.Ignore);
            });

        public ICommand RetryCommand => new MpRelayCommand(
            () => {
                SetNotification(
                    result: MpLoaderNotificationResultType.Retry);
            });

        public ICommand ShutdownCommand => new MpRelayCommand(
            () => {
                System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
            });

        #endregion
    }
}