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
    public partial class MpTemplateHyperlink : UserControl {
        //public static Hyperlink CreateTemplateHyperlink(MpTemplateHyperlinkViewModel thlvm, TextRange tr) {
        //    var thlb = new MpTemplateHyperlink(thlvm);
        //    //var container = new InlineUIContainer(thlb);
        //    //tr.Text = string.Empty;
        //    var hl = new Hyperlink(tr.Start, tr.End);
        //    hl.DataContext = thlvm;
        //    hl.Inlines.Clear();
        //    var tb = (TextBlock)thlb.FindName("TemplateTextBlock");
        //    hl.Inlines.Add(tb);
        //    return hl;
        //}
        public MpTemplateHyperlink(MpTemplateHyperlinkViewModel thlvm) {
            DataContext = thlvm;
            InitializeComponent();
        }
    }
}
