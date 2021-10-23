using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpMultiSelectListBox : AnimatedListBox {
        protected override DependencyObject GetContainerForItemOverride() {
            return new MpMultiSelectListBoxItem();
        }

        #region SelectionChangedCommandParameter dep prop

        public static object GetSelectionChangedCommandParameter(DependencyObject obj) {
            return (int)obj.GetValue(SelectionChangedCommandParameterProperty);
        }
        public static void SetSelectionChangedCommandParameter(DependencyObject obj, object value) {
            obj.SetValue(SelectionChangedCommandParameterProperty, value);
        }
        public static readonly DependencyProperty SelectionChangedCommandParameterProperty =
          DependencyProperty.RegisterAttached(
            "SelectionChangedCommandParameter",
            typeof(object),
            typeof(MpMultiSelectListBox),
            new FrameworkPropertyMetadata());

        #endregion

        #region SelectionChangedCommand dep prop

        public static ICommand GetSelectionChangedCommand(DependencyObject obj) {
            return (ICommand)obj.GetValue(SelectionChangedCommandProperty);
        }
        public static void SetSelectionChangedCommand(DependencyObject obj, ICommand value) {
            obj.SetValue(SelectionChangedCommandProperty, value);
        }
        public static readonly DependencyProperty SelectionChangedCommandProperty =
          DependencyProperty.RegisterAttached(
            "SelectionChangedCommand",
            typeof(ICommand),
            typeof(MpMultiSelectListBox),
            new FrameworkPropertyMetadata {
                PropertyChangedCallback = (obj, e) => {
                    if (e.NewValue == null) {
                        return;
                    }
                }
            });

        #endregion

        

        class MpMultiSelectListBoxItem : ListBoxItem {
            private static MpContentContextMenuView _ContentContextMenu;
            private bool _deferSelection = false;
            private bool _isDeferSelectionEnabled = false;

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

                if (_ContentContextMenu == null) {
                    _ContentContextMenu = new MpContentContextMenuView();
                }
                ContextMenu = _ContentContextMenu;
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

                if(_isDeferSelectionEnabled) {
                    if (DataContext is MpClipTileViewModel ctvm) {
                        ctvm.OnPropertyChanged(nameof(ctvm.IsSelected));
                    }
                    if (DataContext is MpContentItemViewModel civm) {
                        civm.OnPropertyChanged(nameof(civm.IsSelected));
                    }
                }
            }

            protected override void OnUnselected(RoutedEventArgs e) {
                base.OnUnselected(e);

                if (_isDeferSelectionEnabled) {
                    if (DataContext is MpClipTileViewModel ctvm) {
                        ctvm.OnPropertyChanged(nameof(ctvm.IsSelected));
                    }
                    if (DataContext is MpContentItemViewModel civm) {
                        civm.OnPropertyChanged(nameof(civm.IsSelected));
                    }
                }
            }

            private void SelectItem() {
                //this.UpdateExtendedSelection();
                //return;

                if (!IsSelected) {
                    IsSelected = true;

                    if (DataContext is MpClipTileViewModel ctvm) {
                        if (ctvm.SelectedItems.Count == 0 && ctvm.HeadItem != null) {
                            ctvm.HeadItem.IsSelected = true;
                        }
                        if (!MpShortcutCollectionViewModel.Instance.IsMultiSelectKeyDown) {
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
                        if (!MpShortcutCollectionViewModel.Instance.IsMultiSelectKeyDown) {
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
    }
}
