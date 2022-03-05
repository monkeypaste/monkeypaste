using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
namespace MonkeyPaste {   
    public class MpNotificationCollectionViewModel : MpViewModelBase {
        #region Static Variables
        #endregion

        #region Private Variables        

        private MpINotificationBalloonView _nbv;

        #endregion

        #region Properties

        #region MpISingletonViewModel Implementation

        private static MpNotificationCollectionViewModel _instance;
        public static MpNotificationCollectionViewModel Instance => _instance ?? (_instance = new MpNotificationCollectionViewModel());

        #endregion

        #region View Models

        public ObservableCollection<MpNotificationViewModelBase> NotificationQueue { get; private set; } = new ObservableCollection<MpNotificationViewModelBase>();

        public MpNotificationViewModelBase CurrentNotificationViewModel => NotificationQueue.FirstOrDefault();

        #endregion

        #region State

        public bool IsInitialLoad { get; private set; } = false;

        public bool IsVisible { get; set; } = false;

        #endregion

        #region Appearance


        #endregion

        #region Model

        public List<int> DoNotShowNotificationIds { get; private set; } = new List<int>();

        #endregion

        #endregion

        #region Constructors

        public MpNotificationCollectionViewModel() : base(null) {
            PropertyChanged += MpStandardBalloonViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public async Task Init() {
            await Task.Delay(1);
        }

        public async Task Init(List<int> doNotShowNotifications) {
            await Task.Delay(1);
            if(doNotShowNotifications != null) {
                DoNotShowNotificationIds = doNotShowNotifications;
            }
        }

        public async Task RegisterWithWindow(MpINotificationBalloonView nbv) {
            await Task.Delay(1);
            _nbv = nbv;

            var lvm = new MpLoaderNotificationViewModel(this) {
                DialogType = MpNotificationDialogType.StartupLoader,
                PercentLoaded = 0.0
            };
            NotificationQueue.Add(lvm);
            OnPropertyChanged(nameof(CurrentNotificationViewModel));
        }

        public async Task<MpDialogResultType> ShowUserAction(
            MpNotificationDialogType dialogType = MpNotificationDialogType.None,
            MpNotificationExceptionSeverityType exceptionType = MpNotificationExceptionSeverityType.None,
            string title = "",
            string msg = "",
            double maxShowTimeMs = -1) {
            if(DoNotShowNotificationIds.Contains((int)dialogType)) {
                return MpDialogResultType.Ignore;
            }

            if(string.IsNullOrEmpty(title)) {
                if (exceptionType == MpNotificationExceptionSeverityType.Warning ||
                    exceptionType == MpNotificationExceptionSeverityType.WarningWithOption) {
                    title = "Warning: ";
                } else {
                    title = "Error: ";
                }
            }
            title += dialogType.EnumToLabel();

            var unvm = new MpUserActionNotificationViewModel(this) {
                DialogType = dialogType,
                ExceptionType = exceptionType,
                Title = title,
                Body = msg
            };

            NotificationQueue.Add(unvm);
            OnPropertyChanged(nameof(CurrentNotificationViewModel));

            if(!IsVisible) {
                ShowBalloon();
            }

            MpConsole.WriteLines(
                $"Notification balloon set to:",
                $"msg: '{msg}'",
                $"type: '{dialogType.ToString()}'",
                $"severity: '{exceptionType.ToString()}'");

            if (maxShowTimeMs > 0) {
                DateTime startTime = DateTime.Now;
                while (unvm.DialogResult == MpDialogResultType.None &&
                       DateTime.Now - startTime <= TimeSpan.FromMilliseconds(maxShowTimeMs)) {
                    await Task.Delay(100);
                }
            } else {
                while (unvm.DialogResult == MpDialogResultType.None) {
                    await Task.Delay(100);
                }
            }

            ShiftToNextNotificationCommand.Execute(null);

            return unvm.DialogResult;
        }


        public void BeginLoader() {
            IsInitialLoad = true;
            if (DoNotShowNotificationIds.Contains((int)MpNotificationDialogType.StartupLoader)) {
                return;
            }

            ShowBalloon();
        }

        public void FinishLoading() {
            IsInitialLoad = false;
            ShiftToNextNotificationCommand.Execute(null);
        }

        public void ShowBalloon() {
            IsVisible = true;
        }

        public void HideBalloon() {
            IsVisible = false;
        }
        #endregion

        #region Private Methods

        private void MpStandardBalloonViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            
        }

        #endregion

        #region Commands

        public MpIAsyncCommand ResetAllNotificationsCommand => new MpAsyncCommand(
            async () => {
                while (IsInitialLoad) {
                    //wait for dependencies to load
                    await Task.Delay(100);
                }
                MpPreferences.DoNotShowAgainNotificationIdCsvStr = string.Empty;

            });

        public MpIAsyncCommand<int> DoNotShowAgainCommand => new MpAsyncCommand<int>(
            async (notificationId) => {
                if(DoNotShowNotificationIds.Contains(notificationId)) {
                    return;
                }
                ShiftToNextNotificationCommand.Execute(null);

                while(IsInitialLoad) {
                    //wait for dependencies to load
                    await Task.Delay(100);
                }

                DoNotShowNotificationIds.Add(notificationId);

                MpPreferences.DoNotShowAgainNotificationIdCsvStr = string.Join(",", DoNotShowNotificationIds);
            });

        public ICommand ShiftToNextNotificationCommand => new MpCommand(
             () => {                
                 if (CurrentNotificationViewModel == null || NotificationQueue.Count == 1) {
                    HideBalloon();
                 } else {
                     NotificationQueue.RemoveAt(0);
                     OnPropertyChanged(nameof(CurrentNotificationViewModel));
                 }

                 
             });
        #endregion
    }
}