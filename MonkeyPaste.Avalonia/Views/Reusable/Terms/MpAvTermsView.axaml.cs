using Avalonia.Controls;
using MonkeyPaste.Common;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WindowStartupLocation = Avalonia.Controls.WindowStartupLocation;

namespace MonkeyPaste.Avalonia {
    public class MpAvTermsAgreementViewModel : MpAvViewModelBase {
        public string Author { get; set; }
        public string PackageName { get; set; }
        public string LicenseUri { get; set; }
    }

    public class MpAvTermsAgreementCollectionViewModel : MpAvViewModelBase, MpIWantsTopmostWindowViewModel {
        public IList<MpAvTermsAgreementViewModel> Items { get; set; }
        public string IntroText { get; set; }
        public string OutroText { get; set; }
        public bool WantsTopmost =>
            true;
        public MpWindowType WindowType =>
            MpWindowType.Modal;
    }

    [DoNotNotify]
    public partial class MpAvTermsView : MpAvUserControl {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        public static async Task<bool> ShowTermsAgreementWindowAsync(MpAvTermsAgreementCollectionViewModel tacvm) {
            var tw = new MpAvWindow() {
                Width = 425,
                Height = 250,
                Topmost = true,
                ShowInTaskbar = true,
                ShowActivated = true,
                WindowType = MpWindowType.Modal,
                Title = UiStrings.TermsWindowTitle,
                Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("GavelImage", typeof(WindowIcon), null, null) as WindowIcon,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = new MpAvTermsView(),
                DataContext = tacvm
            };
            bool is_open = true;
            void Tw_Closed(object sender, EventArgs e) {
                if (tw.DialogResult == null) {
                    tw.DialogResult = false;
                }
                is_open = false;
                tw.Closed -= Tw_Closed;
            }
            tw.Closed += Tw_Closed;
            tw.Show();
            while (is_open) {
                await Task.Delay(100);
            }
            return (bool)tw.DialogResult;

        }
        #endregion



        public MpAvTermsView() {
            InitializeComponent();
            NoButton.Click += NoButton_Click;
            YesButton.Click += YesButton_Click;
        }

        private void YesButton_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (TopLevel.GetTopLevel(this) is not MpAvWindow w) {
                return;
            }
            w.DialogResult = true;
            w.Close();
        }

        private void NoButton_Click(object sender, global::Avalonia.Interactivity.RoutedEventArgs e) {
            if (TopLevel.GetTopLevel(this) is not MpAvWindow w) {
                return;
            }
            w.DialogResult = false;
            w.Close();
        }
    }
}
