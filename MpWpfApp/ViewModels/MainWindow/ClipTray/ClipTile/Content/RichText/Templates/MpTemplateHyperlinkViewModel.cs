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
    public class MpTemplateHyperlinkViewModel : MpViewModelBase, ICloneable {
        #region Private Variables
        #endregion

        #region View Models
        private MpClipTileViewModel _clipTileViewModel = null;
        public MpClipTileViewModel ClipTileViewModel {
            get {
                return _clipTileViewModel;
            }
            set {
                if (_clipTileViewModel != value) {
                    _clipTileViewModel = value;
                    OnPropertyChanged(nameof(ClipTileViewModel));
                    OnPropertyChanged(nameof(TemplateDisplayValue));
                    OnPropertyChanged(nameof(TemplateTextBlockCursor));
                    OnPropertyChanged(nameof(TemplateName));
                    OnPropertyChanged(nameof(TemplateDisplayName));
                }
            }
        }
        #endregion

        #region Property Reflection Referencer
        public object this[string propertyName] {
            get {
                // probably faster without reflection:
                // like:  return Properties.Settings.Default.PropertyValues[propertyName] 
                // instead of the following
                Type myType = typeof(MpTemplateHyperlinkViewModel);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                if (myPropInfo == null) {
                    throw new Exception("Unable to find property: " + propertyName);
                }
                return myPropInfo.GetValue(this, null);
            }
            set {
                Type myType = typeof(MpTemplateHyperlinkViewModel);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                myPropInfo.SetValue(this, value, null);
            }
        }
        #endregion

        #region Properties

        #region Layout Properties        
        #endregion

        #region Appearance Properties
        public Cursor TemplateTextBlockCursor {
            get {
                if(ClipTileViewModel != null && 
                  (ClipTileViewModel.IsEditingTile || ClipTileViewModel.IsEditingTemplate)) {
                    return Cursors.Hand;
                }
                return Cursors.Arrow;
            }
        }
        #endregion

        #region Visibility Properties
        #endregion

        #region Brush Properties
        public Brush TemplateBorderBrush {
            get {
                if(ClipTileViewModel != null && 
                  !ClipTileViewModel.IsEditingTile && 
                  !ClipTileViewModel.IsEditingTemplate && 
                  !ClipTileViewModel.IsPastingTemplateTile) {
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
                if(ClipTileViewModel != null &&
                    (ClipTileViewModel.IsEditingTile || ClipTileViewModel.IsPastingTemplateTile)) {
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
        #endregion

        #region Business Logic Properties
        public string TemplateDisplayValue {
            get {
                if (ClipTileViewModel.IsPastingTemplateTile && 
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
            private set {
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
        public int CopyItemTemplateId {
            get {
                if (CopyItemTemplate == null) {
                    return 0;
                }
                return CopyItemTemplate.CopyItemTemplateId;
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
                return CopyItemTemplate.TemplateColor;
            }
            set {
                if (CopyItemTemplate != null &&
                    CopyItemTemplate.TemplateColor != value) {
                    CopyItemTemplate.TemplateColor = value;
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

        #region Factory Methods
        public static Hyperlink CreateTemplateHyperlink(MpClipTileViewModel ctvm, MpCopyItemTemplate cit, TextRange tr) {
            var thlvm = new MpTemplateHyperlinkViewModel(ctvm, cit);

            //if the range for the template contains a sub-selection of a hyperlink the hyperlink(s)
            //needs to be broken into their text before the template hyperlink can be created
            var trSHl = (Hyperlink)MpHelpers.Instance.FindParentOfType(tr.Start.Parent, typeof(Hyperlink));
            var trEHl = (Hyperlink)MpHelpers.Instance.FindParentOfType(tr.End.Parent, typeof(Hyperlink));
            var trText = tr.Text;

            if (trSHl != null) {
                var linkText = new TextRange(trSHl.ElementStart, trSHl.ElementEnd).Text;
                trSHl.Inlines.Clear();
                var span = new Span(new Run(linkText), trSHl.ElementStart);
                tr =  MpHelpers.Instance.FindStringRangeFromPosition(span.ContentStart, trText, true);
            }
            if (trEHl != null && trEHl != trSHl) {
                var linkText = new TextRange(trEHl.ElementStart, trEHl.ElementEnd).Text;
                trEHl.Inlines.Clear();
                var span = new Span(new Run(linkText), trEHl.ElementStart);
                tr = MpHelpers.Instance.FindStringRangeFromPosition(span.ContentStart, trText, true);
            }
            thlvm.TemplateHyperlinkRange = tr;

            var tb = new TextBlock();
            tb.DataContext = thlvm;
            tb.Loaded += thlvm.TemplateHyperLinkRun_Loaded;

            var b = new Border();
            b.CornerRadius = new CornerRadius(5);
            b.BorderThickness = new Thickness(1.5);
            b.DataContext = thlvm;
            b.Child = tb;

            var iuic = new InlineUIContainer();
            iuic.DataContext = thlvm;
            iuic.Child = b;

            //var matchRun = new Run(tr.Text);
            //tr.Text = "";
            // DO NOT REMOVE this extra link ensures selection is retained!
            //var hlink = new Hyperlink(matchRun, tr.Start);
            var hl = new Hyperlink(tr.Start,tr.End);
            hl.DataContext = thlvm;
            hl.Inlines.Clear();
            hl.Inlines.Add(iuic);
            thlvm.TemplateHyperlink = hl;

            return hl;
        }
        #endregion
        #region Public Methods
        //public MpTemplateHyperlinkViewModel() :this(new MpClipTileViewModel(new MpCopyItem()),new MpCopyItemTemplate()) { }

        public MpTemplateHyperlinkViewModel(MpClipTileViewModel ctvm, MpCopyItemTemplate cit) : base() {
            ClipTileViewModel = ctvm;
            if (cit == null) {
                //case of a new template create new w/ unique name & color
                int uniqueIdx = 1;
                string namePrefix = "<Template #";
                while (ClipTileViewModel.CopyItemPlainText.ToLower().Contains(namePrefix.ToLower() + uniqueIdx) || 
                       ClipTileViewModel.RichTextBoxViewModelCollection.SelectedClipTileRichTextBoxViewModel.TemplateHyperlinkCollectionViewModel.Where(x => x.TemplateName == namePrefix + uniqueIdx + ">").ToList().Count > 0) {
                    uniqueIdx++;
                }
                Brush randColor = (Brush)new SolidColorBrush(MpHelpers.Instance.GetRandomColor());
                //while (ClipTileViewModel.RichTextBoxViewModels.SelectedClipTileRichTextBoxViewModel.TemplateHyperlinkCollectionViewModel.Where(x => x.TemplateBrush == randColor).ToList().Count > 0) {
                //    randColor = (Brush)new SolidColorBrush(MpHelpers.Instance.GetRandomColor());
                //}
                cit = new MpCopyItemTemplate(ctvm.CopyItemId, randColor, namePrefix + uniqueIdx + ">");
            }
            CopyItemTemplate = cit;         
        }
        
        public void TemplateHyperLinkRun_Loaded(object sender, RoutedEventArgs args) {
            var tb = (TextBlock)sender;
            var b = (Border)tb.Parent;
            var hl = (Hyperlink)MpHelpers.Instance.FindParentOfType(b, typeof(Hyperlink));

            MpHelpers.Instance.CreateBinding(this, new PropertyPath(nameof(TemplateDisplayValue)), tb, TextBlock.TextProperty);
            //MpHelpers.Instance.CreateBinding(this, new PropertyPath(nameof(TemplateBackgroundBrush)), tb, TextBlock.BackgroundProperty);
            MpHelpers.Instance.CreateBinding(this, new PropertyPath(nameof(TemplateForegroundBrush)), tb, TextBlock.ForegroundProperty);
            MpHelpers.Instance.CreateBinding(this, new PropertyPath(nameof(TemplateTextBlockCursor)), tb, TextBlock.CursorProperty);

            MpHelpers.Instance.CreateBinding(this, new PropertyPath(nameof(TemplateBackgroundBrush)), b, Border.BackgroundProperty);
            MpHelpers.Instance.CreateBinding(this, new PropertyPath(nameof(TemplateBorderBrush)), b, Border.BorderBrushProperty);
            MpHelpers.Instance.CreateBinding(this, new PropertyPath(nameof(TemplateTextBlockCursor)), b, Border.CursorProperty);

            //MpHelpers.Instance.CreateBinding(this, new PropertyPath(nameof(TemplateBackgroundBrush)), hl, Hyperlink.BackgroundProperty);
            //MpHelpers.Instance.CreateBinding(this, new PropertyPath(nameof(TemplateForegroundBrush)), hl, Hyperlink.ForegroundProperty);
            MpHelpers.Instance.CreateBinding(this, new PropertyPath(nameof(TemplateTextBlockCursor)), hl, Hyperlink.CursorProperty);

            hl.Tag = MpSubTextTokenType.TemplateSegment;
            hl.IsEnabled = true;
            hl.TextDecorations = null;
            hl.NavigateUri = new Uri(Properties.Settings.Default.TemplateTokenUri);
            hl.Unloaded += (s, e) => {
                //occurs when template is deleted in edit tile mode
                Dispose(false);
            };
            hl.MouseEnter += (s, e) => {
                IsHovering = true;
            };
            hl.MouseLeave += (s, e) => {
                IsHovering = false;
            };
            hl.PreviewMouseLeftButtonDown += (s, e) => {
                if (!ClipTileViewModel.IsSelected) {
                    return;
                }
                if (ClipTileViewModel.IsEditingTile || ClipTileViewModel.IsPastingTemplateTile) {
                    IsSelected = true;
                }
                if (ClipTileViewModel.IsEditingTile) {
                    e.Handled = true;
                    ClipTileViewModel.EditTemplateToolbarViewModel.SetTemplate(this, true);
                }
                if(ClipTileViewModel.IsPastingTemplateTile) {
                    e.Handled = true;
                    ClipTileViewModel.PasteTemplateToolbarViewModel.SetTemplate(TemplateName);
                }
            };

            #region Context Menu
            var editTemplateMenuItem = new MenuItem();
            editTemplateMenuItem.Header = "Edit";
            editTemplateMenuItem.PreviewMouseDown += (s4, e4) => {
                if (!ClipTileViewModel.IsSelected) {
                    return;
                }
                if (ClipTileViewModel.IsEditingTile || ClipTileViewModel.IsPastingTemplateTile) {
                    IsSelected = true;
                }
                if (ClipTileViewModel.IsEditingTile) {
                    e4.Handled = true;
                    ClipTileViewModel.EditTemplateToolbarViewModel.SetTemplate(this, true);
                }
            };

            var deleteTemplateMenuItem = new MenuItem();
            deleteTemplateMenuItem.Header = "Delete";
            deleteTemplateMenuItem.Click += (s4, e4) => {
                Dispose(true);
            };

            hl.ContextMenu = new ContextMenu();
            hl.ContextMenu.Items.Add(editTemplateMenuItem);
            hl.ContextMenu.Items.Add(deleteTemplateMenuItem);
            #endregion

            TemplateHyperlink = hl;
            TemplateTextBlock = tb;
            TemplateHyperlinkRange = new TextRange(hl.ElementStart, hl.ElementEnd);
        }
        
        public void SetTemplateText(string templateText) {
            TemplateText = templateText;
        }
        #endregion

        #region Commands
        
        #endregion             

        #region Overrides
        public override string ToString() {
            return TemplateName;
        }

        public void Dispose(bool fromContextMenu) {
            if(fromContextMenu) {
                ClipTileViewModel.RichTextBoxViewModelCollection.SelectedRtb.Selection.Select(TemplateHyperlinkRange.Start, TemplateHyperlinkRange.End);
                ClipTileViewModel.RichTextBoxViewModelCollection.SelectedRtb.Selection.Text = string.Empty;
            }
            //remove this individual token reference
            ClipTileViewModel.RichTextBoxViewModelCollection.SelectedClipTileRichTextBoxViewModel.TemplateHyperlinkCollectionViewModel.Remove(this);            
        }

        public object Clone() {
            var nthlvm = new MpTemplateHyperlinkViewModel(ClipTileViewModel, CopyItemTemplate);
            nthlvm.TemplateText = TemplateText;
            return nthlvm;
        }
        #endregion
    }
}
