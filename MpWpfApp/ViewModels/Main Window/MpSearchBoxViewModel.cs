using GalaSoft.MvvmLight.CommandWpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpSearchBoxViewModel : MpViewModelBase {
        #region View Models
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

        private Visibility _clearSearchTextButtonVisibility = Visibility.Collapsed;
        public Visibility ClearSearchTextButtonVisibility {
            get {
                return _clearSearchTextButtonVisibility;
            }
            set {
                if (_clearSearchTextButtonVisibility != value) {
                    _clearSearchTextButtonVisibility = value;
                    OnPropertyChanged(nameof(ClearSearchTextButtonVisibility));
                }
            }
        }
        #endregion

        #region Public Methods

        public MpSearchBoxViewModel() {
            PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(SearchText):
                        if(SearchText.Length > 0 && SearchText != Properties.Settings.Default.SearchPlaceHolderText) {
                            ClearSearchTextButtonVisibility = Visibility.Visible;                            
                        } else {
                            ClearSearchTextButtonVisibility = Visibility.Collapsed;
                        }
                        break;
                }
            };
        }
        public void SearchBoxBorder_Loaded(object sender, RoutedEventArgs e) {
            var searchBox = (TextBox)((MpClipBorder)sender).FindName("SearchTextBox");
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

        #region Commands
        private RelayCommand _clearSearchTextCommand;
        public ICommand ClearSearchTextCommand {
            get {
                if (_clearSearchTextCommand == null) {
                    _clearSearchTextCommand = new RelayCommand(ClearSearchText, CanClearSearchText);
                }
                return _clearSearchTextCommand;
            }
        }
        private bool CanClearSearchText() {
            return SearchText.Length > 0;
        }
        private void ClearSearchText() {
            SearchText = string.Empty;
        }
        #endregion
    }
}
