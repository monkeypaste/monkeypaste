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

        private Stack<MpNotificationViewModelBase> _notificationStack = new Stack<MpNotificationViewModelBase>();

        private MpINotificationBalloonView _nbv;

        #endregion

        #region Properties

        #region MpISingletonViewModel Implementation

        private static MpNotificationBalloonViewModel _instance;
        public static MpNotificationBalloonViewModel Instance => _instance ?? (_instance = new MpNotificationBalloonViewModel());

        #endregion

        #region View Models

        public MpNotificationViewModelBase CurrentNotificationViewModel {
            get => _notificationStack.PeekOrDefault();
            set {
                if(CurrentNotificationViewModel != value) {
                    _notificationStack.Push(value);
                    OnPropertyChanged(nameof(CurrentNotificationViewModel));
                }
            }
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
            CurrentNotificationViewModel = await CreateLoaderViewModel();
            //_nbv.SetDataContext(this);
        }

        public async Task<MpDialogResultType> ShowUserActions(
            MpNotificationDialogType notificationType = MpNotificationDialogType.None,
            MpNotificationExceptionSeverityType exceptionType = MpNotificationExceptionSeverityType.None,
            string title = "",
            string msg = "",
            double maxShowTimeMs = -1) {

            if(string.IsNullOrEmpty(title)) {
                if (exceptionType == MpNotificationExceptionSeverityType.Warning ||
                    exceptionType == MpNotificationExceptionSeverityType.WarningWithOption) {
                    title = "Warning";
                } else {
                    title = "Error";
                }
            }

            var n = new MpNotification() {
                NotifierType = MpNotifierType.Dialog,
                DialogType = notificationType,
                SeverityType = exceptionType,
                Title = title,
                Body = msg,
                MaxShowMs = maxShowTimeMs
            };
            CurrentNotificationViewModel = await CreateNotificationViewModel(n);
            var unvm = CurrentNotificationViewModel as MpUserActionNotificationViewModel;

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

            if(!wasVisible) {
                HideBalloon();
            }
            _notificationStack.Pop();
            OnPropertyChanged(nameof(CurrentNotificationViewModel));

            return unvm.DialogResult;
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