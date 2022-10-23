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
    public static class MpNotificationBuilder {
        #region Static Variables
        #endregion

        #region Private Variables        

        #endregion

        #region Properties

        #region MpISingletonViewModel Implementation


        #endregion

        #region View Models

        //public ObservableCollection<MpNotificationViewModelBase> Notifications { get; private set; } = new ObservableCollection<MpNotificationViewModelBase>();

        //public MpNotificationViewModelBase CurrentNotificationViewModel => NotificationQueue.FirstOrDefault();

        #endregion

        #region Model


        #endregion

        #endregion

        #region Constructors


        #endregion

        #region Public Methods

        //public static async Task InitAsync(List<int> doNotShowNotifications) {
        //    await Task.Delay(1);
        //    if(doNotShowNotifications == null) {
        //        doNotShowNotifications = MpPrefViewModel.Instance.DoNotShowAgainNotificationIdCsvStr
        //            .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
        //            .Select(x => Convert.ToInt32(x)).ToList();
        //    }

        //    if(doNotShowNotifications != null) {
        //        DoNotShowNotificationIds = doNotShowNotifications;
        //    }
        //    //IsVisible = true;
        //}

        //public async Task RegisterWithWindowAsync(MpINotificationBalloonView nbv) {
        //    await Task.Delay(1);
        //    //_nbv = nbv;
        //}

        public static async Task ShowMessageAsync(
            string title = "", 
            string msg = "", 
            int maxShowTimeMs = 3000,
            MpNotificationType msgType = MpNotificationType.Message,        
            string iconSourceStr = null) {
            await ShowNotificationAsync(
                notificationType: msgType,
                title: title,
                msg: msg,
                maxShowTimeMs: maxShowTimeMs,
                iconSourceStr: iconSourceStr);
        }

        public static async Task<MpNotificationDialogResultType> ShowLoaderNotificationAsync(MpIProgressLoader loader) {
            var result = await ShowNotificationAsync(loader: loader);
            return result;
        }
        public static async Task<MpNotificationDialogResultType> ShowNotificationAsync(
            MpNotificationType notificationType = MpNotificationType.None,
            string title = "",
            string msg = "",
            int maxShowTimeMs = -1,
            Action<object> retryAction = null,
            object retryActionObj = null,
            string iconSourceStr = null,
            ICommand fixCommand = null,
            object fixCommandArgs = null,
            MpIProgressLoader loader = null) {

            bool isDoNotShowType = MpPrefViewModel.Instance.DoNotShowAgainNotificationIdCsvStr
                    .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x)).Any(x => x == (int)notificationType);

            if (isDoNotShowType) {
                MpConsole.WriteTraceLine($"Notification: {notificationType.ToString()} marked as hidden");
                return MpNotificationDialogResultType.DoNotShow;
            }

            var layoutType = MpNotificationViewModelBase.GetLayoutTypeFromNotificationType(notificationType);
            

            if (string.IsNullOrEmpty(title)) {
                title = notificationType.EnumToLabel();
            }
            object nf_args = null;

            if (loader == null) {
                nf_args = new[] { new[] { retryAction, retryActionObj }, new[] { fixCommand, fixCommandArgs } };
            } else {
                title = loader.Title;
                notificationType = MpNotificationType.Loader;
                nf_args = loader;
            }

            MpNotificationFormat nf = new MpNotificationFormat() {
                Title = title,
                Body = msg,
                MaxShowTimeMs = maxShowTimeMs,
                NotificationType = notificationType,
                IconSourceStr = iconSourceStr
            };

            
            //MpNotificationViewModelBase nvm;
            //switch(layoutType) {
            //    case MpNotificationLayoutType.None:
            //        nvm = new MpMessageNotificationViewModel(this) {
            //            NotificationType = notificationType,
            //            Title = title,
            //            Body = msg,
            //            IconResourceKey = iconSourceStr
            //        };
            //        break;
            //    default:
            //        nvm = new MpUserActionNotificationViewModel(this) {
            //            NotificationType = notificationType,
            //            Title = title,
            //            Body = msg,
            //            FixCommand = fixCommand,
            //            FixCommandArgs = fixCommandArgs,
            //            IconResourceKey = iconSourceStr
            //        };
            //        break;
            //}

            MpConsole.WriteLines(
                $"Notification balloon set to:",
                $"msg: '{msg}'",
                $"type: '{notificationType.ToString()}'",
                $"severity: '{layoutType.ToString()}'");

            var nvm = await CreateNotifcationViewModelAsync(nf, nf_args);

            MpNotificationDialogResultType result = await nvm.ShowNotificationAsync();
            if(result != MpNotificationDialogResultType.Loading) {
                nvm.HideNotification();
            }
            //Notifications.Add(nvm);

            //ShowBalloon(nvm);


            //MpNotificationDialogResultType result = MpNotificationDialogResultType.None;
            //if(nvm is MpMessageNotificationViewModel) {
            //    if (maxShowTimeMs > 0) {
            //        DateTime startTime = DateTime.Now;
            //        while (DateTime.Now - startTime <= TimeSpan.FromMilliseconds(maxShowTimeMs)) {
            //            await Task.Delay(100);

            //            while (nvm.IsHovering) {
            //                await Task.Delay(100);
            //            }
            //        }
            //    }
            //} else if(nvm is MpUserActionNotificationViewModel uanvm) {
            //    while (uanvm.DialogResult == MpNotificationDialogResultType.None) {
            //        await Task.Delay(100);
            //    }
            //    if (uanvm.DialogResult == MpNotificationDialogResultType.Retry) {
            //        retryAction?.Invoke(retryActionObj);
            //    }
            //}

            //RemoveNotificationCommand.Execute(nvm);
            return result;
        }

        //public async Task BeginLoaderAsync(MpIProgressLoader loader) {
        //    if (DoNotShowNotificationIds.Contains((int)loader.DialogType)) {
        //        MpConsole.WriteTraceLine($"Notification: {loader.DialogType.ToString()} marked as hidden");
        //        return;
        //    }
        //    var lnf = new MpNotificationFormat() { NotificationType = MpNotificationType.Loader };

        //    var lvm = await CreateNotifcationViewModelAsync(lnf,loader); 
        //    Notifications.Add(lvm);

        //    //OnPropertyChanged(nameof(CurrentNotificationViewModel));

        //    ShowBalloon(lvm);

        //    // await Task.Delay(100);
        //    // ShowMessageAsync("Test title", "Test Message", 2000, MpNotificationType.Message).FireAndForgetSafeAsync(this);
        //}

        //public void FinishLoading() {
        //    var lvm = Notifications.FirstOrDefault(x => x is MpLoaderNotificationViewModel);
        //    if(lvm != null) {
        //        RemoveNotificationCommand.Execute(lvm);
        //    }
        //}

        #endregion

        #region Private Methods

        private static async Task<MpNotificationViewModelBase> CreateNotifcationViewModelAsync(MpNotificationFormat nf, object nfArgs) {
            switch(nf.NotificationType) {
                case MpNotificationType.Loader:
                    if(nfArgs is MpIProgressLoader loader) {
                        var lvm = new MpLoaderNotificationViewModel();
                        await lvm.InitializeAsync(nf,loader);
                        return lvm;
                    }
                    break;
                case MpNotificationType.Message:
                    var mnvm = new MpMessageNotificationViewModel();
                    await mnvm.InitializeAsync(nf, nfArgs);
                    return mnvm;
                case MpNotificationType.None:
                    throw new Exception("Error uknown notification type");
                default:
                    var uanvm = new MpUserActionNotificationViewModel();
                    await uanvm.InitializeAsync(nf, nfArgs);
                    return uanvm;
            }
            return null;
        }


        #endregion

        #region Commands

        

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

        //public ICommand RemoveNotificationCommand => new MpCommand<object>(
        //    (arg) => {
        //        var nvm = arg as MpNotificationViewModelBase;
        //        if(nvm == null) {
        //            return;
        //        }
        //        HideBalloon(nvm);
        //        //OnPropertyChanged(nameof(CurrentNotificationViewModel));
        //    });
        #endregion
    }
}