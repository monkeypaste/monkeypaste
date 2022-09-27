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
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common;
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

        public async Task InitAsync(List<int> doNotShowNotifications) {
            await Task.Delay(1);
            if(doNotShowNotifications == null) {
                doNotShowNotifications = MpPrefViewModel.Instance.DoNotShowAgainNotificationIdCsvStr
                    .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x)).ToList();
            }

            if(doNotShowNotifications != null) {
                DoNotShowNotificationIds = doNotShowNotifications;
            }
            //IsVisible = true;
        }

        public async Task RegisterWithWindowAsync(MpINotificationBalloonView nbv) {
            await Task.Delay(1);
            _nbv = nbv;
        }

        public async Task ShowMessageAsync(
            string title = "", 
            string msg = "", 
            double maxShowTimeMs = 3000,
            MpNotificationDialogType msgType = MpNotificationDialogType.Message,            
            string iconResourceKey = null,
            string iconBase64Str = null) {
            await ShowNotificationAsync(
                dialogType: msgType,
                title: title,
                msg: msg,
                maxShowTimeMs: maxShowTimeMs,
                iconResourceKey: iconResourceKey,
                iconBase64Str: iconBase64Str);
        }
        public async Task<MpDialogResultType> ShowNotificationAsync(
            MpNotificationDialogType dialogType = MpNotificationDialogType.None,
            string title = "",
            string msg = "",
            double maxShowTimeMs = -1,
            Action<object> retryAction = null,
            object retryActionObj = null,
            string iconResourceKey = null,
            string iconBase64Str = null,
            ICommand fixCommand = null,
            object fixCommandArgs = null) {

            var exceptionType = MpNotificationViewModelBase.GetExceptionFromNotificationType(dialogType);

            if (DoNotShowNotificationIds.Contains((int)dialogType)) {
                MpConsole.WriteTraceLine($"Notification: {dialogType.ToString()} marked as hidden");
                return MpDialogResultType.Ignore;
            }

            if (string.IsNullOrEmpty(title)) {
                //if (exceptionType == MpNotificationExceptionSeverityType.Warning ||
                //    exceptionType == MpNotificationExceptionSeverityType.WarningWithOption) {
                //    title = "Warning: ";
                //} else {
                //    title = "Error: ";
                //}

                title = dialogType.EnumToLabel();
            }

            MpNotificationViewModelBase nvm;
            switch(exceptionType) {
                case MpNotificationExceptionSeverityType.None:
                    nvm = new MpMessageNotificationViewModel(this) {
                        DialogType = dialogType,
                        Title = title,
                        Body = msg,
                        IconImageBase64 = iconBase64Str,
                        IconResourceKey = iconResourceKey
                    };
                    break;
                default:
                    nvm = new MpUserActionNotificationViewModel(this) {
                        DialogType = dialogType,
                        Title = title,
                        Body = msg,
                        FixCommand = fixCommand,
                        FixCommandArgs = fixCommandArgs,
                        IconImageBase64 = iconBase64Str,
                        IconResourceKey = iconResourceKey
                    };
                    break;
            }


            Notifications.Add(nvm);

            ShowBalloon(nvm);

            MpConsole.WriteLines(
                $"Notification balloon set to:",
                $"msg: '{msg}'",
                $"type: '{dialogType.ToString()}'",
                $"severity: '{exceptionType.ToString()}'");

            MpDialogResultType result = MpDialogResultType.None;
            if(nvm is MpMessageNotificationViewModel) {
                if (maxShowTimeMs > 0) {
                    DateTime startTime = DateTime.Now;
                    while (DateTime.Now - startTime <= TimeSpan.FromMilliseconds(maxShowTimeMs)) {
                        await Task.Delay(100);

                        while (nvm.IsHovering) {
                            await Task.Delay(100);
                        }
                    }
                }
            } else if(nvm is MpUserActionNotificationViewModel uanvm) {
                while (uanvm.DialogResult == MpDialogResultType.None) {
                    await Task.Delay(100);
                }
                if (uanvm.DialogResult == MpDialogResultType.Retry) {
                    retryAction?.Invoke(retryActionObj);
                }
            }

            RemoveNotificationCommand.Execute(nvm);
            return result;
        }

        public async Task BeginLoaderAsync(MpIProgressLoader loader) {
            if (DoNotShowNotificationIds.Contains((int)loader.DialogType)) {
                MpConsole.WriteTraceLine($"Notification: {loader.DialogType.ToString()} marked as hidden");
                return;
            }

            var lvm = await CreateLoaderViewModelAsync(loader); 
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

        #endregion

        #region Private Methods

        private void ShowBalloon(MpNotificationViewModelBase nvmb) {
            _nbv.ShowWindow(nvmb);
            //IsVisible = true;
        }

        private void HideBalloon(MpNotificationViewModelBase nvmb) {
            _nbv.HideWindow(nvmb);
            Notifications.Remove(nvmb);
            //IsVisible = false;
        }
        private async Task<MpLoaderNotificationViewModel> CreateLoaderViewModelAsync(MpIProgressLoader loader) {
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
                MpPrefViewModel.Instance.DoNotShowAgainNotificationIdCsvStr = string.Empty;

            }, () => MpBootstrapperViewModelBase.IsCoreLoaded);

        public MpIAsyncCommand<int> DoNotShowAgainCommand => new MpAsyncCommand<int>(
            async (notificationId) => {
                if(DoNotShowNotificationIds.Contains(notificationId)) {
                    return;
                }
                RemoveNotificationCommand.Execute(Notifications.FirstOrDefault(x=>x.NotificationId == notificationId));

                while(!MpBootstrapperViewModelBase.IsCoreLoaded) {
                    //wait for dependencies to load
                    await Task.Delay(100);
                }

                DoNotShowNotificationIds.Add(notificationId);

                MpPrefViewModel.Instance.DoNotShowAgainNotificationIdCsvStr = string.Join(",", DoNotShowNotificationIds);
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