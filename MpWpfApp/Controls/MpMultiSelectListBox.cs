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
            private bool _deferSelection = false;
            private bool _isDeferSelectionEnabled = true;

            protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
                if(_isDeferSelectionEnabled) {
                    OnDeferMouseLeftButtonDown(e);
                } else {
                    SelectItem();
                }                
            }

            protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
                if (_isDeferSelectionEnabled) {
                    OnDeferMouseLeftButtonDown(e);
                } else {
                    base.OnMouseLeftButtonUp(e);
                }
            }

            protected override void OnMouseRightButtonDown(MouseButtonEventArgs e) {
                if(_isDeferSelectionEnabled) {
                    base.OnMouseRightButtonDown(e);
                    return;
                } 

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

            protected override void OnMouseLeave(MouseEventArgs e) {
                if (_isDeferSelectionEnabled) {
                    // abort deferred Down
                    _deferSelection = false;
                }
                base.OnMouseLeave(e);
            }

            protected override void OnSelected(RoutedEventArgs e) {
                if(_isDeferSelectionEnabled) {
                    this.UpdateExtendedSelection();
                } else {
                    base.OnSelected(e);
                }
            }

            private void SelectItem() {
                this.UpdateExtendedSelection();
                return;

                if (!IsSelected) {
                    IsSelected = true;

                    if (DataContext is MpClipTileViewModel ctvm) {
                        if (ctvm.SelectedItems.Count == 0 && ctvm.HeadItem != null) {
                            ctvm.HeadItem.IsSelected = true;
                        }
                        if (!MpHelpers.Instance.IsMultiSelectKeyDown()) {
                            foreach (var octvm in ctvm.Parent.Items) {
                                if (octvm != ctvm) {
                                    octvm.ClearSelection();
                                }
                            }
                        }
                    } else if (DataContext is MpContentItemViewModel civm) {
                        if (civm.IsSelected && !civm.Parent.IsSelected) {
                            civm.Parent.IsSelected = true;
                        }
                        if (!MpHelpers.Instance.IsMultiSelectKeyDown()) {
                            foreach (var octvm in civm.Parent.Parent.Items) {
                                if (octvm != civm.Parent) {
                                    octvm.ClearSelection();
                                }
                            }
                        } 
                    }
                }
            }

            private void OnDeferMouseLeftButtonDown(MouseButtonEventArgs e) {
                if (e.ClickCount == 1 && IsSelected) {
                    // the user may start a drag by clicking into selected items
                    // delay destroying the selection to the Up event
                    _deferSelection = true;
                } else {
                    base.OnMouseLeftButtonDown(e);
                }
            }

            private void OnDeferMouseLeftButtonUp(MouseButtonEventArgs e) {
                if (_deferSelection) {
                    try {
                        base.OnMouseLeftButtonDown(e);
                    }
                    finally {
                        _deferSelection = false;
                    }
                }
                base.OnMouseLeftButtonUp(e);
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
