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
        private RichTextBox _rtb = null;
        #endregion

        #region View Models
        private MpTemplateToolbarViewModel _pasteTemplateToolbarViewModel = null;
        public MpTemplateToolbarViewModel PasteTemplateToolbarViewModel {
            get {
                return _pasteTemplateToolbarViewModel;
            }
            set {
                if (_pasteTemplateToolbarViewModel != value) {
                    _pasteTemplateToolbarViewModel = value;
                    OnPropertyChanged(nameof(PasteTemplateToolbarViewModel));
                }
            }
        }
        #endregion

        #region Properties

        #region Layout Properties
        //public double TemplateFontSize {
        //    get {
        //        return (double)TemplateRange.GetPropertyValue(TextElement.FontSizeProperty);
        //    }
        //}

        //public Typeface TemplateTypeFace {
        //    get {
        //        return new Typeface(
        //            (FontFamily)TemplateRange.GetPropertyValue(TextElement.FontFamilyProperty),
        //            (FontStyle)TemplateRange.GetPropertyValue(TextElement.FontStyleProperty),
        //            (FontWeight)TemplateRange.GetPropertyValue(TextElement.FontWeightProperty),
        //            (FontStretch)TemplateRange.GetPropertyValue(TextElement.FontStretchProperty));
        //    }
        //}

        //public double TemplateBorderPadding {
        //    get {
        //        return 2;
        //    }
        //}

        //public double TemplateBorderHeight {
        //    get {
        //        return TemplateTextBlockHeight + TemplateBorderPadding;
        //    }
        //}

        //public double TemplateBorderWidth {
        //    get {
        //        if(PasteTemplateToolbarViewModel.ClipTileViewModel.IsEditingTile) {
        //            return TemplateTextBlockWidth + TemplateDeleteButtonSize + (TemplateBorderPadding * 2);
        //        }
        //        return TemplateTextBlockWidth + (TemplateBorderPadding * 2);
        //    }
        //}

        //public double TemplateTextBlockWidth {
        //    get {
        //        return MpHelpers.MeasureText(
        //            TemplateDisplayValue,
        //            TemplateTypeFace,
        //            TemplateFontSize).Width;
        //    }
        //}

        //public double TemplateTextBlockHeight {
        //    get {
        //        return MpHelpers.MeasureText(
        //            TemplateDisplayValue,
        //            TemplateTypeFace,
        //            TemplateFontSize).Height;
        //    }
        //}

        //public double TemplateDeleteButtonSize {
        //    get {
        //        return TemplateTextBlockHeight - 2;
        //    }
        //}
        #endregion

        #region Visibility Properties
        public Visibility DeleteTemplateTextButtonVisibility {
            get {
                if (PasteTemplateToolbarViewModel.ClipTileViewModel.IsEditingTile) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }
        #endregion

        #region Brush Properties
        public Brush TemplateBorderBrush {
            get {     
                //if(IsSelected) {
                //    return Brushes.Red;
                //}
                if (IsSelected) {
                    //if (string.IsNullOrEmpty(TemplateText)) {
                    //    return Brushes.Red;
                    //}
                    return Brushes.Green;
                }
                if (IsHovering) {
                    return Brushes.Yellow;
                }
                return TemplateBrush;
            }
        }

        public Brush TemplateForegroundBrush {
            get {
                if (MpHelpers.IsBright(((SolidColorBrush)TemplateBrush).Color)) {
                    return Brushes.Black;
                }
                return Brushes.White;
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
                    OnPropertyChanged(nameof(TemplateBorderBrush));
                    OnPropertyChanged(nameof(DeleteTemplateTextButtonVisibility));
                }
            }
        }
        #endregion

        #region Business Logic Properties
        public string TemplateDisplayValue {
            get {
                if (PasteTemplateToolbarViewModel.IsPastingTemplateTile && 
                    !string.IsNullOrEmpty(TemplateText)) {
                    return TemplateText;
                }
                return TemplateName;
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
                    OnPropertyChanged(nameof(TemplateName));
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
                }
            }
        }

        private MpCopyItemTemplate _copyItemTemplate = new MpCopyItemTemplate();
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
        //public MpTemplateHyperlinkViewModel() : this(new MpPasteTemplateToolbarViewModel(), new MpCopyItemTemplate()) { }

        public MpTemplateHyperlinkViewModel(MpTemplateToolbarViewModel pttbvm, MpCopyItemTemplate cit) : base() {
            PasteTemplateToolbarViewModel = pttbvm;
            CopyItemTemplate = cit;
        }
        
        public void TemplateHyperLink_Loaded(object sender, RoutedEventArgs args) {
            var uc = (UserControl)sender;
            var hl = (Hyperlink)uc.GetVisualAncestor<InlineUIContainer>().Parent;
            var b = (Border)uc.FindName("TemplateHyperlinkBorder");
            var tb = (TextBlock)uc.FindName("TemplateTextBlock");
            var dbImg = (Image)hl.FindName("DeleteTemplateTokenButton");
            var rtb = PasteTemplateToolbarViewModel.ClipTileViewModel.GetRtb();
            var path = @"pack://application:,,,/Resources/Images/";
            dbImg.MouseEnter += (s, e) => {
                //db.Background = Brushes.Transparent;
                dbImg.Source = new BitmapImage(new Uri(path + "close1.png"));
            };
            dbImg.MouseLeave += (s, e) => {
                //db.Background = Brushes.Transparent;
                dbImg.Source = new BitmapImage(new Uri(path + "close2.png"));
            };
            dbImg.MouseLeftButtonDown += (s, e) => {
                rtb.Selection.Select(hl.ElementStart, hl.ElementEnd);
                rtb.Selection.Text = string.Empty;
                Dispose();
            };


            b.MouseEnter += (s, e) => {
                if (IsSelected) {
                    return;
                }
                IsHovering = true;
            };
            b.MouseLeave += (s, e) => {
                if (IsSelected) {
                    return;
                }
                IsHovering = false;
            };
            b.PreviewMouseLeftButtonDown += (s, e) => {
                IsSelected = true;
                //if (ctvm.IsPastingTemplateTile) {
                //    e.Handled = true;
                //    int i = 0;
                //    for (; i < ctvm.TemplateTokenLookupDictionary.Count; i++) {
                //        if (ctvm.TemplateTokenLookupDictionary.ElementAt(i).Key == thlvm.TemplateName) {
                //            break;
                //        }
                //    }
                //    ctvm.CurrentTemplateLookupIdx = i;
                //    thlvm.IsSelected = true;
                //    thlvm.WasTypeViewed = true;
                //}
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
                PasteTemplateToolbarViewModel.EditTemplateHyperlinkViewModel.SetTemplate(this, true);
                PasteTemplateToolbarViewModel.IsEditingTemplate = true;
                //MpEditTemplateHyperlinkViewModel.ShowTemplateTokenEditModalWindow(PasteTemplateToolbarViewModel, this, true);
            };

            var editTemplateMenuItem = new MenuItem();
            editTemplateMenuItem.Header = "Edit";
            editTemplateMenuItem.PreviewMouseDown += (s4, e4) => {
                e4.Handled = true;
                _rtb.Selection.Select(hl.ElementStart, hl.ElementEnd);
                //MpEditTemplateHyperlinkViewModel.ShowTemplateTokenEditModalWindow(PasteTemplateToolbarViewModel, this, true);
                PasteTemplateToolbarViewModel.EditTemplateHyperlinkViewModel.SetTemplate(this, true);
                PasteTemplateToolbarViewModel.IsEditingTemplate = true;
            };

            var deleteTemplateMenuItem = new MenuItem();
            deleteTemplateMenuItem.Header = "Delete";
            deleteTemplateMenuItem.Click += (s4, e4) => {
                _rtb.Selection.Select(hl.ElementStart, hl.ElementEnd);
                _rtb.Selection.Text = string.Empty;
                Dispose();
            };
            hl.ContextMenu = new ContextMenu();
            hl.ContextMenu.Items.Add(editTemplateMenuItem);
            hl.ContextMenu.Items.Add(deleteTemplateMenuItem);
        }
        #endregion

        #region Commands
        
        #endregion

        #region Overrides
        public override string ToString() {
            return string.Format(
                @"{0}{1}{0}{2}{0}",
                Properties.Settings.Default.TemplateTokenMarker,
                TemplateName,
                TemplateBrush.ToString());
        }

        public void Dispose() {
            //remove this individual token reference
            PasteTemplateToolbarViewModel.Remove(this);

            //checking clip's remaing templates, if it was the last of its type remove its dictionary keyvalue
            //if (ClipTileViewModel.TemplateTokens.Where(x => x.TemplateName == TemplateName).ToList().Count == 0) {
            //    ClipTileViewModel.TemplateTokenLookupDictionary.Remove(TemplateName);
            //}
        }
        #endregion
    }
}
