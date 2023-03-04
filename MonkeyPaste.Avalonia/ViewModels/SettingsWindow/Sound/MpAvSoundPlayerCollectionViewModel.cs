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
        private static readonly Lazy<MpAvSoundPlayerCollectionViewModel> _Lazy = new Lazy<MpAvSoundPlayerCollectionViewModel>(() => new MpAvSoundPlayerCollectionViewModel());
        public static MpAvSoundPlayerCollectionViewModel Instance { get { return _Lazy.Value; } }
        #endregion

        #region Properties

        #region View Models
        private ObservableCollection<MpAvSoundNotificationViewModel> _soundNotificationViewModels = new ObservableCollection<MpAvSoundNotificationViewModel>();
        public ObservableCollection<MpAvSoundNotificationViewModel> SoundNotificationViewModels {
            get {
                return _soundNotificationViewModels;
            }
            set {
                if (_soundNotificationViewModels != value) {
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
        private MpAvSoundPlayerCollectionViewModel() : base(null) {
            SoundNotificationViewModels.Add(new MpAvSoundNotificationViewModel(MpSoundNotificationType.NotificationDoCopySound, MpPrefViewModel.Instance.NotificationDoCopySound));
            SoundNotificationViewModels[0].PerformNotificationCommand.Execute(null);

            MpPrefViewModel.Instance.NotificationDoCopySound = false;

            SoundNotificationViewModels[0].PerformNotificationCommand.Execute(null);
        }
        #endregion
    }
}
