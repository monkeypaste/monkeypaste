using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {

    public class MpAvNotificationBuilder : MpINotificationBuilder {
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
        public async Task ShowMessageAsync(
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

        public async Task<MpNotificationDialogResultType> ShowLoaderNotificationAsync(MpIProgressLoaderViewModel loader) {
            var result = await ShowNotificationAsync(
                title: loader.Title,
                notificationType: MpNotificationType.Loader,
                loader: loader);
            return result;
        }

        public async Task<MpNotificationDialogResultType> ShowNotificationAsync(
            MpNotificationType notificationType = MpNotificationType.None,
            string title = "",
            object body = null,
            int maxShowTimeMs = -1,
            Func<object, object> retryAction = null,
            object retryActionObj = null,
            object iconSourceObj = null,
            object anchor = null,
            ICommand fixCommand = null,
            object fixCommandArgs = null,
            MpIProgressLoaderViewModel loader = null,
            object owner = null,
            object otherArgs = null) {
            if (body == null) {
                body = string.Empty;
            }
            if (string.IsNullOrEmpty(title)) {
                title = notificationType.EnumToUiString();
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
                OtherArgs = loader == null ? otherArgs : loader,
                Owner = owner
            };

            MpConsole.WriteLine($"Notification balloon set to:", true);
            MpConsole.WriteLine($"\tmsg: '{body}'");
            MpConsole.WriteLine($"\ttype: '{notificationType.ToString()}'");

            MpNotificationDialogResultType result = await ShowNotificationAsync(nf);
            return result;
        }

        public async Task<string> ShowInputResultNotificationAsync(
            string title,
            string body,
            string currentInput = null,
            string placeholderText = null,
            object anchor = null,
            object iconResourceObj = null,
            object owner = null,
            char passwordChar = default,
            MpNotificationType ntfType = MpNotificationType.ModalTextBoxOkCancelMessageBox) {
            MpNotificationFormat nf = new MpNotificationFormat() {
                Title = title,
                Body = body,
                OtherArgs = currentInput,
                Detail = placeholderText,
                AnchorTarget = anchor,
                NotificationType = ntfType,
                PasswordChar = passwordChar,
                IconSourceObj = iconResourceObj,
                Owner = owner
            };
            var nvm = await CreateNotifcationViewModelAsync(nf);
            if (nvm is MpAvUserActionNotificationViewModel uanvm) {
                string result = await uanvm.ShowInputResultNotificationAsync();
                return result;
            }
            return null;
        }
        public async Task<MpNotificationDialogResultType> ShowNotificationAsync(MpINotificationFormat inf) {
            var nf = inf as MpNotificationFormat;
            if (nf == null && inf is MpPluginUserNotificationFormat pnf) {
                // convert plugin notification to core nf
                nf = new MpNotificationFormat(pnf);
            }
            var nvm = await CreateNotifcationViewModelAsync(nf);

            MpNotificationDialogResultType result = await nvm.ShowNotificationAsync();
            return result;
        }


        #endregion

        #region Private Methods

        private async Task<MpAvNotificationViewModelBase> CreateNotifcationViewModelAsync(MpNotificationFormat nf) {
            MpNotificationLayoutType layoutType = MpAvNotificationViewModelBase.GetLayoutTypeFromNotificationType(nf.NotificationType);
            MpAvNotificationViewModelBase nvmb = null;
            switch (layoutType) {
                case MpNotificationLayoutType.Loader:
                    nvmb = new MpAvLoaderNotificationViewModel();
                    break;
                case MpNotificationLayoutType.Welcome:
                    nvmb = MpAvWelcomeNotificationViewModel.Instance;
                    break;
                case MpNotificationLayoutType.Warning:
                case MpNotificationLayoutType.Error:
                case MpNotificationLayoutType.Message:
                    nvmb = new MpAvMessageNotificationViewModel();
                    break;
                case MpNotificationLayoutType.UserAction:
                case MpNotificationLayoutType.ErrorWithOption:
                case MpNotificationLayoutType.ErrorAndShutdown:
                    nvmb = new MpAvUserActionNotificationViewModel();
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