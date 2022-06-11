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
    public partial class MpAddTemplateToolbarButton : MpUserControl<MpTemplateCollectionViewModel> {
        RichTextBox Rtb {
            get {
                var ctv = this.GetVisualAncestor<MpClipTileView>();
                if(ctv == null) {
                    return null;
                }
                return ctv.GetVisualDescendent<RichTextBox>();
            }
        }

        public MpAddTemplateToolbarButton() {
            InitializeComponent();
        }


        private void Mi_Unloaded(object sender, RoutedEventArgs e) {
            var mi = sender as MenuItem;
            if(mi == null) {
                return;
            }
            mi.Unloaded -= Mi_Unloaded;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e) {
            //if(Rtb == null || Rtb.DataContext == null) {
            //    MpConsole.WriteTraceLine("No rtb or rtb context");
            //    return;
            //}
            //var rtbv = Rtb.GetVisualAncestor<MpRtbContentView>();
            //rtbv.NewOriginalText = Rtb.Selection.Text;
            //rtbv.NewStartRange = Rtb.Selection;

            //var ctvm = rtbv.BindingContext;
            //if (ctvm.HeadItem.TemplateCollection.Items.Count == 0) {
            //    //when no templates exist create a new default one
            //    var thl = MpTemplateHyperlink.Create(Rtb.Selection, null);
            //    var thlvm = thl.DataContext as MpTemplateViewModel;
            //    thlvm.EditTemplateCommand.Execute(null);
            //} else {
            //    //otherwise show template menu
            //    AddButton.ContextMenu.IsOpen = true;
            //}
            if(BindingContext.Items.Count == 0) {
                BindingContext.CreateTemplateViewModelCommand.Execute(Rtb.Selection.Text);
                return;
            }

            MpContextMenuView.Instance.DataContext = BindingContext.MenuItemViewModel;
            MpContextMenuView.Instance.PlacementTarget = this;
            MpContextMenuView.Instance.IsOpen = true;
        }

        private void AddButton_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(AddButton.IsEnabled) {
                AddButtonImage.Source = (BitmapSource)new BitmapImage(new Uri(MpPreferences.AbsoluteResourcesPath + @"/Images/MpRichTextBox/templateadd.png"));
                AddButton.ToolTip = "Add Template";
            } else {
                AddButtonImage.Source = (BitmapSource)new BitmapImage(new Uri(MpPreferences.AbsoluteResourcesPath + @"/Images/MpRichTextBox/templatedisabled.png"));
                var tb = new TextBlock() {
                    Foreground = Brushes.Red,
                    Background = Brushes.White,
                    FontSize = 12,
                    Text = @"-current selection intersects with a template" + Environment.NewLine +
                            @"-contains a space" + Environment.NewLine +
                            @"-contains more than 10 characters"
                };
                AddButton.ToolTip = tb;
            }
        }
    }
}
