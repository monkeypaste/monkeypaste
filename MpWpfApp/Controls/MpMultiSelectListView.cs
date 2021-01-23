using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpMultiSelectListView : ListView {
        protected override DependencyObject GetContainerForItemOverride() {
            return new MpMultiSelectListViewItem(); 
        }

        class MpMultiSelectListViewItem : ListViewItem {
            private bool _deferSelection = false;

            protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
                if (e.ClickCount == 1 && IsSelected) {
                    // the user may start a drag by clicking into selected items
                    // delay destroying the selection to the Up event
                    _deferSelection = true;
                } else {
                    base.OnMouseLeftButtonDown(e);
                }
            }

            protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
                if (_deferSelection) {
                    try {
                        base.OnMouseLeftButtonDown(e);
                    } finally {
                        _deferSelection = false;
                    }
                }
                base.OnMouseLeftButtonUp(e);
            }

            protected override void OnMouseLeave(MouseEventArgs e) {
                // abort deferred Down
                _deferSelection = false;
                base.OnMouseLeave(e);
            }
        }

        public ScrollViewer ScrollViewer {
            get {
                Border border = (Border)VisualTreeHelper.GetChild(this, 0);

                return (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
            }
        }
    }
}
