using Avalonia.Controls;
using MonkeyPaste.Common;
using System;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvAboutViewModel : MpAvViewModelBase, MpIActiveWindowViewModel, MpICloseWindowViewModel {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        private static MpAvAboutViewModel _instance;
        public static MpAvAboutViewModel Instance => _instance ?? (_instance = new MpAvAboutViewModel());
        #endregion

        #region Interfaces

        #region MpIWindowViewModel Implementation
        public MpWindowType WindowType =>
            MpWindowType.Modal;
        public bool IsWindowOpen { get; set; }
        public bool IsWindowActive { get; set; }

        #endregion
        #endregion

        #region Properties

        #region State
        public bool IsOverCredits { get; set; }

        public string VersionUrl =>
            $"{MpServerConstants.VERSION_BASE_URL}/{Mp.Services.ThisAppInfo.ThisAppProductVersion}";

        public string TermsUrl =>
            $"{MpServerConstants.LEGAL_BASE_URL}/terms/index.html";
        #endregion

        #region Model
        public string ProductName =>
            Mp.Services.ThisAppInfo.ThisAppProductName;
        public string ProductVersion =>
            string.Format("Version {0}", Mp.Services.ThisAppInfo.ThisAppProductVersion);
        public string CompanyName =>
            Mp.Services.ThisAppInfo.ThisAppCompanyName;

        public string LegalDetail =>
            string.Format("All Rights Reserved ({0}).", DateTime.Now.Year);

        public string CreditsHtml { get; private set; }
        #endregion

        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        private MpAvWindow CreateAboutWindow() {
            var aw = new MpAvWindow() {
                MinWidth = 300,
                MinHeight = 200,
                Width = 500,
                Height = 330,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                CanResize = true,
                Title = UiStrings.AboutWindowTitlePrefix.ToWindowTitleText(),
                Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("AppImage", typeof(WindowIcon), null, null) as WindowIcon,
                DataContext = this,
                Content = new MpAvAboutView()
            };
            return aw;
        }
        #endregion

        #region Commands
        public ICommand ShowAboutWindowCommand => new MpCommand(
            () => {
                if (string.IsNullOrEmpty(CreditsHtml)) {
                    CreditsHtml = MpFileIo.ReadTextFromFile(Mp.Services.PlatformInfo.CreditsPath);
                }
                if (IsWindowOpen) {
                    if (IsWindowActive) {
                        return;
                    }
                    IsWindowActive = true;
                    return;
                }
                var aw = CreateAboutWindow();
                aw.Show();
            });


        #endregion


    }
}
