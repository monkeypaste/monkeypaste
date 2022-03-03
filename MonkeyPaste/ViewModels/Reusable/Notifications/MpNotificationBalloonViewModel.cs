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

        private Queue<MpNotificationViewModelBase> _notificationQueue = new Queue<MpNotificationViewModelBase>();

        private MpINotificationBalloonView _nbv;

        #endregion

        #region Properties

        #region MpISingletonViewModel Implementation

        private static MpNotificationBalloonViewModel _instance;
        public static MpNotificationBalloonViewModel Instance => _instance ?? (_instance = new MpNotificationBalloonViewModel());

        #endregion

        #region View Models

        public MpNotificationViewModelBase CurrentNotificationViewModel {
            get => _notificationQueue.PeekOrDefault();
        }

        #endregion

        #region State

        public bool IsVisible { get; set; } = false;

        #endregion

        #region Appearance

        #endregion


        #endregion

        #region Constructors

        public MpNotificationBalloonViewModel() : base(null) {
            PropertyChanged += MpStandardBalloonViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public async Task Init() {
            await Task.Delay(1);
        }

        public async Task Attach(MpINotificationBalloonView nbv) {
            await Task.Delay(1);
            _nbv = nbv;
            var lvm = await CreateLoaderViewModel();
            _notificationQueue.Enqueue(lvm);
        }

        public async Task<MpDialogResultType> ShowUserAction(
            MpNotificationDialogType notificationType = MpNotificationDialogType.None,
            MpNotificationExceptionSeverityType exceptionType = MpNotificationExceptionSeverityType.None,
            string title = "",
            string msg = "",
            double maxShowTimeMs = -1) {

            if(string.IsNullOrEmpty(title)) {
                if (exceptionType == MpNotificationExceptionSeverityType.Warning ||
                    exceptionType == MpNotificationExceptionSeverityType.WarningWithOption) {
                    title = "Warning: ";
                } else {
                    title = "Error: ";
                }
            }
            title += notificationType.EnumToLabel();

            var n = new MpNotification() {
                NotifierType = MpNotifierType.Dialog,
                DialogType = notificationType,
                SeverityType = exceptionType,
                Title = title,
                Body = msg,
                MaxShowMs = maxShowTimeMs
            };
            var unvm = await CreateUserActionViewModel(n);
            _notificationQueue.Enqueue(unvm);

            bool wasVisible = IsVisible;
            if(!IsVisible) {
                ShowBalloon();
            }

            MpConsole.WriteLines(
                $"Notification balloon set to:",
                $"msg: '{msg}'",
                $"type: '{notificationType.ToString()}'",
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

            _notificationQueue.Dequeue();
            OnPropertyChanged(nameof(CurrentNotificationViewModel));

            if (!wasVisible || CurrentNotificationViewModel == null) {
                HideBalloon();
            }

            return unvm.DialogResult;
        }
        public void BeginLoader() {
            _nbv.ShowBalloon();
            IsVisible = true;
        }

        public void FinishLoading() {
            _notificationQueue.Dequeue();
            OnPropertyChanged(nameof(CurrentNotificationViewModel));
            if(CurrentNotificationViewModel == null) {
                HideBalloon();
            }
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

        private async Task<MpLoaderNotificationViewModel> CreateLoaderViewModel() {
            var ln = new MpNotification() {
                NotifierType = MpNotifierType.Startup
            };
            var lvm = await CreateNotificationViewModel(ln);
            return lvm as MpLoaderNotificationViewModel;
        }

        private async Task<MpUserActionNotificationViewModel> CreateUserActionViewModel(MpNotification n) {
            var lvm = await CreateNotificationViewModel(n);
            return lvm as MpUserActionNotificationViewModel;
        }

        private async Task<MpNotificationViewModelBase> CreateNotificationViewModel(MpNotification n) {
            MpNotificationViewModelBase nvm = null;
            switch (n.NotifierType) {
                case MpNotifierType.Startup:
                    nvm = new MpLoaderNotificationViewModel(this);
                    break;
                case MpNotifierType.Dialog:
                    nvm = new MpUserActionNotificationViewModel(this);
                    break;

            }
            await nvm.InitializeAsync(n);
            return nvm;
        }

        private void MpStandardBalloonViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            
        }

        #endregion

    }
}