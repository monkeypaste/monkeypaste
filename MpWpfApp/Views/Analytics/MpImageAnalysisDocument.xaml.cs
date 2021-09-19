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
    /// Interaction logic for MpImageDetectionDocument.xaml
    /// </summary>
    public partial class MpImageAnalysisDocument : UserControl {
        public MpImageAnalysisDocument() {
            InitializeComponent();
            DataContext = new MpImageAnalysisViewModel();
        }
        public MpImageAnalysisDocument(string analysisData) {
            InitializeComponent();
            DataContext = new MpImageAnalysisViewModel(null,analysisData);
        }
    }
}
