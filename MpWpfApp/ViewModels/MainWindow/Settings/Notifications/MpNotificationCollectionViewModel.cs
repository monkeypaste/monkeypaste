using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
namespace MpWpfApp {
    public class MpNotificationCollectionViewModel : MpViewModelBase {
        #region Singleton
        private static readonly Lazy<MpNotificationCollectionViewModel> _Lazy = new Lazy<MpNotificationCollectionViewModel>(() => new MpNotificationCollectionViewModel());
        public static MpNotificationCollectionViewModel Instance { get { return _Lazy.Value; } }
        #endregion

        #region Properties

        #region View Models
        private ObservableCollection<MpNotificationViewModel> _notificationViewModels = new ObservableCollection<MpNotificationViewModel>();
        public ObservableCollection<MpNotificationViewModel> NotificationViewModels {
            get {
                return _notificationViewModels;
            }
            set {
                if(_notificationViewModels != value) {
                    _notificationViewModels = value;
                    OnPropertyChanged(nameof(NotificationViewModels));
                }
            }
        }
        #endregion

        #endregion
        #region Public Methods
        public void Init() {
            //empty to trigger singleton constructor
        }
        #endregion

        #region Private Methods
        private MpNotificationCollectionViewModel() : base(null) {
            NotificationViewModels.Add(new MpNotificationViewModel(MpNotificationType.NotificationDoCopySound, MpPreferences.NotificationDoCopySound));
            NotificationViewModels[0].PerformNotificationCommand.Execute(null);

            MpPreferences.NotificationDoCopySound = false;

            NotificationViewModels[0].PerformNotificationCommand.Execute(null);
        }
        #endregion
    }
}
