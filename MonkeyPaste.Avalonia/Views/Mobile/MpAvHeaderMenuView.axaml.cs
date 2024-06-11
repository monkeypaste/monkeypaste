using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvHeaderMenuView : MpAvUserControl<MpAvIHeaderMenuViewModel> {

        public MpAvHeaderMenuView() {
            InitializeComponent();
        }


        protected override void OnLoaded(RoutedEventArgs e) {
            base.OnLoaded(e);
            if (this.GetVisualAncestor<MpAvOverlayContainerView>() != null) {
                this.BackButton.Classes.Add("popout");
            } else {
                this.BackButton.Classes.Remove("popout");
            }

        }

        #region Commands
        public ICommand DefaultBackCommand => new MpCommand<object>(
                    (args) => {
                        if(this.DataContext is MpAvIFocusHeaderMenuViewModel) {
                            MpAvMainView.Instance.Focus();
                        } else if(this.GetVisualAncestor<MpAvChildWindow>() is { } cw) {
                            cw.Close();
                        }
                        MpMessenger.SendGlobal(MpMessageType.FocusItemChanged);
                    });
        #endregion
    }
}
