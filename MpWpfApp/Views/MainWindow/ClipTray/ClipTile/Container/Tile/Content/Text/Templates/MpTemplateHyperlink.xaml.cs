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
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpTemplateHyperlink.xaml
    /// </summary>
    public partial class MpTemplateHyperlink : Hyperlink {
        public static async Task<MpTemplateHyperlink> Create(TextRange tr, MpTextToken cit) {
            //if the range for the template contains a sub-selection of a hyperlink the hyperlink(s)
            //needs to be broken into their text before the template hyperlink can be created
            var trSHl = tr.Start.Parent.FindParentOfType<Hyperlink>();
            var trEHl = tr.End.Parent.FindParentOfType<Hyperlink>();
            var trText = tr.Text;

            if (trSHl != null) {
                var linkText = new TextRange(trSHl.ElementStart, trSHl.ElementEnd).Text;
                trSHl.Inlines.Clear();
                var span = new Span(new Run(linkText), trSHl.ElementStart);
                tr = MpHelpers.FindStringRangeFromPosition(span.ContentStart, trText, true);
                MpConsole.WriteLine("Splitting range");
            }
            if (trEHl != null && trEHl != trSHl) {
                var linkText = new TextRange(trEHl.ElementStart, trEHl.ElementEnd).Text;
                trEHl.Inlines.Clear();
                var span = new Span(new Run(linkText), trEHl.ElementStart);
                tr = MpHelpers.FindStringRangeFromPosition(span.ContentStart, trText, true);
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
                cit = await MpTextToken.Create(
                            thcvm.Parent.CopyItemId,
                            templateName);
                await cit.WriteToDatabaseAsync();
            }

            tr.Text = string.Empty;

            MpTemplateViewModel thlvm = thcvm.CreateTemplateViewModel(cit);

            var nthl = new MpTemplateHyperlink(tr, thlvm);
            nthl.Tag = Application.Current.Resources["TemplateHyperlinkTag"] as string;
           //tb.GetVisualAncestor<MpRtbView>().TemplateViews.Add(nthl);
            return nthl;
        }

        public MpTemplateHyperlink() : base() {
            InitializeComponent();
        }

        public MpTemplateHyperlink(TextRange tr, MpTemplateViewModel thlvm) : base(tr.Start, tr.End) {
            DataContext = thlvm;
            InitializeComponent();
        }

        private void Hyperlink_Loaded(object sender, RoutedEventArgs e) {
            Tag = Application.Current.Resources["TemplateHyperlinkTag"] as string;
            var rtbv = ElementStart.Parent.FindParentOfType<MpRtbView>();
            rtbv.TemplateViews.Add(this);

            var thlvm = DataContext as MpTemplateViewModel;
            //MpConsole.WriteLine($"template {thlvm.TemplateName} loaded from: " + sender.GetType().ToString());
        }

        private void Hyperlink_Unloaded(object sender, RoutedEventArgs e) {
            var rtbv = ElementStart.Parent.FindParentOfType<MpRtbView>();
            if(rtbv != null) {
                rtbv.TemplateViews.Remove(this);
            }

            //var thlvm = DataContext as MpTemplateViewModel;
            //bool wasMovedToDiffTile = thlvm.HostClipTileViewModel.IsPlaceholder;
            //MpConsole.WriteLine("Ignoring unload");
            //return;
            //if (Tag != null && !wasMovedToDiffTile) {
            //    MpConsole.WriteLine($"UNLOAD-DELETING template {thlvm.TemplateName} from item {thlvm.Parent.Parent.CopyItemTitle}");
            //    //Tag is null when formatting is cleared, it is non-null when the instance should be removed
            //    var rtb = ElementStart.Parent.FindParentOfType<RichTextBox>();
            //    if (rtb != null) {
            //        var rtbv = rtb.FindParentOfType<MpRtbView>();
            //        if (rtbv != null) {
            //            if (rtbv.TemplateViews.Contains(this)) {
            //                rtbv.TemplateViews.Remove(this);
            //            }
            //        }
            //    }
            //    thlvm.Parent.RemoveItem(thlvm.TextToken, false);
            //} else {
            //    MpConsole.WriteLine($"UNLOAD-CLEARING template {thlvm.TemplateName} from item {thlvm.Parent.Parent.CopyItemTitle}");
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
            
            if (!thlvm.Parent.Parent.IsSelected) {
                //only works with preview, clicking unexpanded item template doesn't select item/tile
                //without doing this
                thlvm.Parent.Parent.IsSelected = true;
            }
            if (!thlvm.HostClipTileViewModel.IsReadOnly) {
                if(thlvm.Parent.Parent.IsPastingTemplate) {
                    thlvm.IsSelected = true;
                } else {
                    EditTemplate();
                }
            }            
        }

        public void EditTemplate() {
            var rtb = ElementStart.Parent.FindParentOfType<RichTextBox>();
            var thlvm = DataContext as MpTemplateViewModel;
            if(thlvm.Parent.Parent.IsPastingTemplate) {
                return;
            }
            var rtbv = rtb.FindParentOfType<MpRtbView>();
            rtbv.LastEditedHyperlink = this;

            thlvm.EditTemplateCommand.Execute(null);
        }

        public void Clear() {
            var thlvm = DataContext as MpTemplateViewModel;
            //MpConsole.WriteLine($"CLEARING template {thlvm.TemplateName} from item {thlvm.Parent.Parent.CopyItemTitle}");
            //flag Tag so unloaded doesn't delete
            Tag = null;
            string text = thlvm.Parent.Parent.IsPastingTemplate ? thlvm.TextToken.TemplateToken:thlvm.TemplateDisplayValue;
            Inlines.Clear();
            new Span(new Run(text), ElementStart);
        }

        public void Delete() {
            var thlvm = DataContext as MpTemplateViewModel;
            MpConsole.WriteLine($"DELETING template {thlvm.TemplateName} from item {thlvm.Parent.Parent.CopyItemTitle}");
            //ensure Tag is non null so deleted in unload
            Tag = Application.Current.Resources["TemplateHyperlinkTag"] as string;
            Inlines.Clear();
            new Span(new Run(string.Empty), ElementStart);

            //var rtb = ElementStart.Parent.FindParentOfType<RichTextBox>();
            //var rtbv = rtb.FindParentOfType<MpRtbView>();
            //rtbv.TemplateViews.Remove(this);
            //thlvm.Parent.RemoveItem(thlvm.TextToken, false);
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
