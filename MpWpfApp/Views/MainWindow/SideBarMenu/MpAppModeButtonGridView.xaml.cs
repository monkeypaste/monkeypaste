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

            tb.ContextMenu.Placement = PlacementMode.AbsolutePoint;

            Rect mouseModeRect = tb.Bounds(Application.Current.MainWindow);

            tb.ContextMenu.HorizontalOffset = mouseModeRect.Right;
            tb.ContextMenu.VerticalOffset = mouseModeRect.Top;

            tb.ContextMenu.IsOpen = true;
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
    }
}
