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

        public static event EventHandler<MpAvIHeaderMenuViewModel> OnDefaultBackCommandInvoked;
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
                        bool is_popup = false;
                        MpAvIHeaderMenuViewModel hmvm = this.DataContext as MpAvIHeaderMenuViewModel;

                        if (this.DataContext is MpICloseWindowViewModel cwvm) {
                            is_popup = cwvm.IsWindowOpen;
                        }
                        if(this.DataContext is MpAvIFocusHeaderMenuViewModel &&
                            !is_popup) {

                            MpAvMainView.Instance.FocusThisHeader();
                        } else if(this.GetVisualAncestor<MpAvChildWindow>() is { } cw) {
                            cw.Close();
                            MpMessenger.SendGlobal(MpMessageType.FocusItemChanged);
                        }
                        OnDefaultBackCommandInvoked?.Invoke(this, hmvm);
                    });
        #endregion
    }
}
