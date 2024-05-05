using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using MonkeyPaste.Common;
using PropertyChanged;
using System;
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

        public static readonly DirectProperty<MpAvHtmlPanel, MpPoint> ScrollOffsetProperty =
           AvaloniaProperty.RegisterDirect<MpAvHtmlPanel, MpPoint>(nameof(ScrollOffset), o => o.ScrollOffset, (x, o) => x.ScrollOffset = o);

        public bool CanScrollX {
            get { return (bool)GetValue(CanScrollXProperty); }
            private set { SetValue(CanScrollXProperty, value); }
        }
        public bool CanScrollY {
            get { return (bool)GetValue(CanScrollYProperty); }
            private set { SetValue(CanScrollYProperty, value); }
        }

        public MpPoint ScrollOffset {
            get => new MpPoint(_horizontalScrollBar.Value, _verticalScrollBar.Value);
            set {
                if (ScrollOffset.X != value.X || ScrollOffset.Y != value.Y) {
                    var old_val = ScrollOffset;
                    _horizontalScrollBar.Value = value.X;
                    _verticalScrollBar.Value = value.Y;
                    if (_htmlContainer != null) {
                        _htmlContainer.ScrollOffset = new Point(-value.X, -value.Y);
                        InvalidateVisual();
                    }
                    this.RaisePropertyChanged(ScrollOffsetProperty, old_val, value);
                }
            }
        }

        public async Task ScrollToHomeAsync(double t = 0) {
            await ScrollToOffsetAsync(new(), t);
        }

        public async Task ScrollToElementAsync(string elementId, double t = 0) {
            if (_htmlContainer == null ||
                _htmlContainer.GetElementRectangle(elementId) is not Rect rect) {
                return;
            }
            await BringIntoViewAsync(rect, t);
        }
        private async Task BringIntoViewAsync(Rect rect, double t_s = 0) {
            if (this.Bounds.Contains(rect)) {
                return;
            }
            MpPoint scroll_delta = this.Bounds.ToPortableRect().FindTranslation(rect.ToPortableRect());
            var new_offset = ScrollOffset + scroll_delta;
            await ScrollToOffsetAsync(new_offset, t_s);
        }
        private async Task ScrollToOffsetAsync(MpPoint offset, double t_s = 0) {
            if (_htmlContainer == null) {
                return;
            }
            var d = offset - ScrollOffset;
            double time_step = 20d / 1000d;
            var vel = t_s == 0 ? d : (d / t_s) * time_step;
            double dt = 0;
            while (true) {
                ScrollOffset += vel;
                if (dt >= t_s) {
                    return;
                }
                await Task.Delay(20);
                dt += time_step;
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
