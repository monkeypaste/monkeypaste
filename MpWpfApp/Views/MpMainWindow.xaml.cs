using MpWinFormsClassLibrary;
using System;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace MpWpfApp {
    public partial class MpMainWindow : Window {
        
        public MpMainWindow() {
            InitializeComponent();            
        }
        #region Clip Tile Events 
        private void TrayListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            //mainwindowviewmodel
            var mwvm = (MpMainWindowViewModel)(((ListBox)sender).DataContext);

            foreach(MpClipTileViewModel clipTile in e.RemovedItems) {
                clipTile.ToggleSelected();
                //mwvm.SelectedClipTiles.Remove(clipTile);
                var clipBorder = (Border)((MpMainWindow)Application.Current.MainWindow).FindName("Clip"+ mwvm.ClipTiles.IndexOf(clipTile));
                clipBorder.BorderBrush = Brushes.Yellow;
            }
            foreach(MpClipTileViewModel clipTile in e.AddedItems) {
                clipTile.ToggleSelected();
                //mwvm.SelectedClipTiles.Add(clipTile);
                var clipBorder = (Border)((MpMainWindow)Application.Current.MainWindow).FindName("Clip" + mwvm.ClipTiles.IndexOf(clipTile));
                clipBorder.BorderBrush = Brushes.Red;
            }
        }
        private void TileBorder_MouseEnter(object sender, MouseEventArgs e) {
            ((Border)sender).BorderBrush = Brushes.Yellow;
        }
        private void TileBorder_MouseLeave(object sender, MouseEventArgs e) {
            if(!((MpClipTileViewModel)((Border)sender).DataContext).IsSelected) {
                ((Border)sender).BorderBrush = Brushes.Transparent;
            } else {
                ((Border)sender).BorderBrush = Brushes.Red;
            }
        }
        private void ClipTile_Loaded(object sender, RoutedEventArgs e) {
            this.RegisterName("Clip" + ((MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext).ClipTiles.IndexOf(((MpClipTileViewModel)((Border)sender).DataContext)), ((Border)sender));

        }
        #endregion

        #region Tag Tile Events 
        private void TagTileTray_Loaded(object sender, RoutedEventArgs e) {
            this.RegisterName("Tag" + ((MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext).TagTiles.IndexOf(((MpTagTileViewModel)((Border)sender).DataContext)),((Border)sender));

            if(((MpTagTileViewModel)((Border)sender).DataContext).TagName == "History") {
                ((Border)sender).BorderBrush = Brushes.Red;
                ((Border)sender).Background = ((MpTagTileViewModel)((Border)sender).DataContext).TagColor;
                ((MpTagTileViewModel)((Border)sender).DataContext).ToggleSelected();
                ((MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext).SelectedTagTiles.Add(((MpTagTileViewModel)((Border)sender).DataContext));
            }
        }
        private void TagListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
           //if(((MpMainWindowViewModel)((ListBox)sender).DataContext).SelectedTagTiles.Count == 0) {
           //     var historyTagBorder = (Border)((MpMainWindow)Application.Current.MainWindow).FindName("Tag0");
           //     historyTagBorder.BorderBrush = Brushes.Red;
           //     historyTagBorder.Background = ((MpTagTileViewModel)historyTagBorder.DataContext).TagColor;
           //     ((MpTagTileViewModel)historyTagBorder.DataContext).ToggleSelected();
           //     ((MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext).SelectedTagTiles.Add(((MpTagTileViewModel)historyTagBorder.DataContext));
           // }             
        }
        private void TagBorder_MouseEnter(object sender, MouseEventArgs e) {
            ((Border)sender).BorderBrush = Brushes.White;
        }
        private void TagBorder_MouseLeave(object sender, MouseEventArgs e) {
            if(!((MpTagTileViewModel)((Border)sender).DataContext).IsSelected) {
                ((Border)sender).BorderBrush = Brushes.Transparent;
            } else {
                ((Border)sender).BorderBrush = Brushes.Red;
            }
        }
        private void TagBorder_MouseUp(object sender, MouseButtonEventArgs e) {
            ((MpTagTileViewModel)((Border)sender).DataContext).ToggleSelected();

            if(((MpTagTileViewModel)((Border)sender).DataContext).IsSelected) {
                ((Border)sender).BorderBrush = Brushes.Red;
                ((Border)sender).Background = ((MpTagTileViewModel)((Border)sender).DataContext).TagColor;
                ((TextBlock)((Border)sender).FindName("TagNameTextBlock")).Foreground = Brushes.Black;
                ((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).SelectedTagTiles.Add(((MpTagTileViewModel)((Border)sender).DataContext));
            } else {
                ((Border)sender).BorderBrush = Brushes.Yellow;
                ((Border)sender).Background = Brushes.Black;
                ((TextBlock)((Border)sender).FindName("TagNameTextBlock")).Foreground = Brushes.White;
                ((MpMainWindowViewModel)Application.Current.MainWindow.DataContext).SelectedTagTiles.Remove(((MpTagTileViewModel)((Border)sender).DataContext));
            }

            //if all tags are inactive turn the history tag back on
            bool isAnyTagActive = false;
            foreach(MpTagTileViewModel tagTile in ((MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext).TagTiles) {
                if(tagTile.IsSelected) {
                    isAnyTagActive = true;
                    break;
                }
            }
            if(!isAnyTagActive) {
                var historyTagBorder = (Border)((MpMainWindow)Application.Current.MainWindow).FindName("Tag0");
                historyTagBorder.BorderBrush = Brushes.Red;
                historyTagBorder.Background = ((MpTagTileViewModel)historyTagBorder.DataContext).TagColor;
                ((MpTagTileViewModel)historyTagBorder.DataContext).ToggleSelected();
                ((MpMainWindowViewModel)((MpMainWindow)Application.Current.MainWindow).DataContext).SelectedTagTiles.Add(((MpTagTileViewModel)historyTagBorder.DataContext));
            }
        }
        #endregion        
    }
}
