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
using Windows.UI.Text;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpTextHyperlink.xaml
    /// </summary>
    public partial class MpTextHyperlink : Hyperlink {
        public static MpTextHyperlink Create(TextRange tr,  MpSubTextTokenType linkType) {            
            var hl = new MpTextHyperlink(tr);
            hl.Tag = linkType;
            var fe = tr.Start.Parent.FindParentOfType<FrameworkElement>();
            if (fe != null) {
                hl.DataContext = fe.DataContext as MpContentItemViewModel;
            }

            return hl;
        }

        public MpTextHyperlink() : base() {
            InitializeComponent();
        }

        public MpTextHyperlink(TextRange tr) : base(tr.Start, tr.End) {
            InitializeComponent();
        }

        private void Hyperlink_MouseEnter(object sender, MouseEventArgs e) {
            
        }

        private void Hyperlink_MouseLeave(object sender, MouseEventArgs e) {
            
        }

        private void Hyperlink_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
                     
        }

        public void Clear() {
            Tag = null;
            string text = new TextRange(ContentStart, ContentEnd).Text;
            Inlines.Clear();
            new Span(new Run(text), ElementStart);
        }

        private void ConvertToQr_Click(object sender, RoutedEventArgs e) {
            
        }
    }
}
