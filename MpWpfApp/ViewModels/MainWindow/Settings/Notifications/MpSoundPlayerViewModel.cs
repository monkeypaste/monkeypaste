using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpSoundPlayerViewModel : MpViewModelBase<object> {
        #region Private Variables
        private SoundPlayer _soundPlayer = null;
        #endregion

        #region Properties
        private bool _isSelected = false;
        public bool IsSelected {
            get {
                return _isSelected;
            }
            set {
                if (_isSelected != value) {
                    _isSelected = value;
                    OnPropertyChanged_old(nameof(IsSelected));
                }
            }
        }

        private MpSoundType _soundType = MpSoundType.None;
        public MpSoundType SoundType {
            get {
                return _soundType;
            }
            set {
                if (_soundType != value) {
                    _soundType = value;
                    OnPropertyChanged_old(nameof(SoundType));
                }
            }
        }

        private string _soundDisplayName = string.Empty;
        public string SoundDisplayName {
            get {
                return _soundDisplayName;
            }
            set {
                if (_soundDisplayName != value) {
                    _soundDisplayName = value;
                    OnPropertyChanged_old(nameof(SoundDisplayName));
                }
            }
        }
        #endregion

        #region Public Methods
        public MpSoundPlayerViewModel(MpSoundType soundType, string path, bool isAbsolute = false) : base(null) {
            SoundType = soundType;            
            LoadFile(path, isAbsolute);
        }
        public void LoadFile(string path, bool isAbsolute) {
            if(!isAbsolute) {
                string resourcesFolderPath = Path.Combine(Directory.GetParent(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Parent.FullName, "Resources");
                path = resourcesFolderPath + "\\" + path;
            }
            SoundDisplayName = Path.GetFileNameWithoutExtension(path).ToLower();
            if(_soundPlayer == null) {
                _soundPlayer = new SoundPlayer(path);
            } else {
                _soundPlayer.SoundLocation = path;
            }
            _soundPlayer.LoadAsync();
        }

        public void Play() {
            _soundPlayer.Play();
        }
        #endregion
    }    
    public enum MpSoundType {
        None = 0,
        Copy,
        Paste,
        AppendOn,
        AppendOff,
        AutoCopyOn,
        AutoCopyOff,
        MousePasteOn,
        MousePasteOff,
        Loaded
    }
}
