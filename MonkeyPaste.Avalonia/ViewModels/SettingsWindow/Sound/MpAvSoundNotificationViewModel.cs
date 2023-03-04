
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
namespace MonkeyPaste.Avalonia {
    public class MpAvSoundNotificationViewModel : MpViewModelBase {
        #region Properties
        public MpSoundNotificationType NotificationType { get; set; } = MpSoundNotificationType.None;

        public bool IsVisual { get; set; }

        public bool IsAudible { get; set; }

        private object _property = null;
        #endregion

        #region Public Methods
        public MpAvSoundNotificationViewModel(MpSoundNotificationType type, object notificationProperty) : base(null) {
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
        private MpCommand _performNotificationCommand = null;
        public ICommand PerformNotificationCommand {
            get {
                if (_performNotificationCommand == null) {
                    _performNotificationCommand = new MpCommand(PerformNotification, CanPerformNotification);
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
