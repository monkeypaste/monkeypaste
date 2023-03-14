using System;

namespace MonkeyPaste.Avalonia {
    public enum MpThemeResourceKey {
        GlobalBgOpacity_desktop,
        GlobalBgOpacity_mobile,
        GlobalBgOpacity,
        DefaultGridSplitterFixedDimensionLength_desktop,
        DefaultGridSplitterFixedDimensionLength_mobile,
        DefaultGridSplitterFixedDimensionLength,
        BaseEditableDefaultFontFamily,
        BaseReadOnlyDefaultFontFamily,
        DefaultEditableFontFamily,
        DefaultReadOnlyFontFamily
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
                    //MpPrefViewModel.Instance.MainWindowOpacity = clamped_value;
                    //HasModelChanged = true;
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

        public string DefaultFontFamily {
            get => Mp.Services.PlatformResource.GetResource(MpThemeResourceKey.DefaultEditableFontFamily.ToString()) as string;
            set {
                if (DefaultFontFamily != value) {
                    string ro_ff, e_ff;
                    if (value != null &&
                        value.ToLower() != Mp.Services.PlatformResource.GetResource(MpThemeResourceKey.BaseEditableDefaultFontFamily.ToString()) as string) {
                        ro_ff = value;
                        e_ff = value;
                    } else {
                        ro_ff = Mp.Services.PlatformResource.GetResource(MpThemeResourceKey.BaseReadOnlyDefaultFontFamily.ToString()) as string;
                        e_ff = Mp.Services.PlatformResource.GetResource(MpThemeResourceKey.BaseEditableDefaultFontFamily.ToString()) as string;
                    }


                    Mp.Services.PlatformResource.SetResource(MpThemeResourceKey.DefaultEditableFontFamily.ToString(), e_ff);
                    Mp.Services.PlatformResource.SetResource(MpThemeResourceKey.DefaultReadOnlyFontFamily.ToString(), ro_ff);
                    MpPrefViewModel.Instance.DefaultFontFamily = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(DefaultFontFamily));
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
            MpPrefViewModel.Instance.PropertyChanged += Instance_PropertyChanged;
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

        private void Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(MpPrefViewModel.Instance.MainWindowOpacity):
                    GlobalBgOpacity = MpPrefViewModel.Instance.MainWindowOpacity;
                    break;
                case nameof(MpPrefViewModel.Instance.DefaultFontFamily):
                    if (!string.IsNullOrEmpty(MpPrefViewModel.Instance.DefaultFontFamily))
                        DefaultFontFamily = MpPrefViewModel.Instance.DefaultFontFamily;
                    break;
            }
        }
        #endregion

        #region Commands
        #endregion
    }
}
