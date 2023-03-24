using MonkeyPaste.Common;
using NetCoreAudio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace MonkeyPaste.Avalonia {
    public enum MpSoundGroupType {
        None = 0,
        Minimal,
        Spacey,
        Jungle
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

    public class MpAvSoundPlayerViewModel : MpViewModelBase {
        #region Private Variables

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
            !OperatingSystem.IsBrowser();
        public int SelectedItemIdx {
            get => (int)SelectedSoundGroup;
            set {
                if (SelectedItemIdx != value) {
                    SelectedSoundGroup = (MpSoundGroupType)value;
                    OnPropertyChanged(nameof(SelectedItemIdx));
                }
            }
        }

        public MpSoundGroupType SelectedSoundGroup { get; set; }

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
                await UpdateVolumeCommand.ExecuteAsync();
            }
            SelectedSoundGroup = (MpSoundGroupType)MpPrefViewModel.Instance.NotificationSoundGroupIdx;

            _soundPathLookup.Clear();
            if (SelectedSoundGroup == MpSoundGroupType.None) {
                IsBusy = false;
                return;
            }
            string player_dir = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Player)).Location).TrimEnd('\\').TrimEnd('/');
            string base_dir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\').TrimEnd('/');
            if (player_dir != base_dir) {
                // path mismatch
                MpDebug.Break();
            }
            for (int i = 0; i < Enum.GetNames(typeof(MpSoundNotificationType)).Length; i++) {
                MpSoundNotificationType snt = (MpSoundNotificationType)i;
                if (snt == MpSoundNotificationType.None) {
                    continue;
                }
                // NOTE sounds are handled like other assets but are NOT avaloniaResources 
                // because sound player only accepts file paths (stream's don't work on linux)
                // Sounds have no build action and are copied to output directory so path is resolved here

                string resource_key = $"{SelectedSoundGroup}{snt}Sound";
                string resource_val = Mp.Services.PlatformResource.GetResource(resource_key) as string;
                if (string.IsNullOrEmpty(resource_val)) {
                    continue;
                }
                string test = Assembly.GetAssembly(typeof(Player)).Location;
                List<string> path_parts = new List<string>() { AppDomain.CurrentDomain.BaseDirectory };
                path_parts.AddRange(new Uri(resource_val).LocalPath.Split(@"/"));
                string sound_path = Path.Combine(path_parts.ToArray());
                if (sound_path.Length > MAX_PATH && OperatingSystem.IsWindows()) {
                    MpNotificationBuilder.ShowMessageAsync(
                        title: $"Error",
                        body: $"Cannot load sound from path '{sound_path}' file path must be no more than '{MAX_PATH}' characters. You will need to move this apps folder to a higher directory to hear that sound.",
                        msgType: MpNotificationType.FileIoError).FireAndForgetSafeAsync(this);
                    continue;
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
                    MpPrefViewModel.Instance.NotificationSoundGroupIdx = SelectedItemIdx;
                    InitializeAsync().FireAndForgetSafeAsync(this);
                    break;
            }
        }

        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ContentAdded:
                    PlaySoundCommand.Execute(MpSoundNotificationType.Copy);
                    break;
                case MpMessageType.ContentPasted:
                    PlaySoundCommand.Execute(MpSoundNotificationType.Paste);
                    break;
                case MpMessageType.AppError:
                    PlaySoundCommand.Execute(MpSoundNotificationType.Error);
                    break;
                case MpMessageType.AutoCopyEnabled:
                    PlaySoundCommand.Execute(MpSoundNotificationType.AutoCopyOn);
                    break;
                case MpMessageType.AutoCopyDisabled:
                    PlaySoundCommand.Execute(MpSoundNotificationType.AutoCopyOff);
                    break;
                case MpMessageType.RightClickPasteEnabled:
                    PlaySoundCommand.Execute(MpSoundNotificationType.MousePasteOn);
                    break;
                case MpMessageType.RightClickPasteDisabled:
                    PlaySoundCommand.Execute(MpSoundNotificationType.MousePasteOff);
                    break;
                case MpMessageType.AppendModeActivated:
                    PlaySoundCommand.Execute(MpSoundNotificationType.AppendOn);
                    break;
                case MpMessageType.AppendModeDeactivated:
                    PlaySoundCommand.Execute(MpSoundNotificationType.AppendOff);
                    break;
                case MpMessageType.MainWindowLoadComplete:
                    PlaySoundCommand.Execute(MpSoundNotificationType.Loaded);
                    break;

            }
        }

        #endregion

        #region Commands

        public MpIAsyncCommand UpdateVolumeCommand => new MpAsyncCommand(
            async () => {

                byte volume = (byte)((double)byte.MaxValue * MpPrefViewModel.Instance.NotificationSoundVolume);
                await _player.SetVolume(volume);

            }, () => IsOsSupported);

        public ICommand PlaySoundCommand => new MpAsyncCommand<object>(
            async (args) => {

                MpSoundNotificationType snt = (MpSoundNotificationType)args;
                if (!_soundPathLookup.ContainsKey(snt)) {
                    MpConsole.WriteLine($"Missing sound resource for ntf type: '{snt}'. Maybe intentional if path was too long, due to MCI (windows) limitation.");
                    return;
                }
                while (_player.Playing) {
                    MpConsole.WriteLine($"Sound already playing, waiting to play '{snt}'...");
                    await Task.Delay(100);
                }
                await _player.Play(_soundPathLookup[snt]);

            }, (args) => {
                return
                    IsOsSupported &&
                    args is MpSoundNotificationType && SelectedSoundGroup != MpSoundGroupType.None;
            });


#if WINDOWS
        const int MAX_PATH = 128;

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern int GetShortPathName(
            [MarshalAs(UnmanagedType.LPTStr)] string path,
            [MarshalAs(UnmanagedType.LPTStr)] StringBuilder shortPath,
            int shortPathLength);

        private static string GetShortPath(string path) {
            var shortPath = new StringBuilder(MAX_PATH);
            GetShortPathName(path, shortPath, MAX_PATH);
            return shortPath.ToString();
        }
#else
        const int MAX_PATH = int.MaxValue;
#endif

        #endregion
    }

}
