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

        public static MpTemplateHyperlink Create(TextRange tr,MpCopyItemTemplate cit) {
            var startPointer = tr.Start;
            string origText = tr.Text;

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

            var rtb = tr.Start.Parent.FindParentOfType<RichTextBox>();
            var rtbvm = rtb.DataContext as MpRtbItemViewModel;
            var thcvm = rtbvm.TemplateHyperlinkCollectionViewModel;

            MpTemplateHyperlinkViewModel thlvm = thcvm.CreateTemplateHyperlinkViewModel(cit);
            thlvm.CopyItemTemplate.WriteToDatabase();

            var nthl = new MpTemplateHyperlink(tr, thlvm);
            nthl.NewStartPointer = startPointer;
            nthl.NewOriginalText = origText;

            var dupCheck = thcvm.Templates.Where(x => x.CopyItemTemplateId == cit.Id).FirstOrDefault();
            if(dupCheck == null) {
                thcvm.Templates.Add(thlvm);
            }
            return nthl;
        }

        public MpTemplateHyperlink() {
            InitializeComponent();
        }

        public MpTemplateHyperlink(TextRange tr, MpTemplateHyperlinkViewModel thlvm) : base(tr.Start,tr.End) {
            InitializeComponent();
            DataContext = thlvm;
        }
        
        public void DeleteHyperlink(bool fromContextMenu) {
            var rtb = this.ElementStart.Parent.FindParentOfType<RichTextBox>();

            if (fromContextMenu) {
                rtb.Selection.Select(ElementStart, ElementEnd);
                rtb.Selection.Text = string.Empty;
            }
            var thlvm = DataContext as MpTemplateHyperlinkViewModel;
            var thlcvm = thlvm.HostTemplateCollectionViewModel;
            //remove this individual token reference
            if (thlcvm != null) {
                thlcvm.Templates.Remove(thlvm);
            }

            if(!string.IsNullOrEmpty(NewOriginalText)) {
                rtb.Selection.Select(ElementStart, ElementEnd);
                rtb.Selection.Text = string.Empty;

                rtb.Selection.Select(NewStartPointer, NewStartPointer);
                rtb.Selection.Text = NewOriginalText;
            }
        }

        private void Hyperlink_Unloaded(object sender, RoutedEventArgs e) {
            DeleteHyperlink(false);
        }

        private void Hyperlink_MouseEnter(object sender, MouseEventArgs e) {
            var thlvm = DataContext as MpTemplateHyperlinkViewModel;
            thlvm.IsHovering = true;
        }

        private void Hyperlink_MouseLeave(object sender, MouseEventArgs e) {
            var thlvm = DataContext as MpTemplateHyperlinkViewModel;
            thlvm.IsHovering = false;
        }

        private void Hyperlink_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var thlvm = DataContext as MpTemplateHyperlinkViewModel;
            thlvm.IsSelected = true;
        }
    }
}
