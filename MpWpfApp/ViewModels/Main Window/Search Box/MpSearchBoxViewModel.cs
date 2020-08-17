using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpSearchBoxViewModel : MpViewModelBase {
        #region View Models
        public MpMainWindowViewModel MainWindowViewModel { get; set; }
        #endregion
        #region Properties
        private string _searchText;
        public string SearchText {
            get {
                return _searchText;
            }
            set {
                if (_searchText != value) {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                }
            }
        }

        private bool _isSearchTextBoxFocused = false;
        public bool IsSearchTextBoxFocused {
            get {
                return _isSearchTextBoxFocused;
            }
            set {
                if (_isSearchTextBoxFocused != value) {
                    _isSearchTextBoxFocused = value;
                    OnPropertyChanged(nameof(IsSearchTextBoxFocused));
                }
            }
        }

        private Brush _searchTextBoxBorderBrush = Brushes.Transparent;
        public Brush SearchTextBoxBorderBrush {
            get {
                return _searchTextBoxBorderBrush;
            }
            set {
                if (_searchTextBoxBorderBrush != value) {
                    _searchTextBoxBorderBrush = value;
                    OnPropertyChanged(nameof(SearchTextBoxBorderBrush));
                }
            }
        }

        private SolidColorBrush _searchTextBoxTextBrush = Brushes.DimGray;
        public SolidColorBrush SearchTextBoxTextBrush {
            get {
                return _searchTextBoxTextBrush;
            }
            set {
                if (_searchTextBoxTextBrush != value) {
                    _searchTextBoxTextBrush = value;
                    OnPropertyChanged(nameof(SearchTextBoxTextBrush));
                }
            }
        }

        private FontStyle _searchTextBoxFontStyle = FontStyles.Italic;
        public FontStyle SearchTextBoxFontStyle {
            get {
                return _searchTextBoxFontStyle;
            }
            set {
                if (_searchTextBoxFontStyle != value) {
                    _searchTextBoxFontStyle = value;
                    OnPropertyChanged(nameof(SearchTextBoxFontStyle));
                }
            }
        }
        #endregion

        #region Constructor/Initializers
        public MpSearchBoxViewModel(MpMainWindowViewModel parent) {
            MainWindowViewModel = parent;
        }
        public void SearchBoxBorder_Loaded(object sender,RoutedEventArgs e) {
            var searchBox = (TextBox)((MpClipBorder)sender).FindName("SearchTextBox");
            searchBox.KeyDown += (s, e3) => {
                if (e3.Key == Key.Return) {
                    MainWindowViewModel.ClipTrayViewModel.PerformSearch();
                }
            };
            searchBox.GotFocus += (s, e4) => {
                //make text
                if (SearchText == Properties.Settings.Default.SearchPlaceHolderText) {
                    SearchText = "";
                }
                SearchTextBoxFontStyle = FontStyles.Normal;
                SearchTextBoxTextBrush = Brushes.Black;
                IsSearchTextBoxFocused = true;
            };
            searchBox.LostFocus += (s, e5) => {
                //var searchTextBox = (TextBox)e.Source;
                if (string.IsNullOrEmpty(SearchText)) {
                    SearchText = Properties.Settings.Default.SearchPlaceHolderText;
                    SearchTextBoxFontStyle = FontStyles.Italic;
                    SearchTextBoxTextBrush = Brushes.DimGray;
                }
                IsSearchTextBoxFocused = false;
            };
            SearchText = Properties.Settings.Default.SearchPlaceHolderText;

        }
        #endregion
    }
}
