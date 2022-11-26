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
            int maxShowTimeMs = MpNotificationFormat.MAX_MESSAGE_DISPLAY_MS,
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
            var result = await ShowNotificationAsync(
                title: loader.Title,
                notificationType: MpNotificationType.Loader,
                loader: loader);
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

            if (string.IsNullOrEmpty(title)) {
                title = notificationType.EnumToLabel();
            }

            MpNotificationFormat nf = new MpNotificationFormat() {
                Title = title,
                Body = msg,
                MaxShowTimeMs = maxShowTimeMs,
                NotificationType = notificationType,
                IconSourceStr = iconSourceStr,
                FixCommand = fixCommand,
                FixCommandArgs = fixCommandArgs,
                RetryAction = retryAction,
                RetryActionObj = retryActionObj,
                OtherArgs = loader
            };


            MpConsole.WriteLine($"Notification balloon set to:", true);
            MpConsole.WriteLine($"\tmsg: '{msg}'");
            MpConsole.WriteLine($"\ttype: '{notificationType.ToString()}'");

            MpNotificationDialogResultType result = await ShowNotificationAsync(nf);
            return result;            
        }

        public static async Task<MpNotificationDialogResultType> ShowNotificationAsync(MpINotificationFormat inf) {
            var nf = inf as MpNotificationFormat;
            if(nf == null && inf is MpPluginUserNotificationFormat pnf) {
                // convert plugin notification to core nf
                nf = new MpNotificationFormat(pnf);
            }

            bool isDoNotShowType = MpPrefViewModel.Instance.DoNotShowAgainNotificationIdCsvStr
                    .Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x)).Any(x => x == (int)nf.NotificationType);

            if (isDoNotShowType) {
                MpConsole.WriteTraceLine($"Notification: {nf.NotificationType.ToString()} marked as hidden");
                return MpNotificationDialogResultType.DoNotShow;
            }

            var nvm = await CreateNotifcationViewModelAsync(nf);

            MpNotificationDialogResultType result = await nvm.ShowNotificationAsync();
            return result;
        }
        #endregion

        #region Private Methods

        private static async Task<MpNotificationViewModelBase> CreateNotifcationViewModelAsync(MpNotificationFormat nf) {            
            MpNotificationLayoutType layoutType = MpNotificationViewModelBase.GetLayoutTypeFromNotificationType(nf.NotificationType);
            MpNotificationViewModelBase nvmb = null;
            switch (layoutType) {
                case MpNotificationLayoutType.Loader:
                    nvmb = new MpLoaderNotificationViewModel();
                    break;
                case MpNotificationLayoutType.Default:
                case MpNotificationLayoutType.Warning:
                case MpNotificationLayoutType.Error:
                case MpNotificationLayoutType.Message:
                    nvmb = new MpMessageNotificationViewModel();
                    break;
                case MpNotificationLayoutType.WarningWithOption:
                case MpNotificationLayoutType.ErrorWithOption:
                case MpNotificationLayoutType.ErrorAndShutdown:
                    nvmb = new MpUserActionNotificationViewModel();
                    break;
                default:
                    throw new Exception("Unhandled notification type: " + nf.NotificationType);
            }
            await nvmb.InitializeAsync(nf);
            return nvmb;
        }


        #endregion
    }
}