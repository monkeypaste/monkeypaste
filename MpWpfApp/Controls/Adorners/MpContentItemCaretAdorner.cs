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

        private Pen caretPen {
            get {
                return new Pen(Brushes.Red, 1.5); ;
            }
        }

        private Pen linePen {
            get {
                return new Pen(Brushes.DimGray, 0.5); ;
            }
        }
        #endregion

        #region Public Methods

        public MpContentItemCaretAdorner(UIElement uie) : base(uie) { }

        #endregion

        #region Overrides

        protected override void OnRender(DrawingContext drawingContext) {
            //foreach(var p in Test) {

            //    Visibility = Visibility.Visible;
            //    drawingContext.DrawRectangle(Brushes.Orange,pen, p);
            //}
            //return;
            var civm = (AdornedElement as FrameworkElement).DataContext as MpContentItemViewModel;
            IsShowing = MpRtbView.DropOverHomeItemId == civm.CopyItemId || MpRtbView.DropOverEndItemId == civm.CopyItemId;
            if (IsShowing) {
                Visibility = Visibility.Visible;
                drawingContext.DrawLine(caretPen, CaretLine[0], CaretLine[1]);
            } else {
                Visibility = Visibility.Hidden;
            }
        }

        #endregion
    }
}
