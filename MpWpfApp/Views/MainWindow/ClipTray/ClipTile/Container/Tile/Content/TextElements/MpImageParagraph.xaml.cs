using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpFileItemParagraph.xaml
    /// </summary>
    public partial class MpImageParagraph : Paragraph {
        public MpImageParagraph() : base() {
            InitializeComponent();
        }

        private void ContentImage_Loaded(object sender, RoutedEventArgs e) {
            var ctvm = DataContext as MpClipTileViewModel;

            ctvm.UnformattedContentSize =
                new Size(
                    (ContentImage.Source as BitmapSource).PixelWidth,
                    (ContentImage.Source as BitmapSource).PixelHeight);

        }
    }
}
