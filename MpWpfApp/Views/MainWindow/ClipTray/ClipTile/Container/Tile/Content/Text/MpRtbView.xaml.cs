using GongSolutions.Wpf.DragDrop.Utilities;
using MonkeyPaste;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for Mpxaml
    /// </summary>
    public partial class MpRtbView : UserControl {
        public TextRange NewStartRange;
        public string NewOriginalText;
        public Hyperlink LastEditedHyperlink;

        public List<MpTemplateHyperlink> TemplateViews = new List<MpTemplateHyperlink>();
        //public bool IsClearing = false;

        //public Dictionary<int, List<Hyperlink>> TemplateLookUp = new Dictionary<int, List<Hyperlink>>();

        public MpRtbView() {
            InitializeComponent();
            Rtb.SpellCheck.IsEnabled = MonkeyPaste.MpPreferences.Instance.UseSpellCheck;
        }      

        public void SyncModels() {
            MpHelpers.Instance.RunOnMainThreadAsync(async () => {
                var rtbvm = DataContext as MpContentItemViewModel;


                //clear any search highlighting when saving the document then restore after save
                //rtbvm.Parent.HighlightTextRangeViewModelCollection.HideHighlightingCommand.Execute(rtbvm);

                //rtbvm.Parent.HighlightTextRangeViewModelCollection.UpdateInDocumentsBgColorList(Rtb);

                //if(!MpMainWindowViewModel.IsMainWindowLoading) 

                await MpHelpers.Instance.RunOnMainThreadAsync(ClearHyperlinks);

                rtbvm.CopyItemData = Rtb.Document.ToRichText();

                rtbvm.CopyItem.WriteToDatabase();

                await CreateHyperlinks();

                MpHelpers.Instance.RunOnMainThread(UpdateLayout);
                //rtbvm.Parent.HighlightTextRangeViewModelCollection.ApplyHighlightingCommand.Execute(rtbvm);

                var scvml = MpShortcutCollectionViewModel.Instance.Shortcuts.Where(x => x.CopyItemId == rtbvm.CopyItem.Id).ToList();
                if (scvml.Count > 0) {
                    rtbvm.ShortcutKeyString = scvml[0].KeyString;
                }

                rtbvm.IsChanged = false;
            });
        }


        private void Rtb_Loaded(object sender, RoutedEventArgs e) {
            if (DataContext != null && DataContext is MpContentItemViewModel rtbivm) {
                if(rtbivm.Parent.IsPlaceholder) {
                    return;
                }
                rtbivm.OnUiResetRequest += Rtbivm_OnRtbResetRequest;
                rtbivm.OnScrollWheelRequest += Rtbivm_OnScrollWheelRequest;
                rtbivm.OnUiUpdateRequest += Rtbivm_OnUiUpdateRequest;
                //rtbivm.OnClearTemplatesRequest += Rtbivm_OnClearHyperlinksRequest;
                //rtbivm.OnCreateTemplatesRequest += Rtbivm_OnCreateHyperlinksRequest;
                rtbivm.OnSyncModels += Rtbivm_OnSyncModels;

                if (rtbivm.IsNewAndFirstLoad) {
                    //force new items to have left alignment
                    Rtb.CaretPosition = Rtb.Document.ContentStart;
                    Rtb.Document.TextAlignment = TextAlignment.Left;
                    rtbivm.IsNewAndFirstLoad = false;
                }
                SyncModels();
            }
        }
        private void Rtbivm_OnSyncModels(object sender, EventArgs e) {
            SyncModels();
        }

        //private void Rtbivm_OnCreateHyperlinksRequest(object sender, EventArgs e) {
        //    CreateHyperlinks();
        //}

        //private void Rtbivm_OnClearHyperlinksRequest(object sender, EventArgs e) {
        //    ClearHyperlinks();
        //}

        private void Rtbivm_OnUiUpdateRequest(object sender, EventArgs e) {
            Rtb.UpdateLayout();
        }

        private void Rtbivm_OnScrollWheelRequest(object sender, int e) {
            Rtb.ScrollToVerticalOffset(Rtb.VerticalOffset + e);
        }

        private void Rtbivm_OnRtbResetRequest(object sender, bool focusRtb) {
            Rtb.ScrollToHome();
            Rtb.CaretPosition = Rtb.Document.ContentStart;
            if(focusRtb) {
                Rtb.Focus();
            }
        }

        private void Rtb_SelectionChanged(object sender, RoutedEventArgs e) {
            var rtbvm = DataContext as MpContentItemViewModel;
            if (rtbvm.IsEditingContent && Rtb.IsFocused) {
                var thl = Rtb.Selection.Start.Parent.FindParentOfType<MpTemplateHyperlink>();
                bool canAddTemplate = true;
                if (thl != null) {
                    canAddTemplate = false;
                }
                var plv = this.GetVisualAncestor<MpContentListView>();
                if (plv != null) {
                    var et = plv.GetVisualDescendent<MpRtbEditToolbarView>();
                    if (et != null) {
                        et.AddTemplateButton.IsEnabled = canAddTemplate;
                    }
                }
            }
        }

        private void Rtb_TextChanged(object sender, TextChangedEventArgs e) {
            if(e.Changes.Count > 0) {
                var rtbvm = DataContext as MpContentItemViewModel;
                rtbvm.HasViewChanged = true;
                rtbvm.OnPropertyChanged(nameof(rtbvm.CurrentSize));
            }
        }

        private void Rtb_GotFocus(object sender, RoutedEventArgs e) {
            var plv = this.GetVisualAncestor<MpContentListView>();
            if (plv != null) {
                var et = plv.GetVisualDescendent<MpRtbEditToolbarView>();
                var ettb = plv.GetVisualDescendent<MpEditTemplateToolbarView>();
                var pttb = plv.GetVisualDescendent<MpPasteTemplateToolbarView>();
                if (et != null) {
                    et.SetActiveRtb(Rtb);
                }
                if (ettb != null) {
                    ettb.SetActiveRtb(Rtb);
                }
                if (pttb != null) {
                    pttb.SetActiveRtb(Rtb);
                }
            }
        }


        private void Rtb_PreviewKeyUp(object sender, KeyEventArgs e) {
            var civm = DataContext as MpContentItemViewModel;
            if (e.Key == Key.Space && civm.IsEditingContent) {
                Task.Run(async () => {
                    // TODO Update regex hyperlink matches (but ignore current ones??)
                    await ClearHyperlinks();
                    await CreateHyperlinks();
                });
            } 
        }

        #region Template/Hyperlinks

        public async Task<List<Hyperlink>> GetAllHyperlinksFromDoc() {
            var hlList = new List<Hyperlink>();
            if (Rtb == null) {
                return hlList;
            }
            await MpHelpers.Instance.RunOnMainThreadAsync(() => {
                var rtbSelection = Rtb?.Selection;

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

                //foreach (var thl in TemplateLookUp) {
                //    hlList.AddRange(thl.Value);
                //}
                if (rtbSelection != null) {
                    Rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
                }
            });
            return hlList;
        }

        public async Task ClearHyperlinks() {
            await MpHelpers.Instance.RunOnMainThreadAsync(async () => {
                //var rtbSelection = Rtb?.Selection;
                var hlList = await GetAllHyperlinksFromDoc();
                foreach (var hl in hlList) {
                    string linkText;
                    if (hl.DataContext == null || hl.DataContext is MpContentItemViewModel) {
                        linkText = new TextRange(hl.ElementStart, hl.ElementEnd).Text;
                    } else {
                        var thlvm = (MpTemplateViewModel)hl.DataContext;
                        linkText = thlvm.TemplateName;
                    }
                    hl.Inlines.Clear();
                    new Span(new Run(linkText), hl.ElementStart);
                }
                foreach (var hl in TemplateViews) {
                    hl.Tag = null;
                    var thlvm = hl.DataContext as MpTemplateViewModel;
                    hl.Inlines.Clear();
                    new Span(new Run(thlvm.TemplateName), hl.ElementStart);
                }
                TemplateViews.Clear();
                var rtbvm = Rtb.DataContext as MpContentItemViewModel;
                rtbvm.TemplateCollection.Templates.Clear();
                //if (rtbSelection != null) {
                //    Rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
                //}
            });
        }

        public async Task<List<MpCopyItemTemplate>> GetTemplatesFromDbAsync() {
            var rtbvm = DataContext as MpContentItemViewModel;
            var tl = new List<MpCopyItemTemplate>();
            if (rtbvm.CopyItem == null) {
                return tl;
            }
            var result = await MpDb.Instance.GetItemsAsync<MpCopyItemTemplate>();
            return result.Where(x => x.CopyItemId == rtbvm.CopyItem.Id).ToList();
        }

        public List<MpCopyItemTemplate> GetTemplatesFromDb() {
            var rtbvm = DataContext as MpContentItemViewModel;
            var tl = new List<MpCopyItemTemplate>();
            if (rtbvm.CopyItem == null) {
                return tl;
            }
            return MpDb.Instance.GetItems<MpCopyItemTemplate>()
                                .Where(x => x.CopyItemId == rtbvm.CopyItem.Id)
                                .ToList();
        }

        public string GetTemplateRegExMatchString() {
            var outStr = string.Empty;
            foreach (var t in GetTemplatesFromDb()) {
                if (outStr.Contains(t.TemplateName)) {
                    continue;
                }
                outStr += t.TemplateName + "|";
            }
            if (!string.IsNullOrEmpty(outStr)) {
                return outStr.Remove(outStr.Length - 1, 1);
            }
            return outStr;
        }

        public async Task<string> GetTemplateRegExMatchStringAsync() {
            var templates = await GetTemplatesFromDbAsync();
            var outStr = string.Empty;
            foreach (var t in templates) {
                if (outStr.Contains(t.TemplateName)) {
                    continue;
                }
                outStr += t.TemplateName + "|";
            }
            if (!string.IsNullOrEmpty(outStr)) {
                return outStr.Remove(outStr.Length - 1, 1);
            }
            return outStr;
        }

        public async Task CreateHyperlinks() {
            var rtbvm = Rtb.DataContext as MpContentItemViewModel;
            var fd = Rtb.Document;

            if (Rtb == null) {
                return;
            }
            string templateRegEx = await GetTemplateRegExMatchStringAsync();
            var templates = await GetTemplatesFromDbAsync();
            var rtbSelection = Rtb?.Selection.Clone();
            await MpHelpers.Instance.RunOnMainThreadAsync(() => {
                string pt = rtbvm.CopyItem.ItemData.ToPlainText();
                for (int i = 1; i < MpRegEx.Instance.RegExList.Count; i++) {
                    var linkType = (MpSubTextTokenType)i;
                    if (linkType == MpSubTextTokenType.StreetAddress) {
                        //doesn't consistently work and presents bugs so disabling for now
                        continue;
                    }
                    var lastRangeEnd = fd.ContentStart;
                    var regExStr = MpRegEx.Instance.GetRegExForTokenType(linkType);
                    if (linkType == MpSubTextTokenType.TemplateSegment) {
                        regExStr = templateRegEx;
                    } else {
                        continue;
                    }
                    if (string.IsNullOrEmpty(regExStr)) {
                        //this occurs for templates when copyitem has no templates
                        continue;
                    }
                    var mc = Regex.Matches(pt, regExStr, RegexOptions.ExplicitCapture | RegexOptions.Multiline);
                    foreach (Match m in mc) {
                        foreach (Group mg in m.Groups) {
                            foreach (Capture c in mg.Captures) {
                                Hyperlink hl = null;
                                var matchRange = MpHelpers.Instance.FindStringRangeFromPosition(lastRangeEnd, c.Value, true);
                                if (matchRange == null || string.IsNullOrEmpty(matchRange.Text)) {
                                    continue;
                                }
                                lastRangeEnd = matchRange.End;
                                if (linkType == MpSubTextTokenType.TemplateSegment) {
                                    var copyItemTemplate = GetTemplatesFromDb().Where(x => x.TemplateName == matchRange.Text).FirstOrDefault(); //TemplateHyperlinkCollectionViewModel.Where(x => x.TemplateName == matchRange.Text).FirstOrDefault().CopyItemTemplate;
                                                                                                                                                //matchRange.Text = string.Empty;
                                                                                                                                                //var thlvm = rtbvm.TemplateCollection.AddItem(copyItemTemplate);                                                                                                      //CopyItem.GetTemplateByName(matchRange.Text);
                                    hl = MpTemplateHyperlink.Create(matchRange, copyItemTemplate);
                                } else {
                                    var matchRun = new Run(matchRange.Text);
                                    matchRange.Text = "";
                                    // DO NOT REMOVE this extra link ensures selection is retained!
                                    var hlink = new Hyperlink(matchRun, matchRange.Start);
                                    hl = new Hyperlink(matchRange.Start, matchRange.End);
                                    hl = hlink;
                                    var linkText = c.Value;
                                    hl.Tag = linkType;
                                    //if (linkText == @"DragAction.Cancel") {
                                    //    linkText = linkText;
                                    //}
                                    MpHelpers.Instance.CreateBinding(rtbvm, new PropertyPath(nameof(rtbvm.IsSelected)), hl, Hyperlink.IsEnabledProperty);
                                    hl.MouseEnter += (s3, e3) => {
                                        hl.Cursor = rtbvm.Parent.IsSelected ? Cursors.Hand : Cursors.Arrow;
                                    };
                                    hl.MouseLeave += (s3, e3) => {
                                        hl.Cursor = Cursors.Arrow;
                                    };
                                    hl.MouseLeftButtonDown += (s4, e4) => {
                                        if (hl.NavigateUri != null && rtbvm.Parent.IsSelected) {
                                            MpHelpers.Instance.OpenUrl(hl.NavigateUri.ToString());
                                        }
                                    };

                                    var convertToQrCodeMenuItem = new MenuItem();
                                    convertToQrCodeMenuItem.Header = "Convert to QR Code";
                                    convertToQrCodeMenuItem.Click += (s5, e1) => {
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
                                            try {
                                                string urlText = MpHelpers.Instance.GetFullyFormattedUrl(linkText);
                                                if (MpHelpers.Instance.IsValidUrl(urlText) &&
                                                   Uri.IsWellFormedUriString(urlText, UriKind.RelativeOrAbsolute)) {
                                                    hl.NavigateUri = new Uri(urlText);
                                                } else {
                                                    MonkeyPaste.MpConsole.WriteLine(@"Rejected Url: " + urlText + @" link text: " + linkText);
                                                    var par = hl.Parent.FindParentOfType<Paragraph>();
                                                    var s = new Span();
                                                    s.Inlines.AddRange(hl.Inlines.ToArray());
                                                    par.Inlines.InsertAfter(hl, s);
                                                    par.Inlines.Remove(hl);
                                                }
                                            }
                                            catch (Exception ex) {
                                                MonkeyPaste.MpConsole.WriteLine("CreateHyperlinks error creating uri from: " + linkText + " replacing as run and ignoring with exception: " + ex);
                                                var par = hl.Parent.FindParentOfType<Paragraph>();
                                                var s = new Span();
                                                s.Inlines.AddRange(hl.Inlines.ToArray());
                                                par.Inlines.InsertAfter(hl, s);
                                                par.Inlines.Remove(hl);
                                                par.Inlines.Remove(hlink);
                                                break;

                                            }
                                            MenuItem minifyUrl = new MenuItem();
                                            minifyUrl.Header = "Minify with bit.ly";
                                            minifyUrl.Click += async (s1, e2) => {
                                                Hyperlink link = (Hyperlink)((MenuItem)s1).Tag;
                                                string minifiedLink = await MpMinifyUrl.Instance.ShortenUrl(link.NavigateUri.ToString());
                                                if (!string.IsNullOrEmpty(minifiedLink)) {
                                                    matchRange.Text = minifiedLink;
                                                    await ClearHyperlinks ();
                                                    await CreateHyperlinks();
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
                                            try {
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
                                            }
                                            catch (Exception ex) {
                                                MonkeyPaste.MpConsole.WriteLine("Create Hyperlinks warning, cannot connect to currency converter: " + ex);
                                            }
                                            break;
                                        case MpSubTextTokenType.HexColor8:
                                        case MpSubTextTokenType.HexColor6:
                                            var rgbColorStr = linkText;
                                            if (rgbColorStr.Length > 7) {
                                                rgbColorStr = rgbColorStr.Substring(0, 7);
                                            }
                                            hl.NavigateUri = new Uri(@"https://www.hexcolortool.com/" + rgbColorStr);

                                            MenuItem changeColorItem = new MenuItem();
                                            changeColorItem.Header = "Change Color";
                                            changeColorItem.Click += (s, e) => {
                                                var result = MpHelpers.Instance.ShowColorDialog((Brush)new BrushConverter().ConvertFrom(linkText), true);
                                                if (result != null) {
                                                    var run = new Run(result.ToString());
                                                    hl.Inlines.Clear();
                                                    hl.Inlines.Add(run);
                                                    var bgBrush = result;
                                                    var fgBrush = MpHelpers.Instance.IsBright(((SolidColorBrush)bgBrush).Color) ? Brushes.Black : Brushes.White;
                                                    var tr = new TextRange(run.ElementStart, run.ElementEnd);
                                                    tr.ApplyPropertyValue(TextElement.BackgroundProperty, bgBrush);
                                                    tr.ApplyPropertyValue(TextElement.ForegroundProperty, fgBrush);
                                                }
                                            };
                                            hl.ContextMenu.Items.Add(changeColorItem);

                                            hl.Background = (Brush)new BrushConverter().ConvertFromString(linkText);
                                            hl.Foreground = MpHelpers.Instance.IsBright(((SolidColorBrush)hl.Background).Color) ? Brushes.Black : Brushes.White;
                                            break;
                                        default:
                                            MonkeyPaste.MpConsole.WriteLine("Unhandled token type: " + Enum.GetName(typeof(MpSubTextTokenType), (MpSubTextTokenType)hl.Tag) + " with value: " + linkText);
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            });

            if (rtbSelection != null) {
                Rtb.Selection.Select(rtbSelection.Start, rtbSelection.End);
            }
        }
        #endregion

    }
}
