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
    /// Interaction logic for MpDesignerItemView.xaml
    /// </summary>
    public partial class MpDesignerItemView : MpUserControl<MpIDesignerItemViewModel> {

        public object DesignerContent { get; set; }

        public MpDesignerItemView() {
            InitializeComponent();
            DesignerContainer.Content = DesignerContent;
        }

    }
}
