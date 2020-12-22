using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public enum MpCurrencyType {
        None = 0,
        Dollars,
        Pounds,
        Euros,
        Yen
    }

    public enum MpSubTextTokenType {
        None = 0,
        Uri = 1,
        Email,
        PhoneNumber,
        Currency,
        HexColor,
        StreetAddress,
        TemplateSegment,
        CopyItemSegment
    }
    public class MpTemplateHyperlinkViewModel : MpViewModelBase, IDisposable {
        #region Private Variables
        private RichTextBox _rtb = null;
        #endregion

        #region View Models
        public MpClipTileViewModel ClipTileViewModel { get; set; } = null;
        #endregion

        #region Properties
        public Brush TemplateBorderBrush {
            get {
                if (IsHovering) {
                    return Brushes.LightGray;
                }
                if (IsSelected) {
                    if(string.IsNullOrEmpty(TemplateText)) {
                        return Brushes.Red;
                    }
                    return Brushes.Green;
                } 
                if (IsPasteMode) {
                    return Brushes.White;
                }
                return Brushes.DarkGray;
            }
        }

        public Brush TemplateForegroundBrush { 
            get {
                if(MpHelpers.IsBright(((SolidColorBrush)TemplateBackgroundBrush).Color)) {
                    return Brushes.Black;
                }
                return Brushes.White;
            }
        }

        private Brush _templateBackgroundBrush = Brushes.Pink;
        public Brush TemplateBackgroundBrush {
            get {
                return _templateBackgroundBrush;
            }
            set {
                if (_templateBackgroundBrush != value) {
                    _templateBackgroundBrush = value;
                    OnPropertyChanged(nameof(TemplateBackgroundBrush));
                    OnPropertyChanged(nameof(TemplateForegroundBrush));
                }
            }
        }

        private Visibility _deleteTemplateTextButtonVisibility = Visibility.Hidden;
        public Visibility DeleteTemplateTextButtonVisibility  {
            get {
                if(ClipTileViewModel.IsSelected) {
                    return Visibility.Visible;
                }
                return Visibility.Hidden;
            }
            set {
                if(_deleteTemplateTextButtonVisibility != value) {
                    _deleteTemplateTextButtonVisibility = value;
                    OnPropertyChanged(nameof(DeleteTemplateTextButtonVisibility));
                }
            }
        }

        public Visibility TemplateTextBlockVisibility {
            get {
                if (IsEditMode) {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
        }

        private double _templateFontSize = 12;
        public double TemplateFontSize {
            get {
                return _templateFontSize;
            }
            set {
                if(_templateFontSize != value) {
                    _templateFontSize = value;
                    OnPropertyChanged(nameof(TemplateFontSize));
                    OnPropertyChanged(nameof(TemplateBorderWidth));
                }
            }
        }

        public double TemplateBorderWidth {
            get {
                if(IsEditMode) {
                    return (TemplateText.Length * TemplateFontSize) * 0.65;
                }
                return (TemplateName.Length * TemplateFontSize) * 0.65;
            }
        }

        public double TemplateTextBlockWidth {
            get {
                return TemplateBorderWidth - 5;
            }
        }

        private string _templateText = string.Empty;
        public string TemplateText {
            get {
                return _templateText;
            }
            set {
                if (_templateText != value) {
                    _templateText = value;
                    OnPropertyChanged(nameof(TemplateText));
                    OnPropertyChanged(nameof(TemplateBorderWidth));
                    OnPropertyChanged(nameof(TemplateDisplayValue));
                }
            }
        }

        private string _templateName = string.Empty;
        public string TemplateName {
            get {
                return _templateName;
            }
            set {
                if (_templateName != value) {
                    _templateName = value;
                    OnPropertyChanged(nameof(TemplateName));
                    OnPropertyChanged(nameof(TemplateBorderWidth));
                    OnPropertyChanged(nameof(TemplateDisplayValue));
                }
            }
        }

        public string TemplateDisplayValue {
            get {
                if (IsPasteMode) {
                    if(WasTypeViewed) {
                        return TemplateText;
                    }
                }
                return TemplateName;
            }
        }

        private bool _isHovering = false;
        public bool IsHovering {
            get {
                return _isHovering;
            }
            set {
                if (_isHovering != value) {
                    _isHovering = value;
                    OnPropertyChanged(nameof(IsHovering));
                    OnPropertyChanged(nameof(TemplateBorderBrush));
                    OnPropertyChanged(nameof(DeleteTemplateTextButtonVisibility));
                }
            }
        }

        private bool _isFocused = false;
        public bool IsFocused {
            get {
                return _isFocused;
            }
            set {
                if (_isFocused != value) {
                    _isFocused = value;
                    OnPropertyChanged(nameof(IsFocused));
                }
            }
        }

        private bool _isSelected = false;
        public bool IsSelected {
            get {
                return _isSelected;
            }
            set {
                if (_isSelected != value) {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    OnPropertyChanged(nameof(TemplateBorderBrush));
                }
            }
        }

        private bool _wasEdited = false;
        public bool WasEdited {
            get {
                return _wasEdited;
            }
            set {
                if (_wasEdited != value) {
                    _wasEdited = value;
                    OnPropertyChanged(nameof(WasEdited));
                }
            }
        }

        private bool _wasTypeViewed = false;
        public bool WasTypeViewed {
            get {
                return _wasTypeViewed;
            }
            set {
                if (_wasTypeViewed != value) {
                    _wasTypeViewed = value;
                    OnPropertyChanged(nameof(WasTypeViewed));
                    OnPropertyChanged(nameof(TemplateDisplayValue));
                }
            }
        }

        private bool _isPasteMode = false;
        public bool IsPasteMode {
            get {
                return _isPasteMode;
            }
            set {
                if (_isPasteMode != value) {
                    _isPasteMode = value;
                    OnPropertyChanged(nameof(IsPasteMode));
                    OnPropertyChanged(nameof(TemplateBorderBrush));
                    OnPropertyChanged(nameof(TemplateBorderWidth));
                    OnPropertyChanged(nameof(TemplateTextBlockVisibility));
                    OnPropertyChanged(nameof(TemplateDisplayValue));
                }
            }
        }

        private bool _isEditMode = false;
        public bool IsEditMode {
            get {
                return _isEditMode;
            }
            set {
                if (_isEditMode != value) {
                    _isEditMode = value;
                    OnPropertyChanged(nameof(IsEditMode));
                    OnPropertyChanged(nameof(TemplateBorderBrush));
                    OnPropertyChanged(nameof(TemplateBorderWidth));
                    OnPropertyChanged(nameof(TemplateTextBlockVisibility));
                    OnPropertyChanged(nameof(TemplateDisplayValue));
                }
            }
        }
        #endregion

        #region Public Methods
        public MpTemplateHyperlinkViewModel() : this(null) { }

        public MpTemplateHyperlinkViewModel(MpClipTileViewModel ctvm) : this(ctvm, "Template Name", MpHelpers.GetRandomBrushColor(), 0, null) { }

        public MpTemplateHyperlinkViewModel(
            MpClipTileViewModel ctvm,
            string templateName,
            Brush templateColor,
            double fontSize,
            RichTextBox rtb) {
            PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(TemplateText):
                        if (IsEditMode) {
                            WasEdited = true;
                        }
                        break;
                }
            };
            _rtb = rtb;
            ClipTileViewModel = ctvm;
            TemplateName = templateName;
            TemplateBackgroundBrush = templateColor;
            TemplateFontSize = Math.Max(fontSize, 12);
        }

        public void TemplateHyperLink_Loaded(object sender, RoutedEventArgs args) {
            var hl = (Hyperlink)sender;
            var b = (Border)hl.FindName("TemplateHyperlinkBorder");
            var db = (Button)hl.FindName("DeleteTemplateTokenButton");

            db.Click += (s, e) => {
                _rtb.Selection.Select(hl.ElementStart, hl.ElementEnd);
                _rtb.Selection.Text = string.Empty;
                Dispose();
            };

            
            b.MouseEnter += (s, e) => {
                if (IsFocused) {
                    return;
                }
                IsHovering = true;
            };
            b.MouseLeave += (s, e) => {
                if (IsFocused) {
                    return;
                }
                IsHovering = false;
            };
            b.PreviewMouseLeftButtonDown += (s, e) => {
                IsFocused = true;
            };

            hl.Tag = MpSubTextTokenType.TemplateSegment;
            hl.IsEnabled = true;
            hl.TextDecorations = null;
            hl.NavigateUri = new Uri(Properties.Settings.Default.TemplateTokenUri);
            hl.Unloaded += (s, e) => {
                if (hl.DataContext != null) {
                    ((MpTemplateHyperlinkViewModel)hl.DataContext).Dispose();
                }
            };
            hl.RequestNavigate += (s4, e4) => {
                // TODO Add logic to convert to editable region if in paste mode on click
                _rtb.Selection.Select(hl.ContentStart, hl.ContentEnd);
                MpTemplateTokenEditModalWindowViewModel.ShowTemplateTokenEditModalWindow(_rtb, hl, true);
            };

            var editTemplateMenuItem = new MenuItem();
            editTemplateMenuItem.Header = "Edit";
            editTemplateMenuItem.PreviewMouseDown += (s4, e4) => {
                e4.Handled = true;
                _rtb.Selection.Select(hl.ElementStart, hl.ElementEnd);
                MpTemplateTokenEditModalWindowViewModel.ShowTemplateTokenEditModalWindow(_rtb, hl, true);
            };

            var deleteTemplateMenuItem = new MenuItem();
            deleteTemplateMenuItem.Header = "Delete";
            deleteTemplateMenuItem.Click += (s4, e4) => {
                _rtb.Selection.Select(hl.ElementStart, hl.ElementEnd);
                _rtb.Selection.Text = string.Empty;
            };
            hl.ContextMenu = new ContextMenu();
            hl.ContextMenu.Items.Add(editTemplateMenuItem);
            hl.ContextMenu.Items.Add(deleteTemplateMenuItem);
        }
        #endregion

        #region Commands
        private RelayCommand _editTemplateCommand;
        public ICommand EditTemplateCommand {
            get {
                if (_editTemplateCommand == null) {
                    _editTemplateCommand = new RelayCommand(EditTemplate, CanEditTemplate);
                }
                return _editTemplateCommand;
            }
        }
        private bool CanEditTemplate() {
            return !IsPasteMode;
        }
        private void EditTemplate() {
           // _rtb.Selection.Select(hl.ElementStart, hl.ElementEnd);
            //MpTemplateTokenEditModalWindowViewModel.ShowTemplateTokenEditModalWindow(_rtb, hl, true);
        }
        #endregion

        #region Overrides
        public override string ToString() {
            return string.Format(
                @"{0}{1}{0}{2}{0}",
                Properties.Settings.Default.TemplateTokenMarker,
                TemplateName,
                TemplateBackgroundBrush.ToString());
        }

        public void Dispose() {
            if(ClipTileViewModel == null) {
                Console.WriteLine("TemplateHyperlinkViewModel error, disposing Template: " + ToString() + " without a reference to a clipTileViewModel");                return;
            }
            //remove this individual token reference
            ClipTileViewModel.TemplateTokens.Remove(this);

            //checking clip's remaing templates, if it was the last of its type remove its dictionary keyvalue
            if(ClipTileViewModel.TemplateTokens.Where(x => x.TemplateName == TemplateName).ToList().Count == 0) {
                ClipTileViewModel.TemplateTokenLookupDictionary.Remove(TemplateName);
            }
        }
        #endregion
    }
}
