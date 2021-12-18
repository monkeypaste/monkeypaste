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
            MpHelpers.Instance.SetColorChooserMenuItem(
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
            MpApp app = null;
            if (MpClipTrayViewModel.Instance.SelectedModels.Count == 1) {
                app = MpClipTrayViewModel.Instance.SelectedModels[0].Source.App;
            }

            if(!MpLanguageTranslator.Instance.IsLoaded) {
                await MpLanguageTranslator.Instance.Init();

                MpClipTrayViewModel.Instance.TranslateLanguageMenuItems.Clear();
                foreach (var languageName in MpLanguageTranslator.Instance.LanguageList) {
                    var ltmivm = new MpContextMenuItemViewModel(
                        languageName, 
                        MpClipTrayViewModel.Instance.TranslateSelectedClipTextAsyncCommand, 
                        languageName, 
                        false);

                    MpClipTrayViewModel.Instance.TranslateLanguageMenuItems.Add(ltmivm);
                }

                MpClipTrayViewModel.Instance.OnPropertyChanged(nameof(MpClipTrayViewModel.Instance.TranslateLanguageMenuItems));
            }

            MpClipTrayViewModel.Instance.TagMenuItems = await MpClipTrayViewModel.Instance.GetTagMenuItemsForSelectedItems();

            Tag = DataContext;
            MpPasteToAppPathViewModelCollection.Instance.UpdatePasteToMenuItem(this);

            string primarySourceName = MpClipTrayViewModel.Instance.PrimaryItem.PrimaryItem.CopyItem.Source.PrimarySource.SourceName;
            string primarySourceIcon64 = MpClipTrayViewModel.Instance.PrimaryItem.PrimaryItem.CopyItem.Source.PrimarySource.SourceIcon.IconImage.ImageBase64;
            ExcludeApplication.Header = string.Format(@"Exclude '{0}'", primarySourceName);

            ExcludeApplication.Icon = new Image() {
                Source = MpHelpers.Instance.MergeImages(
                            new List<BitmapSource>() {
                                primarySourceIcon64.ToBitmapSource().Scale(new Size(0.75,0.75)),
                                (Application.Current.Resources["NoEntryIcon"] as string).ToBitmapSource()
                            })
            };
            //MpHelpers.Instance.CombineBitmap
            //int removeCount = miToRemove.Count;
            //while(removeCount > 0) {
            //    this.Items.RemoveAt(this.Items.Count - 1);
            //    removeCount--;
            //}

            MpShortcutCollectionViewModel.Instance.UpdateInputGestures(this);

            MpAnalyticItemCollectionViewModel.Instance.UpdateQuickActionMenuItem(this);
            //if(quickActionSep != null) {
            //    var quickActions = MpQuickActionAnalyzerCollectionViewModel.Instance.GetQuickActionAnalyzerMenuItems();
            //    if(quickActions != null && quickActions.Count > 0) {
            //        quickActionSep.Visibility = Visibility.Visible;
            //        foreach (var qami in quickActions) {
            //            var mi = new MenuItem() {
            //                DataContext = qami
            //            };
            //            mi.ItemContainerStyle = this.Resources["DefaultItemStyle"] as Style;
            //            this.Items.Add(mi);
            //            mi.UpdateLayout();
            //            this.UpdateLayout();
            //            mi.UpdateDefaultStyle();
            //            mi.Height = 25;
            //            mi.Width = 300;
            //        }
            //    } else {
            //        quickActionSep.Visibility = Visibility.Collapsed;
            //    }
            //}
            
        }

        private void ClipTile_ContextMenu_Closed(object sender, RoutedEventArgs e) {
            if (DataContext is MpClipTileViewModel ctvm) {
                ctvm.IsContextMenuOpened = false;
            } else if (DataContext is MpContentItemViewModel civm) {
                civm.IsContextMenuOpen = false;
            }

        }
    }
}
