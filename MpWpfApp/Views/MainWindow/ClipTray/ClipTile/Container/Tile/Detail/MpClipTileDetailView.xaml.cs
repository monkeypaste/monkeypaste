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
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using MonkeyPaste;
using System.Windows.Media.Animation;
using System.Globalization;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpClipTileDetailView.xaml
    /// </summary>
    public partial class MpClipTileDetailView : MpUserControl<MpContentItemViewModel> {
        private Hyperlink h;
        public MpClipTileDetailView() {
            InitializeComponent();
        }

        private void ClipTileDetailTextBlock_MouseEnter(object sender, MouseEventArgs e) {
            var civm = DataContext as MpContentItemViewModel;
            civm.CycleDetailCommand.Execute(null);                       

            if (civm.CurDetailType == MpCopyItemDetailType.AppInfo || 
                civm.CurDetailType == MpCopyItemDetailType.UrlInfo) {
                string linkText = civm.DetailText;
                string linkPath = civm.DetailText;
                string toolTip = string.Empty;
                h = new Hyperlink();
                if(civm.CurDetailType == MpCopyItemDetailType.AppInfo) {
                    if(File.Exists(civm.DetailText)) {
                        linkText = "Source Folder";
                        linkPath = Path.GetDirectoryName(civm.DetailText);
                    }
                    toolTip = civm.CopyItem.Source.App.AppName;
                } else {
                    linkText = "Source Url";
                    linkPath = civm.DetailText;
                    toolTip = civm.CopyItem.Source.Url.UrlTitle;
                }
                h.Inlines.Add(linkText);
                h.NavigateUri = new Uri(linkPath);
                h.IsEnabled = true;
                h.Click += H_Click;
                ClipTileDetailTextBlock.Inlines.Clear();
                ClipTileDetailTextBlock.Inlines.Add(h);
                ClipTileDetailTextBlock.ToolTip = toolTip;
            } else {
                ClipTileDetailTextBlock.Inlines.Clear();
                ClipTileDetailTextBlock.Inlines.Add(new Run(civm.DetailText));
                ClipTileDetailTextBlock.ToolTip = Enum.GetName(typeof(MpCopyItemDetailType),BindingContext.CurDetailType);
            }
        }

        private void ClipTileDetailTextBlock_MouseLeave(object sender, MouseEventArgs e) {
            var civm = DataContext as MpContentItemViewModel;
            if(h != null) {
                h.Click -= H_Click;
                h = null;
            }
        }

        private void H_Click(object sender, RoutedEventArgs e) {
            MpHelpers.Instance.OpenUrl((sender as Hyperlink).NavigateUri.ToString());
        }

        private void ClipTileToggleEditButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            ClipTileToggleEditButton.IsChecked = !ClipTileToggleEditButton.IsChecked;
            e.Handled = true;
        }

        private void ClipTileHotkeyButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            BindingContext.AssignHotkeyCommand.Execute(null);
            e.Handled = true;
        }
    }
}
