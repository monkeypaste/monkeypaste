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
        public static MpTemplateHyperlink Create(TextRange tr, MpTextTemplate cit) {
            var rtb = tr.Start.Parent.FindParentOfType<RichTextBox>();
            var rtbvm = rtb.DataContext as MpContentItemViewModel;
            var thcvm = rtbvm.TemplateCollection;
            tr.Text = string.Empty;

            MpTemplateViewModel thlvm = thcvm.CreateTemplateViewModel(cit);

            var nthl = new MpTemplateHyperlink(tr, thlvm);
            //ensure tag is set so decode document doesn't override
            nthl.Tag = thlvm;
            return nthl;
        }

        public MpTemplateHyperlink() : base() {
            InitializeComponent();
        }

        public MpTemplateHyperlink(TextRange tr, MpTemplateViewModel thlvm) : base(tr.Start, tr.End) {
            DataContext = thlvm;
            InitializeComponent();
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
            if (!thlvm.HostClipTileViewModel.IsContentReadOnly) {
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
            string text = thlvm.Parent.Parent.IsPastingTemplate ? thlvm.TextToken.EncodedTemplate:thlvm.TemplateDisplayValue;
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
