using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;
namespace MpWpfApp {
    public class MpSoundNotificationViewModel : MpViewModelBase {
        #region Properties
        public MpSoundNotificationType NotificationType { get; set; } = MpSoundNotificationType.None;

        public bool IsVisual { get; set; }

        public bool IsAudible { get; set; }

        private object _property = null;
        #endregion

        #region Public Methods
        public MpSoundNotificationViewModel(MpSoundNotificationType type, object notificationProperty) : base(null) {
            //PropertyChanged += (s, e) => {
                //switch (e.PropertyName) {
                    //case nameof(NotificationType):
                    //    string ntstr = Enum.GetName(typeof(MpNotificationType), type);
                    //    IsAudible = ntstr.ToLower().Contains("Sound");
                    //    IsVisual = ntstr.ToLower().Contains("Show");
                    //    break;
                //}
            //};
            NotificationType = type;
            _property = notificationProperty;
        }
        #endregion

        #region Commands
        private RelayCommand _performNotificationCommand = null;
        public ICommand PerformNotificationCommand {
            get {
                if(_performNotificationCommand == null) {
                    _performNotificationCommand = new RelayCommand(PerformNotification, CanPerformNotification);
                }
                return _performNotificationCommand;
            }
        }
        private bool CanPerformNotification() {
            return (bool)_property;
        }
        private void PerformNotification() {
            MpConsole.WriteLine("Executed " + Enum.GetName(typeof(MpSoundNotificationType), NotificationType));
        }
        #endregion
    }
    public enum MpSoundNotificationType {
        None = 0,
        NotificationDoPasteSound,
        NotificationDoCopySound,
        NotificationShowCopyToast,
        NotificationShowAppendBufferToast,
        NotificationShowModeChangeToast,
        NotificationDoModeChangeSound
    }
}
