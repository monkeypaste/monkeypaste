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
    public class MpClipTrayAdorner : MpLineAdorner {

        #region Public Methods
        public MpClipTrayAdorner(ListBox lb) : base(lb) { }
        #endregion

        #region Overrides
        protected override void OnRender(DrawingContext drawingContext) {          
            var clipTrayVm = ((FrameworkElement)this.AdornedElement).DataContext as MpClipTrayViewModel;
            if (clipTrayVm != null && clipTrayVm.IsTrayDropping) {
                base.OnRender(drawingContext);
            }
        }
        #endregion
    }
}
