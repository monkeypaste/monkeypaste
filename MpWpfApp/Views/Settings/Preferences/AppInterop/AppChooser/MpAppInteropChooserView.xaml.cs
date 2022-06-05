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
using System.Linq;
using System.Collections.ObjectModel;
using MonkeyPaste;
using MonkeyPaste.Common;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for MpAppInteropChooserView.xaml
    /// </summary>
    public partial class MpAppInteropChooserView : MpUserControl<MpAppCollectionViewModel> {
        public MpAppInteropChooserView() {
            InitializeComponent();
        }

        private void Border_Loaded(object sender, RoutedEventArgs e) {
            BindingContext.PropertyChanged += BindingContext_PropertyChanged;
            BindingContext.FilteredApps = BindingContext.Items;

            if(BindingContext.SelectedItem == null && BindingContext.Items.Count > 0) {
                BindingContext.SelectedItem = BindingContext.Items[0];
            }
        }

        private void BindingContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(BindingContext.SelectedItem):
                    if(BindingContext.SelectedItem == null) {
                        AppInteropDataGridView.Visibility = Visibility.Hidden;
                    } else {
                        AppInteropDataGridView.Visibility = Visibility.Visible;
                        AppInteropDataGridView.AppInteropDataGrid.Items.Refresh();
                    }
                    break;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {

            var tb = sender as TextBox;
            if (BindingContext.SelectedItem.AppName.ToLower() == tb.Text.ToLower()) {
                return;
            }

            UpdateAppPaths(tb.Text);
            
            if(BindingContext.SelectedItem == null || 
               BindingContext.SelectedItem.AppName.ToLower() != tb.Text.ToLower()) {
                AppChooserPopup.IsOpen = true;
                
            } else {
                AppChooserPopup.IsOpen = false;
            }
            //AppChooserListBox.Items.Refresh();
        }

        private void UpdateAppPaths(string text) {
            BindingContext.FilteredApps = new ObservableCollection<MpAppViewModel>(
                BindingContext.Items.Where(x => x.AppName.ToLower().StartsWith(text.ToLower())));

            if(BindingContext.SelectedItem == null || BindingContext.FilteredApps.Count == 0 ||
                BindingContext.SelectedItem.AppName.ToLower() == SelectedAppTextBox.Text.ToLower()) {
                BindingContext.FilteredApps = BindingContext.Items;
            }

        }

        private void SelectedAppTextBox_KeyUp(object sender, KeyEventArgs e) {
            if (BindingContext.SelectedItem == null) {
                if(BindingContext.Items.Count > 0) {
                    BindingContext.SelectedItem = BindingContext.Items[0];
                } else {
                    return;
                }                
            }
            int curSelectedIdx = BindingContext.FilteredApps.IndexOf(BindingContext.SelectedItem);
            int newSelectedIdx = curSelectedIdx;
            if(e.Key == Key.Up) {
                newSelectedIdx--;
            } else if(e.Key == Key.Down) {
                newSelectedIdx++;
            } else if(e.Key == Key.Enter || e.Key == Key.Escape) {
                AppChooserPopup.IsOpen = false;
            }
            if(newSelectedIdx < 0 || newSelectedIdx >= BindingContext.FilteredApps.Count) {
                newSelectedIdx = curSelectedIdx;
                AppChooserPopup.IsOpen = true;
            }
            //if(newSelectedIdx != curSelectedIdx) {
            //    BindingContext.FilteredApps.ForEach(x => x.IsSelected = BindingContext.FilteredApps.IndexOf(x) == newSelectedIdx);
            //}
        }

        private void AppChooserListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if(BindingContext.SelectedItem == null) {
                return;
            }
            BindingContext.OnPropertyChanged(nameof(BindingContext.SelectedItem));
            SelectedAppTextBox.Text = BindingContext.SelectedItem.AppName;
            Keyboard.Focus(SelectedAppTextBox);
            SelectedAppTextBox.SelectAll();
        }

        private void AppChooserListBox_MouseUp(object sender, MouseButtonEventArgs e) {
            AppChooserPopup.IsOpen = false;
        }

        private void AppChooserListBox_KeyUp(object sender, KeyEventArgs e) {
            if(e.Key == Key.Enter || e.Key == Key.Escape) {
                AppChooserPopup.IsOpen = false;
                if(e.Key == Key.Enter) {
                    BindingContext.FilteredApps.ForEach(x => x.IsSelected = BindingContext.FilteredApps.IndexOf(x) == AppChooserListBox.Items.IndexOf(AppChooserListBox.SelectedItem));
                }
            }
        }

        private void SelectedAppTextBox_GotFocus(object sender, RoutedEventArgs e) {
            UpdateAppPaths(SelectedAppTextBox.Text);
            AppChooserPopup.IsOpen = true;
        }

        private void SelectedAppTextBox_LostFocus(object sender, RoutedEventArgs e) {
            if(!AppChooserListBox.IsFocused) {
                AppChooserPopup.IsOpen = false;
            }
        }

        private void SelectedAppTextBox_MouseUp(object sender, MouseButtonEventArgs e) {
            UpdateAppPaths(SelectedAppTextBox.Text);
            AppChooserPopup.IsOpen = true;
        }

        private void ListBoxItem_LostFocus(object sender, RoutedEventArgs e) {
            AppChooserPopup.IsOpen = false;
        }
    }
}
