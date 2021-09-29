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
using static System.Net.Mime.MediaTypeNames;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpClipTileDetailView.xaml
    /// </summary>
    public partial class MpClipTileDetailView : UserControl {
        private Hyperlink h;
        public MpClipTileDetailView() {
            InitializeComponent();
        }

        private void ClipTileDetailTextBlock_MouseEnter(object sender, MouseEventArgs e) {
            var civm = DataContext as MpContentItemViewModel;
            civm.CycleDetailCommand.Execute(null);

            if(Uri.IsWellFormedUriString(civm.DetailText, UriKind.Absolute)) {
                h = new Hyperlink();
                h.Inlines.Add(civm.DetailText);
                h.NavigateUri = new Uri(civm.DetailText);
                h.IsEnabled = true;
                h.Click += H_Click;
                ClipTileDetailTextBlock.Inlines.Clear();
                ClipTileDetailTextBlock.Inlines.Add(h);
            } else {
                ClipTileDetailTextBlock.Inlines.Clear();
                ClipTileDetailTextBlock.Inlines.Add(new Run(civm.DetailText));
            }
        }

        private void ClipTileDetailTextBlock_MouseLeave(object sender, MouseEventArgs e) {
            var civm = DataContext as MpContentItemViewModel;
            if(h != null) {
                h.Click -= H_Click;
                h = null;
            }
        }

        private void H_Click(object sender, RoutedEventArgs e) {
            MpHelpers.Instance.OpenUrl((sender as Hyperlink).NavigateUri.ToString());
        }
    }
}
