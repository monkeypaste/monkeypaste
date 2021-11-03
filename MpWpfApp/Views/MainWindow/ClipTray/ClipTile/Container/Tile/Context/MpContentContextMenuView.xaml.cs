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
            //if(!MpLanguageTranslator.Instance.IsLoaded) {
            //    MpLanguageTranslator.Instance.Init();
            //    if(!MpLanguageTranslator.Instance)
            //}

            MenuItem cmi = null;
            foreach (var mi in Items) {
                if (mi == null || mi is Separator) {
                    continue;
                }
                (mi as MenuItem).DataContext = DataContext;
                if ((mi as MenuItem).Name == "ClipTileColorContextMenuItem") {
                    cmi = (MenuItem)mi;
                }
            }
            MpHelpers.Instance.SetColorChooserMenuItem(
                    this,
                    cmi,
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

            foreach (var mi in Items) {
                if (mi == null || mi is Separator) {
                    continue;
                }
                if ((mi as MenuItem).Name == @"ToolsMenuItem") {
                    foreach (var smi in (mi as MenuItem).Items) {
                        if (smi == null || smi is Separator) {
                            continue;
                        }
                        if ((smi as MenuItem).Name == "ExcludeApplication") {
                            if (app == null) {
                                //hide for multi-subselection
                                (smi as MenuItem).Visibility = Visibility.Collapsed;
                            } else {
                                var eami = smi as MenuItem;
                                eami.Header = @"Exclude Application '" + app.AppName + "'";
                                eami.Icon = app.Icon.IconImage.ImageBase64.ToBitmapSource();
                            }
                        }
                    }
                }
            }

            MpShortcutCollectionViewModel.Instance.UpdateInputGestures(this);
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
