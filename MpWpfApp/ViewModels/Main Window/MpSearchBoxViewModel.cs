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
        private bool _isTextBoxFocused = false;
        public bool IsTextBoxFocused {
            get {
                return _isTextBoxFocused;
            }
            set {
                //omitting duplicate check to enforce change in ui
                //if (_isTextBoxFocused != value) 
                {
                    _isTextBoxFocused = value;
                    OnPropertyChanged(nameof(IsTextBoxFocused));
                }
            }
        }

        private string _searchText = string.Empty;
        public string SearchText {
            get {
                return _searchText;
            }
            set {
                if (_searchText != value) {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    OnPropertyChanged(nameof(SearchTextBoxFontStyle));
                    OnPropertyChanged(nameof(SearchTextBoxTextBrush));
                    OnPropertyChanged(nameof(SearchTextBoxBorderBrush));
                }
            }
        }

        public Brush SearchTextBoxBorderBrush {
            get {
                if(MainWindowViewModel.ClipTrayViewModel.VisibileClipTiles.Count == 0 && 
                    SearchText != Properties.Settings.Default.SearchPlaceHolderText &&
                    !string.IsNullOrEmpty(SearchText)) {
                    return Brushes.Red;
                }
                return Brushes.Transparent;
            }
        }

        public SolidColorBrush SearchTextBoxTextBrush {
            get {
                if (SearchText != Properties.Settings.Default.SearchPlaceHolderText || IsTextBoxFocused) {
                    return Brushes.Black;
                }
                return Brushes.DimGray;
            }
        }

        public FontStyle SearchTextBoxFontStyle {
            get {
                if(SearchText == Properties.Settings.Default.SearchPlaceHolderText) {
                    return FontStyles.Italic;
                }
                return FontStyles.Normal;
            }
        }

        public Visibility ClearSearchTextButtonVisibility {
            get {
                if (SearchText.Length > 0 && 
                    SearchText != Properties.Settings.Default.SearchPlaceHolderText) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }
        #endregion

        #region Public Methods

        public MpSearchBoxViewModel() : base() {
        }
        public void SearchBoxBorder_Loaded(object sender, RoutedEventArgs e) {
            var searchBox = (TextBox)((MpClipBorder)sender).FindName("SearchTextBox");
            searchBox.GotFocus += (s, e4) => {
                //make text
                if (SearchText == Properties.Settings.Default.SearchPlaceHolderText) {
                    SearchText = string.Empty;
                }
                
                IsTextBoxFocused = true;
                MainWindowViewModel.ClipTrayViewModel.ResetClipSelection();
                OnPropertyChanged(nameof(SearchTextBoxFontStyle));
                OnPropertyChanged(nameof(SearchTextBoxTextBrush));
            };
            searchBox.LostFocus += (s, e5) => {
                IsTextBoxFocused = false;
                if (string.IsNullOrEmpty(SearchText)) {
                    SearchText = Properties.Settings.Default.SearchPlaceHolderText;
                }
            };
            SearchText = Properties.Settings.Default.SearchPlaceHolderText;
            MainWindowViewModel.ClipTrayViewModel.ItemsVisibilityChanged += (s1, e7) => {
                OnPropertyChanged(nameof(SearchTextBoxBorderBrush));
            };
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
            IsTextBoxFocused = true;
        }
        #endregion
    }
}
