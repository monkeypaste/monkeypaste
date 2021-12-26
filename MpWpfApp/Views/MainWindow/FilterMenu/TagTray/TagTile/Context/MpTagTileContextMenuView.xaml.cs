using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
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
    public partial class MpTagTileContextMenuView : ContextMenu {
        public MpTagTileContextMenuView() {
            InitializeComponent();
        }

        //private void TagTile_ContextMenu_Loaded(object sender, RoutedEventArgs e) {
        //    var ttvm = base.DataContext as MpTagTileViewModel;
        //    var cm = (ContextMenu)sender;
        //    cm.DataContext = ttvm;
        //    MenuItem cmi = null;
        //    foreach (var mi in cm.Items) {
        //        if (mi == null || mi is Separator) {
        //            continue;
        //        }
        //        if ((mi as MenuItem).Name == "ClipTileColorContextMenuItem") {
        //            cmi = (MenuItem)mi;
        //            break;
        //        }
        //    }
        //    MpHelpers.Instance.SetColorChooserMenuItem(
        //            cm,
        //            cmi,
        //            (s, e1) => {
        //                ttvm.ChangeColorCommand.Execute((Brush)((Border)s).Tag);
        //            }
        //        );
        //}

        //private void TagTile_ContextMenu_Opened(object sender, RoutedEventArgs e) {
        //    if(DataContext == null) {
        //        return;
        //    }
        //    var ttvm = DataContext as MpTagTileViewModel;
        //    ttvm.IsContextMenuOpened = true;

        //    var cm = sender as ContextMenu;
        //    cm.Tag = ttvm;
        //    MpShortcutCollectionViewModel.Instance.UpdateInputGestures(cm);
        //}

        //private void TagTile_ContextMenu_Closed(object sender, RoutedEventArgs e) {
        //    if (DataContext == null) {
        //        return;
        //    }
        //    var ttvm = DataContext as MpTagTileViewModel;
        //    ttvm.IsContextMenuOpened = false;
        //}

        private void TagTile_ContextMenu_Loaded(object sender, RoutedEventArgs e) {
            var ttvm = DataContext as MpTagTileViewModel;
            var cm = (ContextMenu)sender;
            cm.DataContext = ttvm;
            MpHelpers.Instance.SetColorChooserMenuItem(
                        cm,
                        ClipTileColorContextMenuItem,
                        (s, e1) => {
                            ttvm.ChangeColorCommand.Execute((Brush)((Border)s).Tag);
                        }
                    );
        }

        private void TagTile_ContextMenu_Opened(object sender, RoutedEventArgs e) {            
            var ttvm = DataContext as MpTagTileViewModel;

            ttvm.IsContextMenuOpened = true;

            var cm = sender as ContextMenu;
            cm.Tag = ttvm;
            foreach (var i in cm.Items) {
                if (i == null || i is Separator) {
                    continue;
                }
                MenuItem mi = i as MenuItem;
                if (mi.Header != null && mi.Header.ToString() == "Link") {
                    mi.Items.Clear();

                    var dirMenuItem = new MenuItem() {
                        Header = "Directory"
                    };

                    dirMenuItem.Items.Add(new MenuItem() {
                        Icon = new MpStringToIconConverter().Convert(Application.Current.Resources["NoEntryIcon"] as string, typeof(Image), null, CultureInfo.InvariantCulture) as Image,
                        Header = "Add New...",
                        Command = ttvm.TagProperties.AddDirectoryPropertyToTagCommand
                    });
                    if (ttvm.HasProperties) {
                        dirMenuItem.Items.Add(new Separator());

                        foreach (var prop in ttvm.TagProperties.TagProperties) {
                            if (prop is MpDirectoryWatcherTagPropertyViewModel dwtpvm) {
                                dirMenuItem.Items.Add(new MenuItem() {
                                    Icon = new MpFilePathStringToIconConverter().Convert(dwtpvm.PropertyData, typeof(ImageSource), null, CultureInfo.InvariantCulture) as ImageSource,
                                    Header = System.IO.Path.GetDirectoryName(dwtpvm.PropertyData),
                                    Command = dwtpvm.OpenPathCommand,
                                    CommandParameter = dwtpvm.PropertyData
                                });
                            }
                        }

                        dirMenuItem.Items.Add(new Separator());

                        dirMenuItem.Items.Add(new MpContextMenuItemViewModel() {
                            Icon = new MpStringToIconConverter().Convert(Application.Current.Resources["CogIcon"] as string, typeof(Image), null, CultureInfo.InvariantCulture) as Image,
                            Header = "Manage...",
                            Command = ttvm.TagProperties.ManagePropertiesCommand
                        });
                    }

                    mi.Items.Add(dirMenuItem);
                }
            }
            MpShortcutCollectionViewModel.Instance.UpdateInputGestures(cm);
        }

        private void TagTile_ContextMenu_Closed(object sender, RoutedEventArgs e) {            
            var ttvm = DataContext as MpTagTileViewModel;
            ttvm.IsContextMenuOpened = false;
        }

        private void RenameMenuItem_Click(object sender, RoutedEventArgs e) {
            var ttvm = DataContext as MpTagTileViewModel;
            ttvm.RenameTagCommand.Execute(null);
        }

        private void DeleteMenuItem_Clicked(object sender, RoutedEventArgs e) {
            var ttvm = DataContext as MpTagTileViewModel;
            ttvm.Parent.DeleteTagCommand.Execute(ttvm.TagId);
        }
    }
}
