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
        public MpSearchBoxView() {
            InitializeComponent();
        }

        private void SearchTextBoxBorder_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (DataContext != null && DataContext is MpSearchBoxViewModel sbvm) {
                sbvm.OnSearchTextBoxFocusRequest += Sbvm_OnSearchTextBoxFocusRequest;
            }
        }

        private void Sbvm_OnSearchTextBoxFocusRequest(object sender, EventArgs e) {
            SearchBox.Focus();
            SearchBox.CaretIndex = SearchBox.Text.Length - 1;
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e) {
            var sbvm = DataContext as MpSearchBoxViewModel;
            sbvm.IsTextBoxFocused = true;
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e) {
            var sbvm = DataContext as MpSearchBoxViewModel;
            sbvm.IsTextBoxFocused = false;
        }

        private void SearchDropDownButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            var sbvm = DataContext as MpSearchBoxViewModel;
            var searchByContextMenu = new ContextMenu();

            searchByContextMenu.Items.Add(
                CreateSearchByMenuItem("Case Sensitive", sbvm.SearchByIsCaseSensitive));
            searchByContextMenu.Items.Add(
                CreateSearchByMenuItem("Collection", sbvm.SearchByTag));
            searchByContextMenu.Items.Add(
                CreateSearchByMenuItem("Title", sbvm.SearchByTitle));
            searchByContextMenu.Items.Add(
                CreateSearchByMenuItem("Text", sbvm.SearchByRichText));
            searchByContextMenu.Items.Add(
                CreateSearchByMenuItem("Source Url", sbvm.SearchByUrl));
            searchByContextMenu.Items.Add(
                CreateSearchByMenuItem("File List", sbvm.SearchByFileList));
            searchByContextMenu.Items.Add(
                CreateSearchByMenuItem("Image", sbvm.SearchByImage));
            searchByContextMenu.Items.Add(
                CreateSearchByMenuItem("Application Name", sbvm.SearchByApplicationName));
            searchByContextMenu.Items.Add(
                CreateSearchByMenuItem("Process Name", sbvm.SearchByProcessName));


            ((MenuItem)searchByContextMenu.Items[1]).Visibility = Visibility.Collapsed;

            searchByContextMenu.Closed += (s1, e3) => {
                for (int i = 0; i < searchByContextMenu.Items.Count; i++) {
                    var isChecked = ((CheckBox)((MenuItem)searchByContextMenu.Items[i]).Icon).IsChecked.Value;
                    switch (i) {
                        case 0:
                            sbvm.SearchByIsCaseSensitive = isChecked;
                            break;
                        case 1:
                            sbvm.SearchByTag = isChecked;
                            break;
                        case 2:
                            sbvm.SearchByTitle = isChecked;
                            break;
                        case 3:
                            sbvm.SearchByRichText = isChecked;
                            break;
                        case 4:
                            sbvm.SearchByUrl = isChecked;
                            break;
                        case 5:
                            sbvm.SearchByFileList = isChecked;
                            break;
                        case 6:
                            sbvm.SearchByImage = isChecked;
                            break;
                        case 7:
                            sbvm.SearchByApplicationName = isChecked;
                            break;
                        case 8:
                            sbvm.SearchByProcessName = isChecked;
                            break;
                    }
                }
                Properties.Settings.Default.Save();
            };

            SearchDropDownButton.ContextMenu = searchByContextMenu;
            searchByContextMenu.PlacementTarget = SearchTextBoxBorder;
            searchByContextMenu.IsOpen = true;
        }

        private MenuItem CreateSearchByMenuItem(string label, bool propertyValue) {
            var cb = new CheckBox();
            cb.IsChecked = propertyValue;

            var l = new Label();
            l.Content = label;

            var menuItem = new MenuItem();
            menuItem.Icon = cb;
            menuItem.Header = l;

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
            MpMouseViewModel.Instance.CurrentCursor = MpCursorType.IBeam;
        }

        private void SearchBox_MouseLeave(object sender, MouseEventArgs e) {
            MpMouseViewModel.Instance.CurrentCursor = MpCursorType.Default;
        }
    }
}
