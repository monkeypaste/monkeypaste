using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Documents;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace MpWpfApp {
    public class MpRichTextBox : RichTextBox {
        public static MpRichTextBox DraggingRtb { get; private set; } = null;

        #region Overrides
        //public new MpEventEnabledFlowDocument Document { get; set; }

        public MpRichTextBox() : base() {
            Document = new MpEventEnabledFlowDocument();
            Document.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            //Document.IsOptimalParagraphEnabled = true;
        }

        protected override void OnPreviewQueryContinueDrag(QueryContinueDragEventArgs e) {
            base.OnPreviewQueryContinueDrag(e);
            DraggingRtb = this;
        }
        protected override void OnDragEnter(DragEventArgs e) {
            OnDragOver(e);
        }

        protected override void OnDragOver(DragEventArgs e) {
            this.GetVisualAncestor<MpRtbContentView>().ContentViewDropBehavior.OnDragOver(this, e);
        }

        protected override void OnDragLeave(DragEventArgs e) {
            this.GetVisualAncestor<MpRtbContentView>().ContentViewDropBehavior.OnDragLeave(this, e);
        }

        protected override void OnDrop(DragEventArgs e) {            
            this.GetVisualAncestor<MpRtbContentView>().ContentViewDropBehavior.OnDrop(this, e);
            DraggingRtb = null;
        }

        

        protected override void OnSelectionChanged(RoutedEventArgs e) {
            base.OnSelectionChanged(e);

            
        }

        public new TextSelection Selection {
            get {
                return base.Selection;
            }
        }
        #endregion
    }
}
