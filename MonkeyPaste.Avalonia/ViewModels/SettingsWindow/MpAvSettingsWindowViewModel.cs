using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvSettingsWindowViewModel : MpViewModelBase {
        #region Statics

        private static MpAvSettingsWindowViewModel _instance;
        public static MpAvSettingsWindowViewModel Instance => _instance ?? (_instance = new MpAvSettingsWindowViewModel());

        #endregion

        #region Properties

        #region State

        public bool IsVisible { get; set; } = false;

        public int TabIdx { get; set; } = 0; 

        #endregion

        #endregion

        #region Constructors

        public MpAvSettingsWindowViewModel() : base() {
            PropertyChanged += MpAvSettingsWindowViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitAsync() {
            await Task.Delay(1);
        }

        #endregion

        #region Private Methods

        private void MpAvSettingsWindowViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsVisible):
                    MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = IsVisible;

                    if(IsVisible) {
                        var sw = new MpAvSettingsWindow();     
                        
                        sw.Show();
                    } else {

                        
                    }
                    break;
            }
        }
        #endregion

        #region Commands

        public ICommand ShowSettingsWindowCommand => new MpCommand<object>(
            (args) => {
                if (args is int) {
                    TabIdx = (int)args;
                } else if (args is MpAvClipTileViewModel) {
                    args = (args as MpAvClipTileViewModel).SourceCollectionViewModel.PrimaryItem;
                    TabIdx = 1;
                }
                IsVisible = true;
            }, (args) => MpBootstrapperViewModelBase.IsCoreLoaded && !IsVisible);
        #endregion
    }
}
