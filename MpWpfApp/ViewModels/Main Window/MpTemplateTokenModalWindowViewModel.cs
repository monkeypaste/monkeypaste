using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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

        private Hyperlink _selectedTokenHyperlink = null;
        public Hyperlink SelectedTokenHyperlink {
            get {
                return _selectedTokenHyperlink;
            }
            set {
                if (_selectedTokenHyperlink != value) {
                    _selectedTokenHyperlink = value;
                    OnPropertyChanged(nameof(SelectedTokenHyperlink));
                }
            }
        }

        public string SelectedTokenName {
            get {
                return MpHelpers.GetHyperlinkText(SelectedTokenHyperlink);
            }
            set {
                var name = MpHelpers.GetHyperlinkText(SelectedTokenHyperlink);
                if (name != value) {
                    //assign new name
                    SelectedTokenHyperlink = MpHelpers.SetHyperlinkText(SelectedTokenHyperlink, value);
                    if(string.IsNullOrEmpty(name)) {
                        //if new name is empty make Template #N
                        SelectedTokenHyperlink = MpHelpers.SetHyperlinkText(SelectedTokenHyperlink, GetUniqueTemplateName());
                    }
                    
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
            set {
                if (SelectedTokenHyperlink.Background != value) {
                    SelectedTokenHyperlink.Background = value;
                    OnPropertyChanged(nameof(SelectedTokenBrush));
                    OnPropertyChanged(nameof(SelectedTokenHyperlink));
                }
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
            var ttmw = new MpTemplateTokenModalWindow();
            ttmw.DataContext = new MpTemplateTokenModalWindowViewModel(rtb);
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
        public MpTemplateTokenModalWindowViewModel() : this(new RichTextBox()) { }

        public MpTemplateTokenModalWindowViewModel(RichTextBox rtb) {
            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(SelectedTokenHyperlink):

                        break;
                }
            };

            TemplateTokenHyperlinks = new ObservableCollection<Hyperlink>(rtb.GetTemplateHyperlinks());
            if (TemplateTokenHyperlinks.Count == 0) {
                rtb.Selection.Text = GetUniqueTemplateName();
                Hyperlink newTokenLink = new Hyperlink(rtb.Selection.Start, rtb.Selection.End);
                newTokenLink.Background = GetUniqueTemplatColor();
                newTokenLink.Tag = MpSubTextTokenType.TemplateSegment;
                TemplateTokenHyperlinks.Add(newTokenLink);
            }
            SelectedTokenHyperlink = TemplateTokenHyperlinks[0];
        }


        public void TemplateTokenModalWindow_Loaded(object sender, RoutedEventArgs e) {
            _windowRef = (Window)sender;
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

        private Brush GetUniqueTemplatColor() {
            Brush randColor = (Brush)new SolidColorBrush(MpHelpers.GetRandomColor());
            while (TemplateTokenHyperlinks.Where(x => x.Background == randColor).ToList().Count > 0) {
                randColor = (Brush)new SolidColorBrush(MpHelpers.GetRandomColor());
            }
            return randColor;
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
                SelectedTokenBrush = (Brush)MpHelpers.ConvertWinFormsColorToSolidColorBrush(cd.Color);
            }
            Properties.Settings.Default.UserCustomColorIdxArray = cd.CustomColors;
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
                    _okCommand = new RelayCommand(Ok);
                }
                return _okCommand;
            }
        }
        private void Ok() {
            //if new name is a duplicate of another just delete this one and set it to the duplicate
            var dupTokenHyperlink = TemplateTokenHyperlinks.Where(x => MpHelpers.GetHyperlinkText(x) == MpHelpers.GetHyperlinkText(SelectedTokenHyperlink) && x != SelectedTokenHyperlink).ToList();
            if (dupTokenHyperlink != null && dupTokenHyperlink.Count > 0) {
                TemplateTokenHyperlinks.Remove(SelectedTokenHyperlink);
                SelectedTokenHyperlink = dupTokenHyperlink[0];
            }
            _windowRef.DialogResult = true;
            _windowRef.Close();
        }
        #endregion

    }
}
