using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace MonkeyPaste.Avalonia {
    public class MpAvSoundPlayerCollectionViewModel : MpViewModelBase {
        #region Singleton
        private static MpAvSoundPlayerCollectionViewModel _instance;
        public static MpAvSoundPlayerCollectionViewModel Instance => _instance ?? (_instance = new MpAvSoundPlayerCollectionViewModel());
        #endregion

        #region Properties

        #region View Models
        public ObservableCollection<MpAvSoundNotificationViewModel> SoundNotificationViewModels { get; set; } = new ObservableCollection<MpAvSoundNotificationViewModel>();
        #endregion

        #endregion

        #region Public Methods
        public void Init() {
            //empty to trigger singleton constructor
        }
        #endregion

        #region Private Methods
        private MpAvSoundPlayerCollectionViewModel() : base(null) {
            SoundNotificationViewModels.Add(new MpAvSoundNotificationViewModel(MpSoundNotificationType.NotificationDoCopySound, MpPrefViewModel.Instance.NotificationDoCopySound));
            SoundNotificationViewModels[0].PerformNotificationCommand.Execute(null);

            MpPrefViewModel.Instance.NotificationDoCopySound = false;

            SoundNotificationViewModels[0].PerformNotificationCommand.Execute(null);
        }
        #endregion
    }
}
