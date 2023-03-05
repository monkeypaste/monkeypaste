using Avalonia;
using Avalonia.Platform;

using MonkeyPaste;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia
{
    public class MpAvSoundPlayerViewModel : MpViewModelBase<MpAvSoundPlayerGroupCollectionViewModel>
    {
        #region Private Variables
        private SoundPlayer _soundPlayer = null;
        #endregion

        #region Properties
        public bool IsSelected
        {
            get
            {
                if(Parent == null)
                {
                    return false;
                }
                return Parent.SelectedItem == this;
            }
        }

        public MpSoundType SoundType { get; set; }
        #endregion

        #region Public Methods
        public MpAvSoundPlayerViewModel(MpSoundType soundType, string path, bool isAbsolute = false) : base(null)
        {
            SoundType = soundType;
            LoadFile(path, isAbsolute);
        }

        public async Task InitializeAsync(MpSoundType soundType)
        {
            IsBusy = true;
            await Task.Delay(1);

            IsBusy = false;
        }
        public void LoadFile(string path, bool isAbsolute)
        {
            if(!isAbsolute)
            {
                string resourcesFolderPath = Path.Combine(Directory.GetParent(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).Parent.FullName, "Resources");
                path = resourcesFolderPath + "\\" + path;
            }
            SoundDisplayName = Path.GetFileNameWithoutExtension(path).ToLower();
            if(_soundPlayer == null)
            {
                _soundPlayer = new SoundPlayer(path);
            }
            else
            {
                _soundPlayer.SoundLocation = path;
            }
            _soundPlayer.LoadAsync();
        }

        public void Play()
        {
            _soundPlayer.Play();
        }
        #endregion
    }
}
