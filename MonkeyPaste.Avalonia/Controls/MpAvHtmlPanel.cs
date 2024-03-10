using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;
using HtmlAgilityPack;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Avalonia;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvHtmlPanel : HtmlPanel {
        private Point[] _scrollOffsets = [];

        public static readonly AvaloniaProperty CanScrollXProperty =
           AvaloniaProperty.Register<MpAvHtmlPanel, bool>(nameof(CanScrollX), defaultValue: false);

        public static readonly AvaloniaProperty CanScrollYProperty =
           AvaloniaProperty.Register<MpAvHtmlPanel, bool>(nameof(CanScrollY), defaultValue: false);

        public bool CanScrollX {
            get { return (bool)GetValue(CanScrollXProperty); }
            private set { SetValue(CanScrollXProperty, value); }
        }
        public bool CanScrollY {
            get { return (bool)GetValue(CanScrollYProperty); }
            private set { SetValue(CanScrollYProperty, value); }
        }

        public Point ScrollOffset =>
            new Point(_horizontalScrollBar.Value, _verticalScrollBar.Value);

        public void ScrollToHome(double step = 0) {
            ScrollToOffset(new(), step);
        }

        public void ScrollToElement(string elementId, double step = 0) {
            if (_htmlContainer == null) {
                return;
            }
            var rect = _htmlContainer.GetElementRectangle(elementId);
            if (rect.HasValue) {
                Point new_offset = new Point(_horizontalScrollBar.Value, rect.Value.Y);
                ScrollToOffset(new_offset, step);
            }
        }
        public void ScrollToOffset(Point offset, double step = 0) {
            if (_htmlContainer == null) {
                return;
            }
            if (step == 0) {
                SetScrollOffset(offset.X, offset.Y);
                return;
            }
            Dispatcher.UIThread.Post(async () => {
                var cur_offset = new Point(_horizontalScrollBar.Value, _verticalScrollBar.Value);
                var diff = offset - cur_offset;
                double x_dir = diff.X == 0 ? 0 : diff.X > 0 ? 1 : -1;
                double y_dir = diff.Y == 0 ? 0 : diff.Y > 0 ? 1 : -1;
                var vel = new Point(step * x_dir, step * y_dir);
                while (true) {
                    var cur_diff = offset - cur_offset;
                    bool is_done = Math.Abs(cur_diff.X) < 1 && Math.Abs(cur_diff.Y) < 1;
                    if (is_done) {
                        cur_offset = offset;
                    } else {
                        cur_offset += vel;
                    }
                    SetScrollOffset(cur_offset.X, cur_offset.Y);
                    if (is_done) {
                        return;
                    }
                    await Task.Delay(20);
                }
            });
        }
        private void SetScrollOffset(double x, double y) {
            _horizontalScrollBar.Value = x;
            _verticalScrollBar.Value = y;
            UpdateScroll();
        }

        private void UpdateScroll() {
            var newScrollOffset = new Point(-_horizontalScrollBar.Value, -_verticalScrollBar.Value);
            if (!newScrollOffset.Equals(_htmlContainer.ScrollOffset)) {
                _htmlContainer.ScrollOffset = newScrollOffset;
                InvalidateVisual();

            }
        }
        private void UpdateScrollbars() {
            _verticalScrollBar.IsVisible = IsSelectionEnabled && _verticalScrollBar.Visibility == ScrollBarVisibility.Visible;
            _horizontalScrollBar.IsVisible = IsSelectionEnabled && _horizontalScrollBar.Visibility == ScrollBarVisibility.Visible;
            CanScrollX = _horizontalScrollBar.IsVisible;
            CanScrollY = _verticalScrollBar.IsVisible;
            this.Redraw();
        }

        protected override Size MeasureOverride(Size constraint) {
            var result = base.MeasureOverride(constraint);
            var vvis = _verticalScrollBar.IsVisible ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
            var hvis = _horizontalScrollBar.IsVisible ? ScrollBarVisibility.Visible : ScrollBarVisibility.Hidden;
            bool relayout = vvis != _verticalScrollBar.Visibility || hvis != _horizontalScrollBar.Visibility;

            if (relayout) {
                PerformHtmlLayout(constraint);
            }
            UpdateScrollbars();
            return result;
        }
        protected override void OnPointerPressed(PointerPressedEventArgs e) {
            base.OnPointerPressed(e);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs e) {
            try {
                base.OnPropertyChanged(e);
            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"HtmlPanel prop change error. ", ex);
            }

            // On text property change reset the scrollbars to zero.
            if (e.Property == IsSelectionEnabledProperty) {
                UpdateScrollbars();
            }
        }
    }
}
