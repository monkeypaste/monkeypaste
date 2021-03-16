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
    public class MpRichTextBoxListBoxOverlayAdorner : Adorner {
        #region Private Variables
        private ListBox _rtblb;
        #endregion

        #region Public Methods
        public MpRichTextBoxListBoxOverlayAdorner(ListBox rtblb) : base(rtblb) {
            _rtblb = rtblb;
        }
        #endregion

        #region Overrides
        protected override void OnRender(DrawingContext drawingContext) {            
            var rtbvmc = (MpClipTileRichTextBoxViewModelCollection)_rtblb.DataContext;
            var adornedElementRect = new Rect(this.AdornedElement.DesiredSize);
            var blackPen = new Pen(Brushes.Gray, 1);
            blackPen.DashStyle = DashStyles.Dash;            

            double h = 2.5;
            bool wasFound = false;
            foreach(var rtbvm in rtbvmc) {
                if(rtbvm.Next == null || rtbvm.Rtbc == null) {
                    continue;
                }
                var lbi = (ListBoxItem)_rtblb.ItemContainerGenerator.ContainerFromItem(rtbvm);
                //var p = lbi.TranslatePoint(new Point(0.0, 0.0), _rtblb);
                var mp = MpHelpers.Instance.GetMousePosition(rtbvm.Rtbc);
                var rect = new Rect(rtbvm.Rtbc.DesiredSize);
                var bl = rect.BottomLeft;
                var br = rect.BottomRight;

                var lineRect = new Rect(bl.X, bl.Y - h, br.X - bl.X, h * 2);
                if (lineRect.Contains(mp)) {
                    rtbvmc.IsCursorOnItemInnerEdge = true;
                    wasFound = true;
                } 
            }
            if(!wasFound) {
                //rtbvmc.IsCursorOnItemInnerEdge = false;
            }
        }
        #endregion
    }
}
