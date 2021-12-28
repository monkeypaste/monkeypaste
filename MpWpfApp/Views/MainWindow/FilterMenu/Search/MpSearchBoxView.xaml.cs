using MonkeyPaste;
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
    /// Interaction logic for MpSearchBoxView.xaml
    /// </summary>
    public partial class MpSearchBoxView : MpUserControl<MpSearchBoxViewModel> {
        private ContextMenu _searchByContextMenu;

        public MpSearchBoxView() {
            InitializeComponent();
        }
        private void SearchViewContainerStackPanel_Loaded(object sender, RoutedEventArgs e) {
            //SearchBox.Focus();

            if(_searchByContextMenu == null) {
                InitContextMenu();
            }
        }

        private void SearchTextBoxBorder_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (DataContext != null && DataContext is MpSearchBoxViewModel sbvm) {
                sbvm.OnSearchTextBoxFocusRequest += Sbvm_OnSearchTextBoxFocusRequest;
            }
        }

        private void Sbvm_OnSearchTextBoxFocusRequest(object sender, EventArgs e) {
            //SearchBox.Focus(); 
            SearchBox.CaretIndex = SearchBox.Text.Length - 1;
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e) {
            var sbvm = DataContext as MpSearchBoxViewModel;
            sbvm.IsTextBoxFocused = true;

            MpHelpers.Instance.RunOnMainThread(async () => {
                while(MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                    await Task.Delay(100);
                }
                MpShortcutCollectionViewModel.Instance.ApplicationHook.MouseDown += ApplicationHook_MouseDown;
            });
            
        }

        private void SearchBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            BindingContext.IsTextBoxFocused = true;
            MpShortcutCollectionViewModel.Instance.ApplicationHook.MouseDown += ApplicationHook_MouseDown;
        }

        private void ApplicationHook_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
            var tbr = new Rect(0, 0, SearchTextBoxBorder.Width, SearchTextBoxBorder.Height);
            var tb_mp = Application.Current.MainWindow.TranslatePoint(new Point(e.Location.X, e.Location.Y), SearchTextBoxBorder);
            if (!tbr.Contains(tb_mp)) {
                BindingContext.IsTextBoxFocused = false;
                MpShortcutCollectionViewModel.Instance.ApplicationHook.MouseDown -= ApplicationHook_MouseDown;
                
            }
        }

        private void SearchDropDownButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            SearchDropDownButton.ContextMenu = _searchByContextMenu;
            SearchDropDownButton.ContextMenu.PlacementTarget = SearchDropDownButton;
            SearchDropDownButton.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Top;
            SearchDropDownButton.ContextMenu.IsOpen = true;
        }

        private void InitContextMenu() {
            MpHelpers.Instance.RunOnMainThread(async () => {
                _searchByContextMenu = new ContextMenu();

                foreach (var sfvm in BindingContext.Filters) {
                    _searchByContextMenu.Items.Add(CreateSearchByMenuItem(sfvm));
                }
                _searchByContextMenu.Items.Add(new Separator());
                var picker = await CreateSavedSearchPicker();
                _searchByContextMenu.Items.Add(picker);
            });
        }

        private object CreateSearchByMenuItem(MpSearchFilterViewModel sfvm) {
            var menuItem = new MenuItem();
            if(sfvm.IsSeperator) {
                return new Separator();
            } else {
                var cb = new CheckBox();
                MpHelpers.Instance.CreateBinding(
                    sfvm,
                    new PropertyPath(nameof(sfvm.IsChecked)),
                    cb, CheckBox.IsCheckedProperty, BindingMode.TwoWay);

                MpHelpers.Instance.CreateBinding(
                    sfvm,
                    new PropertyPath(nameof(sfvm.IsEnabled)),
                    cb, CheckBox.IsEnabledProperty);

                var l = new Label();

                MpHelpers.Instance.CreateBinding(
                    sfvm,
                    new PropertyPath(nameof(sfvm.Label)),
                    l, Label.ContentProperty);

                menuItem.Icon = cb;
                menuItem.Header = l;
            }

            return menuItem;
        }

        private async Task<object> CreateSavedSearchPicker() {
            var usl = await MpDb.Instance.GetItemsAsync<MpUserSearch>();

            var comboBox = new ComboBox() {
                Width = 200
            };
            usl.OrderByDescending(x => x.CreatedDateTime).ForEach(x => comboBox.Items.Add(x.Name));

            var b = new Border() {
                CornerRadius = new CornerRadius(10),
                BorderThickness = new Thickness(2),
                BorderBrush = Brushes.Black,
                Background = Brushes.DimGray
            };
            b.Child = comboBox;

            var menuItem = new MenuItem();
            menuItem.Icon = null;
            menuItem.Header = b;

            return menuItem;
        }

        private void ClearTextBoxButton_Click(object sender, RoutedEventArgs e) {
            var sbvm = DataContext as MpSearchBoxViewModel;
            sbvm.ClearTextCommand.Execute(null);
            SearchBox.Focus();
        }

        private void SearchBox_KeyUp(object sender, KeyEventArgs e) {
            if(e.Key == Key.Enter) {
                BindingContext.PerformSearchCommand.Execute(null);
            }
        }

        private void ClearTextBoxButton_MouseEnter(object sender, MouseEventArgs e) {
            BindingContext.IsOverClearTextButton = true;
        }

        private void ClearTextBoxButton_MouseLeave(object sender, MouseEventArgs e) {
            BindingContext.IsOverClearTextButton = false;
        }

        private void SearchBox_MouseEnter(object sender, MouseEventArgs e) {
            MpCursorViewModel.Instance.CurrentCursor = MpCursorType.IBeam;
        }

        private void SearchBox_MouseLeave(object sender, MouseEventArgs e) {
            MpCursorViewModel.Instance.CurrentCursor = MpCursorType.Default;
        }

        private void SaveSearchButton_MouseEnter(object sender, MouseEventArgs e) {
            BindingContext.IsOverSaveSearchButton = true;
        }

        private void SaveSearchButton_MouseLeave(object sender, MouseEventArgs e) {
            BindingContext.IsOverSaveSearchButton = false;
        }

        private void AddOrClearSearchCriteriaButton_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                return;
            }
            MpHelpers.Instance.RunOnMainThread(async () => {
                await Task.Delay(500);
                UpdateLayout();
                AddOrClearSearchCriteriaButton.UpdateLayout();
            });
        }
    }
}
