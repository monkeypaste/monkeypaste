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
using System.Windows.Threading;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpActionDesignerView.xaml
    /// </summary>
    public partial class MpActionDesignerView : MpUserControl<MpActionCollectionViewModel> {
        public MpActionDesignerView() {
            InitializeComponent();
        }
    }
}
