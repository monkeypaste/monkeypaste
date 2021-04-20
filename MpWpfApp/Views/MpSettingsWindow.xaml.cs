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
using System.Windows.Shapes;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpSettingsWindow.xaml
    /// </summary>
    public partial class MpSettingsWindow : Window {
        public MpSettingsWindow() {
            InitializeComponent();
            DataContext = new MpSettingsWindowViewModel();
        }
        public MpSettingsWindow(int tabToShow, object args = null) {
            InitializeComponent();
            DataContext = new MpSettingsWindowViewModel(tabToShow,args);
        }
    }
}
