using GongSolutions.Wpf.DragDrop.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpClipTileRichTextBoxViewModel : MpUndoableViewModelBase<MpClipTileRichTextBoxViewModel>, ICloneable {
        #region Private Variables
        #endregion

        #region Properties

        #region ViewModels
        private MpClipTileViewModel _clipTileViewModel;
        public MpClipTileViewModel ClipTileViewModel {
            get {
                return _clipTileViewModel;
            }
            set {
                if (_clipTileViewModel != value) {
                    _clipTileViewModel = value;
                    OnPropertyChanged(nameof(ClipTileViewModel));
                    OnPropertyChanged(nameof(CopyItem));
                    OnPropertyChanged(nameof(CompositeParentCopyItemId));
                    OnPropertyChanged(nameof(CompositeSortOrderIdx));
                    OnPropertyChanged(nameof(Next));
                    OnPropertyChanged(nameof(Previous));
                    OnPropertyChanged(nameof(IsCompositeParent));
                }
            }
        }

        private MpClipTileViewModel _hostClipTileViewModel;
        public MpClipTileViewModel HostClipTileViewModel {
            get {
                return _hostClipTileViewModel;
            }
            set {
                if (_hostClipTileViewModel != value) {
                    _hostClipTileViewModel = value;
                    OnPropertyChanged(nameof(HostClipTileViewModel));
                    OnPropertyChanged(nameof(CopyItem));
                    OnPropertyChanged(nameof(CompositeParentCopyItemId));
                    OnPropertyChanged(nameof(CompositeSortOrderIdx));
                    OnPropertyChanged(nameof(Next));
                    OnPropertyChanged(nameof(Previous));
                    OnPropertyChanged(nameof(IsCompositeParent));
                }
            }
        }

        public MpClipTileRichTextBoxViewModelCollection RichTextBoxViewModelCollection {
            get {
                if(HostClipTileViewModel == null) {
                    return null;
                }
                return HostClipTileViewModel.RichTextBoxViewModelCollection;
            }
        }

        private MpTemplateHyperlinkCollectionViewModel _templateHyperlinkCollectionViewModel = new MpTemplateHyperlinkCollectionViewModel();
        public MpTemplateHyperlinkCollectionViewModel TemplateHyperlinkCollectionViewModel {
            get {
                return _templateHyperlinkCollectionViewModel;
            }
            set {
                if (_templateHyperlinkCollectionViewModel != value) {
                    _templateHyperlinkCollectionViewModel = value;
                    OnPropertyChanged(nameof(TemplateHyperlinkCollectionViewModel));
                }
            }
        }

        private MpClipTileRichTextBoxOverlayViewModel _clipTileRichTextBoxOverlayViewModel;
        public MpClipTileRichTextBoxOverlayViewModel ClipTileRichTextBoxOverlayViewModel {
            get {
                return _clipTileRichTextBoxOverlayViewModel;
            }
            set {
                if(_clipTileRichTextBoxOverlayViewModel != value) {
                    _clipTileRichTextBoxOverlayViewModel = value;
                    OnPropertyChanged(nameof(ClipTileRichTextBoxOverlayViewModel));
                }
            }
        }
        #endregion

        #region Controls 
        private RichTextBox _rtb;
        public RichTextBox Rtb {
            get {
                return _rtb;
            }
            set {
                if (_rtb != value) {
                    _rtb = value;
                    OnPropertyChanged(nameof(Rtb));
                }
            }
        }

        private Canvas _Rtbc;
        public Canvas Rtbc {
            get {
                return _Rtbc;
            }
            set {
                if (_Rtbc != value) {
                    _Rtbc = value;
                    OnPropertyChanged(nameof(Rtbc));
                }
            }
        }


        public AdornerLayer RtbcAdornerLayer { get; set; }
        #endregion

        #region Layout
        public double RtbListBoxItemHeight {
            get {
                if(HostClipTileViewModel == null) {
                    return 0;
                }
                if(!HostClipTileViewModel.IsExpanded && HostClipTileViewModel.RichTextBoxViewModelCollection.Count == 1) {
                    return HostClipTileViewModel.TileRtbHeight;
                }
                if(HostClipTileViewModel.IsExpanded) {
                    if(HostClipTileViewModel.RichTextBoxViewModelCollection.Count == 1) {
                        return Math.Max(CopyItem.ItemFlowDocument.GetDocumentSize().Height,HostClipTileViewModel.TileRtbHeight);
                    }
                    return CopyItem.ItemFlowDocument.GetDocumentSize().Height;
                }
                var doc = CopyItem.ItemFlowDocument;                
                doc.PageWidth = HostClipTileViewModel.TileContentWidth;
                double itemHeight = Math.Max(MpMeasurements.Instance.ClipTileSubItemMinHeight, doc.GetDocumentSize().Height);
                return itemHeight + RtbMargin.Top + RtbMargin.Bottom;
            }
        }

        public double DragButtonSize {
            get {
                return MpMeasurements.Instance.ClipTileSubItemMinHeight;
            }
        }

        public Thickness RtbMargin {
            get {
                if(ClipTileViewModel == null) {
                    return new Thickness(0);
                }                
                if (ClipTileViewModel.IsHovering && ClipTileViewModel.IsSubClipTile) {                    
                    return new Thickness(DragButtonSize + 5, ClipTileViewModel.TitleFontSize+5, 0, 0);
                } 
                return new Thickness(0);
            }
        }
        public double RtbRelativeWidthMax {
            get {
                if(CopyItem == null) {
                    return 0;
                }
                return CopyItem.ItemFlowDocument.GetDocumentSize().Width;
            }
        }
        #endregion

        #region Header & Footer 
        public MpClipTileRichTextBoxViewModel Next {
            get {
                if(HostClipTileViewModel == null || 
                   HostClipTileViewModel.RichTextBoxViewModelCollection == null || 
                   HostClipTileViewModel.RichTextBoxViewModelCollection.Count == 0) {
                    return null;
                }
                int nextIdx = CompositeSortOrderIdx + 1;
                if(nextIdx >= HostClipTileViewModel.RichTextBoxViewModelCollection.Count) {
                    return null;
                }
                return HostClipTileViewModel.RichTextBoxViewModelCollection[nextIdx];
            }
        }

        public MpClipTileRichTextBoxViewModel Previous {
            get {
                if (HostClipTileViewModel == null ||
                   HostClipTileViewModel.RichTextBoxViewModelCollection == null ||
                   HostClipTileViewModel.RichTextBoxViewModelCollection.Count == 0) {
                    return null;
                }
                int prevIdx = CompositeSortOrderIdx - 1;
                if (prevIdx < 0) {
                    return null;
                }
                return HostClipTileViewModel.RichTextBoxViewModelCollection[prevIdx];
            }
        }
        #endregion 

        #region Business Logic 
        public bool HasTemplate {
            get {
                return TemplateHyperlinkCollectionViewModel.Count > 0;
            }
        }
        public string TemplateRichText {
            get;
            set;
        }

        #endregion

        #region State

        public bool IsCompositeParent {
            get {
                if(HostClipTileViewModel == null) {
                    return false;
                }
                if(ClipTileViewModel == null) {
                    return true;
                }
                return false;
            }
        }
        #endregion

        #region Model
        public int CompositeParentCopyItemId {
            get {
                if(CopyItem == null) {
                    return 0;
                }
                return CopyItem.CompositeParentCopyItemId;
            }
            set {
                if(CopyItem != null && CopyItem.CompositeParentCopyItemId != value) {
                    CopyItem.CompositeParentCopyItemId = value;
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CompositeParentCopyItemId));
                    HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.CopyItem));
                }
            }
        }

        public int CompositeSortOrderIdx {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.CompositeSortOrderIdx;
            }
            set {
                if (CopyItem != null && CopyItem.CompositeSortOrderIdx != value) {
                    CopyItem.CompositeSortOrderIdx = value;
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CompositeSortOrderIdx));
                    HostClipTileViewModel.OnPropertyChanged(nameof(HostClipTileViewModel.CopyItem));
                }
            }
        }

        public int CopyItemId {
            get {
                if (CopyItem == null) {
                    return 0;
                }
                return CopyItem.CopyItemId;
            }
        }

        public string CopyItemPlainText {
            get {
                if (CopyItem == null || CopyItem.ItemPlainText == null) {
                    return string.Empty;
                }
                return CopyItem.ItemPlainText;
            }
        }

        public string CopyItemRichText {
            get {
                if (CopyItem == null) {
                    return string.Empty;
                }
                if (string.IsNullOrEmpty(CopyItem.ItemRichText)) {
                    return string.Empty.ToRichText();
                }
                return CopyItem.ItemRichText;
            }
            set {
                if (CopyItem != null && CopyItem.ItemRichText != value) {
                    //value should be raw rtf where templates are encoded into #name#color# groups
                    CopyItem.SetData(value);
                    CopyItem.WriteToDatabase();
                    OnPropertyChanged(nameof(CopyItemRichText));
                    OnPropertyChanged(nameof(CopyItem));
                }
            }
        }

        private string _copyItemFilePath = string.Empty;
        public string CopyItemFilePath {
            get {
                if (CopyItem == null || MainWindowViewModel == null || MainWindowViewModel.ClipTrayViewModel == null) {
                    return string.Empty;
                }
                if(string.IsNullOrEmpty(_copyItemFilePath)) {
                    _copyItemFilePath = CopyItem.GetFileList()[0];
                }
                return _copyItemFilePath;
            }
            set {
                if(_copyItemFilePath != value) {
                    _copyItemFilePath = value;
                    OnPropertyChanged(nameof(CopyItemFilePath));
                }
            }
        }

        public MpCopyItem CopyItem {
            get {
                if(HostClipTileViewModel == null) {
                    return null;
                }
                if(HostClipTileViewModel != null && ClipTileViewModel == null) {
                    return HostClipTileViewModel.CopyItem;
                }
                return ClipTileViewModel.CopyItem;
            }
            set {
                if(ClipTileViewModel == null && HostClipTileViewModel.CopyItem != value) {
                    ClipTileViewModel.SetCopyItem(value);
                    OnPropertyChanged(nameof(CopyItem));
                    OnPropertyChanged(nameof(CompositeParentCopyItemId));
                    OnPropertyChanged(nameof(CompositeSortOrderIdx));
                }
            }
        }
        #endregion

        #endregion

        #region Public Methods
        public MpClipTileRichTextBoxViewModel() : this(null,null) { }

        public MpClipTileRichTextBoxViewModel(MpClipTileViewModel ctvm, MpClipTileViewModel subctvm) : base() {
            ClipTileViewModel = subctvm;
            HostClipTileViewModel = ctvm;
            TemplateHyperlinkCollectionViewModel = new MpTemplateHyperlinkCollectionViewModel(HostClipTileViewModel, this);
            ClipTileRichTextBoxOverlayViewModel = new MpClipTileRichTextBoxOverlayViewModel(this);

            ClipTileViewModel.PropertyChanged += (s, e) => {
                switch (e.PropertyName) {
                    case nameof(ClipTileViewModel.IsHovering):
                        OnPropertyChanged(nameof(RtbMargin));
                        OnPropertyChanged(nameof(RtbListBoxItemHeight));
                        UpdateLayout();
                        break;
                    case nameof(ClipTileViewModel.IsSelected):
                        HostClipTileViewModel.RichTextBoxViewModelCollection.OnPropertyChanged(nameof(HostClipTileViewModel.RichTextBoxViewModelCollection.SelectedClipTileRichTextBoxViewModel));
                        HostClipTileViewModel.RichTextBoxViewModelCollection.OnPropertyChanged(nameof(HostClipTileViewModel.RichTextBoxViewModelCollection.SelectedRtb));
                        break;
                }
            };
        }

        public void ClipTileRichTextBoxListItemCanvas_Loaded(object sender, RoutedEventArgs e) {
            Rtbc = (Canvas)sender;

            Rtb = (RichTextBox)Rtbc.FindName("ClipTileRichTextBox");
            Rtb.Document = MpHelpers.Instance.ConvertRichTextToFlowDocument(CopyItemRichText);

            CreateHyperlinks();

            UpdateLayout();

            #region Drag & Drop
            #endregion

            Rtb.GotFocus += (s, e2) => {
                SetSelection(true);
            };

            if (HostClipTileViewModel.WasAddedAtRuntime) {
                //force new items to have left alignment
                Rtb.SelectAll();
                Rtb.Selection.ApplyPropertyValue(FlowDocument.TextAlignmentProperty, TextAlignment.Left);
                Rtb.CaretPosition = Rtb.Document.ContentStart;
            }

            if(CompositeSortOrderIdx <= 0) {
                SetSelection(true);
            }

            RtbcAdornerLayer = AdornerLayer.GetAdornerLayer(Rtbc);
            RtbcAdornerLayer.Add(new MpRichTextBoxOverlayAdorner(Rtbc));
        }

        public void UpdateLayout() {
            Rtb.Document.PageWidth = Rtb.Width - Rtb.Padding.Left - Rtb.Padding.Right - Rtb.Margin.Left - Rtb.Margin.Right;
            Rtb.Document.PageHeight = Rtb.Height - Rtb.Padding.Top - Rtb.Padding.Bottom - Rtb.Margin.Top - Rtb.Margin.Bottom;
            if (HostClipTileViewModel.IsEditingTile) {
                Rtb.Document.PageWidth -= (MpMeasurements.Instance.ClipTileEditModeContentMargin * 2) + 5;
            }
            OnPropertyChanged(nameof(RtbListBoxItemHeight));
        }

        public void SetSelection(bool newSelection) {
            if(ClipTileViewModel.IsSelected == newSelection) {
                return;
            }
            ClipTileViewModel.IsSelected = newSelection;
            if(ClipTileViewModel.IsSelected) {
                HostClipTileViewModel.RichTextBoxViewModelCollection.SelectRichTextBoxViewModel(HostClipTileViewModel.RichTextBoxViewModelCollection.IndexOf(this));
                HostClipTileViewModel.EditRichTextBoxToolbarViewModel.InitWithRichTextBox(Rtb);
                HostClipTileViewModel.EditTemplateToolbarViewModel.InitWithRichTextBox(Rtb);
                HostClipTileViewModel.PasteTemplateToolbarViewModel.InitWithRichTextBox(Rtb);
            }
        }

        public async Task<string> GetPastableRichText() {
            if (HasTemplate) {
                TemplateRichText = string.Empty;
                HostClipTileViewModel.RichTextBoxViewModelCollection.SelectRichTextBoxViewModel(HostClipTileViewModel.RichTextBoxViewModelCollection.IndexOf(this));
                HostClipTileViewModel.PasteTemplateToolbarViewModel.InitWithRichTextBox(Rtb);

                await Task.Run(() => {
                    while (string.IsNullOrEmpty(TemplateRichText)) {
                        System.Threading.Thread.Sleep(500);
                    }
                    //TemplateRichText is set in PasteTemplateCommand
                });

                return TemplateRichText;
            }
            return CopyItemRichText;

            //both return to ClipTray.GetDataObjectFromSelectedClips
        }

        public void ClearHyperlinks() {
            var rtbSelection = Rtb.Selection;
            var hlList = GetHyperlinkList();
            foreach (var hl in hlList) {
                string linkText = string.Empty;
                if (hl.DataContext == null || hl.DataContext is MpClipTileRichTextBoxViewModel) {
                    linkText = new TextRange(hl.ElementStart, hl.ElementEnd).Text;
                } else {
                    var thlvm = (MpTemplateHyperlinkViewModel)hl.DataContext;
                    linkText = thlvm.TemplateName;
                }
                hl.Inlines.Clear();
                new Span(new Run(linkText), hl.ContentStart);
            }
            TemplateHyperlinkCollectionViewModel.Clear();
            if (rtbSelection != null) {
                Rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
            }
        }

        public void CreateHyperlinks() {
            var regExGroupList = new List<string> {
                //WebLink
                @"(?:https?://|www\.)\S+", 
                //Email
                @"([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})",
                //PhoneNumber
                @"(\+?\d{1,3}?[ -.]?)?\(?(\d{3})\)?[ -.]?(\d{3})[ -.]?(\d{4})",
                //Currency
                @"[$|£|€|¥][\d|\.]([0-9]{0,3},([0-9]{3},)*[0-9]{3}|[0-9]+)?(\.\d{0,2})?",
                //HexColor (no alpha)
                @"#([0-9]|[a-fA-F]){6}",
                //StreetAddress
                @"\d+[ ](?:[A-Za-z0-9.-]+[ ]?)+(?:Avenue|Lane|Road|Boulevard|Drive|Street|Ave|Dr|Rd|Blvd|Ln|St)\.?,\s(?:[A-Z][a-z.-]+[ ]?)+ \b\d{5}(?:-\d{4})?\b",                
                //Text Template (dynamically matching from CopyItemTemplate.TemplateName)
                CopyItem.TemplateRegExMatchString,                
                //HexColor (with alpha)
                @"#([0-9]|[a-fA-F]){8}",
            };
            //var docPlainText = new TextRange(Rtb.Document.ContentStart, Rtb.Document.ContentEnd).Text;

            var rtbSelection = Rtb.Selection.Clone();
            for (int i = 0; i < regExGroupList.Count; i++) {
                var linkType = i + 1 > (int)MpSubTextTokenType.TemplateSegment ? MpSubTextTokenType.HexColor : (MpSubTextTokenType)(i + 1);                
                if (linkType == MpSubTextTokenType.StreetAddress) {
                    //doesn't consistently work and presents bugs so disabling for now
                    continue;
                }
                var lastRangeEnd = Rtb.Document.ContentStart;
                var regExStr = regExGroupList[i];
                if (string.IsNullOrEmpty(regExStr)) {
                    //this occurs for templates when copyitem has no templates
                    continue;
                }
                if(linkType == MpSubTextTokenType.HexColor) {
                    linkType = MpSubTextTokenType.HexColor;
                }
                var mc = Regex.Matches(CopyItem.ItemPlainText, regExStr, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline);
                foreach (Match m in mc) {
                    foreach (Group mg in m.Groups) {
                        foreach (Capture c in mg.Captures) {
                            Hyperlink hl = null;
                            var matchRange = MpHelpers.Instance.FindStringRangeFromPosition(lastRangeEnd, c.Value, true);
                            if (matchRange == null) {
                                continue;
                            }
                            lastRangeEnd = matchRange.End;
                            if (linkType == MpSubTextTokenType.TemplateSegment) {
                                var copyItemTemplate = CopyItem.GetTemplateByName(matchRange.Text);
                                hl = MpTemplateHyperlinkViewModel.CreateTemplateHyperlink(HostClipTileViewModel, copyItemTemplate, matchRange);
                                TemplateHyperlinkCollectionViewModel.Add((MpTemplateHyperlinkViewModel)hl.DataContext);
                            } else {
                                var matchRun = new Run(matchRange.Text);
                                matchRange.Text = "";
                                // DO NOT REMOVE this extra link ensures selection is retained!
                                var hlink = new Hyperlink(matchRun, matchRange.Start);
                                hl = new Hyperlink(matchRange.Start, matchRange.End);
                                var linkText = c.Value;
                                hl.Tag = linkType;
                                MpHelpers.Instance.CreateBinding(HostClipTileViewModel, new PropertyPath(nameof(HostClipTileViewModel.IsSelected)), hl, Hyperlink.IsEnabledProperty);
                                hl.MouseEnter += (s3, e3) => {
                                    hl.Cursor = HostClipTileViewModel.IsSelected ? Cursors.Hand : Cursors.Arrow;
                                };
                                hl.MouseLeave += (s3, e3) => {
                                    hl.Cursor = Cursors.Arrow;
                                };
                                hl.MouseLeftButtonDown += (s4, e4) => {
                                    if (hl.NavigateUri != null && HostClipTileViewModel.IsSelected) {
                                        MpHelpers.Instance.OpenUrl(hl.NavigateUri.ToString());
                                    }
                                };

                                var convertToQrCodeMenuItem = new MenuItem();
                                convertToQrCodeMenuItem.Header = "Convert to QR Code";
                                convertToQrCodeMenuItem.Click += async (s5, e1) => {
                                    var hyperLink = (Hyperlink)((MenuItem)s5).Tag;
                                    var bmpSrc = MpHelpers.Instance.ConvertUrlToQrCode(hyperLink.NavigateUri.ToString());
                                    Clipboard.SetImage(bmpSrc);
                                };
                                convertToQrCodeMenuItem.Tag = hl;
                                hl.ContextMenu = new ContextMenu();
                                hl.ContextMenu.Items.Add(convertToQrCodeMenuItem);

                                switch ((MpSubTextTokenType)hl.Tag) {
                                    case MpSubTextTokenType.StreetAddress:
                                        hl.NavigateUri = new Uri("https://google.com/maps/place/" + linkText.Replace(' ', '+'));
                                        break;
                                    case MpSubTextTokenType.Uri:
                                        if (!linkText.Contains("https://")) {
                                            hl.NavigateUri = new Uri("https://" + linkText);
                                        } else {
                                            hl.NavigateUri = new Uri(linkText);
                                        }
                                        MenuItem minifyUrl = new MenuItem();
                                        minifyUrl.Header = "Minify with bit.ly";
                                        minifyUrl.Click += async (s1, e2) => {
                                            Hyperlink link = (Hyperlink)((MenuItem)s1).Tag;
                                            string minifiedLink = await MpMinifyUrl.Instance.ShortenUrl(link.NavigateUri.ToString());
                                            if (!string.IsNullOrEmpty(minifiedLink)) {
                                                matchRange.Text = minifiedLink;
                                                ClearHyperlinks();
                                                CreateHyperlinks();
                                            }
                                            //Clipboard.SetText(minifiedLink);
                                        };
                                        minifyUrl.Tag = hl;
                                        hl.ContextMenu.Items.Add(minifyUrl);
                                        break;
                                    case MpSubTextTokenType.Email:
                                        hl.NavigateUri = new Uri("mailto:" + linkText);
                                        break;
                                    case MpSubTextTokenType.PhoneNumber:
                                        hl.NavigateUri = new Uri("tel:" + linkText);
                                        break;
                                    case MpSubTextTokenType.Currency:
                                        //"https://www.google.com/search?q=%24500.80+to+yen"
                                        MenuItem convertCurrencyMenuItem = new MenuItem();
                                        convertCurrencyMenuItem.Header = "Convert Currency To";
                                        var fromCurrencyType = MpHelpers.Instance.GetCurrencyTypeFromString(linkText);
                                        foreach (MpCurrency currency in MpCurrencyConverter.Instance.CurrencyList) {
                                            if (currency.Id == Enum.GetName(typeof(CurrencyType), fromCurrencyType)) {
                                                continue;
                                            }
                                            MenuItem subItem = new MenuItem();
                                            subItem.Header = currency.CurrencyName + "(" + currency.CurrencySymbol + ")";
                                            subItem.Click += async (s2, e2) => {
                                                Enum.TryParse(currency.Id, out CurrencyType toCurrencyType);
                                                var convertedValue = await MpCurrencyConverter.Instance.ConvertAsync(
                                                    MpHelpers.Instance.GetCurrencyValueFromString(linkText),
                                                    fromCurrencyType,
                                                    toCurrencyType);
                                                convertedValue = Math.Round(convertedValue, 2);
                                                if (Rtb.Tag != null && ((List<Hyperlink>)Rtb.Tag).Contains(hl)) {
                                                    ((List<Hyperlink>)Rtb.Tag).Remove(hl);
                                                }
                                                Run run = new Run(currency.CurrencySymbol + convertedValue);
                                                hl.Inlines.Clear();
                                                hl.Inlines.Add(run);
                                            };

                                            convertCurrencyMenuItem.Items.Add(subItem);
                                        }

                                        hl.ContextMenu.Items.Add(convertCurrencyMenuItem);
                                        break;
                                    case MpSubTextTokenType.HexColor:
                                        var rgbColorStr = linkText;
                                        if (rgbColorStr.Length > 7) {
                                            rgbColorStr = rgbColorStr.Substring(0, 7);
                                        }
                                        hl.NavigateUri = new Uri(@"https://www.hexcolortool.com/" + rgbColorStr);

                                        MenuItem changeColorItem = new MenuItem();
                                        changeColorItem.Header = "Change Color";
                                        changeColorItem.Click += (s, e) => {
                                            var result = MpHelpers.Instance.ShowColorDialog((Brush)new BrushConverter().ConvertFrom(linkText));
                                        };
                                        hl.ContextMenu.Items.Add(changeColorItem);
                                        break;
                                    default:
                                        Console.WriteLine("Unhandled token type: " + Enum.GetName(typeof(MpSubTextTokenType), (MpSubTextTokenType)hl.Tag) + " with value: " + linkText);
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            if (rtbSelection != null) {
                Rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
            }
        }
        #endregion

        #region Private Methods
        private List<Hyperlink> GetHyperlinkList() {
            var rtbSelection = Rtb.Selection;
            var hlList = new List<Hyperlink>();
            for (TextPointer position = Rtb.Document.ContentStart;
                position != null && position.CompareTo(Rtb.Document.ContentEnd) <= 0;
                position = position.GetNextContextPosition(LogicalDirection.Forward)) {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd) {
                    var hl = MpHelpers.Instance.FindParentOfType(position.Parent, typeof(Hyperlink)) as Hyperlink;
                    if (hl != null && !hlList.Contains(hl)) {
                        hlList.Add(hl);
                    }
                }
            }
            if (rtbSelection != null) {
                Rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
            }
            return hlList;
        }

        public object Clone() {
            var nrtbvm = new MpClipTileRichTextBoxViewModel(HostClipTileViewModel, new MpClipTileViewModel(CopyItem));
            nrtbvm.Rtb = new RichTextBox();
            nrtbvm.Rtb.Document = Rtb.Document.Clone();
            return nrtbvm;
        }
        #endregion
    }

}
