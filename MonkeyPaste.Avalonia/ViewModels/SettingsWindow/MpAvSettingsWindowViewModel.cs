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
                    MpAvMainWindowViewModel.Instance.IsShowingDialog = IsVisible;

                    if(IsVisible) {
                        var sw = new MpAvSettingsWindow();                        
                        sw.ShowDialog(MpAvMainWindow.Instance);
                    } else {

                        
                    }
                    break;
            }
        }
        #endregion

        #region Commands

        public ICommand ShowSettingsWindowCommand => new MpCommand(
            () => {
                IsVisible = true;
            });
        #endregion
    }
}
