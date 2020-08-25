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
        private string _searchText = string.Empty;
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
        public void SearchBoxBorder_Loaded(object sender, RoutedEventArgs e) {
            var searchBox = (TextBox)((Border)sender).FindName("SearchTextBox");
            searchBox.GotFocus += (s, e4) => {
                //make text
                if (SearchText == Properties.Settings.Default.SearchPlaceHolderText) {
                    SearchText = string.Empty;
                }
                SearchTextBoxFontStyle = FontStyles.Normal;
                SearchTextBoxTextBrush = Brushes.Black;
                IsFocused = true;
            };
            searchBox.LostFocus += (s, e5) => {
                //var searchTextBox = (TextBox)e.Source;
                if (string.IsNullOrEmpty(SearchText)) {
                    SearchText = Properties.Settings.Default.SearchPlaceHolderText;
                    SearchTextBoxFontStyle = FontStyles.Italic;
                    SearchTextBoxTextBrush = Brushes.DimGray;
                }
                IsFocused = false;
            };
            searchBox.PreviewKeyUp += MainWindowViewModel.MainWindow_PreviewKeyDown;
            SearchText = Properties.Settings.Default.SearchPlaceHolderText;

        }
        #endregion
    }
}
