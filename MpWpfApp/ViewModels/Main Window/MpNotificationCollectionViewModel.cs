using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpNotificationCollectionViewModel : MpObservableCollectionViewModel<MpNotificationViewModel> {
        private static readonly Lazy<MpNotificationCollectionViewModel> _Lazy = new Lazy<MpNotificationCollectionViewModel>(() => new MpNotificationCollectionViewModel());
        public static MpNotificationCollectionViewModel Instance { get { return _Lazy.Value; } }

        #region Public Methods
        public void Init() {
            //empty to trigger singleton constructor
        }
        #endregion

        #region Private Methods
        private MpNotificationCollectionViewModel() {
            this.Add(new MpNotificationViewModel(MpNotificationType.NotificationDoCopySound, Properties.Settings.Default.NotificationDoCopySound));
            this[0].PerformNotificationCommand.Execute(null);

            Properties.Settings.Default.NotificationDoCopySound = false;

            this[0].PerformNotificationCommand.Execute(null);
        }
        #endregion
    }
}
