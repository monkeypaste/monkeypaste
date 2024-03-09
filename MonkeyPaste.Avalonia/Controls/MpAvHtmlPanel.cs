using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
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
        public void ParseScrollOffsets(string offsetsStr) {
            _scrollOffsets =
                offsetsStr.ToStringOrEmpty()
                .SplitNoEmpty(" ")
                .Select(x => new MpPoint(x.SplitNoEmpty("|").Select(y => double.Parse(y)).ToArray()))
                .Select(x => new Point(x.X, x.Y))
                .ToArray();
            if (_scrollOffsets.Any()) {
                MpConsole.WriteLine($"Offsets for {DataContext}: ", true);
                _scrollOffsets.ForEach(x => MpConsole.WriteLine($"{x}", false, x == _scrollOffsets.Last()));
            }
        }

        public void SetHtml(string html) {
            this.SetCurrentValue(TextProperty, html.DecodeSpecialHtmlEntities());
        }

        public void ScrollToHome(double step = 0) {
            ScrollToOffset(new(), step);
        }
        public void ScrollToOffsetIdx(int offsetIdx, double step = 0) {
            // HACK HtmlPanel.ScrollToElement does NOT work, it always returns an empty rect
            if (_scrollOffsets == null ||
                offsetIdx < 0 ||
                offsetIdx >= _scrollOffsets.Length) {
                return;
            }
            ScrollToOffset(_scrollOffsets[offsetIdx], step);
        }

        public void ScrollToOffset(Point offset, double step = 0) {
            if (_htmlContainer == null) {
                return;
            }
            if (step == 0) {
                _htmlContainer.ScrollOffset = offset;
                return;
            }
            Dispatcher.UIThread.Post(async () => {
                var diff = offset - _htmlContainer.ScrollOffset;
                double x_dir = diff.X == 0 ? 0 : diff.X > 0 ? 1 : -1;
                double y_dir = diff.Y == 0 ? 0 : diff.Y > 0 ? 1 : -1;
                var vel = new Point(step * x_dir, step * y_dir);
                while (true) {
                    var cur_diff = offset - _htmlContainer.ScrollOffset;
                    if (Math.Abs(cur_diff.X) < 1 && Math.Abs(cur_diff.Y) < 1) {
                        _htmlContainer.ScrollOffset = offset;
                        return;
                    }
                    _htmlContainer.ScrollOffset += vel;
                    await Task.Delay(20);
                }
            });
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
