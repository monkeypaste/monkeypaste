using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for MpContentContextMenuView.xaml
    /// </summary>
    public partial class MpContentContextMenuView : ContextMenu {
        public MpContentContextMenuView() {
            InitializeComponent();
        }

        private void ClipTile_ContextMenu_Loaded(object sender, RoutedEventArgs e) {
            MpHelpers.SetColorChooserMenuItem(
                    this,
                    ClipTileColorContextMenuItem,
                    (s, e1) => {
                        MpClipTrayViewModel.Instance.ChangeSelectedClipsColorCommand.Execute((Brush)((Border)s).Tag);
                    }
                );        
        }

        private async void ClipTile_ContextMenu_Opened(object sender, RoutedEventArgs e) {
            await PrepareContextMenu();
        }

        private async Task PrepareContextMenu() {
            //MpClipTrayViewModel.Instance.TagMenuItems = await MpClipTrayViewModel.Instance.GetTagMenuItemsForSelectedItemsAsync();

            Tag = DataContext;
            //MpPasteToAppPathViewModelCollection.Instance.UpdatePasteToMenuItem(this);

            string primarySourceName = MpClipTrayViewModel.Instance.PrimaryItem.PrimaryItem.CopyItem.Source.PrimarySource.SourceName;
            string primarySourceIcon64 = MpClipTrayViewModel.Instance.PrimaryItem.PrimaryItem.CopyItem.Source.PrimarySource.SourceIcon.IconImage.ImageBase64;
            ExcludeApplication.Header = string.Format(@"Exclude '{0}'", primarySourceName);

            ExcludeApplication.Icon = new Image() {
                Source = MpWpfImagingHelper.MergeImages(
                            new List<BitmapSource>() {
                                primarySourceIcon64.ToBitmapSource().Scale(new Size(0.75,0.75)),
                                (Application.Current.Resources["NoEntryIcon"] as string).ToBitmapSource()
                            })
            };

            MpShortcutCollectionViewModel.Instance.UpdateInputGestures(this);
            var aicvm = Application.Current.Resources["AnalyticItemCollectionViewModel"] as MpAnalyticItemCollectionViewModel;
            
            //aicvm.OnPropertyChanged(nameof(aicvm.ContextMenuItems));
        }

        private void ClipTile_ContextMenu_Closed(object sender, RoutedEventArgs e) {
            if (DataContext is MpClipTileViewModel ctvm) {
                ctvm.IsContextMenuOpened = false;
            } else if (DataContext is MpContentItemViewModel civm) {
                civm.IsContextMenuOpen = false;
            }
            this.Items.Refresh();
        }
    }
}
