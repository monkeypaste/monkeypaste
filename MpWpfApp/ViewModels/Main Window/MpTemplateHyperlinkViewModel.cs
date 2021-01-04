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
        private MpTemplateTokenCollectionViewModel _templateTokenCollectionViewModel = new MpTemplateTokenCollectionViewModel();
        public MpTemplateTokenCollectionViewModel TemplateTokenCollectionViewModel {
            get {
                return _templateTokenCollectionViewModel;
            }
            set {
                if(_templateTokenCollectionViewModel != value) {
                    _templateTokenCollectionViewModel = value;
                    OnPropertyChanged(nameof(TemplateTokenCollectionViewModel));
                    OnPropertyChanged(nameof(TemplateBackgroundBrush));
                    OnPropertyChanged(nameof(TemplateBorderWidth));
                    OnPropertyChanged(nameof(DeleteTemplateTextButtonVisibility));
                    OnPropertyChanged(nameof(TemplateDisplayValue));
                    OnPropertyChanged(nameof(TemplateName));
                    OnPropertyChanged(nameof(TemplateStart));
                    OnPropertyChanged(nameof(TemplateEnd));
                    OnPropertyChanged(nameof(TemplateRange));
                }
            }
        }
        #endregion

        #region Properties

        #region Layout Properties
        public double TemplateFontSize {
            get {
                return (double)TemplateRange.GetPropertyValue(TextElement.FontSizeProperty);
            }
        }

        public Typeface TemplateTypeFace {
            get {
                return new Typeface(
                    (FontFamily)TemplateRange.GetPropertyValue(TextElement.FontFamilyProperty),
                    (FontStyle)TemplateRange.GetPropertyValue(TextElement.FontStyleProperty),
                    (FontWeight)TemplateRange.GetPropertyValue(TextElement.FontWeightProperty),
                    (FontStretch)TemplateRange.GetPropertyValue(TextElement.FontStretchProperty));
            }
        }

        public double TemplateBorderPadding {
            get {
                return 2;
            }
        }

        public double TemplateBorderHeight {
            get {
                return TemplateTextBlockHeight + TemplateBorderPadding;
            }
        }

        public double TemplateBorderWidth {
            get {
                if(TemplateTokenCollectionViewModel.PasteTemplateToolbarViewModel.ClipTileViewModel.IsEditingTile) {
                    return TemplateTextBlockWidth + TemplateDeleteButtonSize + (TemplateBorderPadding * 2);
                }
                return TemplateTextBlockWidth + (TemplateBorderPadding * 2);
            }
        }

        public double TemplateTextBlockWidth {
            get {
                return MpHelpers.MeasureText(
                    TemplateDisplayValue,
                    TemplateTypeFace,
                    TemplateFontSize).Width;
            }
        }

        public double TemplateTextBlockHeight {
            get {
                return MpHelpers.MeasureText(
                    TemplateDisplayValue,
                    TemplateTypeFace,
                    TemplateFontSize).Height;
            }
        }

        public double TemplateDeleteButtonSize {
            get {
                return TemplateTextBlockHeight - 2;
            }
        }
        #endregion

        #region Visibility Properties
        public Visibility DeleteTemplateTextButtonVisibility {
            get {
                if (TemplateTokenCollectionViewModel.PasteTemplateToolbarViewModel.ClipTileViewModel.IsEditingTile) {
                    return Visibility.Visible;
                }
                return Visibility.Collapsed;
            }
        }
        #endregion

        #region Brush Properties
        public Brush TemplateBorderBrush {
            get {     
                if(IsFocused) {
                    return Brushes.Red;
                }
                if (TemplateTokenCollectionViewModel.IsSelected) {
                    //if (string.IsNullOrEmpty(TemplateText)) {
                    //    return Brushes.Red;
                    //}
                    return Brushes.Green;
                }
                if (IsHovering) {
                    return Brushes.Yellow;
                }
                return TemplateBackgroundBrush;
            }
        }

        public Brush TemplateForegroundBrush {
            get {
                if (MpHelpers.IsBright(((SolidColorBrush)TemplateBackgroundBrush).Color)) {
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

        //private bool _wasEdited = false;
        //public bool WasEdited {
        //    get {
        //        return _wasEdited;
        //    }
        //    set {
        //        if (_wasEdited != value) {
        //            _wasEdited = value;
        //            OnPropertyChanged(nameof(WasEdited));
        //        }
        //    }
        //}

        //private bool _wasTypeViewed = false;
        //public bool WasTypeViewed {
        //    get {
        //        return _wasTypeViewed;
        //    }
        //    set {
        //        if (_wasTypeViewed != value) {
        //            _wasTypeViewed = value;
        //            OnPropertyChanged(nameof(WasTypeViewed));
        //            OnPropertyChanged(nameof(TemplateDisplayValue));
        //        }
        //    }
        //}

        //private bool _isPasteMode = false;
        //public bool IsPasteMode {
        //    get {
        //        return _isPasteMode;
        //    }
        //    set {
        //        if (_isPasteMode != value) {
        //            _isPasteMode = value;
        //            OnPropertyChanged(nameof(IsPasteMode));
        //            OnPropertyChanged(nameof(TemplateBorderBrush));
        //            OnPropertyChanged(nameof(TemplateBorderWidth));
        //            OnPropertyChanged(nameof(DeleteTemplateTextButtonVisibility));
        //            OnPropertyChanged(nameof(TemplateTextBlockVisibility));
        //            OnPropertyChanged(nameof(TemplateDisplayValue));
        //        }
        //    }
        //}

        //private bool _isEditMode = false;
        //public bool IsEditMode {
        //    get {
        //        return _isEditMode;
        //    }
        //    set {
        //        if (_isEditMode != value) {
        //            _isEditMode = value;
        //            OnPropertyChanged(nameof(IsEditMode));
        //            OnPropertyChanged(nameof(TemplateBorderBrush));
        //            OnPropertyChanged(nameof(TemplateBorderWidth));
        //            OnPropertyChanged(nameof(TemplateTextBlockVisibility));
        //            OnPropertyChanged(nameof(TemplateDeleteButtonSize));
        //            OnPropertyChanged(nameof(DeleteTemplateTextButtonVisibility));
        //            OnPropertyChanged(nameof(TemplateDisplayValue));
        //        }
        //    }
        //}
        #endregion

        #region Business Logic Properties
        public string TemplateDisplayValue {
            get {
                if (TemplateTokenCollectionViewModel.PasteTemplateToolbarViewModel.ClipTileViewModel.IsPastingTemplateTile && 
                    !string.IsNullOrEmpty(TemplateTokenCollectionViewModel.TemplateText)) {
                    return TemplateTokenCollectionViewModel.TemplateText;
                }
                return TemplateTokenCollectionViewModel.TemplateName;
            }
        }

        public bool IsFocused {
            get {
                return TemplateTokenCollectionViewModel.FocusedTemplateHyperlinkViewModel == this;
            }
            set {
                if(IsFocused != value) {
                    TemplateTokenCollectionViewModel.IsSelected = true;
                    TemplateTokenCollectionViewModel.FocusedTemplateHyperlinkViewModel = this;
                }
            }
        }
        #endregion

        #region Model Properties
        public string TemplateName {
            get {
                return TemplateTokenCollectionViewModel.TemplateName;
            }
        }

        public Brush TemplateBackgroundBrush {
            get {
                return TemplateTokenCollectionViewModel.TemplateBrush;
            }
        }        

        public TextPointer TemplateStart {
            get {
                return TemplateTokenCollectionViewModel.PasteTemplateToolbarViewModel.ClipTileViewModel.GetRtb().Document.ContentStart.GetPositionAtOffset(TemplateTextRange.StartIdx);
            }
            set {
                if(TemplateStart.CompareTo(value) != 0) {
                    TemplateTextRange.StartIdx = TemplateTokenCollectionViewModel.PasteTemplateToolbarViewModel.ClipTileViewModel.GetRtb().Document.ContentStart.GetOffsetToPosition(value);
                    OnPropertyChanged(nameof(TemplateStart));
                    OnPropertyChanged(nameof(TemplateRange));
                    OnPropertyChanged(nameof(TemplateFontSize));
                    OnPropertyChanged(nameof(TemplateTypeFace));
                    OnPropertyChanged(nameof(TemplateTextBlockWidth));
                    OnPropertyChanged(nameof(TemplateTextBlockHeight));
                    OnPropertyChanged(nameof(TemplateBorderWidth));
                    OnPropertyChanged(nameof(TemplateBorderHeight));
                }
            }
        }

        public TextPointer TemplateEnd {
            get {
                return TemplateTokenCollectionViewModel.PasteTemplateToolbarViewModel.ClipTileViewModel.GetRtb().Document.ContentStart.GetPositionAtOffset(TemplateTextRange.EndIdx);
            }
            set {
                if (TemplateEnd.CompareTo(value) != 0) {
                    TemplateTextRange.EndIdx = TemplateTokenCollectionViewModel.PasteTemplateToolbarViewModel.ClipTileViewModel.GetRtb().Document.ContentStart.GetOffsetToPosition(value);
                    OnPropertyChanged(nameof(TemplateEnd));
                    OnPropertyChanged(nameof(TemplateRange));
                    OnPropertyChanged(nameof(TemplateFontSize));
                    OnPropertyChanged(nameof(TemplateTypeFace));
                    OnPropertyChanged(nameof(TemplateTextBlockWidth));
                    OnPropertyChanged(nameof(TemplateTextBlockHeight));
                    OnPropertyChanged(nameof(TemplateBorderWidth));
                    OnPropertyChanged(nameof(TemplateBorderHeight));
                }
            }
        }

        public TextRange TemplateRange {
            get {
                return new TextRange(TemplateStart, TemplateEnd);
            }
            set {
                if(TemplateRange != value) {
                    TemplateStart = value.Start;
                    TemplateEnd = value.End;
                    OnPropertyChanged(nameof(TemplateRange));
                    OnPropertyChanged(nameof(TemplateStart));
                    OnPropertyChanged(nameof(TemplateEnd));
                }
            }
        }

        private MpTemplateTextRange _templateTextRange = new MpTemplateTextRange();
        public MpTemplateTextRange TemplateTextRange {
            get {
                return _templateTextRange;
            }
            set {
                if (_templateTextRange != value) {
                    _templateTextRange = value;
                    OnPropertyChanged(nameof(TemplateTextRange));
                    OnPropertyChanged(nameof(TemplateRange));
                }
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpTemplateHyperlinkViewModel() : this(new MpTemplateTokenCollectionViewModel(), new MpTemplateTextRange()) { }

        public MpTemplateHyperlinkViewModel(MpTemplateTokenCollectionViewModel ttcvm, MpTemplateTextRange ttr) : base() {
            TemplateTokenCollectionViewModel = ttcvm;
            TemplateTextRange = ttr;
        }
        
        public void TemplateHyperLink_Loaded(object sender, RoutedEventArgs args) {
            var uc = (UserControl)sender;
            var hl = (Hyperlink)uc.GetVisualAncestor<InlineUIContainer>().Parent;
            var b = (Border)uc.FindName("TemplateHyperlinkBorder");
            var tb = (TextBlock)uc.FindName("TemplateTextBlock");
            var dbImg = (Image)hl.FindName("DeleteTemplateTokenButton");
            var rtb = TemplateTokenCollectionViewModel.PasteTemplateToolbarViewModel.ClipTileViewModel.GetRtb();
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
                MpTemplateTokenEditModalWindowViewModel.ShowTemplateTokenEditModalWindow(TemplateTokenCollectionViewModel.PasteTemplateToolbarViewModel, TemplateTokenCollectionViewModel, true);
            };

            var editTemplateMenuItem = new MenuItem();
            editTemplateMenuItem.Header = "Edit";
            editTemplateMenuItem.PreviewMouseDown += (s4, e4) => {
                e4.Handled = true;
                _rtb.Selection.Select(hl.ElementStart, hl.ElementEnd);
                MpTemplateTokenEditModalWindowViewModel.ShowTemplateTokenEditModalWindow(TemplateTokenCollectionViewModel.PasteTemplateToolbarViewModel, TemplateTokenCollectionViewModel, true);
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
            //remove this individual token reference
            TemplateTokenCollectionViewModel.Remove(this);

            //checking clip's remaing templates, if it was the last of its type remove its dictionary keyvalue
            //if (ClipTileViewModel.TemplateTokens.Where(x => x.TemplateName == TemplateName).ToList().Count == 0) {
            //    ClipTileViewModel.TemplateTokenLookupDictionary.Remove(TemplateName);
            //}
        }
        #endregion
    }
}
