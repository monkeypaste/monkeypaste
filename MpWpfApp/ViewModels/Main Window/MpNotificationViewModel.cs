using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpNotificationViewModel : MpViewModelBase {
        #region Properties
        private MpNotificationType _notificationType = MpNotificationType.None;
        public MpNotificationType NotificationType {
            get {
                return _notificationType;
            }
            set {
                if(_notificationType != value) {
                    _notificationType = value;
                    OnPropertyChanged(nameof(NotificationType));
                }
            }
        }

        private bool _isVisual = false;
        public bool IsVisual {
            get {
                return _isVisual;
            }
            set {
                if (_isVisual != value) {
                    _isVisual = value;
                    OnPropertyChanged(nameof(IsVisual));
                }
            }
        }

        private bool _isAudible = false;
        public bool IsAudible {
            get {
                return _isAudible;
            }
            set {
                if (_isAudible != value) {
                    _isAudible = value;
                    OnPropertyChanged(nameof(IsAudible));
                }
            }
        }

        private object _property = null;
        #endregion

        #region Public Methods
        public MpNotificationViewModel(MpNotificationType type, object notificationProperty) {
            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    //case nameof(NotificationType):
                    //    string ntstr = Enum.GetName(typeof(MpNotificationType), type);
                    //    IsAudible = ntstr.ToLower().Contains("Sound");
                    //    IsVisual = ntstr.ToLower().Contains("Show");
                    //    break;
                }
            };
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
            Console.WriteLine("EExecuted " + Enum.GetName(typeof(MpNotificationType), NotificationType));
        }
        #endregion
    }
    public enum MpNotificationType {
        None = 0,
        NotificationDoPasteSound,
        NotificationDoCopySound,
        NotificationShowCopyToast,
        NotificationShowAppendBufferToast,
        NotificationShowModeChangeToast,
        NotificationDoModeChangeSound
    }
}
