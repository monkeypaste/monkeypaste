using MonkeyPaste;
using System.Windows;
using System.Windows.Forms;

namespace MpWpfApp {
    public class MpMenuExtension : DependencyObject {
        #region ContextMenuViewModel Property

        public static MpIContextMenuViewModel GetContextMenuViewModel(DependencyObject obj) {
            return (MpIContextMenuViewModel)obj.GetValue(ContextMenuViewModelProperty);
        }
        public static void SetContextMenuViewModel(DependencyObject obj, MpIContextMenuViewModel value) {
            obj.SetValue(ContextMenuViewModelProperty, value);
        }

        public static readonly DependencyProperty ContextMenuViewModelProperty =
            DependencyProperty.RegisterAttached(
            "ContextMenuViewModel",
            typeof(MpIContextMenuViewModel),
            typeof(MpMenuExtension),
            new UIPropertyMetadata(null));

        #endregion

        #region PopupMenuViewModel Property

        public static MpIPopupMenuViewModel GetPopupMenuViewModel(DependencyObject obj) {
            return (MpIPopupMenuViewModel)obj.GetValue(PopupMenuViewModelProperty);
        }
        public static void SetPopupMenuViewModel(DependencyObject obj, MpIPopupMenuViewModel value) {
            obj.SetValue(PopupMenuViewModelProperty, value);
        }

        public static readonly DependencyProperty PopupMenuViewModelProperty =
            DependencyProperty.RegisterAttached(
            "PopupMenuViewModel",
            typeof(MpIPopupMenuViewModel),
            typeof(MpMenuExtension),
            new UIPropertyMetadata(null));

        #endregion

        #region ViewModelToSelect Property

        public static MpISelectableViewModel GetViewModelToSelect(DependencyObject obj) {
            return (MpISelectableViewModel)obj.GetValue(ViewModelToSelectProperty);
        }
        public static void SetViewModelToSelect(DependencyObject obj, MpISelectableViewModel value) {
            obj.SetValue(ViewModelToSelectProperty, value);
        }

        public static readonly DependencyProperty ViewModelToSelectProperty =
            DependencyProperty.RegisterAttached(
            "ViewModelToSelect",
            typeof(MpISelectableViewModel),
            typeof(MpMenuExtension),
            new UIPropertyMetadata(null));

        #endregion

        #region IsEnabled Property

        public static bool GetIsEnabled(DependencyObject obj) {
            return (bool)obj.GetValue(IsEnabledProperty);
        }
        public static void SetIsEnabled(DependencyObject obj, bool value) {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(MpMenuExtension),
            new UIPropertyMetadata() {
                DefaultValue = false,
                PropertyChangedCallback = (s,e) => {
                    if(e.NewValue is bool isEnabled) {
                        if(isEnabled) {
                            if (s is FrameworkElement fe) {
                                if(fe.IsLoaded) {
                                    Fe_Loaded(s, null);
                                } else {
                                    fe.Loaded += Fe_Loaded;
                                }
                            }
                        } else {
                            Fe_Unloaded(s, null);
                        }
                    } else {
                        Fe_Unloaded(s, null);
                    }                    
                }
            });

        private static void Fe_Loaded(object sender, RoutedEventArgs e) {
            if(sender is FrameworkElement fe) {
                fe.Unloaded += Fe_Unloaded;
                if(e == null) {
                    fe.Loaded += Fe_Loaded;
                }

                var pumvm = GetPopupMenuViewModel(fe);
                if(pumvm != null) {
                    fe.PreviewMouseLeftButtonDown += Fe_PreviewMouseLeftButtonDown;
                }
                var cmvm = GetContextMenuViewModel(fe);
                if(cmvm != null) {
                    fe.PreviewMouseRightButtonDown += Fe_PreviewMouseRightButtonDown;
                }
            }
        }               

        private static void Fe_Unloaded(object sender, RoutedEventArgs e) {
            if(sender is FrameworkElement fe) {
                fe.Loaded -= Fe_Loaded;
                fe.Unloaded -= Fe_Unloaded;
            }
        }

        private static void Fe_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if(sender is FrameworkElement fe) {
                var svm = GetViewModelToSelect(fe);
                if(svm != null) {
                    svm.IsSelected = true;
                }
                MpContextMenuView.Instance.DataContext = GetPopupMenuViewModel(fe).PopupMenuViewModel;
                MpContextMenuView.Instance.PlacementTarget = fe;
                MpContextMenuView.Instance.IsOpen = true;
            }
        }

        private static void Fe_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (sender is FrameworkElement fe) {
                var svm = GetViewModelToSelect(fe);
                if (svm != null) {
                    svm.IsSelected = true;
                }
                MpContextMenuView.Instance.DataContext = GetContextMenuViewModel(fe).ContextMenuViewModel;
                MpContextMenuView.Instance.PlacementTarget = fe;
                MpContextMenuView.Instance.IsOpen = true;
            }
        }

        #endregion
    }
}