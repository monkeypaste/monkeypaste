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

            MpConsole.WriteLines(
                $"Notification balloon set to:",
                $"msg: '{msg}'",
                $"type: '{notificationType.ToString()}'",
                $"severity: '{layoutType.ToString()}'");

            var nvm = await CreateNotifcationViewModelAsync(nf, nf_args);

            MpNotificationDialogResultType result = await nvm.ShowNotificationAsync();
            return result;
        }

        #endregion

        #region Private Methods

        private static async Task<MpNotificationViewModelBase> CreateNotifcationViewModelAsync(MpNotificationFormat nf, object nfArgs) {
            MpNotificationLayoutType layoutType = MpNotificationViewModelBase.GetLayoutTypeFromNotificationType(nf.NotificationType);
            switch (layoutType) {
                case MpNotificationLayoutType.Loader:
                    if(nfArgs is MpIProgressLoader loader) {
                        var lvm = new MpLoaderNotificationViewModel();
                        await lvm.InitializeAsync(nf,loader);
                        return lvm;
                    }
                    break;
                case MpNotificationLayoutType.Default:
                case MpNotificationLayoutType.Warning:
                case MpNotificationLayoutType.Error:
                case MpNotificationLayoutType.Message:
                    var mnvm = new MpMessageNotificationViewModel();
                    await mnvm.InitializeAsync(nf, nfArgs);
                    return mnvm;
                case MpNotificationLayoutType.WarningWithOption:
                case MpNotificationLayoutType.ErrorWithOption:
                case MpNotificationLayoutType.ErrorAndShutdown:
                    var uanvm = new MpUserActionNotificationViewModel();
                    await uanvm.InitializeAsync(nf, nfArgs);
                    return uanvm;
            }
            throw new Exception("Unhandled notification type: " + nf.NotificationType);
        }


        #endregion
    }
}