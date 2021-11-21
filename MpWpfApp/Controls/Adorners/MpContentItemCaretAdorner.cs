using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpContentItemCaretAdorner : Adorner {
        #region Properties
        public Point[] CaretLine { get; set; } = new Point[2];

        public List<Rect> Test { get; set; } = new List<Rect>(2);

        public bool IsShowing { get; set; } = false;

        private Pen focusPen {
            get {
                return new Pen(Brushes.Red, 1.5);
            }
        }

        private Pen unfocusPen {
            get {
                return new Pen(Brushes.Blue, 1.5);
            }
        }
        #endregion

        #region Public Methods

        public MpContentItemCaretAdorner(UIElement uie) : base(uie) { }

        #endregion

        #region Overrides

        protected override void OnRender(DrawingContext drawingContext) {
            var civm = (AdornedElement as FrameworkElement).DataContext as MpContentItemViewModel;
            IsShowing = MpClipTrayViewModel.Instance.IsAnyTileItemDragging; 
            if (IsShowing) {
                var rtbv = AdornedElement.GetVisualAncestor<MpRtbView>();

                Visibility = Visibility.Visible;
                bool isFocus = MpRtbView.DropOverHomeItemId == civm.CopyItemId || MpRtbView.DropOverEndItemId == civm.CopyItemId;
                
                drawingContext.DrawLine(unfocusPen, rtbv.HomeCaretLine[0], rtbv.HomeCaretLine[1]);
                drawingContext.DrawLine(unfocusPen, rtbv.EndCaretLine[0], rtbv.EndCaretLine[1]);

                if(isFocus) {
                    drawingContext.DrawLine(focusPen, CaretLine[0], CaretLine[1]);
                }
            } else {
                Visibility = Visibility.Hidden;
            }
        }

        #endregion
    }
}
