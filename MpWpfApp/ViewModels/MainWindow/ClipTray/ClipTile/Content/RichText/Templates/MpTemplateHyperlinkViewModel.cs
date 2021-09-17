using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MonkeyPaste;
using System.Collections.ObjectModel;

namespace MpWpfApp {
    public enum MpCurrencyType {
        None = 0,
        Dollars,
        Pounds,
        Euros,
        Yen
    }    
    public class MpTemplateHyperlinkViewModel : MpUndoableViewModelBase<MpTemplateHyperlinkViewModel>, ICloneable {
        #region Private Variables
        private MpCopyItemTemplate _originalModel;
        #endregion

        #region Properties

        #region View Models
        public MpClipTileViewModel HostClipTileViewModel {
            get {
                if(HostTemplateCollectionViewModel == null) {
                    return null;
                }
                return HostClipTileViewModel;
            }
        }
        private MpTemplateHyperlinkCollectionViewModel _hostTemplateCollectionViewModel = null;
        public MpTemplateHyperlinkCollectionViewModel HostTemplateCollectionViewModel {
            get {
                return _hostTemplateCollectionViewModel;
            }
            set {
                if (_hostTemplateCollectionViewModel != value) {
                    _hostTemplateCollectionViewModel = value;
                    OnPropertyChanged(nameof(HostTemplateCollectionViewModel));
                    OnPropertyChanged(nameof(HostClipTileViewModel));
                    OnPropertyChanged(nameof(TemplateDisplayValue));
                    OnPropertyChanged(nameof(TemplateTextBlockCursor));
                    OnPropertyChanged(nameof(TemplateName));
                    OnPropertyChanged(nameof(TemplateDisplayName));
                }
            }
        }
        #endregion

        #region Layout Properties        
        #endregion

        #region Document Properties
        public ObservableCollection<TextRange> TemplateRanges { get; private set; } = new ObservableCollection<TextRange>();
        #endregion

        #region Appearance Properties
        public Cursor TemplateTextBlockCursor {
            get {
                if(HostTemplateCollectionViewModel != null && 
                  (HostClipTileViewModel.IsEditingContent || HostClipTileViewModel.IsEditingTemplate)) {
                    return Cursors.Hand;
                }
                return Cursors.Arrow;
            }
        }
        #endregion

        #region Visibility Properties
        #endregion


        #region Brush Properties
        public Brush TemplateNameTextBoxBorderBrush {
            get {
                return string.IsNullOrEmpty(ValidationText) ? Brushes.Transparent : Brushes.Red;
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
                    OnPropertyChanged(nameof(TemplateNameTextBoxBorderBrush));
                    //OnPropertyChanged(nameof(ValidationVisibility));
                    //OnPropertyChanged(nameof(SelectedTemplateNameTextBoxBorderBrush));
                    //OnPropertyChanged(nameof(SelectedTemplateNameTextBoxBorderBrushThickness)); ;
                }
            }
        }

        public Brush TemplateBorderBrush {
            get {
                if(HostTemplateCollectionViewModel == null && 
                  !HostClipTileViewModel.IsEditingContent && 
                  !HostClipTileViewModel.IsPastingTemplate) {
                    return Brushes.Transparent;
                }
                if(IsSelected) {
                    return Brushes.Red;
                }
                if(IsHovering) {
                    return Brushes.Yellow;
                }
                return Brushes.Transparent;
            }
        }

        public Brush TemplateForegroundBrush {
            get {
                if (MpHelpers.Instance.IsBright(((SolidColorBrush)TemplateBackgroundBrush).Color)) {
                    return Brushes.Black;
                }
                return Brushes.White;
            }
        }

        public Brush TemplateBackgroundBrush {
            get {
                if(HostTemplateCollectionViewModel != null &&
                    (HostClipTileViewModel.IsEditingContent || 
                     HostClipTileViewModel.IsPastingTemplate)) {
                    if (IsHovering) {
                        return MpHelpers.Instance.GetDarkerBrush(TemplateBrush);
                    }
                    if (IsSelected) {
                        return MpHelpers.Instance.GetLighterBrush(TemplateBrush);
                    }
                }
                return TemplateBrush;
            }
        }
        #endregion

        #region State Properties
        private bool _isHovering = false;
        public bool IsHovering {
            get {
                return _isHovering;
            }
            set {
                if (_isHovering != value) {
                    _isHovering = value;
                    OnPropertyChanged(nameof(IsHovering));
                    OnPropertyChanged(nameof(TemplateForegroundBrush));
                    OnPropertyChanged(nameof(TemplateBorderBrush));
                    OnPropertyChanged(nameof(TemplateBackgroundBrush));
                    OnPropertyChanged(nameof(TemplateTextBlockCursor));
                }
            }
        }

