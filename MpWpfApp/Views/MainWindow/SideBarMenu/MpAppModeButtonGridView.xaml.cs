using System;
using System.Collections.Generic;
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

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpAppModeButtonGridView.xaml
    /// </summary>
    public partial class MpAppModeButtonGridView : MpUserControl<MpAppModeViewModel> {
        public ToggleButton AppendModeToggleButton, MouseModeToggleButton;

        public MpAppModeButtonGridView() {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e) {
            var toggleButtonList = this.GetVisualDescendents<ToggleButton>();
            foreach (var tb in toggleButtonList) {
                tb.MouseEnter += Tb_MouseEnter;
            }

            BindingContext.PropertyChanged += BindingContext_PropertyChanged;
        }

        private void BindingContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(BindingContext.IsAppendMode):
                case nameof(BindingContext.IsAppendLineMode):
                    AppendModeToggleButton.ContextMenu.IsOpen = false;
                    AppendModeToggleButton.IsChecked = BindingContext.IsAnyAppendMode;
                    BindingContext.OnPropertyChanged(nameof(BindingContext.IsAnyAppendMode));

                    break;
                case nameof(BindingContext.IsAutoCopyMode):
                case nameof(BindingContext.IsRightClickPasteMode):
                    MouseModeToggleButton.ContextMenu.IsOpen = false;
                    MouseModeToggleButton.IsChecked = BindingContext.IsAnyMouseModeEnabled;
                    BindingContext.OnPropertyChanged(nameof(BindingContext.IsAnyMouseModeEnabled));
                    break;
            }
        }

        private void Tb_MouseEnter(object sender, MouseEventArgs e) {
            if(MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                return;
            }
            BindingContext.OnPropertyChanged(nameof(BindingContext.IsAppendModeTooltip));
            BindingContext.OnPropertyChanged(nameof(BindingContext.IsAppPausedTooltip));
            BindingContext.OnPropertyChanged(nameof(BindingContext.IsRighClickPasteModeTooltip));
            BindingContext.OnPropertyChanged(nameof(BindingContext.IsAutoCopyModeTooltip));
            BindingContext.OnPropertyChanged(nameof(BindingContext.IsAutoAnalysisModeTooltip));
        }

        private void ToggleButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            e.Handled = true;

            ToggleButton tb = sender as ToggleButton;


            Rect mouseModeRect = tb.Bounds(Application.Current.MainWindow);

            if (tb == AppModeToggleButton) {
                tb.ContextMenu.Placement = PlacementMode.AbsolutePoint;
                tb.ContextMenu.IsOpen = true;
                tb.ContextMenu.HorizontalOffset = mouseModeRect.Right;
                tb.ContextMenu.VerticalOffset = mouseModeRect.Top;
            } else {
                var popup = tb.GetVisualDescendent<Popup>();
                popup.IsOpen = !popup.IsOpen;
                popup.Placement = PlacementMode.AbsolutePoint;
                if(popup.IsOpen) {
                    popup.HorizontalOffset = mouseModeRect.Left;
                    popup.VerticalOffset = mouseModeRect.Top - popup.ActualHeight;
                }
            }

        }

        private void AnalyzerToggleButton_Click(object sender, RoutedEventArgs e) {
            MpAnalyticItemCollectionViewModel.Instance.IsVisible = !MpAnalyticItemCollectionViewModel.Instance.IsVisible;
            AnalyzerToggleButton.IsChecked = MpAnalyticItemCollectionViewModel.Instance.IsVisible;
            TagTreeToggleButton.IsChecked = MpTagTrayViewModel.Instance.IsVisible;
        }

        private void TagTreeToggleButton_Click(object sender, RoutedEventArgs e) {
            MpTagTrayViewModel.Instance.IsVisible = !MpTagTrayViewModel.Instance.IsVisible;
            TagTreeToggleButton.IsChecked = MpTagTrayViewModel.Instance.IsVisible;
            AnalyzerToggleButton.IsChecked = MpAnalyticItemCollectionViewModel.Instance.IsVisible;
        }

        private void MatcherToggleButton_Click(object sender, RoutedEventArgs e) {
            MpMatcherCollectionViewModel.Instance.IsVisible = !MpMatcherCollectionViewModel.Instance.IsVisible;

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
