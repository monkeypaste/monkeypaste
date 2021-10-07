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
using System.Windows.Controls.Primitives;

namespace MpWpfApp {
    public class MpSelectionBehavior : Behavior<FrameworkElement> {
        private const double MINIMUM_DRAG_DISTANCE = 10;

        private bool isRightClick, wasSelected;

        public static MpContentItemViewModel LastSelectedContentItem;

        private static bool IgnoreSelection;
        private static List<MpSelectionBehavior> _selectors = new List<MpSelectionBehavior>();

        public static void SetIgnoreSelection(bool ignore) {
            IgnoreSelection = ignore;
            if(IgnoreSelection) {
                foreach(var s in _selectors) {
                    s.DetachEvents();
                }
            } else {
                foreach (var s in _selectors) {
                    s.AttachEvents();
                }
            }
        }

        protected override void OnAttached() {
            _selectors.Add(this);
            AttachEvents();
        }

        private void AttachEvents() {
            AssociatedObject.PreviewMouseDown += AssociatedObject_PreviewMouseButtonDown;
            AssociatedObject.PreviewMouseUp += AssociatedObject_PreviewMouseUp;
        }

        private void DetachEvents() {
            AssociatedObject.PreviewMouseDown -= AssociatedObject_PreviewMouseButtonDown;
            AssociatedObject.PreviewMouseUp -= AssociatedObject_PreviewMouseUp;
        }

        private void AssociatedObject_PreviewMouseButtonDown(object sender, MouseButtonEventArgs e) {
            if(IgnoreSelection) {
                e.Handled = false;
                return;
            }
            isRightClick = e.ChangedButton == MouseButton.Right;

            if (MpClipTrayViewModel.Instance.SelectedContentItemViewModels.Count >= 1) {
                //MpClipTrayViewModel.Instance.IsPreSelection = true;
            }
            
            if(AssociatedObject is MpClipTileView) {
                var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;
                ctvm.IsSelected = true;
                ctvm.LastSelectedDateTime = DateTime.Now;
                var civm = ctvm.ItemViewModels.Where(x => x.IsHovering).FirstOrDefault();
                if(civm != null) {
                    civm.IsSelected = true;
                    civm.LastSubSelectedDateTime = DateTime.Now;
                }
            }else if (AssociatedObject.DataContext is MpContentItemViewModel civm) {
                LastSelectedContentItem = civm;
                wasSelected = civm.IsSelected;
                civm.IsSelected = true;
                civm.LastSubSelectedDateTime = DateTime.Now;
                if (!civm.Parent.IsSelected) {
                    civm.Parent.IsSelected = true;
                    civm.Parent.LastSelectedDateTime = DateTime.Now;                    
                }

                if(AssociatedObject.Name == "ClipTileToggleEditButton") {
                    MpClipTrayViewModel.Instance.ToggleTileExpandedCommand.Execute("edit");
                } else if (AssociatedObject.Name == "FlipButton") {
                    MpClipTrayViewModel.Instance.FlipTileCommand.Execute(civm);
                }
                if (!civm.Parent.IsExpanded) {
                    e.Handled = true;
                }
                
            }else if(AssociatedObject is MpRtbView) {
                if (isRightClick && (AssociatedObject.DataContext as MpContentItemViewModel).IsEditingContent) {
                    //show default context menu while editing
                    

                } 
            } 
            if(e.Handled != true)
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
            } else if(!MpHelpers.Instance.IsMultiSelectKeyDown() && !isAnyContentDragging) {
                if (AssociatedObject is MpClipTileView) {
                    var ctvm = AssociatedObject.DataContext as MpClipTileViewModel;
                    foreach (var sctvm in MpClipTrayViewModel.Instance.SelectedItems) {
                        if (sctvm != ctvm) {
                            if (sctvm.IsFlipped) {
                                sctvm.Parent.FlipTileCommand.Execute(sctvm);
                            }
                            sctvm.IsSelected = false;
                        }
                        foreach (var scivm in sctvm.SelectedItems) {
                            if (scivm != LastSelectedContentItem) {
                                scivm.IsSelected = false;
                            }
                        }
                    }
                } else {
                    //var civm = AssociatedObject.DataContext as MpContentItemViewModel;
                    //foreach (var sctvm in MpClipTrayViewModel.Instance.SelectedItems) {
                    //    if (sctvm != civm.Parent) {
                    //        if (sctvm.IsFlipped) {
                    //            sctvm.Parent.FlipTileCommand.Execute(sctvm);
                    //        }
                    //        sctvm.IsSelected = false;
                    //    }
                    //    foreach (var scivm in sctvm.SelectedItems) {
                    //        if (scivm != civm) {
                    //            scivm.IsSelected = false;
                    //        }
                    //    }
                    //}
                }
                LastSelectedContentItem = null;
            }

            if(!isAnyContentDragging && AssociatedObject is MpContentItemView) {
                e.Handled = true;
            }

            if (isRightClick) {
                ShowContextMenu();
            }
            
            isRightClick = false;
            wasSelected = false;
            //MpClipTrayViewModel.Instance.IsPreSelection = false;
        }

        #region Selection
     
        private void ShowContextMenu() {
            AssociatedObject.ContextMenu = new MpContentContextMenuView();
            AssociatedObject.ContextMenu.IsOpen = true;
        }

        #endregion
    }
}
