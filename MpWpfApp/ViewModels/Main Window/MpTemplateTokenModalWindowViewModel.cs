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
    public class MpTemplateTokenModalWindowViewModel : MpViewModelBase {
        #region Static Variables
        public static bool IsOpen = false;
        #endregion

        #region Private Variables
        private Window _windowRef = null;
        private RichTextBox _rtb = null;
        private string _originalText = string.Empty;
        private string _templateText = string.Empty;
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
                return SelectedTokenHyperlink.TargetName;
            }
            set {
                SelectedTokenHyperlink.TargetName = value;
                Validate();
                OnPropertyChanged(nameof(SelectedTokenName));
                OnPropertyChanged(nameof(SelectedTokenHyperlink));
            }
        }



        public Brush SelectedTokenBrush {
            get {
                return SelectedTokenHyperlink.Background;
            }
            set {
                SelectedTokenHyperlink.Background = value;
                OnPropertyChanged(nameof(SelectedTokenBrush));
                OnPropertyChanged(nameof(SelectedTokenHyperlink));
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
        public static bool ShowTemplateTokenModalWindow(RichTextBox rtb) {
            var ttmw = new MpTemplateTokenModalWindow(rtb);
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
        public MpTemplateTokenModalWindowViewModel(RichTextBox rtb) {
            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(SelectedTokenHyperlink):
                        _templateText = String.Format(@"{0}{1}{0}", Properties.Settings.Default.TemplateTokenMarker, SelectedTokenName);
                        break;
                }
            };
            _rtb = rtb; 
            _originalText = _rtb.Selection.Text;
            TemplateTokenHyperlinks = new ObservableCollection<Hyperlink>(_rtb.GetTemplateHyperlinkList());
            var newLink = new Hyperlink();
            newLink.TargetName = GetUniqueTemplateName();
            newLink.NavigateUri = new Uri(Properties.Settings.Default.TemplateTokenUri);
            newLink.RequestNavigate += (s, e1) => {
                MessageBox.Show(MpHelpers.ConvertFlowDocumentToRichText(_rtb.Document));
            };
            newLink.Tag = MpSubTextTokenType.TemplateSegment;
            newLink.IsEnabled = true;
            TemplateTokenHyperlinks.Add(newLink);
            SelectedTokenHyperlink = newLink;
            //_templateText = String.Format(@"{0}{1}{0}", Properties.Settings.Default.TemplateTokenMarker, GetUniqueTemplateName());
            //Run run = new Run(GetUniqueTemplateName());
            //run.Background = GetUniqueTemplateColor();

            //var hyperlink = new Hyperlink(_rtb.Selection.Start, _rtb.Selection.End);
            //hyperlink.Inlines.Clear();
            //hyperlink.Inlines.Add(run);
            //hyperlink.TargetName = _rtb.Selection.Text;
            //hyperlink.NavigateUri = new Uri(Properties.Settings.Default.TemplateTokenUri);
            //hyperlink.RequestNavigate += (s, e1) => {
            //    MessageBox.Show(MpHelpers.ConvertFlowDocumentToRichText(_rtb.Document));
            //};
            //hyperlink.Tag = MpSubTextTokenType.TemplateSegment;
            //hyperlink.IsEnabled = true;
            //hyperlink.Name = "Template" + TemplateTokenHyperlinks.Count;
            //rtb.Document.RegisterName(hyperlink.Name, hyperlink);
            //TemplateTokenHyperlinks.Add(hyperlink);
            //SelectedTokenHyperlink = hyperlink;

            //_rtb.IsDocumentEnabled = true;
            //_rtb.IsReadOnly = false;

            //TextBlock tb = new TextBlock();
            //Binding textBinding = new Binding("MyDataProperty");
            //textBinding.Source = SelectedTokenName;
            //BindingOperations.SetBinding(tb, TextBlock.TextProperty, textBinding);
        }


        public void TemplateTokenModalWindow_Loaded(object sender, RoutedEventArgs e) {
            _windowRef = (Window)sender;
            //var tb = (TextBox)_windowRef.FindName("TemplateNameEditorTextBox");
            //tb.PreviewKeyUp += (s, e1) => {
            //    if(e1.Key == Key.Enter) {
            //        OkCommand.Execute(null);
            //        e1.Handled = true;
            //    }
            //};
        }
        #endregion

        #region Private Methods
        private string GetUniqueTemplateName() {
            int uniqueIdx = 1;
            string namePrefix = "Template #";
            while(TemplateTokenHyperlinks.Where(x => x.TargetName == namePrefix + uniqueIdx).ToList().Count > 0) {
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
            if (string.IsNullOrEmpty(SelectedTokenName)) {
                ValidationText = "Name cannot be empty!";
                return false;
            }
            //if new name is a duplicate of another just delete this one and set it to the duplicate
            var dupTokenHyperlink = TemplateTokenHyperlinks.Where(x => x.TargetName == SelectedTokenName && x != SelectedTokenHyperlink).ToList();
            if (dupTokenHyperlink != null && dupTokenHyperlink.Count > 0) {
                ValidationText = SelectedTokenName + " already exists!";
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
                //SelectedTokenHyperlink = MpHelpers.SetHyperlinkBackgroundBrush(SelectedTokenHyperlink, (Brush)MpHelpers.ConvertWinFormsColorToSolidColorBrush(cd.Color));
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
            //_rtb.Selection.Select(SelectedTokenHyperlink.ElementStart,SelectedTokenHyperlink.ElementEnd);
            //_rtb.Selection.Text = String.Format(@"{0}{1}{0}",Properties.Settings.Default.TemplateTokenMarker,SelectedTokenName);
            //Hyperlink hyperlink = new Hyperlink(_rtb.Selection.Start, _rtb.Selection.End);
            //hyperlink.Background = SelectedTokenBrush;
            //hyperlink.Tag = MpSubTextTokenType.TemplateSegment;
            //hyperlink.IsEnabled = true;
            //((List<Hyperlink>)_rtb.Tag).Add(SelectedTokenHyperlink);
            //_rtb.ClearHyperlinks();
            //_rtb.CreateHyperlinks();
            _rtb.Selection.Text = String.Format(@"{0}{1}{0}", Properties.Settings.Default.TemplateTokenMarker, SelectedTokenName);            
            _rtb.ClearHyperlinks();
            _windowRef.DialogResult = true;
            _windowRef.Close();
        }
        #endregion

    }
}
 