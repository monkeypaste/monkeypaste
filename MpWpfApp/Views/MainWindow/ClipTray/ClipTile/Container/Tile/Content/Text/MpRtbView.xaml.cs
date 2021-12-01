using GongSolutions.Wpf.DragDrop.Utilities;
using MonkeyPaste;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using System.Windows.Threading;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for Mpxaml
    /// </summary>
    public partial class MpRtbView : MpUserControl<MpContentItemViewModel> {
        public static int DropOverHomeItemId = -1;
        public static int DropOverEndItemId = -1;
        public static MpRtbView CurDropOverRtbView = null;

        protected MpContentItemCaretAdorner CaretAdorner;
        protected AdornerLayer CaretAdornerLayer;

        //public MpLine HomeCaretLine = new MpLine();
        //public MpLine EndCaretLine = new MpLine();

        public Rect HomeRect, EndRect;

        public TextRange NewStartRange;
        public string NewOriginalText;
        public Hyperlink LastEditedHyperlink;


        public ObservableCollection<MpTemplateHyperlink> TemplateViews = new ObservableCollection<MpTemplateHyperlink>();

        public MpRtbView() {
            InitializeComponent();
            Rtb.SpellCheck.IsEnabled = MonkeyPaste.MpPreferences.Instance.UseSpellCheck;
        }
        private void ReceivedClipTrayViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ItemDragBegin:
                case MpMessageType.ItemDragEnd:
                    CaretAdornerLayer.Update();
                    break;
            }
        }
        #region Event Handlers

        private void Rtb_Loaded(object sender, RoutedEventArgs e) {
            if (DataContext != null && DataContext is MpContentItemViewModel rtbivm) {
                if(rtbivm.IsPlaceholder) {
                    return;
                }

                if (rtbivm.IsNewAndFirstLoad) {
                    //force new items to have left alignment
                    Rtb.CaretPosition = Rtb.Document.ContentStart;
                    Rtb.Document.TextAlignment = TextAlignment.Left;
                    rtbivm.IsNewAndFirstLoad = false;
                }

                MpHelpers.Instance.RunOnMainThread(async () => {
                    await CreateHyperlinksAsync();
                });
                MpMessenger.Instance.Register<MpMessageType>(MpClipTrayViewModel.Instance, ReceivedClipTrayViewModelMessage);
            }
        }

        private void Rtb_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != null && e.OldValue is MpContentItemViewModel ocivm) {
                ocivm.OnUiResetRequest -= Rtbivm_OnRtbResetRequest;
                ocivm.OnScrollWheelRequest -= Rtbivm_OnScrollWheelRequest;
                ocivm.OnUiUpdateRequest -= Rtbivm_OnUiUpdateRequest;
                ocivm.OnSyncModels -= Rtbivm_OnSyncModels;
                ocivm.OnFitContentRequest -= Ncivm_OnFitContentRequest;
            }
            if (e.NewValue != null && e.NewValue is MpContentItemViewModel ncivm) {
                if (!ncivm.IsPlaceholder) {
                    ncivm.OnUiResetRequest += Rtbivm_OnRtbResetRequest;
                    ncivm.OnScrollWheelRequest += Rtbivm_OnScrollWheelRequest;
                    ncivm.OnUiUpdateRequest += Rtbivm_OnUiUpdateRequest;
                    ncivm.OnSyncModels += Rtbivm_OnSyncModels;
                    ncivm.OnFitContentRequest += Ncivm_OnFitContentRequest;
                    if(e.OldValue != null) {
                        MpHelpers.Instance.RunOnMainThread(async () => {
                            await CreateHyperlinksAsync();
                        });
                    }
                }
            }
        }


        private void Rtb_SizeChanged(object sender, SizeChangedEventArgs e) {
            var civm = DataContext as MpContentItemViewModel;
            if (civm != null && civm.Parent.IsExpanded) {
                var ctcv = this.GetVisualAncestor<MpClipTileContainerView>();
                if (ctcv != null && e.HeightChanged && !MpTileExpanderBehavior.IsAnyExpandingOrUnexpanding) {
                    ctcv.ExpandBehavior.Resize(e.NewSize.Height - e.PreviousSize.Height);
                }

                if(BindingContext.IsSelected) {
                    Rtb.Focus();
                }
            }
        }

        private void Rtb_MouseEnter(object sender, MouseEventArgs e) {
            if (BindingContext.Parent.IsExpanded) {
                if (BindingContext.IsSelected) {
                    MpMouseViewModel.Instance.CurrentCursor = MpCursorType.IBeam;
                    return;
                }
            }

            MpMouseViewModel.Instance.CurrentCursor = MpCursorType.Default;
        }

        private void Rtb_MouseMove(object sender, MouseEventArgs e) {
            e.Handled = false;
            if (BindingContext.Parent.IsExpanded) {
                if (BindingContext.IsSelected) {
                    MpMouseViewModel.Instance.CurrentCursor = MpCursorType.IBeam;
                    return;
                }
            }

            MpMouseViewModel.Instance.CurrentCursor = MpCursorType.Default;
        }

        private void Rtb_MouseLeave(object sender, MouseEventArgs e) {
            MpMouseViewModel.Instance.CurrentCursor = MpCursorType.Default;
        }

        private void Rtb_Unloaded(object sender, RoutedEventArgs e) {
            if(BindingContext == null) {
                return;
            }
            BindingContext.OnUiResetRequest -= Rtbivm_OnRtbResetRequest;
            BindingContext.OnScrollWheelRequest -= Rtbivm_OnScrollWheelRequest;
            BindingContext.OnUiUpdateRequest -= Rtbivm_OnUiUpdateRequest;
            BindingContext.OnSyncModels -= Rtbivm_OnSyncModels;
            BindingContext.OnFitContentRequest -= Ncivm_OnFitContentRequest;
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
            var rtbvm = DataContext as MpContentItemViewModel;
            if (rtbvm.IsEditingContent && e.Changes.Count > 0) {
                //SizeContainerToContent();
            }
        }

        private void Rtb_GotFocus(object sender, RoutedEventArgs e) {
            var rtbvm = DataContext as MpContentItemViewModel;
            if(rtbvm.Parent.IsExpanded) {
                //rtbvm.IsEditingContent = true;
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
        }

        private void Rtb_PreviewKeyUp(object sender, KeyEventArgs e) {
            var civm = DataContext as MpContentItemViewModel;
            if (e.Key == Key.Space && civm.IsEditingContent) {
               // MpHelpers.Instance.RunOnMainThread(async () => {
                    // TODO Update regex hyperlink matches (but ignore current ones??)
                    //await SyncModelsAsync();
                //});
            } 
        }

        #endregion

        #region View Model Callbacks

        private void Ncivm_OnFitContentRequest(object sender, EventArgs e) {
            Rtb.FitDocToRtb();
        }

        private async void Rtbivm_OnSyncModels(object sender, EventArgs e) {
            await SyncModelsAsync();
        }

        private void Rtbivm_OnUiUpdateRequest(object sender, EventArgs e) {
            ScrollToHome();
            Rtb.UpdateLayout();
        }

        private void Rtbivm_OnScrollWheelRequest(object sender, int e) {
            Rtb.ScrollToVerticalOffset(Rtb.VerticalOffset + e);
        }

        private void Rtbivm_OnRtbResetRequest(object sender, bool focusRtb) {
            ScrollToHome();
            Rtb.CaretPosition = Rtb.Document.ContentStart;
            if (focusRtb) {
                Rtb.Focus();
            }
        }

        #endregion

        #region Merging

        public void ScrollToHome() {
            Rtb.ScrollToHome();
            InitCaretAdorner();
        }

        public void ScrollToEnd() {
            Rtb.ScrollToEnd();
            InitCaretAdorner();
        }

        public void InitCaretAdorner() {
            if(CaretAdorner == null) {
                CaretAdorner = new MpContentItemCaretAdorner(Rtb);
                CaretAdornerLayer = AdornerLayer.GetAdornerLayer(Rtb);
                CaretAdornerLayer.Add(CaretAdorner);
            }
            CaretAdorner.Test.Clear();

            HomeRect = Rtb.Document.ContentStart.GetCharacterRect(LogicalDirection.Forward);

            EndRect = Rtb.Document.ContentEnd.GetCharacterRect(LogicalDirection.Backward);

            CaretAdornerLayer.Update();
        }

        public static void ShowHomeCaretAdorner(MpRtbView rtbView) {
            ClearCaretAdorner();
            DropOverHomeItemId = rtbView.BindingContext.CopyItemId;
            rtbView.ScrollToHome();
            rtbView.CaretAdorner.CaretLine = new MpLine(rtbView.HomeRect.TopLeft, rtbView.HomeRect.BottomLeft);
            rtbView.CaretAdornerLayer.Update();
            CurDropOverRtbView = rtbView;
        }

        public static void ShowEndCaretAdorner(MpRtbView rtbView) {
            ClearCaretAdorner();
            DropOverEndItemId = rtbView.BindingContext.CopyItemId;
            rtbView.ScrollToEnd();
            rtbView.CaretAdorner.CaretLine = new MpLine(rtbView.EndRect.TopRight, rtbView.EndRect.BottomRight);
            rtbView.CaretAdornerLayer.Update();
            CurDropOverRtbView = rtbView;
        }

        public static void ClearCaretAdorner() {
            DropOverHomeItemId = DropOverEndItemId = -1;
            if(CurDropOverRtbView != null) {
                CurDropOverRtbView.ScrollToHome();
                CurDropOverRtbView.CaretAdornerLayer.Update();
                CurDropOverRtbView = null;
            }
        }

        public async Task MergeContentItem(MpCopyItem mci, bool isDuplicating) {
            bool isHomeMerge;
            if(BindingContext.CopyItemId == DropOverHomeItemId) {
                isHomeMerge = true;
            } else if (BindingContext.CopyItemId == DropOverEndItemId) {
                isHomeMerge = false;
            } else {
                throw new Exception("RtbVIew is not flagged for drop");
            }
            BindingContext = DataContext as MpContentItemViewModel;
            BindingContext.IsBusy = true;

            await ClearHyperlinks();

            // merge content
            if (isHomeMerge) {
                BindingContext.CopyItem.ItemData = MpHelpers.Instance.CombineRichText(Rtb.Document.ToRichText(), mci.ItemData);
            } else {
                BindingContext.CopyItem.ItemData = MpHelpers.Instance.CombineRichText(mci.ItemData, Rtb.Document.ToRichText());
            }

            // merge templates
            var citl = await MpDataModelProvider.Instance.GetTemplatesAsync(BindingContext.CopyItemId);
            var mcitl = await MpDataModelProvider.Instance.GetTemplatesAsync(mci.Id);
            foreach (MpCopyItemTemplate mcit in mcitl) {
                if (citl.Any(x => x.TemplateName == mcit.TemplateName)) {
                    //if merged item has template w/ same name just ignore it since it will already be parsed
                    continue;
                }
                mcit.CopyItemId = BindingContext.CopyItemId;
                await mcit.WriteToDatabaseAsync();
            }

            // merge tags
            var tl = await MpDataModelProvider.Instance.GetCopyItemTagsForCopyItemAsync(BindingContext.CopyItemId);
            var mtl = await MpDataModelProvider.Instance.GetCopyItemTagsForCopyItemAsync(mci.Id);
            foreach (MpCopyItemTag mt in mtl) {
                if (tl.Any(x => x.TagId == mt.TagId)) {
                    //if merged item has tags w/ same name just ignore it 
                    continue;
                }
                mt.CopyItemId = BindingContext.CopyItemId;
                await mt.WriteToDatabaseAsync();
            }

            if(!isDuplicating) {
                await mci.DeleteFromDatabaseAsync();
            }

            // write and restore item
            await BindingContext.CopyItem.WriteToDatabaseAsync();

            BindingContext.OnPropertyChanged(nameof(BindingContext.CopyItemData));

            await CreateHyperlinksAsync();
        }

        #endregion

        #region Template/Hyperlinks

        public async Task SyncModelsAsync() {
            var rtbvm = DataContext as MpContentItemViewModel;
            rtbvm.IsBusy = true;
            //clear any search highlighting when saving the document then restore after save
            //rtbvm.Parent.HighlightTextRangeViewModelCollection.HideHighlightingCommand.Execute(rtbvm);

            //rtbvm.Parent.HighlightTextRangeViewModelCollection.UpdateInDocumentsBgColorList(Rtb);
            //Rtb.UpdateLayout();
            //string test = Rtb.Document.ToRichText();

            await ClearHyperlinks();

            rtbvm.CopyItem.ItemData = Rtb.Document.ToRichText();

            await rtbvm.CopyItem.WriteToDatabaseAsync();

            rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemData));

            await CreateHyperlinksAsync();

            //Rtb.UpdateLayout();

            //MpConsole.WriteLine("Item syncd w/ data: " + rtbvm.CopyItemData);
            //MpRtbTemplateCollection.CreateTemplateViews(Rtb);

            //MpHelpers.Instance.RunOnMainThread(UpdateLayout);
            //rtbvm.Parent.HighlightTextRangeViewModelCollection.ApplyHighlightingCommand.Execute(rtbvm);
        }

        public async Task ClearHyperlinks() {
            var rtbvm = Rtb.DataContext as MpContentItemViewModel;
            var tvm_ToRemove = new List<MpTemplateViewModel>();
            if(rtbvm.TemplateCollection.Templates.Count != TemplateViews.Count) {
                // means user deleted template in view
                foreach(var tvm in rtbvm.TemplateCollection.Templates) {
                    //if there are no views for a template then it all instances were deleted
                    if(!TemplateViews.Any(x=>x.TemplateTextBlock.Text == tvm.TemplateDisplayValue)) {
                        tvm_ToRemove.Add(tvm);
                    }
                }
                foreach(var tvm2r in tvm_ToRemove) {
                    rtbvm.TemplateCollection.Templates.Remove(tvm2r);
                }
                await Task.WhenAll(tvm_ToRemove.Select(x => x.CopyItemTemplate.DeleteFromDatabaseAsync()));
            }
            foreach (var hl in TemplateViews) {
                hl.Clear();
            }
            TemplateViews.Clear();
            if (rtbvm.TemplateCollection != null) {
                rtbvm.TemplateCollection.Templates.Clear();
            }
            //var hll = new List<Hyperlink>();
            //foreach (var p in Rtb.Document.Blocks.OfType<Paragraph>()) {
            //    foreach (var hl in p.Inlines.OfType<Hyperlink>()) {
            //        hll.Add(hl);
            //    }
            //}
            //foreach (var hl in hll) {
            //    string linkText;
            //    if (hl.DataContext == null || hl.DataContext is MpContentItemViewModel) {
            //        linkText = new TextRange(hl.ElementStart, hl.ElementEnd).Text;
            //        hl.Inlines.Clear();
            //        new Span(new Run(linkText), hl.ElementStart);
            //    }
            //}
        }

        public async Task CreateHyperlinksAsync(CancellationTokenSource cts = null, DispatcherPriority dp = DispatcherPriority.Normal) {
            var rtbvm = BindingContext;
            rtbvm.IsBusy = true;

            if (Rtb == null || rtbvm.CopyItem == null) {
                return;
            }
            if(cts == null) {
                cts = new CancellationTokenSource();
                //cts.CancelAfter(1000);
            }
            var rtbSelection = Rtb?.Selection;
            var templateModels = await MpDataModelProvider.Instance.GetTemplatesAsync(rtbvm.CopyItemId);
            string templateRegEx = string.Join("|", templateModels.Select(x => x.TemplateToken));
            string pt = rtbvm.CopyItem.ItemData.ToPlainText(); //Rtb.Document.ToPlainText();
            for (int i = 1; i < MpRegEx.Instance.RegExList.Count; i++) {
                var linkType = (MpSubTextTokenType)i;
                if (linkType == MpSubTextTokenType.StreetAddress) {
                    //doesn't consistently work and presents bugs so disabling for now
                    continue;
                }
                var lastRangeEnd = Rtb.Document.ContentStart;
                Regex regEx = MpRegEx.Instance.RegExList[i]; //MpRegEx.Instance.GetRegExForTokenType(linkType);
                if (linkType == MpSubTextTokenType.TemplateSegment) {
                    if (string.IsNullOrEmpty(templateRegEx)) {
                        //this occurs for templates when copyitem has no templates
                        continue;
                    }
                    regEx = new Regex(templateRegEx, RegexOptions.ExplicitCapture | RegexOptions.Multiline);
                }
                
                var mc = regEx.Matches(pt);
                foreach (Match m in mc) {
                    foreach (Group mg in m.Groups) {
                        foreach (Capture c in mg.Captures) {
                            Hyperlink hl = null;
                            var matchRange = await MpHelpers.Instance.FindStringRangeFromPositionAsync(lastRangeEnd, c.Value, cts.Token, dp, true);
                            if (matchRange == null || string.IsNullOrEmpty(matchRange.Text)) {
                                continue;
                            }
                            lastRangeEnd = matchRange.End;
                            if (linkType == MpSubTextTokenType.TemplateSegment) {
                                var copyItemTemplate = templateModels.Where(x => x.TemplateToken == matchRange.Text).FirstOrDefault(); //TemplateHyperlinkCollectionViewModel.Where(x => x.TemplateName == matchRange.Text).FirstOrDefault().CopyItemTemplate;
                                var thl = MpTemplateHyperlink.Create(matchRange, copyItemTemplate);
                            } else {
                                var matchRun = new Run(matchRange.Text);
                                matchRange.Text = "";

                                // DO NOT REMOVE this extra link ensures selection is retained!
                                var hlink = new Hyperlink(matchRun, matchRange.Start);
                                hl = new Hyperlink(matchRange.Start, matchRange.End);
                                hl = hlink;
                                hl.ToolTip = @"[Ctrl + Click to follow link]";
                                var linkText = c.Value;
                                hl.Tag = linkType;
                                //if (linkText == @"DragAction.Cancel") {
                                //    linkText = linkText;
                                //}
                                //MpHelpers.Instance.CreateBinding(rtbvm, new PropertyPath(nameof(rtbvm.IsSelected)), hl, Hyperlink.IsEnabledProperty);

                                KeyEventHandler hlKeyDown = (object o, KeyEventArgs e) => {
                                    // This gives user feedback so if they see the 'ctrl + click to follow'
                                    // and they aren't holding ctrl until they see the message it will change cursor while
                                    // over link
                                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) {
                                        MpMouseViewModel.Instance.CurrentCursor = MpCursorType.Link;
                                    } else {
                                        if (rtbvm.IsEditingContent) {
                                            MpMouseViewModel.Instance.CurrentCursor = MpCursorType.IBeam;
                                        } else {
                                            MpMouseViewModel.Instance.CurrentCursor = MpCursorType.Default;
                                        }
                                    }
                                };
                                MouseEventHandler hlMouseEnter = (object o, MouseEventArgs e) => {
                                    if(Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) {
                                        MpMouseViewModel.Instance.CurrentCursor = MpCursorType.Link;
                                    }                                    
                                    hl.IsEnabled = true;
                                    //Keyboard.AddKeyDownHandler(Application.Current.MainWindow, hlKeyDown);
                                    rtbvm.IsOverHyperlink = true;
                                };
                                MouseEventHandler hlMouseLeave = (object o, MouseEventArgs e) => {
                                    if (rtbvm.Parent.IsAnyEditingContent) {
                                        MpMouseViewModel.Instance.CurrentCursor = MpCursorType.IBeam;                                        
                                    } else {
                                        MpMouseViewModel.Instance.CurrentCursor = MpCursorType.Default;
                                    }
                                    hl.IsEnabled = false;
                                    //Keyboard.RemoveKeyDownHandler(Application.Current.MainWindow, hlKeyDown);
                                    rtbvm.IsOverHyperlink = false;
                                };
                                MouseButtonEventHandler hlMouseLeftButtonDown = (object o, MouseButtonEventArgs e) => {
                                    if (hl.NavigateUri != null && Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) {
                                        MpHelpers.Instance.OpenUrl(hl.NavigateUri.ToString());
                                    }
                                };
                                RoutedEventHandler hlUnload = null;
                                hlUnload = (object o, RoutedEventArgs e) =>{
                                    hl.MouseEnter -= hlMouseEnter;
                                    hl.MouseLeave -= hlMouseLeave;
                                    hl.MouseLeftButtonDown -= hlMouseLeftButtonDown;
                                    hl.Unloaded -= hlUnload;
                                };
                                hl.MouseEnter += hlMouseEnter;
                                hl.MouseLeave += hlMouseLeave;
                                hl.MouseLeftButtonDown += hlMouseLeftButtonDown;
                                hl.Unloaded += hlUnload;

                                var convertToQrCodeMenuItem = new MenuItem();
                                convertToQrCodeMenuItem.Header = "Convert to QR Code";
                                RoutedEventHandler qrItemClick = (object o, RoutedEventArgs e) => {
                                    var hyperLink = (Hyperlink)((MenuItem)o).Tag;
                                    var bmpSrc = MpHelpers.Instance.ConvertUrlToQrCode(hyperLink.NavigateUri.ToString());
                                    MpClipboardManager.Instance.SetImageWrapper(bmpSrc);
                                };
                                convertToQrCodeMenuItem.Click += qrItemClick;
                                RoutedEventHandler qrUnload = null;
                                qrUnload = (object o, RoutedEventArgs e) => {
                                    convertToQrCodeMenuItem.Click -= qrItemClick;
                                    convertToQrCodeMenuItem.Unloaded -= qrUnload;
                                };
                                convertToQrCodeMenuItem.Unloaded += qrUnload;

                                convertToQrCodeMenuItem.Tag = hl;
                                hl.ContextMenu = new ContextMenu();
                                hl.ContextMenu.Items.Add(convertToQrCodeMenuItem);

                                switch ((MpSubTextTokenType)hl.Tag) {
                                    case MpSubTextTokenType.StreetAddress:
                                        hl.NavigateUri = new Uri("https://google.com/maps/place/" + linkText.Replace(' ', '+'));
                                        break;
                                    case MpSubTextTokenType.Uri:
                                        try {
                                            string urlText = MonkeyPaste.MpHelpers.Instance.GetFullyFormattedUrl(linkText);
                                            if (MpHelpers.Instance.IsValidUrl(urlText) /*&&
                                                   Uri.IsWellFormedUriString(urlText, UriKind.RelativeOrAbsolute)*/) {
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
                                        RoutedEventHandler minItemClick = async (object o, RoutedEventArgs e) => {
                                            Hyperlink link = (Hyperlink)((MenuItem)o).Tag;
                                            string minifiedLink = await MpMinifyUrl.Instance.ShortenUrl(link.NavigateUri.ToString());
                                            if (!string.IsNullOrEmpty(minifiedLink)) {
                                                matchRange.Text = minifiedLink;
                                                // ClearHyperlinks();
                                                // CreateHyperlinks();
                                            }
                                            //Clipboard.SetText(minifiedLink);
                                        };
                                        minifyUrl.Click += minItemClick;

                                        RoutedEventHandler minUnload = null;
                                        minUnload = (object o, RoutedEventArgs e) => {
                                            minifyUrl.Click -= minItemClick;
                                            minifyUrl.Unloaded -= minUnload;
                                        };
                                        minifyUrl.Unloaded += minUnload;

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
                                                RoutedEventHandler subItemClick = async (object o, RoutedEventArgs e) => {
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
                                                subItem.Click += subItemClick;
                                                RoutedEventHandler subUnload = null;
                                                subUnload = (object o, RoutedEventArgs e) => {
                                                    subItem.Click -= subItemClick;
                                                    subItem.Unloaded -= subUnload;
                                                };
                                                subItem.Unloaded += subUnload;
                                                convertCurrencyMenuItem.Items.Add(subItem);
                                            }

                                            hl.ContextMenu.Items.Add(convertCurrencyMenuItem);
                                        }
                                        catch (Exception ex) {
                                            MonkeyPaste.MpConsole.WriteLine("Create Hyperlinks warning, cannot connect to currency converter: " + ex);
                                        }
                                        break;
                                    case MpSubTextTokenType.HexColor:
                                        var rgbColorStr = linkText;
                                        if (rgbColorStr.Length > 7) {
                                            rgbColorStr = rgbColorStr.Substring(0, 7);
                                        }
                                        hl.NavigateUri = new Uri(@"https://www.hexcolortool.com/" + rgbColorStr);
                                        hl.IsEnabled = true;
                                        Action showChangeColorDialog = () => {
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
                                        //hl.MouseLeftButtonDown -= hlMouseLeftButtonDown;
                                        hl.Click += (s, e) => {
                                            if(Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) {
                                                showChangeColorDialog.Invoke();
                                            }
                                        };
                                        MouseButtonEventHandler hexColorMouseLeftButtonDown = (object o, MouseButtonEventArgs e) => {
                                            showChangeColorDialog.Invoke();
                                        };
                                        hl.MouseLeftButtonDown += hexColorMouseLeftButtonDown;


                                        RoutedEventHandler hexColorUnload = null;
                                        hexColorUnload = (object o, RoutedEventArgs e) => {
                                            hl.MouseLeftButtonDown -= hexColorMouseLeftButtonDown;
                                            hl.Unloaded -= hexColorUnload;
                                        };

                                        hl.Unloaded += hexColorUnload;
                                        MenuItem changeColorItem = new MenuItem();
                                        changeColorItem.Header = "Change Color";
                                        RoutedEventHandler changeColorClick = (object o, RoutedEventArgs e) => {
                                            showChangeColorDialog.Invoke();
                                        };
                                        changeColorItem.Click += changeColorClick;

                                        RoutedEventHandler changeColorUnload = null;
                                        changeColorUnload = (object o, RoutedEventArgs e) => {
                                            changeColorItem.Click -= changeColorClick;
                                            changeColorItem.Unloaded -= changeColorUnload;
};
                                        changeColorItem.Unloaded += changeColorUnload;
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

            if(rtbSelection != null) {
                Rtb.Selection.Select(rtbSelection.Start,rtbSelection.End);
            }

            InitCaretAdorner();

            BindingContext.IsBusy = false;
        }

        #endregion
    }
}