        private bool _isSelected = false;
        public bool IsSelected {
            get {
                return _isSelected;
            }
            set {
                if (_isSelected != value) 
                    {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    OnPropertyChanged(nameof(TemplateForegroundBrush));
                    OnPropertyChanged(nameof(TemplateBorderBrush));
                    OnPropertyChanged(nameof(TemplateBackgroundBrush));
                    OnPropertyChanged(nameof(TemplateTextBlockCursor));
                }
               
            }
        }

        public bool HasText {
            get {
                return !string.IsNullOrEmpty(TemplateText);
            }
        }

        private bool _isEditingTemplate = false;
        public bool IsEditingTemplate {
            get {
                return _isEditingTemplate;
            }
            set {
                if (_isEditingTemplate != value) {
                    _isEditingTemplate = value;
                    OnPropertyChanged(nameof(IsEditingTemplate));
                }
            }
        }

        private bool _wasVisited = false;
        public bool WasVisited {
            get {
                return _wasVisited;
            }
            set {
                if (_wasVisited != value) {
                    _wasVisited = value;
                    OnPropertyChanged(nameof(WasVisited));
                }

            }
        }
        #endregion

        #region Business Logic Properties
        public int TemplateTokenTag {
            get {
                return (int)MpSubTextTokenType.TemplateSegment;
            }
        }

        public string TemplateDisplayValue {
            get {
                if (HostClipTileViewModel.IsPastingTemplate && 
                    !string.IsNullOrEmpty(TemplateText)) {
                    return TemplateText;
                }
                return TemplateName;
            }
        }

