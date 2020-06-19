using MpWinFormsClassLibrary;
using System;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using MpWinFormsClassLibrary;

namespace MpWpfApp {
    public partial class MpMainWindow : Window {
        
        public MpMainWindow() {
            InitializeComponent();
        }

        private void TagListBoxItem_MouseEnter(object sender, MouseEventArgs e) {
            if(sender.GetType().IsSubclassOf(typeof(Control))) {
                ((MpTagTileViewModel)((Control)sender).DataContext).IsHovering = true;
            }
        }

        private void TagListBoxItem_MouseLeave(object sender, MouseEventArgs e) {
            if(sender.GetType().IsSubclassOf(typeof(Control))) {
                ((MpTagTileViewModel)((Control)sender).DataContext).IsHovering = false;
            }            
        }
        private void ClipListBoxItem_MouseEnter(object sender, MouseEventArgs e) {
            if(sender.GetType().IsSubclassOf(typeof(Control))) {
                ((MpClipTileViewModel)((Control)sender).DataContext).IsHovering = true;
            }            
        }

        private void ClipListBoxItem_MouseLeave(object sender, MouseEventArgs e) {
            if(!Extensions.IsNamedObject(((Control)sender).DataContext))
            {
                ((MpClipTileViewModel)((Control)sender).DataContext).IsHovering = false;
            }            
        }

        private void TagNameTextBox_KeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.Enter) {
                ((MpTagTileViewModel)((TextBox)sender).DataContext).IsEditing = false;
            } else if(e.Key == Key.Delete || e.Key == Key.Back) {
                ((MpTagTileViewModel)((TextBox)sender).DataContext).DeleteTagCommand.Execute(null);
            }
        }

        private void TagTile_LostFocus(object sender, RoutedEventArgs e) {
            ((MpTagTileViewModel)((TextBox)sender).DataContext).IsEditing = false;
        }

        private void TextBox_SizeChanged(object sender, SizeChangedEventArgs e) {
            //used for bot tags and clips
            if(sender != null && ((TextBox)sender).ActualHeight > 0 && !((TextBox)sender).IsFocused) {
                ((TextBox)sender).Focus();
                ((TextBox)sender).SelectAll();
            }
        }

        private void ClipTile_KeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.Delete || e.Key == Key.Back) {
                //delete clip which shifts focus to neighbor
                ((MpClipTileViewModel)((TextBox)sender).DataContext).DeleteClipCommand.Execute(null);
            } else if(e.Key == Key.Enter) {
                //In order to paste the app must hide first
                Application.Current.MainWindow.Hide();
                foreach(var clipTile in ((MpMainWindowViewModel)DataContext).SelectedClipTiles) {
                    MpDataStore.Instance.ClipboardManager.PasteCopyItem(clipTile.CopyItem.Text);
                }
            }
        }
        private void TagTile_KeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.Delete || e.Key == Key.Back && ((MpTagTileViewModel)((TextBox)sender).DataContext).DeleteTagCommand.CanExecute(null)) {
                ((MpTagTileViewModel)((TextBox)sender).DataContext).DeleteTagCommand.Execute(null);
            }
        }

        private void ClipTile_LostFocus(object sender, RoutedEventArgs e) {
            ((MpClipTileViewModel)((TextBox)sender).DataContext).IsEditingTitle = false;
        }

        private void ClipTitleTextBox_KeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.Enter) {
                ((MpClipTileViewModel)((TextBox)sender).DataContext).IsEditingTitle = false;
            }
        }

        private void ClipTray_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            var scrollViewer = ((ListBox)sender).GetChildOfType<ScrollViewer>();
            double lastOffset = scrollViewer.HorizontalOffset;
            ((ListBox)sender).GetChildOfType<ScrollViewer>().ScrollToHorizontalOffset(lastOffset - (double)e.Delta);
        }

        private void ClipTile_Loaded(object sender, RoutedEventArgs e) {
            var clipTile = ((MpClipTileViewModel)((Border)sender).DataContext);
            ((ListBox)FindName("ClipTray")).RegisterName("Clip" + clipTile.CopyItem.CopyItemId, ((Border)sender));
            if(((MpMainWindowViewModel)DataContext).SelectedClipTiles.Contains(clipTile)) {
                ((Border)sender).Focus();
            }
        }

    }
}
