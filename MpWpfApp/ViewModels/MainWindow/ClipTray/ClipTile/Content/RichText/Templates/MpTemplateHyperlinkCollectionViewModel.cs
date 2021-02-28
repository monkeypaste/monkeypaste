using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpTemplateHyperlinkCollectionViewModel : MpUndoableObservableCollectionViewModel<MpTemplateHyperlinkCollectionViewModel,MpTemplateHyperlinkViewModel>, ICloneable {
        #region Private Variables
        #endregion

        #region Properties

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
                    OnPropertyChanged(nameof(SelectedTemplateHyperlinkViewModel));
                }
            }
        }

        public MpObservableCollection<MpTemplateHyperlinkViewModel> UniqueTemplateHyperlinkViewModelListByDocOrder {
            get {
                var ul = new MpObservableCollection<MpTemplateHyperlinkViewModel>();
                var docOrderList = this.ToList();
                docOrderList.Sort(CompareTemplatesByDocOrder);
                foreach (var thlvm in docOrderList) {
                    bool itExists = false;
                    foreach (var unm in ul) {
                        if (unm.TemplateName == thlvm.TemplateName) {
                            itExists = true;
                        }
                    }
                    if (!itExists) {
                        ul.Add(thlvm);
                    }
                }
                return ul;
            }
        }

        public MpTemplateHyperlinkViewModel SelectedTemplateHyperlinkViewModel {
            get {
                foreach (var ttcvm in this) {
                    if (ttcvm.IsSelected) {
                        return ttcvm;
                    }
                }
                //if none selected but exist select first one
                if (this.Count > 0) {
                    this[0].IsSelected = true;
                    return SelectedTemplateHyperlinkViewModel;
                }
                return null;
            }
            set {
                if (SelectedTemplateHyperlinkViewModel != value && this.Contains(value)) {
                    foreach (var ttcvm in this) {
                        if (ttcvm != value) {
                            //clear any other selections
                            ttcvm.IsSelected = false;
                        } else {
                            ttcvm.IsSelected = true;
                        }
                    }
                    OnPropertyChanged(nameof(SelectedTemplateHyperlinkViewModel));
                }
            }
        }
        #endregion        
        #endregion

        #region Public Methods
        public MpTemplateHyperlinkCollectionViewModel() : base() { }

        public MpTemplateHyperlinkCollectionViewModel(MpClipTileViewModel parent) :base() {
            CollectionChanged += (s, e) => {
                OnPropertyChanged(nameof(UniqueTemplateHyperlinkViewModelListByDocOrder));
            };

            ClipTileViewModel = parent;
            //ClipTileRichTextBoxViewModel = rtbvm;

            ClipTileViewModel.PropertyChanged += (s, e) => {
                switch(e.PropertyName) {
                    case nameof(ClipTileViewModel.IsPastingTemplateTile):
                        Reset();
                        break;
                }
            };
            //templates are added in the CreateHyperlinks rtb extension
        }
        public void Reset() {
            foreach (var thlvm in this) {
                thlvm.SetTemplateText(string.Empty);
            }
            if(this.Count > 0) {
                this[0].IsSelected = true;
            }
        }
        public void SelectTemplate(string templateName) {
            foreach(var thlvm in this) {
                thlvm.IsSelected = thlvm.TemplateName == templateName;
            }
        }

        public void SetTemplateText(string templateName, string templateText) {
            foreach(var thlvm in this) {
                if(thlvm.TemplateName == templateName) {
                    thlvm.SetTemplateText(templateText);
                    thlvm.IsSelected = true;
                } else {
                    thlvm.IsSelected = false;
                }
            }
        }              

        public void ClearHyperlinks() {
            var rtb = ClipTileViewModel.Rtb;
            var rtbSelection = rtb.Selection;
            var hlList = GetHyperlinkList();
            foreach (var hl in hlList) {
                string linkText = string.Empty;
                if (hl.DataContext == null || hl.DataContext is MpClipTileViewModel) {
                    linkText = new TextRange(hl.ElementStart, hl.ElementEnd).Text;
                } else {
                    var thlvm = (MpTemplateHyperlinkViewModel)hl.DataContext;
                    linkText = thlvm.TemplateName;
                }
                hl.Inlines.Clear();
                new Span(new Run(linkText), hl.ContentStart);
            }
            this.Clear();
            if (rtbSelection != null) {
                rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
            }
        }

        public void CreateHyperlinks() {
            var rtb = ClipTileViewModel.Rtb;
            var copyItem = ClipTileViewModel.CopyItem;
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
                copyItem.TemplateRegExMatchString,                
                //HexColor (with alpha)
                @"#([0-9]|[a-fA-F]){8}",
            };
            //var docPlainText = new TextRange(Rtb.Document.ContentStart, Rtb.Document.ContentEnd).Text;

            var rtbSelection = rtb.Selection.Clone();
            for (int i = 0; i < regExGroupList.Count; i++) {
                var linkType = i + 1 > (int)MpSubTextTokenType.TemplateSegment ? MpSubTextTokenType.HexColor : (MpSubTextTokenType)(i + 1);
                if (linkType == MpSubTextTokenType.StreetAddress) {
                    //doesn't consistently work and presents bugs so disabling for now
                    continue;
                }
                var lastRangeEnd = rtb.Document.ContentStart;
                var regExStr = regExGroupList[i];
                if (string.IsNullOrEmpty(regExStr)) {
                    //this occurs for templates when copyitem has no templates
                    continue;
                }
                if (linkType == MpSubTextTokenType.HexColor) {
                    linkType = MpSubTextTokenType.HexColor;
                }
                var mc = Regex.Matches(copyItem.ItemPlainText, regExStr, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Multiline);
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
                                var copyItemTemplate = copyItem.GetTemplateByName(matchRange.Text);
                                hl = MpTemplateHyperlinkViewModel.CreateTemplateHyperlink(ClipTileViewModel, copyItemTemplate, matchRange);
                                this.Add((MpTemplateHyperlinkViewModel)hl.DataContext);
                            } else {
                                var matchRun = new Run(matchRange.Text);
                                matchRange.Text = "";
                                // DO NOT REMOVE this extra link ensures selection is retained!
                                var hlink = new Hyperlink(matchRun, matchRange.Start);
                                hl = new Hyperlink(matchRange.Start, matchRange.End);
                                var linkText = c.Value;
                                hl.Tag = linkType;
                                MpHelpers.Instance.CreateBinding(ClipTileViewModel, new PropertyPath(nameof(ClipTileViewModel.IsSelected)), hl, Hyperlink.IsEnabledProperty);
                                hl.MouseEnter += (s3, e3) => {
                                    hl.Cursor = ClipTileViewModel.IsSelected ? Cursors.Hand : Cursors.Arrow;
                                };
                                hl.MouseLeave += (s3, e3) => {
                                    hl.Cursor = Cursors.Arrow;
                                };
                                hl.MouseLeftButtonDown += (s4, e4) => {
                                    if (hl.NavigateUri != null && ClipTileViewModel.IsSelected) {
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
                                                if (rtb.Tag != null && ((List<Hyperlink>)rtb.Tag).Contains(hl)) {
                                                    ((List<Hyperlink>)rtb.Tag).Remove(hl);
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
                rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
            }
        }
        #endregion

        #region Private Methods
        private List<Hyperlink> GetHyperlinkList() {
            var rtb = ClipTileViewModel.Rtb;
            var rtbSelection = rtb.Selection;
            var hlList = new List<Hyperlink>();
            for (TextPointer position = rtb.Document.ContentStart;
                position != null && position.CompareTo(rtb.Document.ContentEnd) <= 0;
                position = position.GetNextContextPosition(LogicalDirection.Forward)) {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.ElementEnd) {
                    var hl = MpHelpers.Instance.FindParentOfType(position.Parent, typeof(Hyperlink)) as Hyperlink;
                    if (hl != null && !hlList.Contains(hl)) {
                        hlList.Add(hl);
                    }
                }
            }
            if (rtbSelection != null) {
                rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
            }
            return hlList;
        }

        private int CompareTemplatesByDocOrder(MpTemplateHyperlinkViewModel a, MpTemplateHyperlinkViewModel b) {
            if (a == null) {
                if (b == null) {
                    return 0;
                } else {
                    return -1;
                }
            } else {
                if (b == null) {
                    return 1;
                } else {
                    return a.TemplateHyperlink.ElementStart.CompareTo(b.TemplateHyperlink.ElementStart);
                }
            }
        }
        #endregion

        #region Commands

        #endregion

        #region Overrides
        public new void Add(MpTemplateHyperlinkViewModel thlvm) {
            base.Add(thlvm);
            thlvm.CopyItemTemplate.WriteToDatabase();
        }

        public object Clone() {
            var nthlcvm = new MpTemplateHyperlinkCollectionViewModel(ClipTileViewModel);
            foreach(var thlvm in this) {
                nthlcvm.Add((MpTemplateHyperlinkViewModel)thlvm.Clone());
            }
            return nthlcvm;
        }
        #endregion
    }
}
