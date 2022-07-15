using Avalonia.Controls;
using PropertyChanged;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvScrollViewer : ScrollViewer {
        public new double HorizontalScrollBarValue {
            get { return GetValue(ScrollViewer.HorizontalScrollBarValueProperty); }
            set { SetValue(ScrollViewer.HorizontalScrollBarValueProperty, value); }
        }
        public new double VerticalScrollBarValue {
            get { return GetValue(ScrollViewer.VerticalScrollBarValueProperty); }
            set { SetValue(ScrollViewer.VerticalScrollBarValueProperty, value); }
        }

        public new double HorizontalScrollBarMaximum {
            get { return GetValue(ScrollViewer.HorizontalScrollBarMaximumProperty); }
            set { SetValue(ScrollViewer.HorizontalScrollBarMaximumProperty, value); }
        }
        public new double VerticalScrollBarMaximum {
            get { return GetValue(ScrollViewer.VerticalScrollBarMaximumProperty); }
            set { SetValue(ScrollViewer.VerticalScrollBarMaximumProperty, value); }
        }
    }
}
