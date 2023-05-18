using Avalonia;
using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace MonkeyPaste.Avalonia {
    public class MpAvNotificationPositioner {
        #region Private Variables

        private ObservableCollection<Window> _windows = new ObservableCollection<Window>();
        #endregion

        #region Statics

        public static PixelPoint GetSystemTrayWindowPosition(Window w, double pad = 10) {
            Size s = GetWindowSize(w);
            // NOTE this should account for mw show behavior (i think) show 'system tray' is BR of active monitor
            // TODO test when other window behaviors are implemented
            var primaryScreen = MpAvMainWindowViewModel.Instance.MainWindowScreen;
            if (primaryScreen == null) {
                // happens before loader attached
                return new PixelPoint();
            }

            double x = primaryScreen.WorkArea.Right - s.Width - pad;
            double y = primaryScreen.WorkArea.Bottom - s.Height - pad;
            x *= primaryScreen.Scaling;
            y *= primaryScreen.Scaling;

            // when y is less than 0 i think it screws up measuring mw dimensions so its a baby
            y = Math.Max(0, y);
            return new PixelPoint((int)x, (int)y);
        }
        #endregion

        #region Properties

        #endregion

        #region Constructors
        public MpAvNotificationPositioner() {
            MpMessenger.RegisterGlobal(ReceivedGlobalMessage);
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
        private void ReceivedGlobalMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.MainWindowOpening:
                    UpdateWindowPositions();
                    break;

            }
        }

        private void _ntfWindows_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            UpdateWindowPositions();
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
                    PositionWindowToAnchor(w, nvmb.AnchorTarget);
                    return;
            }
        }

        private void PositionWindowToAnchor(Window w, object anchor) {
            if (w.WindowStartupLocation == WindowStartupLocation.CenterOwner) {
                // ignore positioning
                return;
            }
            w.Position = FindAnchorPoint(w, null);
        }

        private PixelPoint FindAnchorPoint(Window w, object anchor) {
            var s = GetWindowSize(w);
            MpRect anchor_rect = null;
            double anchor_pd = w.VisualPixelDensity();
            if (anchor == null) {
                // find active screen center
                MpIPlatformScreenInfo primaryScreen = Mp.Services.ScreenInfoCollection.Screens.FirstOrDefault(x => x.IsPrimary);
                if (primaryScreen == null) {
                    // happens before loader attached
                    return new PixelPoint();
                }
                anchor_rect = primaryScreen.WorkArea;
                anchor_pd = primaryScreen.Scaling;
            }

            if (anchor is Control ac) {
                anchor_rect = ac.Bounds.ToPortableRect(ac.Parent == null ? null : ac.Parent as Control, true);
                anchor_pd = ac.VisualPixelDensity();
            } else if (anchor is MpRect ar) {
                anchor_rect = ar;
            }

            if (anchor_rect == null) {
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
            var primaryScreen = MpAvMainWindowViewModel.Instance.MainWindowScreen;
            if (primaryScreen == null) {
                // happens before loader attached
                return;
            }
            //var s = GetWindowSize(w);

            double pad = 10;

            var anchor_pos = GetSystemTrayWindowPosition(w, pad);
            int w_x = anchor_pos.X;
            int w_y = anchor_pos.Y;

            int offsetY = (int)
                _windows
                    .Where(x => _windows.IndexOf(x) < _windows.IndexOf(w))
                    .Sum(x => (GetWindowSize(x).Height + pad) * primaryScreen.Scaling);
            w_y -= offsetY;

            //if (w.DataContext is MpMessageNotificationViewModel mnw) {
            //    // for messages allow y stacking but carry x animation through
            //    if (mnw.OpenStartX == null) {
            //        // set msg keyframes
            //        mnw.OpenStartX = w.Screens.Primary.Bounds.Right + (w.Screens.Primary.Bounds.Right - w_x);
            //        mnw.OpenEndX = w_x;
            //        w_x = mnw.OpenStartX.Value;
            //        // trigger window anim
            //        mnw.OnPropertyChanged(nameof(mnw.IsOpenAnimated));
            //    } else {
            //        // carry animated x through
            //        w_x = w.Position.X;
            //    }
            //}
            w.Position = new PixelPoint(w_x, w_y);
        }

        private static Size GetWindowSize(Window w) {
            if (w.Width > 0 && w.Height > 0) {
                double th = GetWindowTitleHeight(w);
                return new Size(w.Width, w.Height + th);
            }
            double width = w.Bounds.Width.IsNumber() && w.Bounds.Width != 0 ? w.Bounds.Width : 350;
            double height = w.Bounds.Height.IsNumber() && w.Bounds.Height != 0 ? w.Bounds.Height : 150;
            if (w.DataContext is MpMessageNotificationViewModel mnvm) {
                width = mnvm.MessageWindowFixedWidth;
            }
            return new Size(width, height + GetWindowTitleHeight(w));
        }

        private static double GetWindowTitleHeight(Window w) {
            if (w == null) {
                return 0;
            }
            return w.FrameSize.HasValue ? w.FrameSize.Value.Height - w.ClientSize.Height : 0;
        }

        #endregion

        #region Commands
        #endregion
    }
}
