using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
namespace MpWpfApp {
    public class MpSoundPlayerCollectionViewModel : MpViewModelBase {
        #region Singleton
        private static readonly Lazy<MpSoundPlayerCollectionViewModel> _Lazy = new Lazy<MpSoundPlayerCollectionViewModel>(() => new MpSoundPlayerCollectionViewModel());
        public static MpSoundPlayerCollectionViewModel Instance { get { return _Lazy.Value; } }
        #endregion

        #region Properties

        #region View Models
        private ObservableCollection<MpSoundNotificationViewModel> _soundNotificationViewModels = new ObservableCollection<MpSoundNotificationViewModel>();
        public ObservableCollection<MpSoundNotificationViewModel> SoundNotificationViewModels {
            get {
                return _soundNotificationViewModels;
            }
            set {
                if(_soundNotificationViewModels != value) {
                    _soundNotificationViewModels = value;
                    OnPropertyChanged(nameof(SoundNotificationViewModels));
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
        private MpSoundPlayerCollectionViewModel() : base(null) {
            SoundNotificationViewModels.Add(new MpSoundNotificationViewModel(MpSoundNotificationType.NotificationDoCopySound, MpPrefViewModel.Instance.NotificationDoCopySound));
            SoundNotificationViewModels[0].PerformNotificationCommand.Execute(null);

            MpPrefViewModel.Instance.NotificationDoCopySound = false;

            SoundNotificationViewModels[0].PerformNotificationCommand.Execute(null);
        }
        #endregion
    }
}
