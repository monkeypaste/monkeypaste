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

            var rtb = tr.Start.Parent.FindParentOfType<RichTextBox>();
            var rtbvm = rtb.DataContext as MpContentItemViewModel;
            var thcvm = rtbvm.TemplateCollection;

            string origText = tr.Text;
            bool newCit = cit == null;

            if(cit == null) {
                //for new templates create a default name
                cit = MpCopyItemTemplate.Create(
                            thcvm.Parent.CopyItem.Id,
                            string.IsNullOrWhiteSpace(origText) ? thcvm.GetUniqueTemplateName():thcvm.GetFormattedTemplateName(origText));
                cit.WriteToDatabase();
            }

            tr.Text = string.Empty;

            MpTemplateViewModel thlvm = thcvm.AddItem(cit);

            var nthl = new MpTemplateHyperlink(tr, thlvm);

            nthl.Rtb = rtb;

            rtb.GetVisualAncestor<MpRtbView>().TemplateViews.Add(nthl);
            return nthl;
        }

        public MpTemplateHyperlink() {
            InitializeComponent();
        }

        public MpTemplateHyperlink(TextRange tr, MpTemplateViewModel thlvm) : base(tr.Start,tr.End) {
            DataContext = thlvm;

            InitializeComponent();
        }

        private void Hyperlink_Loaded(object sender, RoutedEventArgs e) {
            var thl = sender as MpTemplateHyperlink;
            var thlvm = DataContext as MpTemplateViewModel;
            MpConsole.WriteLine($"template {thlvm.TemplateName} loaded from: " + sender.GetType().ToString());
            Rtb = thl.FindParentOfType<RichTextBox>();
            var rtbv = Rtb.GetVisualAncestor<MpRtbView>();
            if(!rtbv.TemplateViews.Contains(this)) {
                rtbv.TemplateViews.Add(this);
            }
        }

        private void Hyperlink_Unloaded(object sender, RoutedEventArgs e) {
            var thl = sender as MpTemplateHyperlink;
            
            if(thl.Tag != null) {
                var thlvm = DataContext as MpTemplateViewModel;
                if (thlvm != null) {
                    var thlcvm = thlvm.Parent;
                    if (thlcvm != null) {
                        thlcvm.RemoveItem(thlvm.CopyItemTemplate, false);
                        var rtbv = Rtb.FindParentOfType<MpRtbView>();
                        rtbv.TemplateViews.Remove(this);
                        rtbv.SyncModels();
                        MpConsole.WriteLine($"template {thlvm.TemplateName} REMOVED from: " + sender.GetType().ToString());
                    }
                }
            }
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
            if(thlvm.Parent.Parent.IsEditingContent) {
                EditTemplate();
            }
        }

        public void EditTemplate() {
            var rtbv = Rtb.FindParentOfType<MpRtbView>();
            rtbv.LastEditedHyperlink = this;
            var thlvm = DataContext as MpTemplateViewModel;

            thlvm.EditTemplateCommand.Execute(null);
        }

        private void DeleteAll_Click(object sender, RoutedEventArgs e) {
            var thlvm = DataContext as MpTemplateViewModel;
            var rtbv = Rtb.FindParentOfType<MpRtbView>();
            var hlToRemove = new List<MpTemplateHyperlink>();
            

            foreach(var hl in rtbv.TemplateViews) {
                if(hl.DataContext != thlvm) {
                    continue;
                }
                hlToRemove.Add(hl);
                hl.Inlines.Clear();
                new Span(new Run(string.Empty), hl.ElementStart);
            }
            foreach(var hl2r in hlToRemove) {
                rtbv.TemplateViews.Remove(hl2r);
            }
            Rtb.UpdateLayout();
            rtbv.SyncModels();
        }
    }
}
