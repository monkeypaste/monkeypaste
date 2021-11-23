using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpScrollContentPresenter : ContentPresenter, IScrollInfo {
        private ScrollContentPresenter _scp;

        public MpScrollContentPresenter() {
            _scp = new ScrollContentPresenter();
        }

        #region IScrollInfo

        public void LineUp() {
            _scp.LineUp();
        }

        public void LineDown() {
            _scp.LineDown();
        }

        public void LineLeft() {
            _scp.LineLeft();
        }

        public void LineRight() {
            _scp.LineRight();
        }

        public void PageUp() {
            _scp.PageUp();
        }

        public void PageDown() {
            _scp.PageDown();
        }

        public void PageLeft() {
            _scp.PageLeft();
        }

        public void PageRight() {
            _scp.PageRight();
        }

        public void MouseWheelUp() {
            _scp.MouseWheelUp();
        }

        public void MouseWheelDown() {
            _scp.MouseWheelDown();
        }

        public void MouseWheelLeft() {
            _scp.MouseWheelLeft();
        }

        public void MouseWheelRight() {
            _scp.MouseWheelRight();
        }

        public void SetHorizontalOffset(double offset) {
            _scp.SetHorizontalOffset(offset);
        }

        public void SetVerticalOffset(double offset) {
            _scp.SetVerticalOffset(offset);
        }

        public Rect MakeVisible(Visual visual, Rect rectangle) {
            return _scp.MakeVisible(visual, rectangle);
        }

        public bool CanVerticallyScroll {
            get => _scp.CanVerticallyScroll;
            set => _scp.CanVerticallyScroll = value;
        }

        public bool CanHorizontallyScroll {
            get => _scp.CanHorizontallyScroll;
            set => _scp.CanHorizontallyScroll = value;
        }

        public double ExtentWidth  => _scp.ExtentWidth;

        public double ExtentHeight => _scp.ExtentHeight;

        public double ViewportWidth => _scp.ViewportWidth;

        public double ViewportHeight => _scp.ViewportHeight;

        public double HorizontalOffset => _scp.HorizontalOffset;

        public double VerticalOffset => _scp.VerticalOffset;

        #endregion

        public bool CanContentScroll {
            get { return (bool)GetValue(CanContentScrollProperty); }
            set { SetValue(CanContentScrollProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanContentScroll.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanContentScrollProperty =
            DependencyProperty.Register("CanContentScroll", typeof(bool), typeof(MpScrollContentPresenter), new PropertyMetadata(false));

        public AdornerLayer AdornerLayer => _scp.AdornerLayer;

        public ScrollViewer ScrollOwner {
            get => _scp.ScrollOwner;
            set => _scp.ScrollOwner = value;
        }

    }

    public class MpScrollViewer : ScrollViewer {
        private MpScrollContentPresenter _mpscp;

        protected new internal IScrollInfo ScrollInfo { 
            get {
                if(_mpscp == null) {
                    _mpscp = new MpScrollContentPresenter();
                    _mpscp.ScrollOwner = this;
                }
                return _mpscp;
            }
            set {
                base.ScrollInfo = value;
            }
        }

        public MpScrollViewer() : base() {
            MpConsole.WriteLine("YOYOYO custom scroll viewer in da hoooouse");
        }
    }
    public class MpListBox : ListBox {

    }
}
