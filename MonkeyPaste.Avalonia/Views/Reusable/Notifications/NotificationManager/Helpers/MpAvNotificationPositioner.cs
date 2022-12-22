using Avalonia.Controls;
using Avalonia;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public class MpAvNotificationPositioner {
        #region Private Variables

        private ObservableCollection<Window> _windows = new ObservableCollection<Window>();
        #endregion

        #region Statics

        #endregion

        #region Properties

        #endregion

        #region Constructors
        public MpAvNotificationPositioner() {
            _windows.CollectionChanged += _ntfWindows_CollectionChanged;
            MpAvNotificationWindowManager.Instance.OnNotificationWindowIsVisibleChanged += Instance_OnNotificationWindowIsVisibleChanged;
        }


        #endregion

        #region Public Methods

        #endregion

        #region Private Methods
        private void Instance_OnNotificationWindowIsVisibleChanged(object sender, Window w) {
            if (w.IsVisible) {
                if (!_windows.Contains(w)) {
                    _windows.Add(w);
                    w.EffectiveViewportChanged += W_EffectiveViewportChanged; 
                    w.GetObservable(Window.BoundsProperty).Subscribe(value => OnNotificationWindowBoundsChangedHandler(w));

                }
            } else {
                _windows.Remove(w);
            }
        }


        private void _ntfWindows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            UpdateWindowPositions();
            //MpAvMainWindowViewModel.Instance.IsAnyDialogOpen = _windows.Count > 0;
        }

        private void W_EffectiveViewportChanged(object sender, global::Avalonia.Layout.EffectiveViewportChangedEventArgs e) {
            UpdateWindowPositions();
        }
        private void OnNotificationWindowBoundsChangedHandler(Window w) {
            if (!_windows.Contains(w)) {
                _windows.Add(w);
            }
            UpdateWindowPositions();
        }
        private void UpdateWindowPositions() {
            _windows.ForEach(x => PositionWindowByNotificationType(x));
        }

        private void PositionWindowByNotificationType(Window w) {
            var nvmb = w.DataContext as MpNotificationViewModelBase;
            if (nvmb == null) {
                return;
            }

            MpNotificationPlacementType placement = nvmb.PlacementType;
            switch (placement) {
                case MpNotificationPlacementType.SystemTray:
                    PositionWindowToSystemTray(w);
                    return;
                case MpNotificationPlacementType.ModalAnchor:
                    PositionWindowToAnchor(w,nvmb.AnchorTarget);
                    return;
            }
        }

        private void PositionWindowToAnchor(Window w, object anchor) {
            w.Position = FindAnchorPoint(w, anchor);
        }

        private PixelPoint FindAnchorPoint(Window w, object anchor) {
            var s = SetupSize(w);
            MpRect anchor_rect = null;
            double anchor_pd = w.VisualPixelDensity();
            if (anchor == null) {
                // find active screen center
                var primaryScreen = new MpAvScreenInfoCollection().Screens.FirstOrDefault(x => x.IsPrimary);
                if (primaryScreen == null) {
                    // happens before loader attached
                    return new PixelPoint();
                }
                anchor_rect = primaryScreen.WorkArea;
                anchor_pd = primaryScreen.PixelDensity;
            }

            if(anchor is Control ac) {
                anchor_rect = ac.Bounds.ToPortableRect(ac.Parent == null ? null : ac.Parent as Control, true);
                anchor_pd = ac.VisualPixelDensity();
            } else if(anchor is MpRect ar) {
                anchor_rect = ar;
            }

            if(anchor_rect == null) {
                // anchor error
                Debugger.Break();
                return new PixelPoint();
            }
            MpPoint anchor_centroid = anchor_rect.Centroid();

            double window_hw = s.Width / 2;
            double window_hh = s.Height / 2;

            MpPoint window_position = new MpPoint(anchor_centroid.X - window_hw, anchor_centroid.Y - window_hh);
            return window_position.ToAvPixelPoint(anchor_pd);
        }

        private void PositionWindowToSystemTray(Window w) {
            // TODO this should somehow know where system tray is on device, it just assumes its bottom right (windows)
            var s = SetupSize(w);


            var primaryScreen = new MpAvScreenInfoCollection().Screens.FirstOrDefault(x => x.IsPrimary);
            if (primaryScreen == null) {
                // happens before loader attached
                return;
            }
            double pad = 10;

            double x = primaryScreen.WorkArea.Right - s.Width - pad;
            double offsetY = _windows.Where(x => _windows.IndexOf(x) < _windows.IndexOf(w)).Sum(x => x.Bounds.Height + pad);
            offsetY += s.Height + pad;
            double y = primaryScreen.WorkArea.Bottom - offsetY;

            //if(OperatingSystem.IsWindows()) 
            {
                x *= primaryScreen.PixelDensity;
                y *= primaryScreen.PixelDensity;
            }

            // when y is less than 0 i think it screws up measuring mw dimensions so its a baby
            y = Math.Max(0, y);

            w.Position = new PixelPoint((int)x, (int)y);
            //MpConsole.WriteLine($"Notification Idx {_windows.IndexOf(this)} density {primaryScreen.PixelDensity} x {this.Position.X} y {this.Position.Y}  width {s.Width} height {s.Height}");
        }

        private MpSize SetupSize(Window w) {
            double width = w.Width.IsNumber() && w.Bounds.Width != 0 ? w.Bounds.Width : 350;
            double height = w.Bounds.Height.IsNumber() && w.Bounds.Height != 0 ? w.Bounds.Height : 150;
            return new MpSize(width, height);
        }

        #endregion

        #region Commands
        #endregion
    }
}
