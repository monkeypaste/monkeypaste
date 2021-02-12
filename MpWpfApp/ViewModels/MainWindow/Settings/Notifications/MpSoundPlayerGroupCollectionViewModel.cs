using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpSoundPlayerGroupCollectionViewModel : MpObservableCollectionViewModel<MpSoundPlayerViewModel> {
        private static readonly Lazy<MpSoundPlayerGroupCollectionViewModel> _Lazy = new Lazy<MpSoundPlayerGroupCollectionViewModel>(() => new MpSoundPlayerGroupCollectionViewModel());
        public static MpSoundPlayerGroupCollectionViewModel Instance { get { return _Lazy.Value; } }

        #region Properties
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

        #region Public Methods
        public void Init() {
        }
        #endregion

        #region Private Methods
        private MpSoundPlayerGroupCollectionViewModel() : this((MpSoundGroup)Properties.Settings.Default.NotificationSoundGroupIdx) { }

        private MpSoundPlayerGroupCollectionViewModel(MpSoundGroup group) : base() {
            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(SelectedSoundGroupNameIdx):
                        this.Clear();
                        SoundGroup = (MpSoundGroup)SelectedSoundGroupNameIdx;
                        switch (SoundGroup) {
                            case MpSoundGroup.None:
                                //do nothing so collections empty and play commands don't execute
                                break;
                            case MpSoundGroup.Minimal:
                                this.Add(new MpSoundPlayerViewModel(MpSoundType.Copy, Properties.Settings.Default.NotificationCopySound1Path));
                                this.Add(new MpSoundPlayerViewModel(MpSoundType.AppendOn, Properties.Settings.Default.NotificationAppendModeOnSoundPath));
                                this.Add(new MpSoundPlayerViewModel(MpSoundType.AppendOff, Properties.Settings.Default.NotificationAppendModeOffSoundPath));
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
            return Properties.Settings.Default.NotificationDoCopySound && this.Count > 0;
        }
        private void PlayCopySound() {
            this.Where(x => x.SoundType == MpSoundType.Copy).ToList()[0].Play();
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
            return Properties.Settings.Default.NotificationDoModeChangeSound && this.Count > 0;
        }
        private void PlayModeChange(bool isOn) {
            this.Where(x => isOn ? x.SoundType == MpSoundType.AppendOn: x.SoundType == MpSoundType.AppendOff).ToList()[0].Play();
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
