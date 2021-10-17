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
        public static MpTemplateHyperlink Create(TextRange tr, MpCopyItemTemplate cit) {
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
                MpConsole.WriteLine("Splitting range");
            }
            if (trEHl != null && trEHl != trSHl) {
                var linkText = new TextRange(trEHl.ElementStart, trEHl.ElementEnd).Text;
                trEHl.Inlines.Clear();
                var span = new Span(new Run(linkText), trEHl.ElementStart);
                tr = MpHelpers.Instance.FindStringRangeFromPosition(span.ContentStart, trText, true);
                MpConsole.WriteLine("Splitting range");
            }

            var rtb = tr.Start.Parent.FindParentOfType<RichTextBox>();
            var rtbvm = rtb.DataContext as MpContentItemViewModel;
            var thcvm = rtbvm.TemplateCollection;

            string templateName = tr.Text;

            if (cit == null) {
                //occurs when its a new template
                if (string.IsNullOrWhiteSpace(templateName)) {
                    templateName = thcvm.GetUniqueTemplateName();
                } 
                cit = MpCopyItemTemplate.Create(
                            thcvm.Parent.CopyItemId,
                            templateName);
                cit.WriteToDatabase();
            }

            tr.Text = string.Empty;

            MpTemplateViewModel thlvm = thcvm.CreateTemplateViewModel(cit);

            var nthl = new MpTemplateHyperlink(tr, thlvm);
            nthl.Tag = MpSubTextTokenType.TemplateSegment;
            rtb.GetVisualAncestor<MpRtbView>().TemplateViews.Add(nthl);
            return nthl;
        }

        public static Span ConvertToSpan(MpTemplateHyperlink thl) {
            if (thl.DataContext != null && thl.DataContext is MpTemplateViewModel thlvm) {
                //making Tag null lets unloaded event know not to remove this template instance
                string tokenText = thlvm.TemplateDisplayValue;
                thl.Tag = null;
                thl.Inlines.Clear();
                return new Span(new Run(tokenText), thl.ElementStart);
            }
            return null;
        }

        public MpTemplateHyperlink() : base() {
            InitializeComponent();
        }

        public MpTemplateHyperlink(TextRange tr, MpTemplateViewModel thlvm) : base(tr.Start, tr.End) {
            DataContext = thlvm;
            InitializeComponent();
        }

        private void Hyperlink_Loaded(object sender, RoutedEventArgs e) {
            var thl = sender as MpTemplateHyperlink;
            var thlvm = DataContext as MpTemplateViewModel;
            MpConsole.WriteLine($"template {thlvm.TemplateName} loaded from: " + sender.GetType().ToString());
        }

        private void Hyperlink_Unloaded(object sender, RoutedEventArgs e) {
            if (Tag != null) {
                //Tag is null when formatting is cleared, it is non-null when the instance should be removed
                var rtb = ElementStart.Parent.FindParentOfType<RichTextBox>();
                if(rtb != null) {
                    var rtbv = rtb.FindParentOfType<MpRtbView>();
                    if(rtbv != null) {
                        if(rtbv.TemplateViews.Contains(this)) {
                            rtbv.TemplateViews.Remove(this);
                        }
                    }
                }
                var thlvm = DataContext as MpTemplateViewModel;
                thlvm.Parent.RemoveItem(thlvm.CopyItemTemplate, false);
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
            if (thlvm.Parent.Parent.IsEditingContent) {
                EditTemplate();
            }
        }

        public void EditTemplate() {
            var rtb = ElementStart.Parent.FindParentOfType<RichTextBox>();
            var rtbv = rtb.FindParentOfType<MpRtbView>();
            rtbv.LastEditedHyperlink = this;
            var thlvm = DataContext as MpTemplateViewModel;

            thlvm.EditTemplateCommand.Execute(null);
        }

        public void Clear() {
            //flag Tag so unloaded doesn't delete
            Tag = null;
            var thlvm = DataContext as MpTemplateViewModel;
            string text = thlvm.TemplateDisplayValue;
            Inlines.Clear();
            new Span(new Run(text), ElementStart);
        }

        public void Delete() {
            //ensure Tag is non null so deleted in unload
            Tag = MpSubTextTokenType.TemplateSegment;
            Inlines.Clear();
            new Span(new Run(string.Empty), ElementStart);

            var rtb = ElementStart.Parent.FindParentOfType<RichTextBox>();
            var rtbv = rtb.FindParentOfType<MpRtbView>();
            rtbv.TemplateViews.Remove(this);
            var thlvm = DataContext as MpTemplateViewModel;
            thlvm.Parent.RemoveItem(thlvm.CopyItemTemplate, false);
        }

        private void DeleteAll_Click(object sender, RoutedEventArgs e) {
            var thlvm = DataContext as MpTemplateViewModel;
            var rtb = ElementStart.Parent.FindParentOfType<RichTextBox>();
            var rtbv = rtb.FindParentOfType<MpRtbView>();
            var hlToRemove = rtbv.TemplateViews.Where(x => x.DataContext == thlvm).ToList();

            foreach (var thl in hlToRemove) {
                thl.Delete();
            }
        }
    }
}
