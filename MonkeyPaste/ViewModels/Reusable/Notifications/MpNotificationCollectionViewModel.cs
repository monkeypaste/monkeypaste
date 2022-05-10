using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Xamarin.Forms;
using MonkeyPaste.Plugin;
using Xamarin.Essentials;

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

        public ObservableCollection<MpNotificationViewModelBase> Notifications { get; private set; } = new ObservableCollection<MpNotificationViewModelBase>();

        //public MpNotificationViewModelBase CurrentNotificationViewModel => NotificationQueue.FirstOrDefault();

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
            //IsVisible = true;
        }

        public async Task RegisterWithWindow(MpINotificationBalloonView nbv) {
            await Task.Delay(1);
            _nbv = nbv;
        }

        public async Task ShowMessage(
            string title = "", 
            string msg = "", 
            double maxShowTimeMs = 3000) {
            await ShowUserAction(
                dialogType: MpNotificationDialogType.Message,
                title: title,
                msg: msg,
                maxShowTimeMs: maxShowTimeMs);
        }
        public async Task<MpDialogResultType> ShowUserAction(
            MpNotificationDialogType dialogType = MpNotificationDialogType.None,
            MpNotificationExceptionSeverityType exceptionType = MpNotificationExceptionSeverityType.None,
            string title = "",
            string msg = "",
            double maxShowTimeMs = -1,
            Action<object> retryAction = null,
            object retryActionObj = null) {

            MpDialogResultType userActionResult = MpDialogResultType.None;

            if (DoNotShowNotificationIds.Contains((int)dialogType)) {
                MpConsole.WriteTraceLine($"Notification: {dialogType.ToString()} marked as hidden");
                userActionResult = MpDialogResultType.Ignore;
            }

            if (string.IsNullOrEmpty(title)) {
                if (exceptionType == MpNotificationExceptionSeverityType.Warning ||
                    exceptionType == MpNotificationExceptionSeverityType.WarningWithOption) {
                    title = "Warning: ";
                } else {
                    title = "Error: ";
                }

                title += dialogType.EnumToLabel();
            }

            var unvm = new MpUserActionNotificationViewModel(this) {
                DialogType = dialogType,
                ExceptionType = exceptionType,
                Title = title,
                Body = msg
            };

            Notifications.Add(unvm);
            //OnPropertyChanged(nameof(CurrentNotificationViewModel));

            //if (!IsVisible) 
            {
                ShowBalloon(unvm);
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

                    while(unvm.IsHovering) {
                        await Task.Delay(100);
                    }
                }
            } else {
                while (unvm.DialogResult == MpDialogResultType.None) {
                    await Task.Delay(100);
                }
            }

            RemoveNotificationCommand.Execute(unvm);

            if (unvm.DialogResult == MpDialogResultType.Retry &&
               retryAction != null) {
                retryAction.Invoke(retryActionObj);
            }
            userActionResult = unvm.DialogResult;

            return userActionResult;
        }

        public async Task BeginLoader(MpIProgressLoader loader) {
            if (DoNotShowNotificationIds.Contains((int)loader.DialogType)) {
                MpConsole.WriteTraceLine($"Notification: {loader.DialogType.ToString()} marked as hidden");
                return;
            }

            var lvm = await CreateLoaderViewModel(loader); 
            Notifications.Add(lvm);

            //OnPropertyChanged(nameof(CurrentNotificationViewModel));

            ShowBalloon(lvm);
        }

        public void FinishLoading() {
            var lvm = Notifications.FirstOrDefault(x => x is MpLoaderNotificationViewModel);
            if(lvm != null) {
                RemoveNotificationCommand.Execute(lvm);
            }
        }

        public void ShowBalloon(MpNotificationViewModelBase nvmb) {
            _nbv.ShowWindow(nvmb);
            //IsVisible = true;
        }

        public void HideBalloon(MpNotificationViewModelBase nvmb) {
            _nbv.HideWindow(nvmb);
            Notifications.Remove(nvmb);
            //IsVisible = false;
        }
        #endregion

        #region Private Methods

        private async Task<MpLoaderNotificationViewModel> CreateLoaderViewModel(MpIProgressLoader loader) {
            var lvm = new MpLoaderNotificationViewModel(this);
            await lvm.InitializeAsync(loader);
            return lvm;
        }

        private void MpStandardBalloonViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            
        }

        #endregion

        #region Commands

        public MpIAsyncCommand ResetAllNotificationsCommand => new MpAsyncCommand(
            async () => {
                MpPreferences.DoNotShowAgainNotificationIdCsvStr = string.Empty;

            }, () => MpBootstrapperViewModelBase.IsLoaded);

        public MpIAsyncCommand<int> DoNotShowAgainCommand => new MpAsyncCommand<int>(
            async (notificationId) => {
                if(DoNotShowNotificationIds.Contains(notificationId)) {
                    return;
                }
                RemoveNotificationCommand.Execute(Notifications.FirstOrDefault(x=>x.NotificationId == notificationId));

                while(!MpBootstrapperViewModelBase.IsLoaded) {
                    //wait for dependencies to load
                    await Task.Delay(100);
                }

                DoNotShowNotificationIds.Add(notificationId);

                MpPreferences.DoNotShowAgainNotificationIdCsvStr = string.Join(",", DoNotShowNotificationIds);
            });

        //public ICommand ShiftToNextNotificationCommand => new MpCommand(
        //     () => {                
        //         if (CurrentNotificationViewModel == null || NotificationQueue.Count <= 1) {
        //            if(NotificationQueue.Count > 0) {
        //                 HideBalloon(NotificationQueue[0]);
        //             } else {
        //                 HideBalloon(null);
        //             }
        //         }

        //         if (NotificationQueue.Count >= 1) {
        //             NotificationQueue.RemoveAt(0);
        //         }
        //         OnPropertyChanged(nameof(CurrentNotificationViewModel));
        //     });

        public ICommand RemoveNotificationCommand => new MpCommand<object>(
            (arg) => {
                var nvm = arg as MpNotificationViewModelBase;
                if(nvm == null) {
                    return;
                }
                HideBalloon(nvm);
                //OnPropertyChanged(nameof(CurrentNotificationViewModel));
            });
        #endregion
    }
}