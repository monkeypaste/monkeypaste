using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpMultiSelectListBox : AnimatedListBox {
        private static MpContentContextMenuView _contentContextMenu;

        protected override DependencyObject GetContainerForItemOverride() {
            return new MpMultiSelectListBoxItem();
        }

        class MpMultiSelectListBoxItem : ListBoxItem {
            //private bool _deferSelection = false;

            protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
                //base.OnMouseLeftButtonDown(e);
                SelectItem();
            }

            protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
                base.OnMouseLeftButtonUp(e);
            }

            protected override void OnMouseRightButtonDown(MouseButtonEventArgs e) {
                SelectItem();
                if (DataContext is MpClipTileViewModel ctvm) {
                    if(ctvm.IsAnyEditingContent) {
                        base.OnMouseRightButtonDown(e);
                        return;
                    }
                } else if (DataContext is MpContentItemViewModel civm) {
                    if(civm.IsEditingContent) {
                        base.OnMouseRightButtonDown(e);
                        return;
                    }
                }

                if (_contentContextMenu == null) {
                    _contentContextMenu = new MpContentContextMenuView();
                }
                ContextMenu = _contentContextMenu;
                ContextMenu.PlacementTarget = this;
                ContextMenu.IsOpen = true;
            }

            private void SelectItem() {
                if (!IsSelected) {
                    IsSelected = true;

                    if (DataContext is MpClipTileViewModel ctvm) {
                        ctvm.LastSelectedDateTime = DateTime.Now;
                        if (ctvm.SelectedItems.Count == 0 && ctvm.HeadItem != null) {
                            ctvm.HeadItem.IsSelected = true;
                            ctvm.HeadItem.LastSubSelectedDateTime = DateTime.Now;
                        }
                        if (!MpHelpers.Instance.IsMultiSelectKeyDown()) {
                            foreach (var octvm in ctvm.Parent.ClipTileViewModels) {
                                if (octvm != ctvm) {
                                    octvm.ClearSelection();
                                }
                            }
                        }
                    } else if (DataContext is MpContentItemViewModel civm) {
                        civm.LastSubSelectedDateTime = DateTime.Now;
                        if (civm.IsSelected && !civm.Parent.IsSelected) {
                            civm.Parent.IsSelected = true;
                            civm.Parent.LastSelectedDateTime = DateTime.Now;

                            if (!MpHelpers.Instance.IsMultiSelectKeyDown()) {
                                foreach (var octvm in civm.Parent.Parent.ClipTileViewModels) {
                                    if (octvm != civm.Parent) {
                                        octvm.ClearSelection();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public ScrollViewer ScrollViewer {
            get {
                Border border = (Border)VisualTreeHelper.GetChild(this, 0);

                return (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
            }
        }
    }
}