        private string _templateDisplayName = string.Empty;
        public string TemplateDisplayName {
            get {
                if(string.IsNullOrEmpty(TemplateName)) {
                    return string.Empty;
                }
                return TemplateName.Replace("<", String.Empty).Replace(">", string.Empty);
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
                    OnPropertyChanged(nameof(TemplateDisplayValue));
                }
            }
        }

        private TextRange _templateTextRange = null;
        public TextRange TemplateHyperlinkRange {
            get {
                return _templateTextRange;
            }
            set {
                if (_templateTextRange != value) {
                    _templateTextRange = value;
                    OnPropertyChanged(nameof(TemplateHyperlinkRange));
                    OnPropertyChanged(nameof(TemplateDisplayValue));
                }
            }
        }

        private Hyperlink _templateHyperlink = null;
        public Hyperlink TemplateHyperlink {
            get {
                return _templateHyperlink;
            }
            set {
                if(_templateHyperlink != value) {
                    _templateHyperlink = value;
                    OnPropertyChanged(nameof(TemplateHyperlink));
                }
            }
        }

        private TextBlock _templateTextBlock = null;
        public TextBlock TemplateTextBlock {
            get {
                return _templateTextBlock;
            }
            set {
                if (_templateTextBlock != value) {
                    _templateTextBlock = value;
                    OnPropertyChanged(nameof(TemplateTextBlock));
                }
            }
        }
        #endregion

        #region Model Properties
        public bool IsNew {
            get {
                if(CopyItemTemplate == null) {
                    return false;
                }
                return CopyItemTemplate.Id == 0;
            }
        }

        public int CopyItemTemplateId {
            get {
                if (CopyItemTemplate == null) {
                    return 0;
                }
                return CopyItemTemplate.Id;
            }
        }

        public int CopyItemId {
            get {
                if(CopyItemTemplate == null) {
                    return 0;
                }
                return CopyItemTemplate.CopyItemId;
            }
        }

        public string TemplateName {
            get {
                if (CopyItemTemplate == null) {
                    return "TEMPLATE UNKNOWN";
                }
                return CopyItemTemplate.TemplateName;
            }
            set {
                if (CopyItemTemplate == null) {
                    return;
                }
                if (CopyItemTemplate.TemplateName != value) {
                    CopyItemTemplate.TemplateName = value;
                    if(CopyItemTemplate.TemplateName == null) {
                        CopyItemTemplate.TemplateName = string.Empty;
                    }
                    if(!CopyItemTemplate.TemplateName.StartsWith("<")) {
                        CopyItemTemplate.TemplateName = "<" + CopyItemTemplate.TemplateName;
                    }
                    if(!CopyItemTemplate.TemplateName.EndsWith(">")) {
                        CopyItemTemplate.TemplateName = CopyItemTemplate.TemplateName + ">";
                    }
                }

                OnPropertyChanged(nameof(TemplateName));
                OnPropertyChanged(nameof(TemplateDisplayValue));
                OnPropertyChanged(nameof(TemplateDisplayName));
                OnPropertyChanged(nameof(CopyItemTemplate));
            }
        }

        public Brush TemplateBrush {
            get {
                if (CopyItemTemplate == null) {
                    return Brushes.Pink;
                }
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(CopyItemTemplate.HexColor));
            }
            set {
                if (CopyItemTemplate != null) {
                    CopyItemTemplate.HexColor = value.ToString();                    
                    OnPropertyChanged(nameof(TemplateBrush));
                    OnPropertyChanged(nameof(TemplateForegroundBrush));
                    OnPropertyChanged(nameof(TemplateBackgroundBrush));
                    OnPropertyChanged(nameof(CopyItemTemplate));
                }
            }
        }

        private MpCopyItemTemplate _copyItemTemplate = null;
        public MpCopyItemTemplate CopyItemTemplate {
            get {
                return _copyItemTemplate;
            }
            set {
                if (_copyItemTemplate != value) {
                    _copyItemTemplate = value;
                    OnPropertyChanged(nameof(CopyItemTemplate));
                    OnPropertyChanged(nameof(TemplateBrush));
                    OnPropertyChanged(nameof(TemplateName)); 
                    OnPropertyChanged(nameof(TemplateDisplayName));
                    OnPropertyChanged(nameof(TemplateDisplayValue));
                    OnPropertyChanged(nameof(CopyItemTemplateId));
                    OnPropertyChanged(nameof(CopyItemId));
                }
            }
        }
        #endregion

        #endregion

        public event EventHandler OnTemplateSelected;

        #region Factory Methods
        //public static Hyperlink CreateTemplateHyperlink(MpRtbItemViewModel rtbvm, MpCopyItemTemplate cit, TextRange tr) {
        //    var thlvm = new MpTemplateHyperlinkViewModel(rtbvm, cit);

        //    //if the range for the template contains a sub-selection of a hyperlink the hyperlink(s)
        //    //needs to be broken into their text before the template hyperlink can be created
        //    var trSHl = tr.Start.Parent.FindParentOfType<Hyperlink>();
        //    var trEHl = tr.End.Parent.FindParentOfType<Hyperlink>();
        //    var trText = tr.Text;

        //    if (trSHl != null) {
        //        var linkText = new TextRange(trSHl.ElementStart, trSHl.ElementEnd).Text;
        //        trSHl.Inlines.Clear();
        //        var span = new Span(new Run(linkText), trSHl.ElementStart);
        //        tr =  MpHelpers.Instance.FindStringRangeFromPosition(span.ContentStart, trText, true);
        //    }
        //    if (trEHl != null && trEHl != trSHl) {
        //        var linkText = new TextRange(trEHl.ElementStart, trEHl.ElementEnd).Text;
        //        trEHl.Inlines.Clear();
        //        var span = new Span(new Run(linkText), trEHl.ElementStart);
        //        tr = MpHelpers.Instance.FindStringRangeFromPosition(span.ContentStart, trText, true);
        //    }
        //    thlvm.TemplateHyperlinkRange = tr;

        //    var tb = new TextBlock();
        //    tb.DataContext = thlvm;
        //    tb.Loaded += thlvm.TemplateHyperLinkRun_Loaded;

        //    var b = new Border();
        //    b.CornerRadius = new CornerRadius(5);
        //    b.BorderThickness = new Thickness(1.5);
        //    b.DataContext = thlvm;
        //    b.Child = tb;
            
        //    var iuic = new InlineUIContainer();
        //    iuic.DataContext = thlvm;
        //    iuic.Child = b;
            
        //    var hl = new Hyperlink(tr.Start,tr.End);
        //    hl.DataContext = thlvm;
        //    hl.Inlines.Clear();
        //    hl.Inlines.Add(iuic);

        //    b.RenderTransform = new TranslateTransform(0, 4);
        //    //add trailing run of one space to allow clicking after iuic
        //    var tailStartPointer = hl.ElementEnd.GetInsertionPosition(LogicalDirection.Forward);
        //    if(new TextRange(tr.End,rtbvm.Rtb.Document.ContentEnd).IsEmpty) {
        //        new Run(@" ", rtbvm.Rtb.Document.ContentEnd);
        //    }
            

        //    thlvm.TemplateHyperlink = hl;

        //    return hl;
        //}
        #endregion

        #region Public Methods
        public MpTemplateHyperlinkViewModel() : base() { }
        public MpTemplateHyperlinkViewModel(MpTemplateHyperlinkCollectionViewModel thlcvm, MpCopyItemTemplate cit) : base() {
            PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(IsSelected):
                        if(IsSelected) {
                             OnTemplateSelected?.Invoke(this, null);
                        }
                        break;
                    case nameof(TemplateName):
                        Validate();
                        break;
                }
            };
            HostTemplateCollectionViewModel = thlcvm;
            CopyItemTemplate = cit;         
        }

        public void SetTemplateText(string text) {
            TemplateText = text;
        }

        public bool Validate() {
            HostTemplateCollectionViewModel.HostRtbViewModel.SaveToDatabase();

            if (string.IsNullOrEmpty(TemplateName.Trim())) {
                ValidationText = "Name cannot be empty!";
                return false;
            }

            string pt = HostTemplateCollectionViewModel.HostRtbViewModel.CopyItem.ItemData.ToPlainText();
            if (pt.Contains(TemplateName)) {
                ValidationText = $"{TemplateName} must have a unique name";
                return false;
            }

            ValidationText = string.Empty;
            return true;
        }

        public void Reset() {
            TemplateText = string.Empty;
            IsEditingTemplate = false;
        }

        #endregion

        #region Commands
        public ICommand EditTemplateCommand {
            get {
                return new RelayCommand(
                    () => {
                        _originalModel = CopyItemTemplate.Clone() as MpCopyItemTemplate;

                        HostTemplateCollectionViewModel.ClearAllEditing();
                        HostTemplateCollectionViewModel.ClearSelection();

                        IsSelected = true;
                        IsEditingTemplate = true;

                        HostClipTileViewModel.IsEditingTemplate = true;
                    },
                    () => {
                        if (HostClipTileViewModel == null) {
                            return false;
                        }
                        return HostClipTileViewModel.IsExpanded;
                    });
            }
        }

        public ICommand DeleteTemplateCommand {
            get {
                return new RelayCommand(
                    () => {
                        CopyItemTemplate.DeleteFromDatabase();
                    });
            }
        }

        public ICommand ClearTemplateCommand {
            get {
                return new RelayCommand(
                    () => {
                        TemplateText = string.Empty;
                    },
                    ()=> {
                        return HasText;
                    });
            }
        }

        public ICommand CancelCommand {
            get {
                return new RelayCommand(
                    () => {
                        CopyItemTemplate = _originalModel;
                        IsEditingTemplate = false;
                        IsSelected = false;
                    });
            }
        }

        public ICommand OkCommand {
            get {
                return new RelayCommand(
                    () => {
                        CopyItemTemplate.WriteToDatabase();
                        IsEditingTemplate = false;
                        IsSelected = false;
                    },
                    () => {
                        return !Validate();
                    });
            }
        }

        public ICommand ChangeTemplateColorCommand {
            get {
                return new RelayCommand<object>(
                    (args) => {
                        var templateColorButton = args as Button;
                        var colorMenuItem = new MenuItem();
                        var colorContextMenu = new ContextMenu();
                        colorContextMenu.Items.Add(colorMenuItem);
                        MpHelpers.Instance.SetColorChooserMenuItem(
                            colorContextMenu,
                            colorMenuItem,
                            (s1, e1) => {
                                TemplateBrush = (Brush)((Border)s1).Tag;
                            },
                            MpHelpers.Instance.GetColorColumn(TemplateBrush),
                            MpHelpers.Instance.GetColorRow(TemplateBrush)
                        );
                        templateColorButton.ContextMenu = colorContextMenu;
                        colorContextMenu.PlacementTarget = templateColorButton;
                        colorContextMenu.Width = 200;
                        colorContextMenu.Height = 100;
                        colorContextMenu.IsOpen = true;
                    });
            }
        }
        #endregion

        #region Overrides
        public override string ToString() {
            return TemplateName;
        }

        //public void Dispose(bool fromContextMenu) {
        //    if(fromContextMenu) {
        //        HostTemplateCollectionViewModel.Rtb.Selection.Select(TemplateHyperlinkRange.Start, TemplateHyperlinkRange.End);
        //        HostTemplateCollectionViewModel.Rtb.Selection.Text = string.Empty;
        //    }
        //    //remove this individual token reference
        //    if(HostTemplateCollectionViewModel != null) {
        //        HostTemplateCollectionViewModel.TemplateHyperlinkCollectionViewModel.Templates.Remove(this);
        //    }
            
        //}

        public object Clone() {
            var nthlvm = new MpTemplateHyperlinkViewModel(HostTemplateCollectionViewModel, CopyItemTemplate);
            nthlvm.TemplateText = TemplateText;
            return nthlvm;
        }
        #endregion
    }
}
