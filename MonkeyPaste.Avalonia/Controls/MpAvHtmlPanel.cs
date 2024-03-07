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

        public void ScrollToHome(double step = 0) {
            ScrollToOffset(new(), step);
        }
        public void ScrollToElement(string elementId, double step = 0) {
            if (_htmlContainer == null ||
                _htmlContainer.GetElementRectangle(elementId) is not { } rect) {
                return;
            }
            ScrollToOffset(rect.Position, step);
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
            _verticalScrollBar.IsVisible = IsSelectionEnabled;
            _horizontalScrollBar.IsVisible = IsSelectionEnabled;
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
            base.OnPropertyChanged(e);

            // On text property change reset the scrollbars to zero.
            if (e.Property == IsSelectionEnabledProperty) {
                UpdateScrollbars();
            }
        }
    }
}
