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
    /// Interaction logic for MpShortcutItemView.xaml
    /// </summary>
    public partial class MpShortcutGestureView : MpUserControl<MpShortcutViewModel> {


        public bool IsModalView {
            get { return (bool)GetValue(IsModalViewProperty); }
            set { SetValue(IsModalViewProperty, value); }
        }

        public static readonly DependencyProperty IsModalViewProperty =
            DependencyProperty.Register(
                "IsModalView", 
                typeof(bool), 
                typeof(MpShortcutGestureView), 
                new PropertyMetadata(true));


        public MpShortcutGestureView() {
            InitializeComponent();
        }
    }
}
