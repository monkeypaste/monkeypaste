using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Windows.UI.Xaml.Controls;

namespace MpWpfApp {
    public class MpTemplateTokenPasteModalWindowViewModel : MpViewModelBase {
        #region Static Variables
        public static bool IsOpen = false;
        #endregion

        #region Private Variables
        private Window _windowRef = null;
        private RichTextBox _rtb = null;
        #endregion

        #region Properties
        private string _windowTitle = "Template Manager";
        public string WindowTitle {
            get {
                return _windowTitle;
            }
            set {
                if (_windowTitle != value) {
                    _windowTitle = value;
                    OnPropertyChanged(nameof(WindowTitle));
                }
            }
        }

        private string _validationText = string.Empty;
        public string ValidationText {
            get {
                return _validationText;
            }
            set {
                if (_validationText != value) {
                    _validationText = value;
                    OnPropertyChanged(nameof(ValidationText));
                    OnPropertyChanged(nameof(ValidationVisibility));
                    OnPropertyChanged(nameof(TemplateNameTextBoxBorderBrush));
                    OnPropertyChanged(nameof(TemplateNameTextBoxBorderBrushThickness)); ;
                }
            }
        }

        public Visibility ValidationVisibility {
            get {
                if(string.IsNullOrEmpty(ValidationText)) {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }

        public double TemplateNameTextBoxBorderBrushThickness {
            get {
                if(string.IsNullOrEmpty(ValidationText)) {
                    return 1;
                } 
                return 3;
            }
        }

        public Brush TemplateNameTextBoxBorderBrush {
            get {
                if(string.IsNullOrEmpty(ValidationText)) {
                    return Brushes.Black;
                }
                return Brushes.Red;
            }
        }

        private int _selectedTokenIdx;
        public int SelectedTokenIdx {
            get {
                return _selectedTokenIdx;
            }
            set {
                _selectedTokenIdx = value;
                OnPropertyChanged(nameof(SelectedTokenIdx));
                OnPropertyChanged(nameof(SelectedTokenText));
            }
        }

        public string SelectedTokenText {
            get {
                return TemplateTokenLookupDictionary[TemplateTokenList[SelectedTokenIdx]];
            }
            set {
                TemplateTokenLookupDictionary[TemplateTokenList[SelectedTokenIdx]] = value;
                OnPropertyChanged(nameof(SelectedTokenText));
            }
        }

        private List<string> _templateTokenList = new List<string>();
        public List<string> TemplateTokenList {
            get {
                return _templateTokenList;
            }
            set {
                _templateTokenList = value;
                OnPropertyChanged(nameof(TemplateTokenList));
            }
        }

        public Dictionary<string, string> TemplateTokenLookupDictionary = new Dictionary<string, string>();
        
        #endregion

        #region Static Methods
        public static Dictionary<string, string> ShowTemplateTokenPasteModalWindow(List<Hyperlink> tokenList) {
            var ttmw = new MpTemplateTokenPasteModalWindow(tokenList);
            var ttmwvm = (MpTemplateTokenPasteModalWindowViewModel)ttmw.DataContext;            
            var result = ttmw.ShowDialog();
            return ttmwvm.TemplateTokenLookupDictionary;
        }
        #endregion

        #region Public Methods
        public MpTemplateTokenPasteModalWindowViewModel(List<Hyperlink> tokenList) : base() {
            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                }
            };
            foreach(var tthl in tokenList) {                
                if(!TemplateTokenList.Contains(tthl.TargetName)) {
                    TemplateTokenList.Add(tthl.TargetName);
                    TemplateTokenLookupDictionary.Add(tthl.TargetName, string.Empty);
                }
            }
            SelectedTokenIdx = 0;
        }


        public void TemplateTokenModalWindow_Loaded(object sender, RoutedEventArgs e) {
            _windowRef = (Window)sender;
            IsOpen = true;
        }
        #endregion

        #region Private Methods
        private bool Validate() {
            foreach(var tokenKeyValue in TemplateTokenLookupDictionary) {
                if(string.IsNullOrEmpty(TemplateTokenLookupDictionary[tokenKeyValue.Key])) {
                    ValidationText = tokenKeyValue.Key + " cannot be empty";
                    return false;
                }
            }
            ValidationText = string.Empty;
            return true;
        }
        #endregion

        #region Commands
        private RelayCommand _nextTokenCommand;
        public ICommand NextTokenCommand {
            get {
                if (_nextTokenCommand == null) {
                    _nextTokenCommand = new RelayCommand(NextToken, CanNextToken);
                }
                return _nextTokenCommand;
            }
        }
        private bool CanNextToken() {
            return TemplateTokenList.Count > 1;
        }
        private void NextToken() {
            SelectedTokenIdx = SelectedTokenIdx + 1 >= TemplateTokenList.Count ? 0 : SelectedTokenIdx + 1;
        }

        private RelayCommand _previousTokenCommand;
        public ICommand PreviousTokenCommand {
            get {
                if (_previousTokenCommand == null) {
                    _previousTokenCommand = new RelayCommand(PreviousToken, CanPreviousToken);
                }
                return _previousTokenCommand;
            }
        }
        private bool CanPreviousToken() {
            return TemplateTokenList.Count > 1;
        }
        private void PreviousToken() {
            SelectedTokenIdx = SelectedTokenIdx - 1 < 0 ? TemplateTokenList.Count - 1 : SelectedTokenIdx - 1;
        }

        private RelayCommand _cancelCommand;
        public ICommand CancelCommand {
            get {
                if (_cancelCommand == null) {
                    _cancelCommand = new RelayCommand(Cancel);
                }
                return _cancelCommand;
            }
        }
        private void Cancel() {
            foreach(var token in TemplateTokenList) {
                TemplateTokenLookupDictionary[token] = string.Empty;
            }
            _windowRef.DialogResult = false;
            _windowRef.Close();
            IsOpen = false;
        }

        private RelayCommand _okCommand;
        public ICommand OkCommand {
            get {
                if (_okCommand == null) {
                    _okCommand = new RelayCommand(Ok, CanOk);
                }
                return _okCommand;
            }
        }
        private bool CanOk() {
            return Validate();
        }
        private void Ok() {
            foreach(var templateLookup in TemplateTokenLookupDictionary) {
                Console.WriteLine(string.Format("{0}: {1}", templateLookup.Key, templateLookup.Value));
            }
            _windowRef.DialogResult = true;
            _windowRef.Close(); 
            IsOpen = false;
        }
        #endregion

    }
}
 