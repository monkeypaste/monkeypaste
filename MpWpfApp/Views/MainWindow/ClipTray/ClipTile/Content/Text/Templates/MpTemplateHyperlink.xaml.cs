using GongSolutions.Wpf.DragDrop.Utilities;
using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpTemplateHyperlink.xaml
    /// </summary>
    public partial class MpTemplateHyperlink : Hyperlink {
        public TextPointer NewStartPointer;
        public string NewOriginalText;

        public bool IsNew = false;

        public RichTextBox Rtb;

        public static MpTemplateHyperlink Create(TextRange tr,MpCopyItemTemplate cit) {
            //if the range for the template contains a sub-selection of a hyperlink the hyperlink(s)
            //needs to be broken into their text before the template hyperlink can be created
            var trSHl = tr.Start.Parent.FindParentOfType<Hyperlink>();
            var trEHl = tr.End.Parent.FindParentOfType<Hyperlink>();
            var trText = tr.Text;

            if (trSHl != null) {
                var linkText = new TextRange(trSHl.ElementStart, trSHl.ElementEnd).Text;
                trSHl.Inlines.Clear();
                var span = new Span(new Run(linkText), trSHl.ElementStart);
                tr = MpHelpers.Instance.FindStringRangeFromPosition(span.ContentStart, trText, true);
            }
            if (trEHl != null && trEHl != trSHl) {
                var linkText = new TextRange(trEHl.ElementStart, trEHl.ElementEnd).Text;
                trEHl.Inlines.Clear();
                var span = new Span(new Run(linkText), trEHl.ElementStart);
                tr = MpHelpers.Instance.FindStringRangeFromPosition(span.ContentStart, trText, true);
            }

            var startPointer = tr.Start;
            string origText = tr.Text;

            tr.Text = string.Empty;

            var rtb = tr.Start.Parent.FindParentOfType<RichTextBox>();
            var rtbvm = rtb.DataContext as MpContentItemViewModel;
            var thcvm = rtbvm.TemplateCollection;

            bool newCit = cit == null;

            MpTemplateViewModel thlvm = thcvm.AddItem(cit);

            var nthl = new MpTemplateHyperlink(tr, thlvm);

            if(newCit) {
                nthl.IsNew = true;
                nthl.NewStartPointer = startPointer;
                nthl.NewOriginalText = origText;

                var ettbv = rtb.GetVisualAncestor<MpContentListView>().GetVisualDescendent<MpEditTemplateToolbarView>();
                ettbv.SetActiveRtb(rtb);

                thlvm.IsSelected = true;
            }
            nthl.Rtb = rtb;
            return nthl;
        }

        public MpTemplateHyperlink() {
            InitializeComponent();
        }

        public MpTemplateHyperlink(TextRange tr, MpTemplateViewModel thlvm) : base(tr.Start,tr.End) {
            DataContext = thlvm;

            var rtbv = tr.Start.Parent.FindParentOfType<MpRtbView>();
            rtbv.AddTemplate(thlvm, this);
            InitializeComponent();

        }       
        
        private void Hyperlink_Unloaded(object sender, RoutedEventArgs e) {
            //var thlvm = DataContext as MpTemplateViewModel;
            //if (thlvm != null) {
            //    var thlcvm = thlvm.Parent;
            //    if (thlcvm != null) {
            //        thlcvm.RemoveItem(thlvm.CopyItemTemplate, false);
            //    }
            //}
        }

        private void Hyperlink_MouseEnter(object sender, MouseEventArgs e) {
            var thlvm = DataContext as MpTemplateViewModel;
            thlvm.IsHovering = true;
        }

        private void Hyperlink_MouseLeave(object sender, MouseEventArgs e) {
            var thlvm = DataContext as MpTemplateViewModel;
            thlvm.IsHovering = false;
        }

        private void Hyperlink_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {            
            var thlvm = DataContext as MpTemplateViewModel;
            thlvm.IsSelected = true;

            var rtb = this.FindParentOfType<RichTextBox>();
            var ettbv = rtb.GetVisualAncestor<MpContentListView>().GetVisualDescendent<MpEditTemplateToolbarView>();
            ettbv.DataContext = thlvm;
            ettbv.ShowToolbar();
        }
    }
}
