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
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpAddTemplateToolbarContextMenuView.xaml
    /// </summary>
    public partial class MpAddTableToolbarContextMenuView : MpUserControl<MpContentTableViewModel> {
        RichTextBox Rtb {
            get {
                var ctv = this.GetVisualAncestor<MpClipTileView>();
                if(ctv == null) {
                    return null;
                }
                return ctv.GetVisualDescendent<RichTextBox>();
            }
        }

        public MpAddTableToolbarContextMenuView() {
            InitializeComponent();
        }


        private void AddButton_Click(object sender, RoutedEventArgs e) {
            MpContextMenuView.Instance.DataContext = BindingContext.AddTableMenuItemViewModel;
                MpContextMenuView.Instance.PlacementTarget = this;
                MpContextMenuView.Instance.IsOpen = true;
        }

        private void AddButton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) {
            //if(AddButton.IsEnabled) {
            //    AddButton.Background = Brushes.Transparent;
            //} else {
            //    AddButton.Background = Brushes.DimGray;
            //}


            //if(AddButton.IsEnabled) {
            //    AddButtonImage.Source = (BitmapSource)new BitmapImage(new Uri(MpPreferences.AbsoluteResourcesPath + @"/Images/MpRichTextBox/templateadd.png"));
            //    AddButton.ToolTip = "Add Template";
            //} else {
            //    AddButtonImage.Source = (BitmapSource)new BitmapImage(new Uri(MpPreferences.AbsoluteResourcesPath + @"/Images/MpRichTextBox/templatedisabled.png"));
            //    var tb = new TextBlock() {
            //        Foreground = Brushes.Red,
            //        Background = Brushes.White,
            //        FontSize = 12,
            //        Text = @"-current selection intersects with a template" + Environment.NewLine +
            //                @"-contains a space" + Environment.NewLine +
            //                @"-contains more than 10 characters"
            //    };
            //    AddButton.ToolTip = tb;
            //}
        }
    }
}
