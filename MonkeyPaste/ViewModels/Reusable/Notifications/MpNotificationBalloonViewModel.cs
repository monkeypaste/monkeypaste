using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
namespace MonkeyPaste {   
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

        //public ObservableCollection<Mp> PendingNotifications { get; set; } = new ObservableCollection<MpNotificationViewModel>();

        #endregion

        #region State

        public bool IsVisible { get; set; } = false;

        public bool IsStartupState => NotifierState == MpNotifierStateType.Startup;

        public bool IsWarningState => NotifierState == MpNotifierStateType.Warning;


        public bool IsValid => string.IsNullOrEmpty(ValidationMessage);

        public MpNotifierStateType NotifierState { get; set; } = MpNotifierStateType.None;

        public MpNotificationType CurrentNotification { get; set; } = MpNotificationType.None;

        public MpNotificationExceptionSeverityType ExceptionType { get; set; } = MpNotificationExceptionSeverityType.None;

        

        public MpNotificationUserActionType LastNotificationResult { get; set; } = MpNotificationUserActionType.None;
        #endregion

        #region Appearance

        #endregion

        public string ValidationMessage { get; set; }

        public string PostLoadedMessage { get; set; }

        public string Info { get; set; }


        #endregion

        #region Constructors

        public MpNotificationBalloonViewModel() : base(null) {
            PropertyChanged += MpStandardBalloonViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public async Task Init() {
            await Task.Delay(1);
            NotifierState = MpNotifierStateType.Startup;
        }

        public async Task<MpNotificationViewModelBase> CreateNotificationViewModel(MpNotification n) {
            var nvm = new MpNotificationViewModelBase(this);
            await nvm.InitializeAsync(n);
            return nvm;
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
            double maxShowTimeMs = -1) {

            var n = new MpNotification() {
                NotificationType = notificationType,
                SeverityType = exceptionType,
                ResultType = result,
                Title = notificationType.EnumToLabel(),
                Body = msg,
                MaxShowMs = maxShowTimeMs
            };
            var nvm = await CreateNotificationViewModel(n);

            SetNotification(notificationType, exceptionType, result, msg);
            
            bool wasVisible = IsVisible;
            if (!IsVisible) {
                ShowBalloon();
            }

            if(maxShowTimeMs > 0) {
                DateTime startTime = DateTime.Now;
                while (LastNotificationResult == MpNotificationUserActionType.None &&
                       DateTime.Now - startTime <= TimeSpan.FromMilliseconds(maxShowTimeMs)) {
                    await Task.Delay(100);
                }
            } else {
                while (LastNotificationResult == MpNotificationUserActionType.None) {
                    await Task.Delay(100);
                }
            }

            if(!wasVisible) {
                //Only hide balloon if it wasn't visible before this notification
                HideBalloon();
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

            MpConsole.WriteLines(
                $"Notification balloon set to:",
                $"msg: '{msg}'",
                $"type: '{notificationType.ToString()}'",
                $"severity: '{exceptionType.ToString()}'",
                $"result: '{LastNotificationResult.ToString()}'");
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
        public ICommand IgnoreCommand => new MpCommand(
            () => {
                SetNotification(
                    result: MpNotificationUserActionType.Ignore);
            });

        public ICommand RetryCommand => new MpCommand(
            () => {
                SetNotification(
                    result: MpNotificationUserActionType.Retry);
            });

        public ICommand ShutdownCommand => new MpCommand(
            () => {
                System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow();
            });

        #endregion
    }
}