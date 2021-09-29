using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpSoundPlayerGroupCollectionViewModel : MpViewModelBase<object> {
        private static readonly Lazy<MpSoundPlayerGroupCollectionViewModel> _Lazy = new Lazy<MpSoundPlayerGroupCollectionViewModel>(() => new MpSoundPlayerGroupCollectionViewModel());
        public static MpSoundPlayerGroupCollectionViewModel Instance { get { return _Lazy.Value; } }

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
                    OnPropertyChanged_old(nameof(SoundPlayerViewModels));
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
                    OnPropertyChanged_old(nameof(SoundGroup));
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
                    OnPropertyChanged_old(nameof(SelectedSoundGroupNameIdx));
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

        #region Public Methods
        public void Init() {
        }
        #endregion

        #region Private Methods
        private MpSoundPlayerGroupCollectionViewModel() : this((MpSoundGroup)Properties.Settings.Default.NotificationSoundGroupIdx) { }

        private MpSoundPlayerGroupCollectionViewModel(MpSoundGroup group) : base(null) {
            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(SelectedSoundGroupNameIdx):
                        SoundPlayerViewModels.Clear();
                        SoundGroup = (MpSoundGroup)SelectedSoundGroupNameIdx;
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
                        Properties.Settings.Default.NotificationSoundGroupIdx = (int)SelectedSoundGroupNameIdx;
                        Properties.Settings.Default.Save();
                        break;
                }
            };
            SelectedSoundGroupNameIdx = (int)group;
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
