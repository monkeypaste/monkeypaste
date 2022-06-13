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

namespace MpWpfApp {
    public class MpRichTextBox : RichTextBox {
        public static MpRichTextBox DraggingRtb { get; private set; } = null;


        #region Overrides
        //public new MpEventEnabledFlowDocument Document { get; set; }

        public MpRichTextBox() : base() {
            Document = new MpEventEnabledFlowDocument();
            //Document.LineStackingStrategy = LineStackingStrategy.BlockLineHeight;
            //Document.IsOptimalParagraphEnabled = true;

            CommandManager.RegisterClassCommandBinding(
                typeof(MpRichTextBox),
                new CommandBinding(
                    ApplicationCommands.Copy, 
                    OnCopy, 
                    OnCanExecuteClipboardCommand));

            CommandManager.RegisterClassCommandBinding(
                typeof(MpRichTextBox),
                new CommandBinding(
                    ApplicationCommands.Cut,
                    OnCut,
                    OnCanExecuteClipboardCommand));

            CommandManager.RegisterClassCommandBinding(
                typeof(MpRichTextBox),
                new CommandBinding(
                    ApplicationCommands.Paste,
                    OnPaste,
                    OnCanExecuteClipboardCommand));
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

        #endregion

        #region Application Commands

        private static void OnCanExecuteClipboardCommand(object target, CanExecuteRoutedEventArgs args) {
            MpRichTextBox rtb = (MpRichTextBox)target;
            args.CanExecute = rtb.IsEnabled;
        }

        private static void OnCopy(object sender, ExecutedRoutedEventArgs e) {
            MpRichTextBox rtb = (MpRichTextBox)sender;
            rtb.SetClipboardData(true);
            e.Handled = true;
        }

        private static void OnCut(object sender, ExecutedRoutedEventArgs e) {
            MpRichTextBox rtb = (MpRichTextBox)sender;
            rtb.SetClipboardData(false);
            e.Handled = true;
        }

        private static void OnPaste(object sender, ExecutedRoutedEventArgs e) {
            MpRichTextBox rtb = (MpRichTextBox)sender;

            rtb.Selection.Text = Clipboard.GetText();


            if(rtb.DataContext is MpClipTileViewModel ctvm) {
                MpContentDocumentRtfExtension.SaveTextContent(rtb)
                    .FireAndForgetSafeAsync(ctvm);
            }
            e.Handled = true;
        }

        #endregion

        protected void SetClipboardData(bool isCopy) {
            var ctvm = DataContext as MpClipTileViewModel;
            if(Selection.IsEmpty) {
                MpTextSelectionRangeExtension.SelectAll(ctvm);
            }
            string selectedText = MpContentDocumentRtfExtension.GetEncodedContent(this, false, true);

            MpPlatformWrapper.Services.ClipboardMonitor.IgnoreNextClipboardChangeEvent = true;
            Clipboard.SetText(selectedText);

            if (!isCopy) {
                MpContentDocumentRtfExtension.FinishContentCut(ctvm)
                    .FireAndForgetSafeAsync(ctvm);
            }
        }
    }
}
