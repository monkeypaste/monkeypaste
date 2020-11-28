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

namespace MpWpfApp {
    public class MpTemplateTokenModalWindowViewModel : MpViewModelBase {
        #region Static Variables
        public static bool IsOpen = false;
        #endregion

        #region Private Variables
        private Window _windowRef = null;
        private TextRange _hyperlinkRange = null;
        private RichTextBox _rtb = null;
        private string _originalText = string.Empty;
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

        private Hyperlink _selectedTokenHyperlink = null;
        public Hyperlink SelectedTokenHyperlink {
            get {
                return _selectedTokenHyperlink;
            }
            set {
                if (_selectedTokenHyperlink != value) {
                    _selectedTokenHyperlink = value;
                    OnPropertyChanged(nameof(SelectedTokenHyperlink));
                    OnPropertyChanged(nameof(SelectedTokenName));
                    OnPropertyChanged(nameof(SelectedTokenBrush));
                }
            }
        }

        public string SelectedTokenName {
            get {
                return MpHelpers.GetHyperlinkText(SelectedTokenHyperlink);
            }
            set {
                if (MpHelpers.GetHyperlinkText(SelectedTokenHyperlink) != value) {
                    //assign new name
                    SelectedTokenHyperlink = MpHelpers.SetHyperlinkText(SelectedTokenHyperlink, value);

                    Validate();
                    
                    OnPropertyChanged(nameof(SelectedTokenName));
                    OnPropertyChanged(nameof(SelectedTokenBrush));
                    OnPropertyChanged(nameof(SelectedTokenHyperlink));
                }
            }
        }



        public Brush SelectedTokenBrush {
            get {
                return SelectedTokenHyperlink.Background;
            }
        }

        private ObservableCollection<Hyperlink> _templateTokenHyperlinks = new ObservableCollection<Hyperlink>();
        public ObservableCollection<Hyperlink> TemplateTokenHyperlinks {
            get {
                return _templateTokenHyperlinks;
            }
            set {
                if (_templateTokenHyperlinks != value) {
                    _templateTokenHyperlinks = value;
                    OnPropertyChanged(nameof(TemplateTokenHyperlinks));
                }
            }
        }
        #endregion

        #region Static Methods
        public static bool ShowTemplateTokenModalWindow(RichTextBox rtb, TextRange hyperlinkRange) {
            var ttmw = new MpTemplateTokenModalWindow(rtb, hyperlinkRange);
            var ttmwvm = (MpTemplateTokenModalWindowViewModel)ttmw.DataContext;            
            var result = ttmw.ShowDialog();
            if (result.Value == true) {
                return true;
            } else {
                return false;
            }
        }
        #endregion

        #region Public Methods
        public MpTemplateTokenModalWindowViewModel()  { }

        public MpTemplateTokenModalWindowViewModel(RichTextBox rtb, TextRange hyperlinkRange) {
            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(SelectedTokenHyperlink):

                        break;
                }
            };
            _rtb = rtb; 
            _hyperlinkRange = _rtb.Selection;
            _originalText = _hyperlinkRange.Text;
            TemplateTokenHyperlinks = new ObservableCollection<Hyperlink>(_rtb.GetTemplateHyperlinks());

           Hyperlink hl = new Hyperlink();
            hl = MpHelpers.SetHyperlinkText(hl, _hyperlinkRange.Text);
            hl = MpHelpers.SetHyperlinkBackgroundBrush(hl, GetUniqueTemplateColor());
            hl.Tag = MpSubTextTokenType.TemplateSegment;
            TemplateTokenHyperlinks.Add(hl);
            SelectedTokenHyperlink = hl;


