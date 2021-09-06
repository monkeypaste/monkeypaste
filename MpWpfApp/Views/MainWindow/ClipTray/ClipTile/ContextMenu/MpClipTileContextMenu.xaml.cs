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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpClipTileContextMenu.xaml
    /// </summary>
    public partial class MpClipTileContextMenu : ContextMenu {
        public MpClipTileContextMenu() : base() {
            DataContext = ((Application.Current.MainWindow as MpMainWindow).DataContext as MpMainWindowViewModel).ClipTrayViewModel;
            InitializeComponent();
        }

        private void ClipTile_ContextMenu_Loaded(object sender, RoutedEventArgs e) {
            var ctrvm = DataContext as MpClipTrayViewModel;

            MpHelpers.Instance.SetColorChooserMenuItem(
                    this,
                    ClipTileColorContextMenuItem,
                    (s, e1) => {
                        ctrvm.ChangeSelectedClipsColorCommand.Execute((Brush)((Border)s).Tag);
                        foreach (var sctvm in ctrvm.SelectedClipTiles) {
                            sctvm.CopyItem.WriteToDatabase();
                        }
                    }
                );
        }

        private void ClipTile_ContextMenu_Opened(object sender, RoutedEventArgs e) {
            var ctrvm = DataContext as MpClipTrayViewModel;
            if (ctrvm.SelectedClipTiles.Count == 0) {
                return;
            }
            var ctvm = ctrvm.SelectedClipTiles[0];
            Tag = ctvm;
            ctvm.IsContextMenuOpened = true;

            if (ctvm.CopyItemType == MpCopyItemType.RichText) {
                MpPasteToAppPathViewModelCollection.Instance.UpdatePasteToMenuItem(this);
            }

            MenuItem eami = null;
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
                            eami = smi as MenuItem;
                        }
                    }
                }
            }
            if (eami != null) {
                eami.Header = @"Exclude Application '" + ctvm.CopyItemAppName + "'";
            }

            ctvm.RefreshAsyncCommands();

            ctvm.OnPropertyChanged(nameof(ctvm.TagMenuItems));

            MpShortcutCollectionViewModel.Instance.UpdateInputGestures(this);
        }

        private void ClipTile_ContextMenu_Closed(object sender, RoutedEventArgs e) {
            var ctrvm = DataContext as MpClipTrayViewModel;
            if (ctrvm.SelectedClipTiles.Count == 0) {
                return;
            }
            var ctvm = ctrvm.SelectedClipTiles[0];
            ctvm.IsContextMenuOpened = false;
        }
    }
}
