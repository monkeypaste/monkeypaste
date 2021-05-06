using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MpWpfApp {
    public class MpRtbListBoxItemAdorner : Adorner {
        #region Private Variables
        #endregion

        #region Statics
        
        #endregion

        #region Public Methods
        public MpRtbListBoxItemAdorner(Canvas rtbc) : base(rtbc) {
        }
        #endregion

        #region Overrides
        protected override void OnRender(DrawingContext drawingContext) {            
            var rtbvm = (MpRtbListBoxItemRichTextBoxViewModel)(this.AdornedElement as FrameworkElement).DataContext;
            var adornedElementRect = new Rect(this.AdornedElement.DesiredSize);
            var blackPen = new Pen(Brushes.Gray, 1);
            blackPen.DashStyle = DashStyles.Dash;

            if (rtbvm.RichTextBoxViewModelCollection.IndexOf(rtbvm) < rtbvm.RichTextBoxViewModelCollection.VisibleSubRtbViewModels.Count - 1) {                
                drawingContext.DrawLine(blackPen, adornedElementRect.BottomLeft, adornedElementRect.BottomRight);
            }
        }
        #endregion
    }
}