            //TextBlock tb = new TextBlock();
            //Binding textBinding = new Binding("MyDataProperty");
            //textBinding.Source = SelectedTokenName;
            //BindingOperations.SetBinding(tb, TextBlock.TextProperty, textBinding);
        }


        public void TemplateTokenModalWindow_Loaded(object sender, RoutedEventArgs e) {
            _windowRef = (Window)sender;
            var tb = (TextBox)_windowRef.FindName("TemplateNameEditorTextBox");
            tb.PreviewKeyUp += (s, e1) => {
                if(e1.Key == Key.Enter) {
                    OkCommand.Execute(null);
                    e1.Handled = true;
                }
            };
        }
        #endregion

        #region Private Methods
        private string GetUniqueTemplateName() {
            int uniqueIdx = 1;
            string namePrefix = "Template #";
            while(TemplateTokenHyperlinks.Where(x => new TextRange(x.ContentStart,x.ContentEnd).Text == namePrefix + uniqueIdx).ToList().Count > 0) {
                uniqueIdx++;
            }
            return namePrefix + uniqueIdx;
        }

        private Brush GetUniqueTemplateColor() {
            Brush randColor = (Brush)new SolidColorBrush(MpHelpers.GetRandomColor());
            while (TemplateTokenHyperlinks.Where(x => x.Background == randColor).ToList().Count > 0) {
                randColor = (Brush)new SolidColorBrush(MpHelpers.GetRandomColor());
            }
            return randColor;
        }
        private bool Validate() {
            if(string.IsNullOrEmpty(MpHelpers.GetHyperlinkText(SelectedTokenHyperlink))) {
                ValidationText = "Name cannot be empty!";
                return false;
            }
            //if new name is a duplicate of another just delete this one and set it to the duplicate
            var dupTokenHyperlink = TemplateTokenHyperlinks.Where(x => MpHelpers.GetHyperlinkText(x) == MpHelpers.GetHyperlinkText(SelectedTokenHyperlink) && x != SelectedTokenHyperlink).ToList();
            if (dupTokenHyperlink != null && dupTokenHyperlink.Count > 0) {
                ValidationText = MpHelpers.GetHyperlinkText(SelectedTokenHyperlink) + " already exists!";
                return false;
            }
            ValidationText = string.Empty;
            return true;
        }
        #endregion

        #region Commands
        private RelayCommand _changeTemplateColorCommand;
        public ICommand ChangeTemplateColorCommand {
            get {
                if (_changeTemplateColorCommand == null) {
                    _changeTemplateColorCommand = new RelayCommand(ChangeTemplateColor);
                }
                return _changeTemplateColorCommand;
            }
        }
        private void ChangeTemplateColor() {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            cd.AllowFullOpen = true;
            cd.ShowHelp = true;
            cd.Color = MpHelpers.ConvertSolidColorBrushToWinFormsColor((SolidColorBrush)SelectedTokenHyperlink.Background);
            cd.CustomColors = Properties.Settings.Default.UserCustomColorIdxArray;

            var mw = (MpMainWindow)Application.Current.MainWindow;
            ((MpMainWindowViewModel)mw.DataContext).IsShowingDialog = true;
            // Update the text box color if the user clicks OK 
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                SelectedTokenHyperlink = MpHelpers.SetHyperlinkBackgroundBrush(SelectedTokenHyperlink, (Brush)MpHelpers.ConvertWinFormsColorToSolidColorBrush(cd.Color));
                OnPropertyChanged(nameof(SelectedTokenBrush));
            }
            Properties.Settings.Default.UserCustomColorIdxArray = cd.CustomColors;
            Properties.Settings.Default.Save();
            ((MpMainWindowViewModel)mw.DataContext).IsShowingDialog = false;
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
            //_rtb.Selection.Select(_hyperlinkRange.Start,_hyperlinkRange.End);
            _rtb.Selection.Text = _originalText;
            _windowRef.DialogResult = false;
            _windowRef.Close();
        }

        private RelayCommand _clearCommand;
        public ICommand ClearCommand {
            get {
                if (_clearCommand == null) {
                    _clearCommand = new RelayCommand(Clear);
                }
                return _clearCommand;
            }
        }
        private void Clear() {
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
            //apply link to rtb
            MpHelpers.CreateHyperlink(_hyperlinkRange.Start, _hyperlinkRange.End, SelectedTokenName, SelectedTokenBrush, MpSubTextTokenType.TemplateSegment);

            _windowRef.DialogResult = true;
            _windowRef.Close();
        }
        #endregion

    }
}
