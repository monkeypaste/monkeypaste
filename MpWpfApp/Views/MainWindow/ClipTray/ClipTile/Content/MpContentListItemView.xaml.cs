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
    /// Interaction logic for MpContentListItemView.xaml
    /// </summary>
    public partial class MpContentListItemView : UserControl {
        private int _minDragDist = 10;

        AdornerLayer RtbItemAdornerLayer;
        MpRtbListBoxItemAdorner RtbItemAdorner;

        public MpContentListItemView() : base() {
            InitializeComponent();            
        }


        private void ContentListItemView_Loaded(object sender, RoutedEventArgs e) {
            RtbItemAdorner = new MpRtbListBoxItemAdorner(RtbView);
            RtbItemAdornerLayer = AdornerLayer.GetAdornerLayer(RtbView);
            RtbItemAdornerLayer?.Add(RtbItemAdorner);
        }

        private void ContentListItemView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(DataContext != null && DataContext is MpContentItemViewModel civm) {
                civm.OnScrollWheelRequest += Civm_OnScrollWheelRequest;
                civm.OnUiUpdateRequest += Civm_OnUiUpdateRequest;
            }
        }

        public void UpdateAdorner() {
            RtbItemAdornerLayer.Update();
        }

        #region Event Handlers

        #region View Model Ui Requests

        private void Civm_OnUiUpdateRequest(object sender, EventArgs e) {
            this.UpdateLayout();
        }

        private void Civm_OnScrollWheelRequest(object sender, int e) {
            throw new NotImplementedException();
        }

        #endregion

        #region Drag & Drop

        private void ContentListItemView_MouseEnter(object sender, MouseEventArgs e) {
            var rtbvm = DataContext as MpRtbItemViewModel;
            rtbvm.IsSubHovering = true;
        }

        private void ContentListItemView_MouseLeave(object sender, MouseEventArgs e) {
            var rtbvm = DataContext as MpRtbItemViewModel;
            rtbvm.IsSubHovering = false;
        }
        
        private void DragButton_PreviewGiveFeedback(object sender, GiveFeedbackEventArgs e) {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) {
                Application.Current.MainWindow.Cursor = Cursors.Cross;
                Application.Current.MainWindow.ForceCursor = true;
            }
        }

        private void DragButton_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            var rtbvm = DataContext as MpRtbItemViewModel;
            Application.Current.MainWindow.ForceCursor = false;
            rtbvm.MouseDownPosition = new Point();
            rtbvm.DragDataObject = null;
            rtbvm.IsSubDragging = false;
           // SyncMultiSelectDragButton(true, false);
        }


        private void DragButton_PreviewMouseDown(object sender, MouseButtonEventArgs e) {
            var rtbvm = DataContext as MpRtbItemViewModel;
            //SyncMultiSelectDragButton(true, true);
        }

        private void DragButton_PreviewMouseMove(object sender, MouseEventArgs e7) {
            var rtbvm = DataContext as MpRtbItemViewModel;
            if (e7.MouseDevice.LeftButton == MouseButtonState.Pressed) {
                if (rtbvm.IsEditingContent ||
                  (rtbvm.HostClipTileViewModel.IsExpanded && rtbvm.ContainerViewModel.Count == 1)) {
                    //cannot resort w/ only 1 item and its relative location is not clear
                    //since its isolated
                    return;
                }
                //SyncMultiSelectDragButton(false, true);
                if (rtbvm.MouseDownPosition == new Point()) {
                    rtbvm.MouseDownPosition = e7.GetPosition(rtbvm.Rtbc);
                }
                if (MpHelpers.Instance.DistanceBetweenPoints(rtbvm.MouseDownPosition, e7.GetPosition(rtbvm.Rtbc)) < _minDragDist) {
                    return;
                }
                rtbvm.IsSubDragging = true;
                rtbvm.IsSubSelected = true;
                if (rtbvm.DragDataObject == null) {
                    rtbvm.DragDataObject = MpClipTrayViewModel.Instance.GetDataObjectFromSelectedClips(true, false).Result;//RichTextBoxViewModelCollection.GetDataObjectFromSubSelectedItems(true).Result;
                }
                DragDrop.DoDragDrop(
                            rtbvm.Rtbc,
                            rtbvm.DragDataObject,
                            DragDropEffects.Copy | DragDropEffects.Move);
                e7.Handled = true;
            }
        }

        private void DragButton_MouseEnter(object sender, MouseEventArgs e) {
            var rtbvm = DataContext as MpRtbItemViewModel;
            rtbvm.IsOverDragButton = true;
            //SyncMultiSelectDragButton(true, false);
        }

        private void DragButton_MouseLeave(object sender, MouseEventArgs e) {
            var rtbvm = DataContext as MpRtbItemViewModel;
            rtbvm.IsOverDragButton = false;
           // SyncMultiSelectDragButton(false, false);
        }
        
        #endregion

        #region Title TextBlock Events
        private void RtbTitleTextBlock_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var rtbvm = DataContext as MpRtbItemViewModel;
            if (!rtbvm.HostClipTileViewModel.IsExpanded) {
                rtbvm.IsSubSelected = true;
            }
            rtbvm.IsSubEditingTitle = true;
            e.Handled = true;
        }

        private void RtbTitleTextBlock_MouseEnter(object sender, MouseEventArgs e) {
            var rtbvm = DataContext as MpRtbItemViewModel;
            rtbvm.IsHoveringOnTitleTextBlock = true;
        }

        private void RtbTitleTextBlock_MouseLeave(object sender, MouseEventArgs e) {
            var rtbvm = DataContext as MpRtbItemViewModel;
            rtbvm.IsHoveringOnTitleTextBlock = false;
        }
        #endregion

        #region Title TextBox Events
        private void RtbTitleTextBox_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var rtbvm = DataContext as MpRtbItemViewModel;
            if (rtbvm.RtbListBoxItemTitleTextBoxVisibility == Visibility.Collapsed) {
                //rtbvm.CopyItemTitle = RtbTitleTextBox.Text;
                return;
            }
            //RtbTitleTextBox.Focus();
            //RtbTitleTextBox.SelectAll();
        }

        private void RtbTitleTextBox_LostFocus(object sender, RoutedEventArgs e) {
            var rtbvm = DataContext as MpRtbItemViewModel;
            rtbvm.IsSubEditingTitle = false;
        }

        private void RtbTitleTextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            var rtbvm = DataContext as MpRtbItemViewModel;
            if (e.Key == Key.Enter || e.Key == Key.Escape) {
                rtbvm.IsSubEditingTitle = false;
            }
        }
        #endregion

        #region AppIcon Button Events
        private void RtbItemAppIconImageButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var rtbvm = DataContext as MpRtbItemViewModel;
            //MpHelpers.Instance.OpenUrl(CopyItem.App.AppPath);
            rtbvm.ContainerViewModel.ClearSubSelection();
            rtbvm.IsSubSelected = true;

            foreach (var vctvm in MpClipTrayViewModel.Instance.VisibileClipTiles) {
                if (vctvm.CopyItemAppId != rtbvm.CopyItemAppId) {
                    bool hasSubItemWithApp = false;
                    if (vctvm.ContentContainerViewModel.Count > 1) {
                        foreach (var vrtbvm in vctvm.ContentContainerViewModel.ItemViewModels) {
                            if (vrtbvm.CopyItem.Source.AppId != rtbvm.CopyItemAppId) {
                                vrtbvm.ItemVisibility = Visibility.Collapsed;
                            } else {
                                hasSubItemWithApp = true;
                            }
                        }
                    }
                    if (!hasSubItemWithApp) {
                        vctvm.TileVisibility = Visibility.Collapsed;
                    }
                }
            }
            //this triggers clip tray to swap out the app icons for the filtered app
            //MpClipTrayViewModel.Instance.FilterByAppIcon = rtbvm.CopyItemAppIcon;
            MpClipTrayViewModel.Instance.IsFilteringByApp = true;
        }

        #endregion

        #region Context Menu Events

        private void RtbItem_ContextMenu_Loaded(object sender, RoutedEventArgs e) {
            var rtbvm = DataContext as MpRtbItemViewModel;
            var rtblbvm = rtbvm.ContainerViewModel as MpRtbItemCollectionViewModel;
            var cm = (ContextMenu)sender;
            cm.DataContext = rtbvm;
            MenuItem cmi = null;
            foreach (var mi in cm.Items) {
                if (mi == null || mi is Separator) {
                    continue;
                }
                if ((mi as MenuItem).Name == "ClipTileColorContextMenuItem") {
                    cmi = (MenuItem)mi;
                    break;
                }
            }
            MpHelpers.Instance.SetColorChooserMenuItem(
                    cm,
                    cmi,
                    (s, e1) => {
                        rtblbvm.ChangeSubSelectedClipsColorCommand.Execute((Brush)((Border)s).Tag);
                        foreach (var sctvm in rtblbvm.SubSelectedContentItems) {
                            sctvm.CopyItem.WriteToDatabase();
                        }
                    },
                    MpHelpers.Instance.GetColorColumn(rtbvm.CopyItemColorBrush),
                    MpHelpers.Instance.GetColorRow(rtbvm.CopyItemColorBrush)
                );
        }

        private void RtbItem_ContextMenu_Opened(object sender, RoutedEventArgs e) {
            var cm = (ContextMenu)sender;
            var rtbvm = DataContext as MpRtbItemViewModel;

            cm.Tag = rtbvm;
            rtbvm.IsSubContextMenuOpened = true;
            cm = MpPasteToAppPathViewModelCollection.Instance.UpdatePasteToMenuItem(cm);

            MenuItem eami = null;
            foreach (var mi in cm.Items) {
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
                eami.Header = @"Exclude Application '" + rtbvm.CopyItemAppName + "'";
            }

            rtbvm.RefreshAsyncCommands();

            rtbvm.OnPropertyChanged(nameof(rtbvm.TagMenuItems));

            MpShortcutCollectionViewModel.Instance.UpdateInputGestures(cm);
        }

        private void RtbItem_ContextMenu_Closed(object sender, RoutedEventArgs e) {
            var cm = (ContextMenu)sender;
            var rtbvm = DataContext as MpRtbItemViewModel;

            rtbvm.IsSubContextMenuOpened = false;
            rtbvm.ContainerViewModel.ClearSubSelection();
        }

        #endregion

        #endregion
    }
}
