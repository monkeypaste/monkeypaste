using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MpWpfApp {
    public class MpContentItemCaretAdorner : Adorner {
        #region Properties
        //public Line CaretLine { get; set; } = new Line();

        public MpLine CaretLine { get; set; } = new MpLine();

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

                drawingContext.DrawLine(unfocusPen, rtbv.HomeRect.TopLeft, rtbv.HomeRect.BottomLeft);
                drawingContext.DrawLine(unfocusPen, rtbv.EndRect.TopRight, rtbv.EndRect.BottomRight);
                if (isFocus) {
                    drawingContext.DrawLine(focusPen, CaretLine.P1, CaretLine.P2);
                }
            } else {
                Visibility = Visibility.Hidden;
            }
        }

        #endregion
    }
}
