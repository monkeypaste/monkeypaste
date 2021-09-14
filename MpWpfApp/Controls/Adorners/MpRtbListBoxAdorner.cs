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
    public class MpRtbListBoxAdorner : MpLineAdorner {
        #region Private Variables
        #endregion

        #region Public Methods
        public MpRtbListBoxAdorner(ListBox rtblb) : base(rtblb) {
        }
        #endregion

        #region Overrides
        protected override void OnRender(DrawingContext drawingContext) {    
            var rtbvmc = ((FrameworkElement)this.AdornedElement).DataContext as MpRtbItemCollectionViewModel;

            if (rtbvmc != null && rtbvmc.HostClipTileViewModel.IsClipDropping) {
                base.OnRender(drawingContext);
            }
        }
        #endregion
    }
}
