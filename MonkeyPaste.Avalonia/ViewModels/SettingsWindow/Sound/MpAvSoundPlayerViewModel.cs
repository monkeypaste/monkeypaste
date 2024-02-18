using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using NetCoreAudio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {

    public enum MpSoundType {
        None = 0,
        Monkey,
        Ting,
        Chime,
        Alert,
        Blip,
        Sonar
    }
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
        const int MAX_WIN_SOUND_PATH_LEN = 1;
        private Player _player;

        private Dictionary<MpSoundNotificationType, MpSoundType> _soundNtfLookup = new() {
            {MpSoundNotificationType.Copy,MpSoundType.Ting },
            {MpSoundNotificationType.Paste,MpSoundType.Chime },
            {MpSoundNotificationType.Error,MpSoundType.Alert },
            {MpSoundNotificationType.AutoCopyOn,MpSoundType.Sonar },
            {MpSoundNotificationType.AutoCopyOff,MpSoundType.Blip },
            {MpSoundNotificationType.MousePasteOn,MpSoundType.Sonar },
            {MpSoundNotificationType.MousePasteOff,MpSoundType.Blip },
            {MpSoundNotificationType.AppendOn,MpSoundType.Sonar },
            {MpSoundNotificationType.AppendOff,MpSoundType.Blip },
            {MpSoundNotificationType.Loaded,MpSoundType.Monkey },
        };
        //private SoundPlayer _soundPlayer = null;
        //private Dictionary<MpSoundNotificationType, string> _soundPathLookup = new Dictionary<MpSoundNotificationType, string>();
        private Dictionary<MpSoundType, string> _soundFilePathLookup = new Dictionary<MpSoundType, string>();
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
            _soundFilePathLookup.Clear();
            for (int i = 0; i < Enum.GetNames(typeof(MpSoundType)).Length; i++) {
                MpSoundType st = (MpSoundType)i;
                if (st == MpSoundType.None) {
                    continue;
                }
                string resource_key = $"{st}Sound";
                string sound_uri_path = Mp.Services.PlatformResource.GetResource(resource_key) as string;
                if (sound_uri_path == null) {
                    continue;
                }
                byte[] sound_bytes = await MpFileIo.ReadBytesFromUriAsync(sound_uri_path);
                if (sound_bytes == null || sound_bytes.Length == 0) {
                    continue;
                }
                if (!SoundResourceDir.IsDirectory()) {
                    MpFileIo.CreateDirectory(SoundResourceDir);
                }
                string sound_path = Path.Combine(SoundResourceDir, $"{st}.wav");
                if (!sound_path.IsFile()) {
                    MpFileIo.WriteByteArrayToFile(sound_path, sound_bytes);
                }
                _soundFilePathLookup.Add(st, sound_path);
                MpConsole.WriteLine($"Initialized Sound: '{st}' Path: '{sound_path}'");
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

                byte volume = (byte)(100d * new_norm_volue);
                await _player.SetVolume(volume);

            }, (args) => IsOsSupported);

        public ICommand PlayCustomSoundCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (args is not object[] argParts ||
                    argParts[0] is not string sound_type_enum_key ||
                    argParts[1] is not double norm_sound_vol ||
                    !_soundFilePathLookup.TryGetValue(sound_type_enum_key.ToEnum<MpSoundType>(), out string sound_path) ||
                    !sound_path.IsFile()) {
                    MpConsole.WriteLine($"Error attempting to play custom sound '{args}'");
                    return;
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
                    if (!_player.Playing) {
                        // give it a sec or this exception happens intermittently (windows):
                        // Error executing MCI command 'Close All'. Error code: 288. Message: The specified device is now being closed.  Wait a few seconds, and then try again.
                        await Task.Delay(1500);
                    }
                }
                await UpdateVolumeCommand.ExecuteAsync(null);

            }, (args) => IsOsSupported);
        public ICommand PlaySoundNotificationCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (args is not MpSoundNotificationType snt ||
                    !_soundNtfLookup.TryGetValue(snt, out MpSoundType st) ||
                    !_soundFilePathLookup.TryGetValue(st, out string sound_path) ||
                    !sound_path.IsFile()) {
                    MpConsole.WriteLine($"Error attempting to play sound ntf '{args}'");
                    return;
                }
                while (_player.Playing) {
                    MpConsole.WriteLine($"Sound already playing, waiting to play '{snt}'...");
                    await Task.Delay(100);
                    if (!_player.Playing) {
                        // give it a sec or this exception happens intermittently (windows):
                        // Error executing MCI command 'Close All'. Error code: 288. Message: The specified device is now being closed.  Wait a few seconds, and then try again.
                        await Task.Delay(1500);
                    }
                }
                try {

                    await _player.Play(sound_path);
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
