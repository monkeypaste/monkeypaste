using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using MonkeyPaste;


namespace MpWpfApp {
    public class MpSelectionBehavior : Behavior<FrameworkElement> {
        private const double MINIMUM_DRAG_DISTANCE = 10;

        private bool isRightClick, wasSelected;

        protected override void OnAttached() {
            AssociatedObject.PreviewMouseDown += AssociatedObject_PreviewMouseButtonDown;
            AssociatedObject.PreviewMouseUp += AssociatedObject_PreviewMouseUp                ;
        }

        private void AssociatedObject_PreviewMouseButtonDown(object sender, MouseButtonEventArgs e) {
            isRightClick = e.ChangedButton == MouseButton.Right;

            if (MpClipTrayViewModel.Instance.SelectedContentItemViewModels.Count >= 1) {
                //MpClipTrayViewModel.Instance.IsPreSelection = true;
            }

            if (AssociatedObject is MpContentItemView) {
                var civm = AssociatedObject.DataContext as MpContentItemViewModel;
                wasSelected = civm.IsSelected;
                civm.IsSelected = true;
                civm.LastSubSelectedDateTime = DateTime.Now;
                if (!civm.Parent.IsSelected) {
                    civm.Parent.IsSelected = true;
                    civm.Parent.LastSelectedDateTime = DateTime.Now;
                }
            } else if(AssociatedObject is MpRtbView) {
                if (isRightClick && (AssociatedObject.DataContext as MpContentItemViewModel).IsEditingContent) {
                    //show default context menu while editing
                    

                } 
            }
            e.Handled = false;
        }

        private void AssociatedObject_PreviewMouseUp(object sender, MouseButtonEventArgs e) {
            
            if (AssociatedObject is MpRtbView) {
                return;
            }
            bool isAnyContentDragging = MpClipTrayViewModel.Instance.SelectedContentItemViewModels.Any(x => x.IsSubDragging);
            //i think this only gets called for the clip tile
            if(wasSelected && isRightClick) {
                //do nothing and show context menu for all selected items
            } else if(!MpHelpers.Instance.IsMultiSelectKeyDown()){// || !isAnyContentDragging) {
                ClearPreviousSelection();
            }

            if (isRightClick) {
                ShowContextMenu();
            }

            isRightClick = false;
            wasSelected = false;
            //MpClipTrayViewModel.Instance.IsPreSelection = false;
        }

        #region Selection
     
        private void ClearPreviousSelection() {
            var civm = AssociatedObject.DataContext as MpContentItemViewModel;
            foreach (var sctvm in MpClipTrayViewModel.Instance.SelectedItems) {
                if (sctvm != civm.Parent) {
                    if(sctvm.IsFlipped) {
                       sctvm.Parent.FlipTileCommand.Execute(sctvm);
                    }
                    sctvm.IsSelected = false;
                }
                foreach (var scivm in sctvm.SelectedItems) {
                    if(scivm != civm) {
                        scivm.IsSelected = false;
                    }
                }
            }
        }

        private void ShowContextMenu() {
            AssociatedObject.ContextMenu = new MpContentContextMenuView();
            AssociatedObject.ContextMenu.IsOpen = true;
        }

        #endregion
    }
}
