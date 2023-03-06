using System;

namespace MonkeyPaste.Avalonia {
    public enum MpThemeResourceKey {
        GlobalBgOpacity_desktop,
        GlobalBgOpacity_mobile,
        GlobalBgOpacity,
        DefaultGridSplitterFixedDimensionLength_desktop,
        DefaultGridSplitterFixedDimensionLength_mobile,
        DefaultGridSplitterFixedDimensionLength,
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

        #region Fixed Resources

        public double GlobalBgOpacity_desktop =>
            (double)Mp.Services.PlatformResource.GetResource(MpThemeResourceKey.GlobalBgOpacity_desktop.ToString());
        public double GlobalBgOpacity_mobile =>
            (double)Mp.Services.PlatformResource.GetResource(MpThemeResourceKey.GlobalBgOpacity_mobile.ToString());

        public double DefaultGridSplitterFixedDimensionLength_desktop =>
            (double)Mp.Services.PlatformResource.GetResource(MpThemeResourceKey.DefaultGridSplitterFixedDimensionLength_desktop.ToString());
        public double DefaultGridSplitterFixedDimensionLength_mobile =>
            (double)Mp.Services.PlatformResource.GetResource(MpThemeResourceKey.DefaultGridSplitterFixedDimensionLength_mobile.ToString());

        #endregion

        public double GlobalBgOpacity {
            get => (double)Mp.Services.PlatformResource.GetResource(MpThemeResourceKey.GlobalBgOpacity.ToString());
            set {
                if (GlobalBgOpacity != value) {
                    double clamped_value = Math.Max(0, Math.Min(value, 1.0d));
                    Mp.Services.PlatformResource.SetResource(MpThemeResourceKey.GlobalBgOpacity.ToString(), clamped_value);
                    MpPrefViewModel.Instance.MainWindowOpacity = clamped_value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(GlobalBgOpacity));
                }
            }
        }

        public double DefaultGridSplitterFixedDimensionLength {
            get => (double)Mp.Services.PlatformResource.GetResource(MpThemeResourceKey.DefaultGridSplitterFixedDimensionLength.ToString());
            set {
                if (DefaultGridSplitterFixedDimensionLength != value) {
                    Mp.Services.PlatformResource.SetResource(MpThemeResourceKey.DefaultGridSplitterFixedDimensionLength.ToString(), value);
                    MpPrefViewModel.Instance.MainWindowOpacity = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(DefaultGridSplitterFixedDimensionLength));
                }
            }
        }

        #endregion

        #region State

        public bool IsDesktop =>
            Mp.Services != null &&
            Mp.Services.PlatformInfo != null &&
            Mp.Services.PlatformInfo.IsDesktop;
        #endregion

        #endregion

        #region Constructors
        private MpAvThemeViewModel() {
            PropertyChanged += MpAvThemeViewModel_PropertyChanged;
#if DESKTOP
            GlobalBgOpacity = GlobalBgOpacity_desktop;
            DefaultGridSplitterFixedDimensionLength = DefaultGridSplitterFixedDimensionLength_desktop;
#else
            GlobalBgOpacity = GlobalBgOpacity_mobile;
            DefaultGridSplitterFixedDimensionLength = DefaultGridSplitterFixedDimensionLength_mobile;
#endif
        }


        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        private void MpAvThemeViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(HasModelChanged):

                    break;
            }
        }

        #endregion

        #region Commands
        #endregion
    }
}
