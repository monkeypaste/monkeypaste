using GalaSoft.MvvmLight.CommandWpf;
using GongSolutions.Wpf.DragDrop.Utilities;
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
    public class MpTemplateHyperlinkViewModel : MpViewModelBase, IDisposable {
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
                }
            }
        }
        #endregion

        #region Properties
        
        #region Layout Properties        
        #endregion

        #region Appearance Properties
        public Cursor TemplateTextBlockCursor {
            get {
                if(ClipTileViewModel.IsEditingTile || ClipTileViewModel.IsEditingTemplate) {
                    return Cursors.Hand;
                }
                return Cursors.Arrow;
            }
        }
        #endregion

        #region Visibility Properties
        #endregion

        #region Brush Properties
        public Brush TemplateForegroundBrush {
            get {
                if (MpHelpers.IsBright(((SolidColorBrush)TemplateBackgroundBrush).Color)) {
                    return Brushes.Black;
                }
                return Brushes.White;
            }
        }

        public Brush TemplateBackgroundBrush {
            get {
                if(ClipTileViewModel.IsSelected) {
                    if (IsHovering) {
                        return MpHelpers.GetLighterBrush(TemplateBrush);
                    }
                    if (IsSelected) {
                        return MpHelpers.GetDarkerBrush(TemplateBrush);
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
                if (_isSelected != value) {
                    _isSelected = value;
                    OnPropertyChanged(nameof(IsSelected));
                    OnPropertyChanged(nameof(TemplateForegroundBrush));
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

        private string _dummy = string.Empty;
        public string Dummy {
            get {
                return _dummy;
            }
            set {
                if (_dummy != value) {
                    _dummy = value;
                    OnPropertyChanged(nameof(Dummy));
                }
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
        public TextRange TemplateTextRange {
            get {
                return _templateTextRange;
            }
            set {
                if (_templateTextRange != value) {
                    _templateTextRange = value;
                    OnPropertyChanged(nameof(TemplateTextRange));
                    OnPropertyChanged(nameof(TemplateDisplayValue));
                }
            }
        }
        #endregion

        #region Model Properties
        public int CopyItemTemplateId {
            get {
                return CopyItemTemplate.CopyItemTemplateId;
            }
        }

        public int CopyItemId {
            get {
                return CopyItemTemplate.CopyItemId;
            }
        }

        public string TemplateName {
            get {
                return CopyItemTemplate.TemplateName;
            }
            set {
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
                    OnPropertyChanged(nameof(TemplateName));
                    OnPropertyChanged(nameof(TemplateDisplayValue));
                }
            }
        }

        public Brush TemplateBrush {
            get {
                return CopyItemTemplate.TemplateColor;
            }
            set {
                if (CopyItemTemplate.TemplateColor != value) {
                    CopyItemTemplate.TemplateColor = value;
                    OnPropertyChanged(nameof(TemplateBrush));
                    OnPropertyChanged(nameof(TemplateForegroundBrush));
                    OnPropertyChanged(nameof(TemplateBackgroundBrush));
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
                    OnPropertyChanged(nameof(CopyItemTemplateId));
                    OnPropertyChanged(nameof(CopyItemId));
                }
            }
        }
        #endregion

        #endregion

        #region Public Methods
        //public MpTemplateHyperlinkViewModel() :this(new MpClipTileViewModel(new MpCopyItem()),new MpCopyItemTemplate()) { }

        public MpTemplateHyperlinkViewModel(MpClipTileViewModel ctvm, MpCopyItemTemplate cit) : base() {
            PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(IsSelected):

                        break;
                }
            };
            ClipTileViewModel = ctvm;
            if (cit == null) {
                //case of a new template create new w/ unique name & color
                int uniqueIdx = 1;
                string namePrefix = "<Template #";
                while (ClipTileViewModel.TemplateHyperlinkCollectionViewModel.Where(x => x.TemplateName == namePrefix + uniqueIdx + ">").ToList().Count > 0) {
                    uniqueIdx++;
                }
                Brush randColor = (Brush)new SolidColorBrush(MpHelpers.GetRandomColor());
                //while (ClipTileViewModel.TemplateHyperlinkCollectionViewModel.Where(x => x.TemplateBrush == randColor).ToList().Count > 0) {
                //    randColor = (Brush)new SolidColorBrush(MpHelpers.GetRandomColor());
                //}
                cit = new MpCopyItemTemplate(ctvm.CopyItemId, randColor, namePrefix + uniqueIdx + ">");
            }
            CopyItemTemplate = cit;         
        }
        
        public void TemplateHyperLinkRun_Loaded(object sender, RoutedEventArgs args) {
            var r = (Run)sender;
            var hl = (Hyperlink)r.Parent;

            MpHelpers.CreateBinding(this, new PropertyPath(nameof(TemplateDisplayValue)), r, Run.TextProperty);
            MpHelpers.CreateBinding(this, new PropertyPath(nameof(TemplateBackgroundBrush)), r, Run.BackgroundProperty);
            MpHelpers.CreateBinding(this, new PropertyPath(nameof(TemplateForegroundBrush)), r, Run.ForegroundProperty);
            MpHelpers.CreateBinding(this, new PropertyPath(nameof(TemplateTextBlockCursor)), r, Run.CursorProperty);
            MpHelpers.CreateBinding(this, new PropertyPath(nameof(TemplateTextBlockCursor)), hl, Hyperlink.CursorProperty);

            hl.Tag = MpSubTextTokenType.TemplateSegment;
            hl.IsEnabled = true;
            hl.TextDecorations = null;
            hl.NavigateUri = new Uri(Properties.Settings.Default.TemplateTokenUri);
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
                    //ClipTileViewModel.GetRtb().Selection.Select(hl.ElementStart, hl.ElementEnd);
                    ClipTileViewModel.EditTemplateToolbarViewModel.SetTemplate(this, true);
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
                    //ClipTileViewModel.GetRtb().Selection.Select(hl.ElementStart, hl.ElementEnd);
                    ClipTileViewModel.EditTemplateToolbarViewModel.SetTemplate(this, true);
                }
            };

            var deleteTemplateMenuItem = new MenuItem();
            deleteTemplateMenuItem.Header = "Delete";
            deleteTemplateMenuItem.Click += (s4, e4) => {
                Dispose();
            };

            hl.ContextMenu = new ContextMenu();
            hl.ContextMenu.Items.Add(editTemplateMenuItem);
            hl.ContextMenu.Items.Add(deleteTemplateMenuItem);
            #endregion

            TemplateTextRange = new TextRange(hl.ElementStart, hl.ElementEnd);
        }
        #endregion

        #region Commands
        
        #endregion             

        #region Overrides
        public override string ToString() {
            return TemplateName;
        }

        public void Dispose() {
            ClipTileViewModel.GetRtb().Selection.Select(TemplateTextRange.Start, TemplateTextRange.End);
            ClipTileViewModel.GetRtb().Selection.Text = string.Empty;
            //remove this individual token reference
            ClipTileViewModel.TemplateHyperlinkCollectionViewModel.Remove(this);
            
            if(IsSelected && ClipTileViewModel.IsEditingTemplate) {
                ClipTileViewModel.IsEditingTemplate = false;
            }
        }
        #endregion
    }
}
