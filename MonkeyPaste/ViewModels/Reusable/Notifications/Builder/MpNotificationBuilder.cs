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
            object body = null, 
            int maxShowTimeMs = MpNotificationFormat.MAX_MESSAGE_DISPLAY_MS,
            MpNotificationType msgType = MpNotificationType.Message,        
            object iconSourceObj = null,
            object anchor = null) {
            await ShowNotificationAsync(
                notificationType: msgType,
                title: title,
                body: body,
                anchor: anchor,
                maxShowTimeMs: maxShowTimeMs,
                iconSourceObj: iconSourceObj);
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
            object body = null,
            int maxShowTimeMs = -1,
            Action<object> retryAction = null,
            object retryActionObj = null,
            object iconSourceObj = null,
            object anchor = null,
            ICommand fixCommand = null,
            object fixCommandArgs = null,
            MpIProgressLoader loader = null) {                                   
            if(body == null) {
                body = string.Empty;
            }
            if (string.IsNullOrEmpty(title)) {
                title = notificationType.EnumToLabel();
            }

            MpNotificationDialogResultType result = MpNotificationDialogResultType.None;
            if (notificationType == MpNotificationType.AppendChanged) {
                result = await MpAppendNotificationViewModel.Instance.ShowNotificationAsync();
                return result;
            }

            MpNotificationFormat nf = new MpNotificationFormat() {
                Title = title,
                Body = body,
                AnchorTarget = anchor,
                MaxShowTimeMs = maxShowTimeMs,
                NotificationType = notificationType,
                IconSourceObj = iconSourceObj,
                FixCommand = fixCommand,
                FixCommandArgs = fixCommandArgs,
                RetryAction = retryAction,
                RetryActionObj = retryActionObj,
                OtherArgs = loader
            };


            MpConsole.WriteLine($"Notification balloon set to:", true);
            MpConsole.WriteLine($"\tmsg: '{body}'");
            MpConsole.WriteLine($"\ttype: '{notificationType.ToString()}'");

            result = await ShowNotificationAsync(nf);
            return result;            
        }

        public static async Task<MpNotificationDialogResultType> ShowNotificationAsync(MpINotificationFormat inf) {
            var nf = inf as MpNotificationFormat;
            if(nf == null && inf is MpPluginUserNotificationFormat pnf) {
                // convert plugin notification to core nf
                nf = new MpNotificationFormat(pnf);
            }


            var nvm = await CreateNotifcationViewModelAsync(nf);

            MpNotificationDialogResultType result = await nvm.ShowNotificationAsync();
            return result;
        }


        public static async Task<MpNotificationViewModelBase> CreateNotifcationViewModelAsync(MpNotificationFormat nf) {
            MpNotificationLayoutType layoutType = MpNotificationViewModelBase.GetLayoutTypeFromNotificationType(nf.NotificationType);
            MpNotificationViewModelBase nvmb = null;
            switch (layoutType) {
                case MpNotificationLayoutType.Loader:
                    nvmb = new MpLoaderNotificationViewModel();
                    break;
                case MpNotificationLayoutType.Warning:
                case MpNotificationLayoutType.Error:
                case MpNotificationLayoutType.Message:
                    nvmb = new MpMessageNotificationViewModel();
                    break;
                case MpNotificationLayoutType.UserAction:
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

        #region Private Methods



        #endregion
    }
}