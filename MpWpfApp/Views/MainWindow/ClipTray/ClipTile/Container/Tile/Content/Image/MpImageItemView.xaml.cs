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
    /// Interaction logic for MpImageItemView.xaml
    /// </summary>
    public partial class MpImageItemView : MpUserControl<MpContentItemViewModel> {
        public MpImageItemView() {
            InitializeComponent();
        }

        private void Viewbox_Loaded(object sender, RoutedEventArgs e) {
            //MpHelpers.Instance.RunOnMainThread(async () => {
            //    var test = await MpHelpers.Instance.DetectObjectsAsync(BindingContext.CopyItemData.ToBitmapSource().ToByteArray());
            //    return;
            //});
        }
    }
}
