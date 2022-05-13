using GongSolutions.Wpf.DragDrop.Utilities;
using MonkeyPaste;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Annotations;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MonkeyPaste.Plugin;
using System.Windows.Controls.Primitives;

namespace MpWpfApp {
    /// <summary>
    /// Interaction logic for Mpxaml
    /// </summary>
    public partial class MpContentView : MpUserControl<MpClipTileViewModel> {      
        public TextRange NewStartRange;
        public string NewOriginalText;
        public Hyperlink LastEditedHyperlink;

        private bool _isNew = true;
        public ObservableCollection<MpTemplateHyperlink> TemplateViews = new ObservableCollection<MpTemplateHyperlink>();

        public MpContentView() : base() {
            InitializeComponent();
            Rtb.SpellCheck.IsEnabled = MonkeyPaste.MpPreferences.UseSpellCheck;
        }
        
        private void ReceivedClipTileViewModelMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.IsEditable:
                    Rtb.FitDocToRtb();
                    break;
                case MpMessageType.IsReadOnly:
                    Rtb.FitDocToRtb();
                    MpHelpers.RunOnMainThread(async()=> {
                        await SyncModelsAsync();
                    });
                    break;
                case MpMessageType.ContentItemsChanged:
                    ReattachAllBehaviors();
                    break;
            }
        }

        public void ReattachAllBehaviors() {
            DetachAllBehaviors();
            AttachAllBehaviors();
        }
        private void AttachAllBehaviors() {
            //if(BindingContext.HeadItem.CopyItemTitle == "Untitled2054") {
            //    Debugger.Break();
            //}
            //while (BindingContext.IsAnyBusy) {
            //    await Task.Delay(100);
            //}
            //RtbHighlightBehavior.Attach(this);
            ContentViewDropBehavior.Attach(this);

            var ctv = this.GetVisualAncestor<MpClipTileView>();
            if (ctv != null) {
                var cttv = ctv.TileTitleView;
                if (cttv != null && cttv.ClipTileTitleMarqueeCanvas != null) {
                    MpMarqueeExtension.SetIsEnabled(cttv.ClipTileTitleMarqueeCanvas, false);
                    MpMarqueeExtension.SetIsEnabled(cttv.ClipTileTitleMarqueeCanvas, true);
                }
            }
        }

        private void DetachAllBehaviors() {
            //RtbHighlightBehavior.Detach();
            ContentViewDropBehavior.Detach();

            //var ctv = this.GetVisualAncestor<MpClipTileView>();
            //if (ctv != null) {
            //    var cttv = ctv.TileTitleView;
            //    if (cttv != null && cttv.ClipTileTitleMarqueeCanvas != null) {
            //        MpMarqueeExtension.SetIsEnabled(cttv.ClipTileTitleMarqueeCanvas, true);
            //    }
            //}
        }

        private void ReceivedMainWindowResizeBehviorMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ResizingContent:
                case MpMessageType.ResizeContentCompleted:
                    Rtb.FitDocToRtb();
                    break;
            }
        }

        private void ReceivedDragDropManagerMessage(MpMessageType msg) {
            switch (msg) {
                case MpMessageType.ItemDragBegin:
                    if(BindingContext != null && BindingContext.IsSelected) {
                        if(BindingContext.SelectionLength == 0) {
                            Rtb.SelectAll();
                        }
                    }
                    break;
            }
        }

        #region Event Handlers

        private void Rtb_Loaded(object sender, RoutedEventArgs e) {
            

            if (BindingContext != null) {
                if(_isNew) {
                    _isNew = false;

                    MpMessenger.Register<MpMessageType>(
                        nameof(MpDragDropManager),
                        ReceivedDragDropManagerMessage);

                    MpMessenger.Register<MpMessageType>(
                        BindingContext,
                        ReceivedClipTileViewModelMessage,
                        BindingContext);

                    MpMessenger.Register<MpMessageType>(
                        (Application.Current.MainWindow as MpMainWindow).MainWindowResizeBehvior,
                        ReceivedMainWindowResizeBehviorMessage);

                    RegisterViewModelRequests();

                    if (BindingContext.IsPlaceholder) {
                        return;
                    }
                } else {
                    AttachAllBehaviors();
                }
                
                ScrollToHome(); 



                
                 //
            }
        }


        private void Rtb_Unloaded(object sender, RoutedEventArgs e) {
            //DetachAllBehaviors();

            //base.OnUnload();

            if (BindingContext == null) {
                return;
            }

            UnregisterViewModelRequests();

            if(BindingContext != null) {
                MpMessenger.Unregister<MpMessageType>(
                    BindingContext,
                    ReceivedClipTileViewModelMessage,
                    BindingContext);
            }

            var mw = Application.Current.MainWindow as MpMainWindow;
            if(mw != null) {
                if(mw.MainWindowResizeBehvior != null) {
                    MpMessenger.Unregister<MpMessageType>(
                            mw.MainWindowResizeBehvior,
                            ReceivedMainWindowResizeBehviorMessage);
                }
            }
        }

        private void RegisterViewModelRequests() {
            BindingContext.OnUiUpdateRequest += Rtbivm_OnUiUpdateRequest;
            BindingContext.OnScrollOffsetRequest += BindingContext_OnScrollOffsetRequest;
            BindingContext.OnPastePortableDataObject += BindingContext_OnPastePortableDataObject;
            //BindingContext.OnFocusRequest += BindingContext_OnFocusRequest;
            //BindingContext.OnUiResetRequest -= Rtbivm_OnRtbResetRequest;
            //BindingContext.OnScrollWheelRequest -= Rtbivm_OnScrollWheelRequest;
            //BindingContext.OnSyncModels -= Rtbivm_OnSyncModels;
            //BindingContext.OnFitContenBindingContexttRequest -= Ncivm_OnFitContentRequest;
        }


        private void UnregisterViewModelRequests() {
            BindingContext.OnUiUpdateRequest -= Rtbivm_OnUiUpdateRequest;
            BindingContext.OnScrollOffsetRequest -= BindingContext_OnScrollOffsetRequest;
            BindingContext.OnPastePortableDataObject -= BindingContext_OnPastePortableDataObject;
            //BindingContext.OnUiResetRequest -= Rtbivm_OnRtbResetRequest;
            //BindingContext.OnScrollWheelRequest -= Rtbivm_OnScrollWheelRequest;
            //.OnSyncModels -= Rtbivm_OnSyncModels;
            //BindingContext.OnFitContentRequest -= Ncivm_OnFitContentRequest;
        }

        private void BindingContext_OnFocusRequest(object sender, EventArgs e) {
           // MpIsFocusedExtension.SetIsFocused()
        }

        private void BindingContext_OnPastePortableDataObject(object sender, object portableDataObjectOrCopyItem) {
            ContentViewDropBehavior.Paste(BindingContext, portableDataObjectOrCopyItem).FireAndForgetSafeAsync(BindingContext);
        }

        private void BindingContext_OnScrollOffsetRequest(object sender, Point e) {
            ScrollByPointDelta(e);
        }

        public void ScrollByPointDelta(Point e) {
            if(MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                return;
            }
            var sv = Rtb.GetVisualDescendent<ScrollViewer>();
            if (sv == null) {
                //MpConsole.WriteTraceLine("Warning, scroll viewer not loaded yet. This may need to be async");
                return;
            }
            //MpConsole.WriteLine("pre clamp auto scroll delta: " + e);

            var hsb = sv.GetScrollBar(Orientation.Horizontal);
            var vsb = sv.GetScrollBar(Orientation.Vertical);

            //MpConsole.WriteLine(string.Format(@"Scrollable Width {0} Extent Width {1} ScrollBar Max {2} Track Max {3}", sv.ScrollableWidth, sv.ExtentWidth, hsb.Maximum, hsb.Track.Maximum));

            double new_x_offset = Math.Max(0,Math.Min(sv.HorizontalOffset + e.X, hsb.Maximum));
            double new_y_offset = Math.Max(0, Math.Min(sv.VerticalOffset + e.Y, vsb.Maximum));
            //MpConsole.WriteLine("clamped delta: " + new Point(new_x_offset, new_y_offset));

            ScrollToPoint(new Point(new_x_offset, new_y_offset));

        }

        public void ScrollToPoint(Point p) {
            var sv = Rtb.GetVisualDescendent<ScrollViewer>();
            if (sv == null) {
                //MpConsole.WriteTraceLine("Warning, scroll viewer not loaded yet. This may need to be async");
                return;
            }

            sv.ScrollToHorizontalOffset(p.X);
            sv.ScrollToVerticalOffset(p.Y);

            sv.InvalidateScrollInfo();
        }

        private void Rtb_PreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            if(BindingContext.IsContentReadOnly && 
               !BindingContext.IsSubSelectionEnabled) {
                e.Handled = true;
                return;
            }
            double origVertOffset = Rtb.VerticalOffset;
            if (e.Delta > 0) {
                Rtb.LineDown();
            } else {
                Rtb.LineUp();
            }
            e.Handled = Rtb.VerticalOffset != origVertOffset;
        }

        private void Rtb_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            //if (e.OldValue != null && e.OldValue is MpContentItemViewModel ocivm) {
            //    ocivm.OnUiResetRequest -= Rtbivm_OnRtbResetRequest;
            //    ocivm.OnScrollWheelRequest -= Rtbivm_OnScrollWheelRequest;
            //    ocivm.OnUiUpdateRequest -= Rtbivm_OnUiUpdateRequest;
            //    ocivm.OnSyncModels -= Rtbivm_OnSyncModels;
            //    ocivm.OnFitContentRequest -= Ncivm_OnFitContentRequest;
            //    ocivm.OnMergeRequest -= Ncivm_OnMergeRequest;
            //}
            //if (e.NewValue != null && e.NewValue is MpContentItemViewModel ncivm) {
            //    if (!ncivm.IsPlaceholder) {
            //        ncivm.OnMergeRequest += Ncivm_OnMergeRequest;
            //        ncivm.OnUiResetRequest += Rtbivm_OnRtbResetRequest;
            //        ncivm.OnScrollWheelRequest += Rtbivm_OnScrollWheelRequest;
            //        ncivm.OnUiUpdateRequest += Rtbivm_OnUiUpdateRequest;
            //        ncivm.OnSyncModels += Rtbivm_OnSyncModels;
            //        ncivm.OnFitContentRequest += Ncivm_OnFitContentRequest;
            //        if(e.OldValue != null) {
            //            MpHelpers.RunOnMainThread(async () => {
            //                await CreateHyperlinksAsync(CTS.Token);
            //            });
            //        }
            //    }
            //}
            if(e.NewValue is MpClipTileViewModel ctvm) {
                ctvm.OnUiUpdateRequest += Rtbivm_OnUiUpdateRequest;
            }
        }
        private void Rtb_SizeChanged(object sender, SizeChangedEventArgs e) {
            if (BindingContext != null) {
                if (e.HeightChanged && 
                    !Rtb.IsReadOnly) {
                    //MpMainWindowResizeBehavior.Instance.Resize(e.NewSize.Height - e.PreviousSize.Height);
                }
                if(e.WidthChanged || e.HeightChanged) {
                    if(!MpDragDropManager.IsDragAndDrop) {
                        // NOTE content drop removes wrapping (changes page size)
                        // and fit will discard that
                        Rtb.FitDocToRtb();
                    }
                }

                if(BindingContext.IsSelected &&
                   !BindingContext.IsTitleReadOnly) {
                    Rtb.Focus();
                }
            }
        }
        private void Rtb_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            //if (MpClipTrayViewModel.Instance.IsAnyTileExpanded) {
            //    return;
            //}

            if (!BindingContext.IsSelected) {
                BindingContext.IsSelected = true;
            }

            e.Handled = true;
            var fe = sender as FrameworkElement;

            MpContextMenuView.Instance.DataContext = MpClipTrayViewModel.Instance.MenuItemViewModel;
            MpContextMenuView.Instance.PlacementTarget = this;
            MpContextMenuView.Instance.IsOpen = true;
        }

        private void Rtb_MouseEnter(object sender, MouseEventArgs e) {
            if (!BindingContext.IsContentReadOnly || BindingContext.IsSubSelectionEnabled) {
                MpCursor.SetCursor(BindingContext, MpCursorType.IBeam);
            }
        }
        
        private void Rtb_MouseLeave(object sender, MouseEventArgs e) {
            MpCursor.UnsetCursor(BindingContext);
        }        

        private void Rtb_SelectionChanged(object sender, RoutedEventArgs e) {
            foreach(var civm in BindingContext.Items) {
                var civm_pt = civm.CopyItemData.ToPlainText();

            }
            if(MpDragDropManager.IsDragAndDrop) {
                return;
            }

            //if(!Rtb.Selection.IsEmpty && !BindingContext.IsSubSelectionEnabled) {
            //    // NOTE don't 
            //    Rtb.Selection.Select(Rtb.Selection.Start, Rtb.Selection.Start);
            //}

            //
        }

        private void Rtb_LostFocus(object sender, RoutedEventArgs e) {
            if (MpDragDropManager.IsDragAndDrop) {
                return;
            }
            if (!BindingContext.IsSelected && 
                !BindingContext.IsSubSelectionEnabled && !Rtb.Selection.IsEmpty) {
                //Rtb.Selection.Select(Rtb.Selection.Start, Rtb.Selection.Start);
                //ScrollToHome();
                MpCursor.UnsetCursor(BindingContext);
            }
        }
        private void Rtb_PreviewKeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.V && Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) {
                if(!BindingContext.IsSelected) {
                    BindingContext.IsSelected = true;
                }
                MpClipTrayViewModel.Instance.PasteCurrentClipboardIntoSelectedTileCommand.Execute(null);
            }
        }

        private void Rtb_PreviewKeyUp(object sender, KeyEventArgs e) {
            // BUG For some reason IsContentReadOnly is set to true before this is called
            // so not checking if read only so should be aight
            if (e.Key == Key.Escape) {
                //BindingContext.Parent.ToggleReadOnlyCommand.Execute(null);
                BindingContext.ClearEditing();
            } 
        }

        private void Rtb_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            BindingContext.IsSelected = true;
            MpIsFocusedExtension.SetIsFocused(Rtb, true);

            if (e.ClickCount >= 2 && 
                BindingContext.IsContentReadOnly &&
                !BindingContext.IsSubSelectionEnabled) {
                // NOTE only do select all when enabling otherwise default (select word)
                BindingContext.IsSubSelectionEnabled = true;
                MpCursor.SetCursor(BindingContext, MpCursorType.IBeam);
                //Rtb.SelectAll();
                //ScrollToHome();
                UpdateAdorners();
                return;
            }
            if (!BindingContext.IsTitleReadOnly ||
                !BindingContext.IsContentReadOnly ||
                 BindingContext.Parent.IsAnyResizing ||
                 BindingContext.Parent.CanAnyResize ||
                 MpResizeBehavior.IsAnyResizing) {
                e.Handled = false;
                return;
            }
            if(BindingContext.IsSubSelectionEnabled) {
                // NOTE only check for drag when there is selected text AND
                // drag is from somewhere in the selection range.
                // If mouse down isn't in selection range reset selection to down position
                if(Rtb.Selection.IsEmpty) {
                    e.Handled = false;
                    return;
                }
                if(!Rtb.Selection.IsPointInRange(e.GetPosition(Rtb))) {
                    //var mptp = Rtb.GetPositionFromPoint(e.GetPosition(Rtb),true);
                    //Rtb.Selection.Select(mptp, mptp);
                    e.Handled = false;
                    return;
                }
            }

            MpDragDropManager.StartDragCheck(BindingContext);

            e.Handled = true;
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
            if (!Rtb.IsLoaded) {
                // likely during startup
                return;
            }
            ScrollToHome();
            Rtb.UpdateLayout();
            UpdateAdorners();

            if(BindingContext != null && 
               !BindingContext.IsSubSelectionEnabled &&
               BindingContext.IsContentReadOnly &&
               !BindingContext.IsAnyItemDragging) {
                //Rtb.Selection.Select(Rtb.Document.ContentStart, Rtb.Document.ContentStart);
                Rtb.CaretPosition = Rtb.Document.ContentStart;
            }
        }

        private void Rtbivm_OnScrollWheelRequest(object sender, double e) {
            //double vertScrollAmount = BindingContext.UnformattedContentSize.Height * e;
            //Rtb.ScrollToVerticalOffset(vertScrollAmount);
        }

        private void Rtbivm_OnRtbResetRequest(object sender, bool focusRtb) {
            ScrollToHome();
            Rtb.CaretPosition = Rtb.Document.ContentStart;
            if (focusRtb) {
                Rtb.Focus();
            }
        }

        #endregion

        public void ScrollToHome() {
            Rtb.ScrollToHome();
            ScrollToPoint(new Point());
        }

        public void ScrollToEnd() {
            Rtb.ScrollToEnd();
        }


        #region Template/Hyperlinks

        public async Task SyncModelsAsync() {
            //var rtbvm = DataContext as MpContentItemViewModel;
            //rtbvm.IsBusy = true;
            ////clear any search highlighting when saving the document then restore after save
            ////rtbvm.Parent.HighlightTextRangeViewModelCollection.HideHighlightingCommand.Execute(rtbvm);

            ////rtbvm.Parent.HighlightTextRangeViewModelCollection.UpdateInDocumentsBgColorList(Rtb);
            ////Rtb.UpdateLayout();
            ////string test = Rtb.Document.ToRichText();
            ////RtbHighlightBehavior.HideHighlighting();

            //await ClearHyperlinks();

            //rtbvm.CopyItem.ItemData = Rtb.Document.ToRichText();

            //await rtbvm.CopyItem.WriteToDatabaseAsync();

            //rtbvm.OnPropertyChanged(nameof(rtbvm.CopyItemData));

            //await CreateHyperlinksAsync(CTS.Token);

            ////Rtb.UpdateLayout();

            ////MpConsole.WriteLine("Item syncd w/ data: " + rtbvm.CopyItemData);
            ////MpRtbTemplateCollection.CreateTemplateViews(Rtb);

            ////MpHelpers.RunOnMainThread(UpdateLayout);
            ////rtbvm.Parent.HighlightTextRangeViewModelCollection.ApplyHighlightingCommand.Execute(rtbvm);
        }

        public async Task ClearHyperlinks() {
            //var rtbvm = Rtb.DataContext as MpContentItemViewModel;
            //var tvm_ToRemove = new List<MpTemplateViewModel>();
            //if(rtbvm.TemplateCollection.Templates.Count != TemplateViews.Count) {
            //    // means user deleted template in view
            //    foreach(var tvm in rtbvm.TemplateCollection.Templates) {
            //        //if there are no views for a template then it all instances were deleted
            //        if(!TemplateViews.Any(x=>x.TemplateTextBlock.Text == tvm.TemplateDisplayValue)) {
            //            tvm_ToRemove.Add(tvm);
            //        }
            //    }
            //    foreach(var tvm2r in tvm_ToRemove) {
            //        rtbvm.TemplateCollection.Templates.Remove(tvm2r);
            //    }
            //    await Task.WhenAll(tvm_ToRemove.Select(x => x.TextToken.DeleteFromDatabaseAsync()));
            //}
            //foreach (var hl in TemplateViews) {
            //    hl.Clear();
            //}
            //TemplateViews.Clear();
            //if (rtbvm.TemplateCollection != null) {
            //    rtbvm.TemplateCollection.Templates.Clear();
            //}
            ////var hll = new List<Hyperlink>();
            ////foreach (var p in Rtb.Document.Blocks.OfType<Paragraph>()) {
            ////    foreach (var hl in p.Inlines.OfType<Hyperlink>()) {
            ////        hll.Add(hl);
            ////    }
            ////}
            ////foreach (var hl in hll) {
            ////    string linkText;
            ////    if (hl.DataContext == null || hl.DataContext is MpContentItemViewModel) {
            ////        linkText = new TextRange(hl.ElementStart, hl.ElementEnd).Text;
            ////        hl.Inlines.Clear();
            ////        new Span(new Run(linkText), hl.ElementStart);
            ////    }
            ////}
        }

        public async Task CreateHyperlinksAsync(CancellationToken ct, DispatcherPriority dp = DispatcherPriority.Normal) {
//            var rtbvm = BindingContext;
//            rtbvm.IsBusy = true;

//            if (Rtb == null || rtbvm.CopyItem == null) {
//                return;
//            }
//            var rtbSelection = Rtb?.Selection;
//            var templateModels = await MpDataModelProvider.GetTextTemplatesAsync(rtbvm.CopyItemId);
//            string templateRegEx = string.Join("|", templateModels.Select(x => x.EncodedTemplate));
//            string pt = rtbvm.CopyItem.ItemData.ToPlainText(); //Rtb.Document.ToPlainText();
//            for (int i = 1; i < MpRegEx.RegExList.Count; i++) {
//                var linkType = (MpSubTextTokenType)i;
//                if (linkType == MpSubTextTokenType.StreetAddress) {
//                    //doesn't consistently work and presents bugs so disabling for now
//                    continue;
//                }
//                var lastRangeEnd = Rtb.Document.ContentStart;
//                Regex regEx = MpRegEx.GetRegExForTokenType(linkType);
//                if (linkType == MpSubTextTokenType.TemplateSegment) {
//                    if (string.IsNullOrEmpty(templateRegEx)) {
//                        //this occurs for templates when copyitem has no templates
//                        continue;
//                    }
//                    regEx = new Regex(templateRegEx, RegexOptions.ExplicitCapture | RegexOptions.Multiline);
//                }
                
//                var mc = regEx.Matches(pt);
//                foreach (Match m in mc) {
//                    foreach (Group mg in m.Groups) {
//                        foreach (Capture c in mg.Captures) {
//                            Hyperlink hl = null;
//                            var matchRange = await MpHelpers.FindStringRangeFromPositionAsync(lastRangeEnd, c.Value, ct, dp, true);
//                            if (matchRange == null || string.IsNullOrEmpty(matchRange.Text)) {
//                                continue;
//                            }
//                            lastRangeEnd = matchRange.End;
//                            if (linkType == MpSubTextTokenType.TemplateSegment) {
//                                var copyItemTemplate = templateModels.Where(x => x.EncodedTemplate == matchRange.Text).FirstOrDefault(); //TemplateHyperlinkCollectionViewModel.Where(x => x.TemplateName == matchRange.Text).FirstOrDefault().TextToken;
//                                var thl = await MpTemplateHyperlink.Create(matchRange, copyItemTemplate);
//                            } else {
//                                var hlCheck1 = matchRange.Start.Parent.FindParentOfType<Hyperlink>();
//                                var hlCheck2 = matchRange.End.Parent.FindParentOfType<Hyperlink>();
//                                if(hlCheck1 != null || hlCheck2 != null) {
//                                    //matched text is already a hyperlink (likely from html)
//                                    continue;
//                                }
//                                var matchRun = new Run(matchRange.Text);
//                                matchRange.Text = "";

//                                // DO NOT REMOVE this extra link ensures selection is retained!
//                                var hlink = new Hyperlink(matchRun, matchRange.Start);
//                                hl = new Hyperlink(matchRange.Start, matchRange.End);
//                                hl = hlink;
//                                hl.ToolTip = @"[Ctrl + Click to follow link]";
//                                var linkText = c.Value;
//                                hl.Tag = linkType;
//                                //if (linkText == @"DragAction.Cancel") {
//                                //    linkText = linkText;
//                                //}
//                                //MpHelpers.CreateBinding(rtbvm, new PropertyPath(nameof(rtbvm.IsSelected)), hl, Hyperlink.IsEnabledProperty);

//                                KeyEventHandler hlKeyDown = (object o, KeyEventArgs e) => {
//                                    // This gives user feedback so if they see the 'ctrl + click to follow'
//                                    // and they aren't holding ctrl until they see the message it will change cursor while
//                                    // over link
//                                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) {
//                                        MpCursor.SetCursor(hl, MpCursorType.Hand);                                        
//                                    } else {
//                                        MpCursor.UnsetCursor(hl);
//                                    }
//                                };
//                                MouseEventHandler hlMouseEnter = (object o, MouseEventArgs e) => {
//                                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) {
//                                        MpCursor.SetCursor(hl, MpCursorType.Hand);
//                                    } else {
//                                        MpCursor.UnsetCursor(hl);
//                                    }
//                                    hl.IsEnabled = true;
//                                    //Keyboard.AddKeyDownHandler(Application.Current.MainWindow, hlKeyDown);
//                                    rtbvm.IsOverHyperlink = true;
//                                };
//                                MouseEventHandler hlMouseLeave = (object o, MouseEventArgs e) => {
//                                    //if (rtbvm.Parent.IsAnyEditingContent) {
//                                    //    MpCursorStack.PushCursor(hl, MpCursorType.IBeam);
//                                    //} else {
                                        
//                                    //}
//                                    MpCursor.UnsetCursor(hl);
//                                    hl.IsEnabled = false;
//                                    //Keyboard.RemoveKeyDownHandler(Application.Current.MainWindow, hlKeyDown);
//                                    rtbvm.IsOverHyperlink = false;
//                                };
//                                MouseButtonEventHandler hlMouseLeftButtonDown = (object o, MouseButtonEventArgs e) => {
//                                    if (hl.NavigateUri != null && Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) {
//                                        MpHelpers.OpenUrl(hl.NavigateUri.ToString());
//                                    }
//                                };
//                                RoutedEventHandler hlUnload = null;
//                                hlUnload = (object o, RoutedEventArgs e) =>{
//                                    hl.MouseEnter -= hlMouseEnter;
//                                    hl.MouseLeave -= hlMouseLeave;
//                                    hl.MouseLeftButtonDown -= hlMouseLeftButtonDown;
//                                    hl.Unloaded -= hlUnload;
//                                };
//                                hl.MouseEnter += hlMouseEnter;
//                                hl.MouseLeave += hlMouseLeave;
//                                hl.MouseLeftButtonDown += hlMouseLeftButtonDown;
//                                hl.Unloaded += hlUnload;

//                                var convertToQrCodeMenuItem = new MenuItem();
//                                convertToQrCodeMenuItem.Header = "Convert to QR Code";
//                                RoutedEventHandler qrItemClick = (object o, RoutedEventArgs e) => {
//                                    var hyperLink = (Hyperlink)((MenuItem)o).Tag;
//                                    var bmpSrc = MpHelpers.ConvertUrlToQrCode(hyperLink.NavigateUri.ToString());
//                                    MpClipboardHelper.MpClipboardManager.InteropService.SetDataObjectWrapper(
//                                        new MpDataObject() {
//                                            DataFormatLookup = new Dictionary<MpClipboardFormatType, string>() {
//                                                {
//                                                    MpClipboardFormatType.Bitmap,
//                                                    bmpSrc.ToBase64String()
//                                                }
//                                            }
//                                        });
//                                };
//                                convertToQrCodeMenuItem.Click += qrItemClick;
//                                RoutedEventHandler qrUnload = null;
//                                qrUnload = (object o, RoutedEventArgs e) => {
//                                    convertToQrCodeMenuItem.Click -= qrItemClick;
//                                    convertToQrCodeMenuItem.Unloaded -= qrUnload;
//                                };
//                                convertToQrCodeMenuItem.Unloaded += qrUnload;

//                                convertToQrCodeMenuItem.Tag = hl;
//                                hl.ContextMenu = new ContextMenu();
//                                hl.ContextMenu.Items.Add(convertToQrCodeMenuItem);

//                                switch ((MpSubTextTokenType)hl.Tag) {
//                                    case MpSubTextTokenType.StreetAddress:
//                                        hl.NavigateUri = new Uri("https://google.com/maps/place/" + linkText.Replace(' ', '+'));
//                                        break;
//                                    case MpSubTextTokenType.Uri:
//                                        try {
//                                            string urlText = MonkeyPaste.MpUrlHelpers.GetFullyFormattedUrl(linkText);
//                                            if (MpUrlHelpers.IsValidUrl(urlText) /*&&
//                                                   Uri.IsWellFormedUriString(urlText, UriKind.RelativeOrAbsolute)*/) {
//                                                hl.NavigateUri = new Uri(urlText);
//                                            } else {
//                                                MpConsole.WriteLine(@"Rejected Url: " + urlText + @" link text: " + linkText);
//                                                var par = hl.Parent.FindParentOfType<Paragraph>();
//                                                var s = new Span();
//                                                s.Inlines.AddRange(hl.Inlines.ToArray());
//                                                par.Inlines.InsertAfter(hl, s);
//                                                par.Inlines.Remove(hl);
//                                            }
//                                        }
//                                        catch (Exception ex) {
//                                            MpConsole.WriteLine("CreateHyperlinks error creating uri from: " + linkText + " replacing as run and ignoring with exception: " + ex);
//                                            var par = hl.Parent.FindParentOfType<Paragraph>();
//                                            var s = new Span();
//                                            s.Inlines.AddRange(hl.Inlines.ToArray());
//                                            par.Inlines.InsertAfter(hl, s);
//                                            par.Inlines.Remove(hl);
//                                            par.Inlines.Remove(hlink);
//                                            break;

//                                        }
//                                        MenuItem minifyUrl = new MenuItem();
//                                        minifyUrl.Header = "Minify with bit.ly";
//                                        RoutedEventHandler minItemClick = async (object o, RoutedEventArgs e) => {
//                                            Hyperlink link = (Hyperlink)((MenuItem)o).Tag;
//                                            string minifiedLink = await MpMinifyUrl.Instance.ShortenUrl(link.NavigateUri.ToString());
//                                            if (!string.IsNullOrEmpty(minifiedLink)) {
//                                                matchRange.Text = minifiedLink;
//                                                // ClearHyperlinks();
//                                                // CreateHyperlinks();
//                                            }
//                                            //Clipboard.SetText(minifiedLink);
//                                        };
//                                        minifyUrl.Click += minItemClick;

//                                        RoutedEventHandler minUnload = null;
//                                        minUnload = (object o, RoutedEventArgs e) => {
//                                            minifyUrl.Click -= minItemClick;
//                                            minifyUrl.Unloaded -= minUnload;
//                                        };
//                                        minifyUrl.Unloaded += minUnload;

//                                        minifyUrl.Tag = hl;
//                                        hl.ContextMenu.Items.Add(minifyUrl);
//                                        break;
//                                    case MpSubTextTokenType.Email:
//                                        hl.NavigateUri = new Uri("mailto:" + linkText);
//                                        break;
//                                    case MpSubTextTokenType.PhoneNumber:
//                                        hl.NavigateUri = new Uri("tel:" + linkText);
//                                        break;
//                                    case MpSubTextTokenType.Currency:
//                                        try {
//                                            //"https://www.google.com/search?q=%24500.80+to+yen"
//                                            MenuItem convertCurrencyMenuItem = new MenuItem();
//                                            convertCurrencyMenuItem.Header = "Convert Currency To";
//                                            var fromCurrencyType = MpHelpers.GetCurrencyTypeFromString(linkText);
//                                            foreach (MpCurrency currency in MpCurrencyConverter.Instance.CurrencyList) {
//                                                if (currency.Id == Enum.GetName(typeof(CurrencyType), fromCurrencyType)) {
//                                                    continue;
//                                                }
//                                                MenuItem subItem = new MenuItem();
//                                                subItem.Header = currency.CurrencyName + "(" + currency.CurrencySymbol + ")";
//                                                RoutedEventHandler subItemClick = async (object o, RoutedEventArgs e) => {
//                                                    Enum.TryParse(currency.Id, out CurrencyType toCurrencyType);
//                                                    var convertedValue = await MpCurrencyConverter.Instance.ConvertAsync(
//                                                        MpHelpers.GetCurrencyValueFromString(linkText),
//                                                        fromCurrencyType,
//                                                        toCurrencyType);
//                                                    convertedValue = Math.Round(convertedValue, 2);
//                                                    if (Rtb.Tag != null && ((List<Hyperlink>)Rtb.Tag).Contains(hl)) {
//                                                        ((List<Hyperlink>)Rtb.Tag).Remove(hl);
//                                                    }
//                                                    Run run = new Run(currency.CurrencySymbol + convertedValue);
//                                                    hl.Inlines.Clear();
//                                                    hl.Inlines.Add(run);
//                                                };
//                                                subItem.Click += subItemClick;
//                                                RoutedEventHandler subUnload = null;
//                                                subUnload = (object o, RoutedEventArgs e) => {
//                                                    subItem.Click -= subItemClick;
//                                                    subItem.Unloaded -= subUnload;
//                                                };
//                                                subItem.Unloaded += subUnload;
//                                                convertCurrencyMenuItem.Items.Add(subItem);
//                                            }

//                                            hl.ContextMenu.Items.Add(convertCurrencyMenuItem);
//                                        }
//                                        catch (Exception ex) {
//                                            MpConsole.WriteLine("Create Hyperlinks warning, cannot connect to currency converter: " + ex);
//                                        }
//                                        break;
//                                    case MpSubTextTokenType.HexColor:
//                                        var rgbColorStr = linkText;
//                                        if (rgbColorStr.Length > 7) {
//                                            rgbColorStr = rgbColorStr.Substring(0, 7);
//                                        }
//                                        hl.NavigateUri = new Uri(@"https://www.hexcolortool.com/" + rgbColorStr);
//                                        hl.IsEnabled = true;
//                                        Action showChangeColorDialog = () => {
//                                            var result = new MpWpfCustomColorChooserMenu().ShowCustomColorMenu(linkText,null);
//                                            if (result != null) {
//                                                var run = new Run(result.ToString());
//                                                hl.Inlines.Clear();
//                                                hl.Inlines.Add(run);
//                                                var bgBrush = result.ToBrush();
//                                                var fgBrush = MpWpfColorHelpers.IsBright(((SolidColorBrush)bgBrush).Color) ? Brushes.Black : Brushes.White;
//                                                var tr = new TextRange(run.ElementStart, run.ElementEnd);
//                                                tr.ApplyPropertyValue(TextElement.BackgroundProperty, bgBrush);
//                                                tr.ApplyPropertyValue(TextElement.ForegroundProperty, fgBrush);
//                                            }
//                                        };
//                                        //hl.MouseLeftButtonDown -= hlMouseLeftButtonDown;
//                                        hl.Click += (s, e) => {
//                                            if(Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) {
//                                                showChangeColorDialog.Invoke();
//                                            }
//                                        };
//                                        MouseButtonEventHandler hexColorMouseLeftButtonDown = (object o, MouseButtonEventArgs e) => {
//                                            showChangeColorDialog.Invoke();
//                                        };
//                                        hl.MouseLeftButtonDown += hexColorMouseLeftButtonDown;


//                                        RoutedEventHandler hexColorUnload = null;
//                                        hexColorUnload = (object o, RoutedEventArgs e) => {
//                                            hl.MouseLeftButtonDown -= hexColorMouseLeftButtonDown;
//                                            hl.Unloaded -= hexColorUnload;
//                                        };

//                                        hl.Unloaded += hexColorUnload;
//                                        MenuItem changeColorItem = new MenuItem();
//                                        changeColorItem.Header = "Change Color";
//                                        RoutedEventHandler changeColorClick = (object o, RoutedEventArgs e) => {
//                                            showChangeColorDialog.Invoke();
//                                        };
//                                        changeColorItem.Click += changeColorClick;

//                                        RoutedEventHandler changeColorUnload = null;
//                                        changeColorUnload = (object o, RoutedEventArgs e) => {
//                                            changeColorItem.Click -= changeColorClick;
//                                            changeColorItem.Unloaded -= changeColorUnload;
//};
//                                        changeColorItem.Unloaded += changeColorUnload;
//                                        hl.ContextMenu.Items.Add(changeColorItem);

//                                        hl.Background = (Brush)new BrushConverter().ConvertFromString(linkText);
//                                        hl.Foreground = MpWpfColorHelpers.IsBright(((SolidColorBrush)hl.Background).Color) ? Brushes.Black : Brushes.White;
//                                        break;
//                                    default:
//                                        MpConsole.WriteLine("Unhandled token type: " + Enum.GetName(typeof(MpSubTextTokenType), (MpSubTextTokenType)hl.Tag) + " with value: " + linkText);
//                                        break;
//                                }
//                            }
//                        }
//                    }
//                }
//            }

//            if(rtbSelection != null) {
//                Rtb.Selection.Select(rtbSelection.Start,rtbSelection.End);
//            }

//            InitCaretAdorner();

//            BindingContext.IsBusy = false;
        }


        #endregion

        private void Rtb_MouseMove(object sender, MouseEventArgs e) {
            if (BindingContext.IsHovering && BindingContext.IsSubSelectionEnabled) {
                // BUG when sub selection becomes empty the cursor goes back to default
                // so this ensures it stays ibeam
                MpCursor.SetCursor(BindingContext, MpCursorType.IBeam);
            } else {
                MpCursor.UnsetCursor(BindingContext);
            }
        }

        private void Rtb_TextChanged(object sender, TextChangedEventArgs e) {
            UpdateAdorners();
        }

        public void UpdateAdorners() {
            if (Rtb == null) {
                return;
            }
            var rtb_a = AdornerLayer.GetAdornerLayer(Rtb);
            if (rtb_a == null) {
                return;
            }
            rtb_a.Update();
        }

        
    }
}
