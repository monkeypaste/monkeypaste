using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvExternalDropWindow : Window {
        #region Private Variables
        #endregion

        #region Statics

        private static MpAvExternalDropWindow _instance;
        public static MpAvExternalDropWindow Instance => _instance ?? (_instance = new MpAvExternalDropWindow());

        public void Init() {
            // init singleton
        }

        #endregion

        #region Constructors

        public MpAvExternalDropWindow() {
            AvaloniaXamlLoader.Load(this);
            this.GetObservable(IsVisibleProperty).Subscribe(value => Window_OnIsVisibleChanged());
#if DEBUG
            this.AttachDevTools();
#endif
            DataContext = MpAvExternalDropWindowViewModel.Instance;

            var hdmb = this.FindControl<Border>("HideDropMenuButton");
            hdmb.AddHandler(DragDrop.DragOverEvent, OnHideOver);

            var dilb = this.FindControl<ListBox>("DropItemListBox");
            dilb.EnableItemsControlAutoScroll();
        }
        #endregion


        private void Window_OnIsVisibleChanged() {
            if (!IsVisible) {
                return;
            }
            PositionDropWindow();
        }

        private void PositionDropWindow() {
            MpPoint w_origin = MpPoint.Zero;
            var screen_bounds = MpAvMainWindowViewModel.Instance.MainWindowScreen.Bounds;
            // TODO orient this base on mainwindow orientation
            switch (MpAvMainWindowViewModel.Instance.MainWindowOrientationType) {
                case MpMainWindowOrientationType.Bottom:
                    w_origin = screen_bounds.TopLeft + new MpPoint(10, 10);
                    break;
            }
            Position = w_origin.ToAvPixelPoint(MpAvMainWindowViewModel.Instance.MainWindowScreen.Scaling);
        }

        #region Drop

        #region Drop Events



        private void DragOver(object sender, DragEventArgs e) {
            MpConsole.WriteLine("[DragOver] Dnd Widget Window Cur Formats: " + String.Join(Environment.NewLine, e.Data.GetDataFormats()));
            e.DragEffects = DragDropEffects.None;
            var lb = this.GetVisualDescendant<ListBox>();
            lb.AutoScrollItemsControl(e);
        }

        private void DragLeave(object sender, DragEventArgs e) {
            var lb = this.GetVisualDescendant<ListBox>();
            lb.AutoScrollItemsControl(null);
        }

        private void OnHideOver(object sender, DragEventArgs e) {
            this.FindControl<Control>("DropMenuBorderContainer").IsVisible = false;
        }

        #endregion

        #endregion
    }
}
