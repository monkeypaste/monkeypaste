
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvSoundPlayerGroupCollectionViewModel : MpViewModelBase, MpIAsyncSingletonViewModel<MpAvSoundPlayerGroupCollectionViewModel> {
        #region Properties

        #region View Models
        private ObservableCollection<MpAvSoundPlayerViewModel> _soundPlayerViewModels = new ObservableCollection<MpAvSoundPlayerViewModel>();
        public ObservableCollection<MpAvSoundPlayerViewModel> SoundPlayerViewModels {
            get {
                return _soundPlayerViewModels;
            }
            set {
                if (_soundPlayerViewModels != value) {
                    _soundPlayerViewModels = value;
                    OnPropertyChanged(nameof(SoundPlayerViewModels));
                }
            }
        }
        #endregion

        private MpSoundGroup _soundGroup = MpSoundGroup.None;
        public MpSoundGroup SoundGroup {
            get {
                return _soundGroup;
            }
            set {
                if (_soundGroup != value) {
                    _soundGroup = value;
                    OnPropertyChanged(nameof(SoundGroup));
                }
            }
        }

        private int _selectedSoundGroupNameIdx = -1;
        public int SelectedSoundGroupNameIdx {
            get {
                return _selectedSoundGroupNameIdx;
            }
            set {
                if (_selectedSoundGroupNameIdx != value) {
                    _selectedSoundGroupNameIdx = value;
                    OnPropertyChanged(nameof(SelectedSoundGroupNameIdx));
                }
            }
        }
        public List<string> SoundGroupNames {
            get {
                return new List<string>() {
                    "None",
                    "Minimal",
                    "Spacey",
                    "Jungle"
                };
            }
        }
        #endregion

        #region Constructors

        private static MpAvSoundPlayerGroupCollectionViewModel _instance;
        public static MpAvSoundPlayerGroupCollectionViewModel Instance => _instance ?? (_instance = new MpAvSoundPlayerGroupCollectionViewModel());

        public async Task InitAsync() {

            await SetSoundGroupIdx(MpPrefViewModel.Instance.NotificationSoundGroupIdx);
        }

        public MpAvSoundPlayerGroupCollectionViewModel() : base(null) {
            PropertyChanged += MpSoundPlayerGroupCollectionViewModel_PropertyChanged;
        }
        #endregion

        #region Public Methods

        public async Task SetSoundGroupIdx(int soundGroupIdx) {
            await Task.Run(() => {
                SoundPlayerViewModels.Clear();
                SoundGroup = (MpSoundGroup)soundGroupIdx;
                switch (SoundGroup) {
                    case MpSoundGroup.None:
                        //do nothing so collections empty and play commands don't execute
                        break;
                    case MpSoundGroup.Minimal:
                        SoundPlayerViewModels.Add(new MpAvSoundPlayerViewModel(MpSoundType.Copy, MpPrefViewModel.Instance.NotificationCopySound1Path));
                        SoundPlayerViewModels.Add(new MpAvSoundPlayerViewModel(MpSoundType.AppendOn, MpPrefViewModel.Instance.NotificationAppendModeOnSoundPath));
                        SoundPlayerViewModels.Add(new MpAvSoundPlayerViewModel(MpSoundType.AppendOff, MpPrefViewModel.Instance.NotificationAppendModeOffSoundPath));
                        SoundPlayerViewModels.Add(new MpAvSoundPlayerViewModel(MpSoundType.Loaded, MpPrefViewModel.Instance.NotificationLoadedPath));
                        break;
                }
                MpPrefViewModel.Instance.NotificationSoundGroupIdx = (int)SelectedSoundGroupNameIdx;
            });
        }
        #endregion

        #region Private Methods

        private async void MpSoundPlayerGroupCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(SelectedSoundGroupNameIdx):
                    await SetSoundGroupIdx(SelectedSoundGroupNameIdx);
                    break;
            }
        }
        #endregion

        #region Commands

        public ICommand PlayCopySoundCommand => new MpCommand(
            () => {

                SoundPlayerViewModels.Where(x => x.SoundType == MpSoundType.Copy).ToList()[0].Play();
            }, () => {
                return MpPrefViewModel.Instance.NotificationDoCopySound && SoundPlayerViewModels.Count > 0;
            });


        public ICommand PlayLoadedSoundCommand => new MpCommand(
            () => {
                SoundPlayerViewModels.Where(x => x.SoundType == MpSoundType.Loaded).ToList()[0].Play();
            }, () => {
                return MpPrefViewModel.Instance.NotificationDoLoadedSound && SoundPlayerViewModels.Count > 0;
            });

        public ICommand PlayModeChangeCommand => new MpCommand<object>(
            (args) => {
                bool isOn = args is bool ? (bool)args : true;
                SoundPlayerViewModels.Where(x => isOn ? x.SoundType == MpSoundType.AppendOn : x.SoundType == MpSoundType.AppendOff).ToList()[0].Play();
            }, (args) => {
                return MpPrefViewModel.Instance.NotificationDoModeChangeSound && SoundPlayerViewModels.Count > 0;
            });

        #endregion
    }

    public enum MpSoundGroup {
        None = 0,
        Minimal,
        Spacey,
        Jungle
    }
}
