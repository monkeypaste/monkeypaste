using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using PropertyChanged;
using Avalonia.Input;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvExternalDropWindow : Window {
        #region Private Variables

        private Dictionary<int, bool> _preShowPresetState = new Dictionary<int, bool>();

        #endregion

        private static MpAvExternalDropWindow _instance;
        public static MpAvExternalDropWindow Instance => _instance ?? (_instance = new MpAvExternalDropWindow());

        public void Init() {
            // init singleton
        }

        public MpAvExternalDropWindow() {             
            InitializeComponent();
            this.GetObservable(IsVisibleProperty).Subscribe(value => Window_OnIsVisibleChanged());
#if DEBUG
            this.AttachDevTools();
#endif
            var hdmb = this.FindControl<Button>("HideDropMenuButton");
            hdmb.AddHandler(DragDrop.DragOverEvent, OnHideOver);

            var dilb = this.FindControl<ListBox>("DropItemListBox");
            dilb.AddHandler(DragDrop.DragOverEvent, DragOver);
        }
        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }


        public override void Show() {
            // TODO should add a preference to disable drop widget and ignore show if enabled here
            this.FindControl<Control>("DropMenuBorderContainer").IsVisible = true;
            StoreFormatPresetState();
            ApplyAppPresetFormatState();
            base.Show();
        }

        private void Window_OnIsVisibleChanged() {
            if(!IsVisible) {
                RestoreFormatPresetState();
                return;
            }
            PositionDropWindow();
        }

        private void PositionDropWindow() {
            MpPoint w_origin = MpPoint.Zero;
            var screen_bounds = MpAvMainWindowViewModel.Instance.MainWindowScreen.Bounds;
            // TODO orient this base on mainwindow orientation
            switch(MpAvMainWindowViewModel.Instance.MainWindowOrientationType) {
                case MpMainWindowOrientationType.Bottom:
                    w_origin = screen_bounds.TopLeft + new MpPoint(10, 10);
                    break;
            }
            Position = w_origin.ToAvPixelPoint(MpAvMainWindowViewModel.Instance.MainWindowScreen.PixelDensity);
        }

        private void StoreFormatPresetState() {
            _preShowPresetState =
               MpAvClipboardHandlerCollectionViewModel.Instance.AllAvailableWriterPresets
               .ToDictionary(kvp => kvp.PresetId, kvp => kvp.IsEnabled);

        }

        private void ApplyAppPresetFormatState() {

            // TODO should use MpAppClipboardFormatInfo data for last active here
        }

        private void RestoreFormatPresetState() {
            if(_preShowPresetState == null || _preShowPresetState.Count == 0) {
                return;
            }

            MpAvClipboardHandlerCollectionViewModel.Instance.AllAvailableWriterPresets
                .ForEach(x => x.IsEnabled = _preShowPresetState[x.PresetId]);
        }


        #region Drop

        #region Drop Events

        public void AutoScrollListBox(DragEventArgs e) {
            var lb = this.GetVisualDescendant<ListBox>();

            double amt = 5;
            double max_scroll_dist = 10;
            var lb_mp = e.GetPosition(lb).ToPortablePoint();
            if (Math.Abs(lb_mp.X) <= max_scroll_dist) {
                lb.GetVisualDescendant<ScrollViewer>().ScrollByPointDelta(new MpPoint(-amt, 0));
            } else if (Math.Abs(lb.Bounds.Right - lb_mp.X) <= max_scroll_dist) {
                lb.GetVisualDescendant<ScrollViewer>().ScrollByPointDelta(new MpPoint(amt, 0));
            }

            if (Math.Abs(lb_mp.Y) <= max_scroll_dist) {
                lb.GetVisualDescendant<ScrollViewer>().ScrollByPointDelta(new MpPoint(0, -amt));
            } else if (Math.Abs(lb.Bounds.Bottom - lb_mp.Y) <= max_scroll_dist) {
                lb.GetVisualDescendant<ScrollViewer>().ScrollByPointDelta(new MpPoint(0, amt));
            }
        }

        private void DragOver(object sender, DragEventArgs e) {
            AutoScrollListBox(e);
            e.DragEffects = DragDropEffects.None;
            //MpAvCefNetWebViewGlue.DragDataObject.SetData(MpPortableDataFormats.Text, "GOTCHA!!");
        }

        private void OnHideOver(object sender, DragEventArgs e) {
            this.FindControl<Control>("DropMenuBorderContainer").IsVisible = false;
        }

        #endregion

        #endregion
    }
}
