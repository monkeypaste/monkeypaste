using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using NetCoreAudio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {

    public enum MpSoundNotificationType {
        None = 0,
        Copy,
        Paste,
        AppendOn,
        AppendOff,
        AutoCopyOn,
        AutoCopyOff,
        MousePasteOn,
        MousePasteOff,
        Loaded,
        Error
    }

    public class MpAvSoundPlayerViewModel : MpAvViewModelBase {
        #region Private Variables
        private object _soundPlayerLock = new object();
        const int MAX_WIN_SOUND_PATH_LEN = 128;
        private Player _player;

        //private SoundPlayer _soundPlayer = null;
        private Dictionary<MpSoundNotificationType, string> _soundPathLookup = new Dictionary<MpSoundNotificationType, string>();
        #endregion

        #region Statics

        private static MpAvSoundPlayerViewModel _instance;
        public static MpAvSoundPlayerViewModel Instance => _instance ?? (_instance = new MpAvSoundPlayerViewModel());

        #endregion

        #region Properties

        #region View Models
        public ObservableCollection<string> Items { get; set; } = new ObservableCollection<string>(Enum.GetNames(typeof(MpSoundGroupType)));

        #endregion

        #region State

        public bool IsOsSupported =>
#if BROWSER || ANDROID || MAC
            false;
#else
            true;
#endif

        public int SelectedItemIdx {
            get => (int)SelectedSoundGroup;
            set {
                if (SelectedItemIdx != value) {
                    SelectedSoundGroup = (MpSoundGroupType)value;
                    OnPropertyChanged(nameof(SelectedItemIdx));
                }
            }
        }

        public string SoundResourceDir =>
            Mp.Services == null ||
            Mp.Services.PlatformInfo == null ?
                null :
                Path.Combine(Mp.Services.PlatformInfo.ExecutingDir, "Assets", "Sounds");

        public string SoundResourceBackupDir =>
            Mp.Services == null ||
            Mp.Services.PlatformInfo == null ?
                null :
                Path.Combine(Mp.Services.PlatformInfo.StorageDir, "Sounds");

        public MpSoundGroupType SelectedSoundGroup {
            get => (MpSoundGroupType)MpAvPrefViewModel.Instance.NotificationSoundGroupIdx;
            set {
                if (SelectedSoundGroup != value) {
                    MpAvPrefViewModel.Instance.NotificationSoundGroupIdx = (int)value;
                    OnPropertyChanged(nameof(SelectedSoundGroup));
                }
            }
        }
        bool IsMuted =>
            MpAvPrefViewModel.Instance.NotificationSoundVolume == 0;

        public bool CanPlaySound =>
            IsOsSupported;

        #endregion


        #endregion

        #region Constructors

        public MpAvSoundPlayerViewModel() : base(null) {
            if (!IsOsSupported) {
                return;
            }
            PropertyChanged += MpSoundPlayerGroupCollectionViewModel_PropertyChanged;
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
        }
        #endregion

        #region Public Methods

        public async Task InitializeAsync() {
            if (!IsOsSupported) {
                return;
            }

            IsBusy = true;

            if (_player == null) {
                _player = new Player();
                await UpdateVolumeCommand.ExecuteAsync(null);
            }
            SelectedSoundGroup = (MpSoundGroupType)MpAvPrefViewModel.Instance.NotificationSoundGroupIdx;

            _soundPathLookup.Clear();

            for (int i = 0; i < Enum.GetNames(typeof(MpSoundNotificationType)).Length; i++) {
                MpSoundNotificationType snt = (MpSoundNotificationType)i;
                if (snt == MpSoundNotificationType.None) {
                    continue;
                }
                // NOTE sounds are handled like other assets but are NOT avaloniaResources 
                // because sound player only accepts file paths (stream's don't work on linux)
                // Sounds have no build action and are copied to output directory so path is resolved here

                string resource_key = $"{SelectedSoundGroup}{snt}Sound";
                string sound_file_name = Mp.Services.PlatformResource.GetResource(resource_key) as string;
                if (sound_file_name == null) {
                    continue;
                }
                string sound_path = Path.Combine(SoundResourceDir, sound_file_name);

                if (!sound_path.IsFile()) {
                    //error
                    MpDebug.Break($"Sound load error. file not found '{sound_path}'");
                    continue;
                }

                if (sound_path.Length > MAX_WIN_SOUND_PATH_LEN &&
                    OperatingSystem.IsWindows()) {
                    // path too long for window due to MCI (windows) limitation.
                    // substitute into storage dir on startup
                    if (!SoundResourceBackupDir.IsDirectory()) {
                        // this is intended for initial startup to fallback and store sounds in user storage
                        if (MpFileIo.CreateDirectory(SoundResourceBackupDir)) {
                            MpConsole.WriteLine($"Sound backup folder created successfully at '{SoundResourceBackupDir}'");
                        } else {
                            MpDebug.Break($"Sound load error. Cannot create backup dir '{SoundResourceBackupDir}'");
                            continue;
                        }
                    }
                    string backup_sound_path = Path.Combine(SoundResourceBackupDir, sound_file_name);
                    if (backup_sound_path.Length > MAX_WIN_SOUND_PATH_LEN) {
                        MpDebug.Break($"Sound load error. Backup is still too long.. path is '{backup_sound_path}' max length: {MAX_WIN_SOUND_PATH_LEN}");
                        continue;
                    }
                    if (!backup_sound_path.IsFile()) {
                        // initial create
                        bool success = backup_sound_path == MpFileIo.CopyFileOrDirectory(sound_path, backup_sound_path, false);
                        if (!success) {
                            MpDebug.Break($"Sound load error. Cannot create backup to path '{backup_sound_path}'");
                            continue;
                        }
                    }
                    sound_path = backup_sound_path;
                }

                _soundPathLookup.Add(snt, sound_path);
                MpConsole.WriteLine($"Initialized Sound: '{snt}' Path: '{sound_path}'");
            }

            IsBusy = false;

        }

        #endregion

        #region Private Methods

        private void MpSoundPlayerGroupCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(SelectedItemIdx):
                    if (IsBusy) {
                        break;
                    }
                    MpAvPrefViewModel.Instance.NotificationSoundGroupIdx = SelectedItemIdx;
                    InitializeAsync().FireAndForgetSafeAsync(this);
                    break;
            }
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ContentAdded:
                    PlaySoundNotificationCommand.Execute(MpSoundNotificationType.Copy);
                    break;
                case MpMessageType.ContentPasted:
                    PlaySoundNotificationCommand.Execute(MpSoundNotificationType.Paste);
                    break;
                case MpMessageType.AppError:
                    PlaySoundNotificationCommand.Execute(MpSoundNotificationType.Error);
                    break;
                case MpMessageType.AutoCopyEnabled:
                    PlaySoundNotificationCommand.Execute(MpSoundNotificationType.AutoCopyOn);
                    break;
                case MpMessageType.AutoCopyDisabled:
                    PlaySoundNotificationCommand.Execute(MpSoundNotificationType.AutoCopyOff);
                    break;
                case MpMessageType.RightClickPasteEnabled:
                    PlaySoundNotificationCommand.Execute(MpSoundNotificationType.MousePasteOn);
                    break;
                case MpMessageType.RightClickPasteDisabled:
                    PlaySoundNotificationCommand.Execute(MpSoundNotificationType.MousePasteOff);
                    break;
                case MpMessageType.AppendModeActivated:
                    PlaySoundNotificationCommand.Execute(MpSoundNotificationType.AppendOn);
                    break;
                case MpMessageType.AppendModeDeactivated:
                    PlaySoundNotificationCommand.Execute(MpSoundNotificationType.AppendOff);
                    break;
                case MpMessageType.MainWindowLoadComplete:
                    PlaySoundNotificationCommand.Execute(MpSoundNotificationType.Loaded);
                    break;

            }
        }

        #endregion

        #region Commands

        public MpIAsyncCommand<object> UpdateVolumeCommand => new MpAsyncCommand<object>(
            async (args) => {
                double new_norm_volue =
                    args == null ?
                        MpAvPrefViewModel.Instance.NotificationSoundVolume :
                        (double)args;

                byte volume = (byte)((double)byte.MaxValue * new_norm_volue);
                await _player.SetVolume(volume);

            }, (args) => IsOsSupported);

        public ICommand PlayCustomSoundCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (args is not object[] argParts) {
                    return;
                }
                string sound_path_or_key = argParts[0] as string;
                double norm_sound_vol = (double)argParts[1];
                string sound_path = sound_path_or_key;
                if (!sound_path.IsFile()) {
                    string sound_file_name = Mp.Services.PlatformResource.GetResource<string>(sound_path_or_key);
                    if (sound_file_name == null ||
                    _soundPathLookup.Values.FirstOrDefault(x => x.ToLowerInvariant().EndsWith(sound_file_name.ToLowerInvariant())) is not string lookup_path ||
                    !lookup_path.IsFile()) {
                        MpDebug.Break($"Error loading sound from arg '{sound_path_or_key}'");
                        return;
                    }
                    sound_path = lookup_path;
                }
                await MpFifoAsyncQueue.WaitByConditionAsync(
                    lockObj: _soundPlayerLock,
                    waitWhenTrueFunc: () => {
                        return _player.Playing;
                    },
                    debug_label: $"Sound to play '{sound_path}'");

                // sound played from action ntf
                await UpdateVolumeCommand.ExecuteAsync(norm_sound_vol);

                await _player.Play(sound_path);

                while (_player.Playing) {
                    await Task.Delay(100);
                }
                await UpdateVolumeCommand.ExecuteAsync(null);

            }, (args) => IsOsSupported);
        public ICommand PlaySoundNotificationCommand => new MpAsyncCommand<object>(
            async (args) => {

                MpSoundNotificationType snt = (MpSoundNotificationType)args;
                if (!_soundPathLookup.ContainsKey(snt)) {
                    MpConsole.WriteLine($"Missing sound resource for ntf type: '{snt}'. Maybe intentional if path was too long, due to MCI (windows) limitation.");
                    return;
                }
                while (_player.Playing) {
                    MpConsole.WriteLine($"Sound already playing, waiting to play '{snt}'...");
                    await Task.Delay(100);
                    if (!_player.Playing) {
                        // give it a sec or this exception happens intermittently (windows):
                        // Error executing MCI command 'Close All'. Error code: 288. Message: The specified device is now being closed.  Wait a few seconds, and then try again.
                        await Task.Delay(2000);
                    }
                }
                try {

                    await _player.Play(_soundPathLookup[snt]);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Exception playing sound '{snt}': ", ex);
                }


            }, (args) => {
                return
                    IsOsSupported &&
                    args is MpSoundNotificationType &&
                    !IsMuted;
            });

        #endregion
    }

}
