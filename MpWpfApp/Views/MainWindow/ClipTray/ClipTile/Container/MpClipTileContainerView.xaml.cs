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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Windows.Storage;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpClipTileContainerView.xaml
    /// </summary>
    public partial class MpClipTileContainerView : MpUserControl<MpClipTileViewModel> {
        private static int tileCount = 0;
        public int _TileId = -1;
        public MpClipTileContainerView() {
            if(_TileId < 0) {
                _TileId = tileCount++;
            }
            InitializeComponent();
        }
    }
}
