using Avalonia.Controls;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        #endregion

        #region Model
        string CreditsFileUri =>
            @"avares://MonkeyPaste.Avalonia/Resources/credits.txt";
        public string CreditsText { get; private set; }
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
                Width = 300,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                CanResize = false,
                Title = "About".ToWindowTitleText(),
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
                if (string.IsNullOrEmpty(CreditsText)) {
                    CreditsText = MpAvStringResourceConverter.Instance
                        .Convert(CreditsFileUri, typeof(string), null, null) as string;
                }
                if (IsWindowOpen) {
                    if (IsWindowActive) {
                        return;
                    }
                    IsWindowActive = true;
                    return;
                }
                var aw = CreateAboutWindow();
                aw.ShowChild();
            });


        #endregion


    }
}
