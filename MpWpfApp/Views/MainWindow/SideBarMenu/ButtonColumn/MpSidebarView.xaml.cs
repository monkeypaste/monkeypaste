using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;
namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpSidebarView.xaml
    /// </summary>
    public partial class MpSidebarView : MpUserControl<MpClipTrayViewModel> {
        public ToggleButton AppendModeToggleButton, MouseModeToggleButton;

        public MpSidebarView() {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            var toggleButtonList = this.GetVisualDescendents<ToggleButton>();
            foreach (var tb in toggleButtonList) {
                //tb.MouseEnter += Tb_MouseEnter;
            }

            BindingContext.PropertyChanged += BindingContext_PropertyChanged;
        }

        private void BindingContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(BindingContext.IsAppendMode):
                case nameof(BindingContext.IsAppendLineMode):
                    if (AppendModeToggleButton.ContextMenu != null) {
                        AppendModeToggleButton.ContextMenu.IsOpen = false;
                    }

                    //AppendModeToggleButton.IsChecked = BindingContext.IsAnyAppendMode;
                    BindingContext.OnPropertyChanged(nameof(BindingContext.IsAnyAppendMode));

                    break;
                case nameof(BindingContext.IsAutoCopyMode):
                case nameof(BindingContext.IsRightClickPasteMode):
                    if (AppendModeToggleButton.ContextMenu != null) {
                        AppendModeToggleButton.ContextMenu.IsOpen = false;
                    }
                    MouseModeToggleButton.IsChecked = BindingContext.IsAnyMouseModeEnabled;
                    BindingContext.OnPropertyChanged(nameof(BindingContext.IsAnyMouseModeEnabled));
                    break;
            }
        }

        //private void Tb_MouseEnter(object sender, MouseEventArgs e) {
        //    if(MpMainWindowViewModel.Instance.IsMainWindowLoading) {
        //        return;
        //    }
        //    BindingContext.OnPropertyChanged(nameof(BindingContext.IsAppendModeTooltip));
        //    BindingContext.OnPropertyChanged(nameof(BindingContext.IsAppPausedTooltip));
        //    BindingContext.OnPropertyChanged(nameof(BindingContext.IsRighClickPasteModeTooltip));
        //    BindingContext.OnPropertyChanged(nameof(BindingContext.IsAutoCopyModeTooltip));
        //    BindingContext.OnPropertyChanged(nameof(BindingContext.IsAutoAnalysisModeTooltip));
        //}

        private void ToggleButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            //e.Handled = true;

            ToggleButton tb = sender as ToggleButton;

            Rect mouseModeRect = tb.Bounds(Application.Current.MainWindow);
            AppModeToggleButton.IsChecked = false;
            AppendModeToggleButton.IsChecked = false;
            MouseModeToggleButton.IsChecked = false;

            if (tb == AppModeToggleButton) {
                tb.ContextMenu.Placement = PlacementMode.AbsolutePoint;
                tb.ContextMenu.IsOpen = true;
                tb.ContextMenu.HorizontalOffset = mouseModeRect.Right;
                tb.ContextMenu.VerticalOffset = mouseModeRect.Top;                            
            } else if (tb.Name == "AppendToggleButton") {
                BindingContext.ToggleAppendModeCommand.Execute(null);
                var sp = tb.GetVisualAncestor<StackPanel>();
                sp.GetVisualDescendents<ToggleButton>().Where(x => x != tb)
                    .ForEach(x => x.IsChecked = BindingContext.IsAppendLineMode);
                tb.IsChecked = BindingContext.IsAppendMode;
            } else if (tb.Name == "AppendLineToggleButton") {
                BindingContext.ToggleAppendLineModeCommand.Execute(null);
                var sp = tb.GetVisualAncestor<StackPanel>();
                sp.GetVisualDescendents<ToggleButton>().Where(x => x != tb)
                    .ForEach(x => x.IsChecked = BindingContext.IsAppendMode);
                tb.IsChecked = BindingContext.IsAppendLineMode;
            } else if (tb.Name == "AutoCopyToggleButton") {
                BindingContext.ToggleAutoCopyModeCommand.Execute(null);
                tb.IsChecked = BindingContext.IsAutoCopyMode;
            } else if (tb.Name == "RightClickPasteToggleButton") {
                BindingContext.ToggleRightClickPasteCommand.Execute(null);
                tb.IsChecked = BindingContext.IsRightClickPasteMode;
            } else {
                var popup = tb.GetVisualDescendent<Popup>();
                popup.IsOpen = !popup.IsOpen;
                popup.Placement = PlacementMode.AbsolutePoint;
                if (popup.IsOpen) {
                    popup.HorizontalOffset = mouseModeRect.Left;
                    popup.VerticalOffset = mouseModeRect.Top - popup.ActualHeight;
                }
            }
        }

        private void AutoCopyToggleButton_Loaded(object sender, RoutedEventArgs e) {
            var tb = sender as ToggleButton;
            tb.IsChecked = BindingContext.IsAutoCopyMode;
        }

        private void RightClickPasteToggleButton_Loaded(object sender, RoutedEventArgs e) {
            var tb = sender as ToggleButton;
            tb.IsChecked = BindingContext.IsRightClickPasteMode;
        }

        private void AppendToggleButton_Loaded(object sender, RoutedEventArgs e) {
            var tb = sender as ToggleButton;
            tb.IsChecked = BindingContext.IsAppendMode;
        }

        private void AppendLineToggleButton_Loaded(object sender, RoutedEventArgs e) {
            var tb = sender as ToggleButton;
            tb.IsChecked = BindingContext.IsAppendLineMode;
        }

        private void AppendModeToggleButton_Loaded(object sender, RoutedEventArgs e) {
            var tb = sender as ToggleButton;
            tb.IsChecked = false;
        }

        private void MouseModeToggleButton_Loaded(object sender, RoutedEventArgs e) {
            var tb = sender as ToggleButton;
            tb.IsChecked = false;
        }

        private void AppModeToggleButton_Loaded(object sender, RoutedEventArgs e) {
            var tb = sender as ToggleButton;
            var amtbsp = AppModeToggleButton.Resources["AppModeToggleButtonStackPanel"] as StackPanel;

            MouseModeToggleButton = amtbsp.Children[0] as ToggleButton;
            AppendModeToggleButton = amtbsp.Children[1] as ToggleButton;

            MouseModeToggleButton.Width = AppendModeToggleButton.Width = tb.ActualWidth;
            MouseModeToggleButton.Height = AppendModeToggleButton.Height = tb.ActualHeight;
            MouseModeToggleButton.DataContext = AppendModeToggleButton.DataContext = tb.DataContext;
        }
    }
}
