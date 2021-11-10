using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpAppModeButtonGridView.xaml
    /// </summary>
    public partial class MpAppModeButtonGridView : MpUserControl<MpAppModeViewModel> {
        public MpAppModeButtonGridView() {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            var toggleButtonList = this.GetVisualDescendents<ToggleButton>();
            foreach (var tb in toggleButtonList) {
                tb.MouseEnter += Tb_MouseEnter;
            }
        }

        private void Tb_MouseEnter(object sender, MouseEventArgs e) {
            BindingContext.OnPropertyChanged(nameof(BindingContext.IsAppendModeTooltip));
            BindingContext.OnPropertyChanged(nameof(BindingContext.IsAppPausedTooltip));
            BindingContext.OnPropertyChanged(nameof(BindingContext.IsRighClickPasteModeTooltip));
            BindingContext.OnPropertyChanged(nameof(BindingContext.IsAutoCopyModeTooltip));
            BindingContext.OnPropertyChanged(nameof(BindingContext.IsAutoAnalysisModeTooltip));
        }
    }
}
