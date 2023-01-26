using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public enum MpThemeResourceKey {
        GlobalBgOpacity
    }
    public class MpAvThemeViewModel : MpViewModelBase {
        #region Private Variable
        #endregion

        #region Constants
        #endregion

        #region Statics
        private static MpAvThemeViewModel _instance;
        public static MpAvThemeViewModel Instance => _instance ?? (_instance = new MpAvThemeViewModel());

        public void Init() {
            // empty
        }
        #endregion

        #region Properties

        #region Appearance

        public double GlobalBgOpacity {
            get => (double)MpPlatform.Services.PlatformResource.GetResource(MpThemeResourceKey.GlobalBgOpacity.ToString());
            set {
                if(GlobalBgOpacity != value) {
                    double clamped_value = Math.Max(0, Math.Min(value, 1.0d));
                    MpPlatform.Services.PlatformResource.SetResource(MpThemeResourceKey.GlobalBgOpacity.ToString(), clamped_value);
                    MpPrefViewModel.Instance.MainWindowOpacity = clamped_value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(GlobalBgOpacity));
                }
            }        
        } 
        #endregion

        #endregion

        #region Constructors
        private MpAvThemeViewModel() {
            PropertyChanged += MpAvThemeViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        private void MpAvThemeViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(HasModelChanged):

                    break;
            }
        }

        #endregion

        #region Commands
        #endregion
    }
}
