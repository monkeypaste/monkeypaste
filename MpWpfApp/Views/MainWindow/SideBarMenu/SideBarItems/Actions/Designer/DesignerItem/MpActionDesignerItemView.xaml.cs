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
using MonkeyPaste;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpActionDesignerItemView.xaml
    /// </summary>
    public partial class MpActionDesignerItemView : MpUserControl<MpActionViewModelBase> {
        public MpActionDesignerItemView() {
            InitializeComponent();
        }

        private void ContentControl_PreviewKeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.Delete) {
                if (BindingContext.IsSelected) {
                    e.Handled = true;
                    BindingContext.DeleteThisActionCommand.Execute(null);
                }
            }
            
        }
    }
}
