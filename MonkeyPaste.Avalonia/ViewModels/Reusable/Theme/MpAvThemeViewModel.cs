using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Reflection;

namespace MonkeyPaste.Avalonia {
    public enum MpThemeResourceKey {
        GlobalBgOpacity_desktop,
        GlobalBgOpacity_mobile,
        GlobalBgOpacity,
        DefaultGridSplitterFixedDimensionLength_desktop,
        DefaultGridSplitterFixedDimensionLength_mobile,
        DefaultGridSplitterFixedDimensionLength,
        DefaultEditableFontFamily,
        DefaultReadOnlyFontFamily,
        ContentControlThemeFontFamily
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
            GetThemeValue<double>(MpThemeResourceKey.GlobalBgOpacity_desktop);
        public double GlobalBgOpacity_mobile =>
            GetThemeValue<double>(MpThemeResourceKey.GlobalBgOpacity_mobile);

        public double DefaultGridSplitterFixedDimensionLength_desktop =>
            GetThemeValue<double>(MpThemeResourceKey.DefaultGridSplitterFixedDimensionLength_desktop);
        public double DefaultGridSplitterFixedDimensionLength_mobile =>
            GetThemeValue<double>(MpThemeResourceKey.DefaultGridSplitterFixedDimensionLength_mobile);

        #endregion

        public double GlobalBgOpacity {
            get => GetThemeValue<double>(MpThemeResourceKey.GlobalBgOpacity);
            set {
                if (GlobalBgOpacity != value) {
                    double clamped_value = Math.Max(0, Math.Min(value, 1.0d));
                    SetThemeValue(MpThemeResourceKey.GlobalBgOpacity, clamped_value);
                    OnPropertyChanged(nameof(GlobalBgOpacity));
                }
            }
        }

        public double DefaultGridSplitterFixedDimensionLength {
            get => GetThemeValue<double>(MpThemeResourceKey.DefaultGridSplitterFixedDimensionLength);
            set {
                if (DefaultGridSplitterFixedDimensionLength != value) {
                    SetThemeValue(MpThemeResourceKey.DefaultGridSplitterFixedDimensionLength, value);
                    OnPropertyChanged(nameof(DefaultGridSplitterFixedDimensionLength));
                }
            }
        }

        public string DefaultReadOnlyFontFamily {
            get => GetThemeValue<string>(MpThemeResourceKey.DefaultReadOnlyFontFamily);
            set {
                if (DefaultReadOnlyFontFamily != value) {
                    SetThemeValue(MpThemeResourceKey.DefaultReadOnlyFontFamily, value);
                    SetThemeValue(MpThemeResourceKey.ContentControlThemeFontFamily, value);
                    OnPropertyChanged(nameof(DefaultReadOnlyFontFamily));
                }
            }
        }

        public string DefaultEditableFontFamily {
            get => GetThemeValue<string>(MpThemeResourceKey.DefaultEditableFontFamily);
            set {
                if (DefaultEditableFontFamily != value) {
                    SetThemeValue(MpThemeResourceKey.DefaultEditableFontFamily, value);
                    OnPropertyChanged(nameof(DefaultEditableFontFamily));
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
            MpPrefViewModel.Instance.PropertyChanged += PrefViewModel_Instance_PropertyChanged;
            PropertyChanged += MpAvThemeViewModel_PropertyChanged;
#if DESKTOP
            GlobalBgOpacity = GlobalBgOpacity_desktop;
            DefaultGridSplitterFixedDimensionLength = DefaultGridSplitterFixedDimensionLength_desktop;
#else
            GlobalBgOpacity = GlobalBgOpacity_mobile;
            DefaultGridSplitterFixedDimensionLength = DefaultGridSplitterFixedDimensionLength_mobile;
#endif

            SyncThemePrefs();
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

        private void PrefViewModel_Instance_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            SyncThemePref(e.PropertyName);
        }

        private void SyncThemePrefs() {
            string[] names = new string[] {
                nameof(MpPrefViewModel.Instance.DefaultReadOnlyFontFamily),
                nameof(MpPrefViewModel.Instance.DefaultEditableFontFamily),
                nameof(MpPrefViewModel.Instance.DefaultFontSize),
                nameof(MpPrefViewModel.Instance.GlobalBgOpacity)
            };
            names.ForEach(x => SyncThemePref(x));
        }
        private void SyncThemePref(string prefName) {
            if (!this.HasProperty(prefName)) {
                return;
            }
            this.SetPropertyValue(prefName, MpPrefViewModel.Instance.GetPropertyValue(prefName));
        }

        private T GetThemeValue<T>(MpThemeResourceKey trk) {
            return (T)Mp.Services.PlatformResource.GetResource(trk.ToString());
        }
        private void SetThemeValue(MpThemeResourceKey trk, object value) {
            Mp.Services.PlatformResource.SetResource(trk.ToString(), value);

        }
        #endregion

        #region Commands
        #endregion
    }
}
