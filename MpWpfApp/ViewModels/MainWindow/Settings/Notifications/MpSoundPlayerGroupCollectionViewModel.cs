using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpSoundPlayerGroupCollectionViewModel : MpViewModelBase, MpISingleton<MpSoundPlayerGroupCollectionViewModel> {
        #region Properties

        #region View Models
        private ObservableCollection<MpSoundPlayerViewModel> _soundPlayerViewModels = new ObservableCollection<MpSoundPlayerViewModel>();
        public ObservableCollection<MpSoundPlayerViewModel> SoundPlayerViewModels {
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

        private static MpSoundPlayerGroupCollectionViewModel _instance;
        public static MpSoundPlayerGroupCollectionViewModel Instance => _instance ?? (_instance = new MpSoundPlayerGroupCollectionViewModel());

        public async Task Init() {

            await SetSoundGroupIdx(MpPreferences.Instance.NotificationSoundGroupIdx);
        }

        public MpSoundPlayerGroupCollectionViewModel() : base() {
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
                        SoundPlayerViewModels.Add(new MpSoundPlayerViewModel(MpSoundType.Copy, Properties.Settings.Default.NotificationCopySound1Path));
                        SoundPlayerViewModels.Add(new MpSoundPlayerViewModel(MpSoundType.AppendOn, Properties.Settings.Default.NotificationAppendModeOnSoundPath));
                        SoundPlayerViewModels.Add(new MpSoundPlayerViewModel(MpSoundType.AppendOff, Properties.Settings.Default.NotificationAppendModeOffSoundPath));
                        SoundPlayerViewModels.Add(new MpSoundPlayerViewModel(MpSoundType.Loaded, Properties.Settings.Default.NotificationLoadedPath));
                        break;
                }
                MpPreferences.Instance.NotificationSoundGroupIdx = (int)SelectedSoundGroupNameIdx;
            });
        }
        #endregion

        #region Private Methods

        private async void MpSoundPlayerGroupCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(SelectedSoundGroupNameIdx):
                    await SetSoundGroupIdx(SelectedSoundGroupNameIdx);
                    break;
            }
        }
        #endregion

        #region Commands
        private RelayCommand _playCopySoundCommand = null;
        public ICommand PlayCopySoundCommand {
            get {
                if(_playCopySoundCommand == null) {
                    _playCopySoundCommand = new RelayCommand(PlayCopySound, CanPlayCopySound);
                }
                return _playCopySoundCommand;
            }
        }
        private bool CanPlayCopySound() {
            return Properties.Settings.Default.NotificationDoCopySound && SoundPlayerViewModels.Count > 0;
        }
        private void PlayCopySound() {
            SoundPlayerViewModels.Where(x => x.SoundType == MpSoundType.Copy).ToList()[0].Play();
        }

        private RelayCommand _playLoadedSoundCommand = null;
        public ICommand PlayLoadedSoundCommand {
            get {
                if (_playLoadedSoundCommand == null) {
                    _playLoadedSoundCommand = new RelayCommand(PlayLoadedSound, CanPlayLoadedSound);
                }
                return _playLoadedSoundCommand;
            }
        }
        private bool CanPlayLoadedSound() {
            return Properties.Settings.Default.NotificationDoLoadedSound && SoundPlayerViewModels.Count > 0;
        }
        private void PlayLoadedSound() {
            SoundPlayerViewModels.Where(x => x.SoundType == MpSoundType.Loaded).ToList()[0].Play();
        }

        private RelayCommand<bool> _playModeChangeCommand = null;
        public ICommand PlayModeChangeCommand {
            get {
                if (_playModeChangeCommand == null) {
                    _playModeChangeCommand = new RelayCommand<bool>(PlayModeChange, CanPlayModeChange);
                }
                return _playModeChangeCommand;
            }
        }
        private bool CanPlayModeChange(bool isOn) {
            return Properties.Settings.Default.NotificationDoModeChangeSound && SoundPlayerViewModels.Count > 0;
        }
        private void PlayModeChange(bool isOn) {
            SoundPlayerViewModels.Where(x => isOn ? x.SoundType == MpSoundType.AppendOn: x.SoundType == MpSoundType.AppendOff).ToList()[0].Play();
        }
        #endregion
    }

    public enum MpSoundGroup {
        None = 0,
        Minimal,
        Spacey,
        Jungle
    }
}
