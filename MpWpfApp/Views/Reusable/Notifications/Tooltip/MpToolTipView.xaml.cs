using MonkeyPaste.Common.Wpf;
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
    /// Interaction logic for MpToolTipView.xaml
    /// </summary>
    public partial class MpToolTipView : UserControl {

        #region ToolTipText DependencyProperty

        public string ToolTipText {
            get { return (string)GetValue(ToolTipTextProperty); }
            set { SetValue(ToolTipTextProperty, value); }
        }

        public static readonly DependencyProperty ToolTipTextProperty =
             DependencyProperty.RegisterAttached(
                 "ToolTipText", 
                 typeof(string),
                 typeof(MpToolTipView),
                 new FrameworkPropertyMetadata() {
                     DefaultValue = string.Empty,
                     PropertyChangedCallback = (s,e) => {
                         if(s is MpToolTipView ttv) {
                             if(e.NewValue is string text && !string.IsNullOrEmpty(text)) {
                                 ttv.Visibility = Visibility.Visible;
                                 ttv.ToolTipTextBlock.Text = text;
                             } else {
                                 ttv.Visibility = Visibility.Collapsed;
                             }
                         }
                     }
                 });

        #endregion

        public MpToolTipView() {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e) {
            var tooltip = this.GetVisualAncestor<ToolTip>();
            tooltip.BorderThickness = new Thickness(0);
            tooltip.Background = Brushes.Transparent;
            if(tooltip.Parent is Popup popup) {
                popup.AllowsTransparency = true;
            }           
            
        }
    }
}
