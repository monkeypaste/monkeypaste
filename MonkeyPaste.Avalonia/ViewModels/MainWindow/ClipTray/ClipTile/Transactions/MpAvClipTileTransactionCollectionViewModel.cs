using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public interface MpIPlainTextViewModel : MpIViewModel {
        string PlainText { get; }
    }

    public class MpAvClipTileTransactionCollectionViewModel :
        MpAvViewModelBase<MpAvClipTileViewModel>,
        MpIAnimatedSizeViewModel,
        MpIContextMenuViewModel {
        #region Private Variables

        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region MpIAnimatedSizeViewModel Implementation

        bool MpIAnimatedSizeViewModel.IsAnimating { get; set; }
        double MpIBoundSizeViewModel.ContainerBoundWidth { get; set; }
        double MpIBoundSizeViewModel.ContainerBoundHeight { get; set; }

        #endregion

        #region MpIContextMenuItemViewModel Implementation

        public MpAvMenuItemViewModel ContextMenuViewModel {
            get {
                // NOTE this tries to show menu of all unique souces
                // ordered by their most recent trans
                // with sub-items of pertinent msgs ordered by trans dt
                // then (after seperator) the source menu items (they're all the same so just any of them)
                // if no msgs then no seperator and just the source menu items

                // get distinct sources that are not this app as an app source
                var distinct_sources = SortedTransactions
                        .SelectMany(x => x.Sources)
                        .Where(x => x.SourceRef != null)
                        .DistinctBy(x => new { x.SourceType, x.SourceObjId })
                        .Where(x => !(x.SourceType == MpTransactionSourceType.App && x.SourceObjId == MpDefaultDataModelTools.ThisAppId) && x.ContextMenuItemViewModel != null)
                        .Select(x => x.SourceRef)
                        .OrderBy(x => SortedTransactions.IndexOf(SortedTransactions.FirstOrDefault(y => y.HasSource(x))))
                        .ToList();

                List<MpAvMenuItemViewModel> cmil = new List<MpAvMenuItemViewModel>();
                foreach (var source_ref in distinct_sources) {
                    // find all pertinent msgs for this source ordered by trans dt

                    var analysis_mil =
                        Transactions
                            .OrderByDescending(x => x.TransactionDateTime)
                            .Where(x => x.HasSource(source_ref))
                            .SelectMany(x => x.Messages)
                            .OfType<MpAvParameterRequestMessageViewModel>()
                            .Where(x => x.ContextMenuItemViewModel != null)
                            .Select(x => x.ContextMenuItemViewModel)
                            .ToList();

                    // get any source vm menu item
                    var source_mi =
                        Transactions
                        .FirstOrDefault(x => x.HasSource(source_ref))
                        .Sources
                        .FirstOrDefault(x => x.SourceRef.IsSourceEqual(source_ref))
                        .ContextMenuItemViewModel;

                    var source_sub_mil = source_mi.SubItems.OfType<MpAvMenuItemViewModel>().ToList();

                    if (analysis_mil.Any() &&
                        source_sub_mil.Any()) {
                        source_sub_mil.FirstOrDefault().HasLeadingSeparator = true;
                        // insert seperator between source and messages
                        //source_mi.SubItems.Insert(0, new MpAvMenuItemViewModel() { IsSeparator = true });
                    }

                    // insert msgs (header is trans dt)
                    analysis_mil.ForEach((x, idx) => source_sub_mil.Insert(idx, x));

                    source_mi.SubItems = source_sub_mil;
                    // add source to output list
                    cmil.Add(source_mi);
                }

                if (!cmil.Any()) {
                    // no transactions to show, something must be wrong
                    // to avoid weird menu layout show stub item
                    cmil.Add(
                        new MpAvMenuItemViewModel() {
                            IconResourceKey = "QuestionMarkImage",
                            Header = UiStrings.ClipTileTransactionErrorText
                        });
                }
                return new MpAvMenuItemViewModel() {
                    Header = UiStrings.ClipTileSourcesHeader,
                    HasLeadingSeparator = true,
                    IconResourceKey = "EggImage",
                    SubItems = cmil

                };
            }
        }

        public bool IsContextMenuOpen { get; set; }
        #endregion

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpAvTransactionItemViewModel> Transactions { get; set; } = new ObservableCollection<MpAvTransactionItemViewModel>();
        public IEnumerable<MpAvTransactionItemViewModel> SortedTransactions =>
            IsSortDescending ?
                Transactions.OrderByDescending(x => x.TransactionDateTime).ToList() :
                Transactions.OrderBy(x => x.TransactionDateTime).ToList();

        public IEnumerable<MpAvTransactionItemViewModel> VisibleTransactions =>
            SortedTransactions
            .Where(x => x.Response is MpAvAnnotationMessageViewModel amvm && amvm.RootAnnotationViewModel != null)
            .ToList();

        public MpAvTransactionItemViewModel SelectedTransaction { get; set; }

        public MpAvTransactionItemViewModel MostRecentTransaction =>
            Transactions.OrderByDescending(x => x.TransactionDateTime).FirstOrDefault();

        public IEnumerable<MpAvTransactionMessageViewModelBase> Messages =>
            Transactions.SelectMany(x => x.Messages).ToList();

        public IEnumerable<MpAvTransactionSourceViewModel> AllSources =>
            Transactions.SelectMany(x => x.Sources);

        public IEnumerable<MpAvTransactionMessageViewModelBase> SortedMessages =>
            Messages
            .OrderByDescending(x => x.ComparableSortValue).ToList();

        public MpAvTransactionItemViewModel CreateTransaction =>
            Transactions.OrderBy(x => x.TransactionDateTime).FirstOrDefault();

        #endregion

        #region Layout

        public double DefaultTransactionPanelWidth {
            get {
                return MpAvClipTrayViewModel.Instance.DefaultQueryItemWidth * 0.5;
            }
        }
        public double DefaultTransactionPanelHeight {
            get {
                return MpAvClipTrayViewModel.Instance.DefaultQueryItemHeight * 0.5;
            }
        }
        public double MaxWidth {
            get {
                if (Parent == null) {
                    return 0;
                }
                if (!IsTransactionPaneOpen) {
                    return 0;
                }
                return Parent.BoundWidth * 0.5;
            }
        }

        #endregion

        #region State
        public bool IsPlainTextView { get; set; } = false;
        public bool DoShake { get; set; }
        public bool IsSortDescending { get; set; } = true;
        public bool IsTransactionPaneOpen { get; set; } = false;
        public bool IsTransactionPaneAnimating { get; set; }
        public bool IsAnyBusy =>
            IsBusy || Transactions.Any(x => x.IsBusy);

        public bool IsAnyAnalysisTransaction =>
            Transactions.Any(x => x.IsAnalysisTransaction);


        #endregion

        #region Model

        public int SourceUserDeviceId {
            get {
                if (CreateTransaction == null) {
                    return 0;
                }
                return CreateTransaction.TransactionDeviceId;
            }
        }

        public int CopyItemId {
            get {
                if (Parent == null) {
                    return 0;
                }
                return Parent.CopyItemId;
            }
        }
        #endregion

        #endregion

        #region Constructors

        public MpAvClipTileTransactionCollectionViewModel() : this(null) { }

        public MpAvClipTileTransactionCollectionViewModel(MpAvClipTileViewModel parent) : base(parent) {
            PropertyChanged += MpAvClipTileSourceCollectionViewModel_PropertyChanged;
            Transactions.CollectionChanged += Transactions_CollectionChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(int copyItemId) {
            IsBusy = true;

            Transactions.Clear();

            var ci_transactions = await MpDataModelProvider.GetCopyItemTransactionsByCopyItemIdAsync(copyItemId);
            foreach (var cit in ci_transactions.OrderByDescending(x => x.TransactionDateTime)) {
                var cisvm = await CreateTransactionItemViewModelAsync(cit);
                Transactions.Add(cisvm);
            }

            while (Transactions.Any(x => x.IsAnyBusy)) {
                await Task.Delay(100);
            }

            var trans_to_apply = Transactions.Where(x => !x.HasTransactionBeenApplied);
            Task.WhenAll(trans_to_apply.Select(x => ApplyTransactionAsync(x)))
                .FireAndForgetSafeAsync(this);

            OnPropertyChanged(nameof(Transactions));
            OnPropertyChanged(nameof(CreateTransaction));
            OnPropertyChanged(nameof(SortedMessages));

            IsBusy = false;
        }

        #endregion

        #region Protected Methods

        #region Db Op Overrides

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if (e is MpCopyItemTransaction cit && cit.CopyItemId == CopyItemId) {
                Dispatcher.UIThread.Post(async () => {
                    IsBusy = true;
                    while (Mp.Services.TransactionBuilder.CopyItemTransactionsInProgress.Contains(Parent.CopyItemId)) {
                        // wait for item transactions to complete..
                        await Task.Delay(100);
                    }
                    var cisvm = await CreateTransactionItemViewModelAsync(cit);
                    while (cisvm.IsAnyBusy) {
                        // wait for transaction to load
                        await Task.Delay(100);
                    }
                    IsBusy = false;
                    DoShake = true;

                    await ApplyTransactionAsync(cisvm);

                    //OpenTransactionPaneCommand.Execute(null);
                    //SelectedTransaction = cisvm;
                });
            }
        }


        #endregion

        #endregion

        #region Private Methods

        private async Task<MpAvTransactionItemViewModel> CreateTransactionItemViewModelAsync(MpCopyItemTransaction cit) {
            var cisvm = new MpAvTransactionItemViewModel(this);
            await cisvm.InitializeAsync(cit);
            return cisvm;
        }

        private void MpAvClipTileSourceCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsBusy):
                    if (Parent == null) {
                        break;
                    }
                    OnPropertyChanged(nameof(IsAnyBusy));
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyBusy));
                    break;
                case nameof(IsTransactionPaneOpen):
                    if (Parent == null) {
                        break;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.IsTitleVisible));
                    break;
                case nameof(IsTransactionPaneAnimating):
                    if (IsTransactionPaneAnimating) {
                        break;
                    }

                    if (!Parent.IsPinned) {
                        MpAvClipTrayViewModel.Instance.RefreshQueryTrayLayout();
                    }
                    break;
                case nameof(CreateTransaction):
                    if (Parent == null) {
                        break;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.IconResourceObj));
                    break;
                case nameof(SelectedTransaction):
                    ApplyTransactionAsync(SelectedTransaction).FireAndForgetSafeAsync(this);
                    break;
                case nameof(DoShake):
                    if (!DoShake) {
                        break;
                    }
                    Dispatcher.UIThread.Post(async () => {
                        var sw = Stopwatch.StartNew();
                        while (sw.ElapsedMilliseconds < MpAvThemeViewModel.Instance.ShakeDurMs) {
                            await Task.Delay(100);
                        }
                        DoShake = false;
                    });
                    break;
                case nameof(IsPlainTextView):
                    Transactions.ForEach(x => x.OnPropertyChanged(nameof(x.IsPlainTextView)));
                    break;
            }
        }

        private async Task ApplyTransactionAsync(MpAvTransactionItemViewModel tivm, bool isDefaultShow = false) {
            if (tivm == null) {
                return;
            }

            bool is_new = false;
            if (Transactions.All(x => x.TransactionId != tivm.TransactionId)) {
                Transactions.Add(tivm);
                is_new = true;
            }
            Transactions.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));

            if (!isDefaultShow) {
                if (tivm.HasTransactionBeenApplied) {
                    return;
                }

                if (tivm.IsRunTimeAppliableTransaction &&
                    !IsTransactionPaneOpen &&
                    !is_new) {
                    // immediately open and select new ann
                    //IsTransactionPaneOpen = true;'
                    return;
                }
            }

            while (!Parent.IsEditorLoaded) {
                await Task.Delay(100);
            }

            if (Parent.GetContentView() is not MpIContentView cv) {
                return;
            }
            if (tivm.GetTransactionDelta() is not MpJsonObject updateObj) {
                updateObj = tivm.GetTransactionAnnotation();
            }
            if (updateObj == null) {
                return;
            }

            bool success = await cv.UpdateContentAsync(updateObj);
            if (success) {
                tivm.HasTransactionBeenApplied = true;

                if (tivm.IsOneTimeAppliableTransaction) {
                    tivm.AppliedDateTime = DateTime.Now;

                    // for now, assume this is only text annotator and doesn't need auto-selecting
                    return;
                }

                object to_select_tnvm_or_root_ann_guid = null;
                if (updateObj is MpAnnotationNodeFormat root_anf) {
                    to_select_tnvm_or_root_ann_guid = root_anf.guid;
                } else {
                    to_select_tnvm_or_root_ann_guid = tivm;
                }
                SelectChildCommand.Execute(to_select_tnvm_or_root_ann_guid);
            }
        }

        private void Transactions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(Transactions));
            OnPropertyChanged(nameof(SortedTransactions));
            OnPropertyChanged(nameof(VisibleTransactions));
            OnPropertyChanged(nameof(IsAnyAnalysisTransaction));
        }
        private void SetTransactionViewGridLength(GridLength gl) {
            if (Parent.GetContentView() is not Control cv ||
                    cv.GetVisualAncestor<MpAvClipTileView>() is not MpAvClipTileView ctv ||
                    ctv.FindControl<Grid>("TileGrid") is not Grid tileGrid) {
                return;
            }

            // setting all column 1 view to IsVisible=false doesn't decrease 
            // the column's grid length so all other tileGrid views (column 0) 
            // are <transaction width> less than total width, so this reset column 1 width
            var trans_gl = tileGrid.ColumnDefinitions[1];
            trans_gl.Width = gl;// new GridLength(0, GridUnitType.Auto);
        }

        #endregion

        #region Commands

        public ICommand ShowTransactionContextMenuCommand => new MpCommand<object>(
            (args) => {
                var control = args as Control;
                if (control == null) {
                    return;
                }
                if (!Parent.IsSelected) {
                    Parent.Parent.SelectClipTileCommand.Execute(Parent);
                }

                MpAvMenuView.ShowMenu(
                    target: control,
                    dc: ContextMenuViewModel);
            });

        public ICommand ToggleTransactionPaneOpenCommand => new MpCommand(
            () => {
                if (IsTransactionPaneOpen) {
                    CloseTransactionPaneCommand.Execute(null);
                } else {
                    OpenTransactionPaneCommand.Execute(null);
                }
                OnPropertyChanged(nameof(Parent.IsTitleVisible));
            }, () => {
                return Parent != null;
            });

        public MpIAsyncCommand<object> CreateTransactionFromOleOpCommand => new MpAsyncCommand<object>(
            async (args) => {
                if (args is not object[] argParts ||
                    argParts.Length != 2 ||
                    argParts[0] is not MpTransactionType transType ||
                    argParts[1] is not MpPortableDataObject mpdo) {
                    return;
                }
                if (transType == MpTransactionType.Appended) {
                    // NOTE append sources are added before notifying editor since the source of the event
                    // is clipboard change not drop or paste events which come from editor so
                    // more accurate sources can be obtained checking in build workflow..

                    if (!Parent.IsAppendNotifier) {
                        MpDebug.Break("Append state mismatch");
                    }

                    return;
                }

                // TODO below is current way web view returns dt delta, should put it in 
                // the data object instead (when, if ever change tracking is added)
                //if (!string.IsNullOrEmpty(dataTransferCompleted_ntf.changeDeltaJsonStr)) {
                //    resp_json = dataTransferCompleted_ntf.changeDeltaJsonStr.ToStringFromBase64();
                //}
                string resp_json = mpdo.GetData(MpPortableDataFormats.INTERNAL_CONTENT_DELTA_FORMAT) as string;


                IEnumerable<string> refs = null;
                if (mpdo != null) {
                    var other_refs = await Mp.Services.SourceRefTools.GatherSourceRefsAsync(mpdo);
                    refs = other_refs.Select(x => Mp.Services.SourceRefTools.ConvertToInternalUrl(x));
                }

                if (transType == MpTransactionType.None) {
                    // what's the label?
                    MpDebug.Break();
                    transType = MpTransactionType.Error;
                }
                int ciid = Parent.CopyItemId;
                await Mp.Services.TransactionBuilder.ReportTransactionAsync(
                    copyItemId: ciid,
                    reqType: MpJsonMessageFormatType.DataObject,
                    req: mpdo.SerializeData(),
                    respType: MpJsonMessageFormatType.Delta,
                    resp: resp_json,
                    ref_uris: refs,
                    transType: transType);

            }, (args) => {
                return Parent != null;
            });

        public ICommand RemoveTransactionCommand => new MpCommand<object>(
            (args) => {
                MpAvTransactionItemViewModel tivm = null;
                if (args is MpAvTransactionItemViewModel) {
                    tivm = args as MpAvTransactionItemViewModel;
                }

                if (tivm == null) {
                    return;
                }
                int tivm_to_select_idx = -1;
                if (tivm == SelectedTransaction) {
                    tivm_to_select_idx = SortedTransactions.IndexOf(tivm);
                }
                Transactions.Remove(tivm);
                tivm.Transaction.DeleteFromDatabaseAsync().FireAndForgetSafeAsync(this);
                if (tivm_to_select_idx >= 0 && tivm_to_select_idx < Transactions.Count) {
                    SelectedTransaction = SortedTransactions.ElementAt(tivm_to_select_idx);
                }
            });

        public ICommand RemoveMostRecentTransactionCommand => new MpCommand(
            () => {
                RemoveTransactionCommand.Execute(MostRecentTransaction);
            });


        public MpIAsyncCommand CloseTransactionPaneCommand => new MpAsyncCommand(
            async () => {
                Dispatcher.UIThread.VerifyAccess();
                IsTransactionPaneOpen = false;
                SetTransactionViewGridLength(new GridLength(0, GridUnitType.Auto));
                await Task.Delay(1);
                //SetTransactionViewGridLength(new GridLength(0, GridUnitType.Auto));
                //IsTransactionPaneAnimating = true;
                //double nw = Parent.Parent.DefaultQueryItemWidth;
                //double nh = Parent.Parent.DefaultQueryItemHeight;

                //MpAvResizeExtension.ResizeAnimated(
                //        Parent.GetContentView() as Control,
                //        nw, nh,
                //        () => {
                //            IsTransactionPaneAnimating = false;
                //        });
                //while (IsTransactionPaneAnimating) {
                //    await Task.Delay(100);
                //}
                //// only wait if executed async for timing
                //return;
            }, () => {
                return Parent != null && IsTransactionPaneOpen;
            });

        public MpIAsyncCommand<object> OpenTransactionPaneCommand => new MpAsyncCommand<object>(
            async (args) => {
                Dispatcher.UIThread.VerifyAccess();

                await Task.Delay(1);
                IsTransactionPaneOpen = true;
                SetTransactionViewGridLength(new GridLength(0.6, GridUnitType.Star));
                return;

                //OnPropertyChanged(nameof(MaxWidth));
                ////BoundWidth = DefaultTransactionPanelLength;
                ////BoundHeight = Parent.BoundHeight;
                //if(Parent.GetContentView() is not Control cv ||
                //    cv.GetVisualAncestor<MpAvClipTileView>() is not MpAvClipTileView ctv ||
                //    ctv.FindControl<MpAvClipTileTransactionPaneView>("TransactionPanelColumn") is not MpAvClipTileTransactionPaneView cttpv) {
                //    return;
                //}

                //SetTransactionViewGridLength(new GridLength(DefaultTransactionPanelWidth, GridUnitType.Auto));

                //IsTransactionPaneAnimating = true;
                //double nw = Parent.BoundWidth + DefaultTransactionPanelWidth;
                //double nh = Parent.BoundHeight;

                //MpAvResizeExtension.ResizeAnimated(
                //        cttpv,
                //        nw, nh,
                //        () => {
                //            IsTransactionPaneAnimating = false;
                //        });

                //while (IsTransactionPaneAnimating) {
                //    await Task.Delay(100);
                //}

                //// only wait for executeAsync calls
                //return;
            }, (args) => {
                return
                    Parent != null &&
                    Parent.IsWindowOpen &&
                    !IsTransactionPaneOpen &&
                    VisibleTransactions.Any();
            });

        public MpIAsyncCommand<object> SelectChildCommand => new MpAsyncCommand<object>(
            async (args) => {
                MpAvITransactionNodeViewModel to_select_tnvm = null;
                bool isDblClick = false;
                string to_sel_ann_guid = null;
                if (args is object[] argParts &&
                    argParts[0] is string guidArg &&
                    argParts[1] is bool dblClickArg) {
                    isDblClick = dblClickArg;
                    args = guidArg;
                    to_sel_ann_guid = guidArg;
                }

                if (args is string argStr) {
                    if (argStr.IsStringGuid()) {
                        // this only comes from editor for ann atm
                        to_select_tnvm =
                            Messages
                                .OfType<MpAvAnnotationMessageViewModel>()
                                .SelectMany(x => x.RootAnnotationViewModel.SelfAndAllDescendants().Cast<MpAvAnnotationItemViewModel>())
                                .FirstOrDefault(x => x.AnnotationGuid == argStr);
                    }
                } else if (args is MpAvITransactionNodeViewModel) {
                    to_select_tnvm = args as MpAvITransactionNodeViewModel;
                } else {
                    MpDebug.Assert(to_select_tnvm != null, $"Unhandled transaction.select child arg of type '{args?.GetType()}' What type is it??", silent: true);
                }

                if (to_select_tnvm == null) {
                    if (SelectedTransaction == null) {
                        // only fallback to default select if nothing already selected
                        to_select_tnvm = CreateTransaction;
                    }
                }
                if (to_select_tnvm == null) {
                    // ignore for whateber reason
                    MpConsole.WriteLine($"No child trans node to select with arg '{args}'");
                    return;
                }

                if (!string.IsNullOrEmpty(to_sel_ann_guid) &&
                    isDblClick && !OpenTransactionPaneCommand.CanExecute(null)) {
                    // trans panel not open and user dbl clicked an ann
                    MpAvClipTrayViewModel.Instance.SelectClipTileTransactionNodeCommand.Execute(
                        new object[] { Parent.CopyItemId, to_sel_ann_guid });
                    return;
                }


                await OpenTransactionPaneCommand.ExecuteAsync(null);

                if (to_select_tnvm is not MpAvViewModelBase to_select_vmb) {
                    return;
                }
                // walk up dc tree and select self and all ancestors up to this collection
                var cur_to_select = to_select_vmb;
                while (true) {
                    if (cur_to_select == null) {
                        MpDebug.Break("HUH?");
                    }
                    if (cur_to_select == this) {
                        break;
                    }
                    if (cur_to_select is MpISelectableViewModel svm) {
                        svm.IsSelected = true;

                        MpDebug.Assert(svm.IsSelected, $"{svm} can't be selected directly, fix property set/change handlers");
                    } else {
                        MpDebug.Break("what kinda node is it?");
                    }
                    if (cur_to_select is MpAvTransactionItemViewModel to_select_tivm) {
                        SelectedTransaction = to_select_tivm;
                    }
                    cur_to_select = cur_to_select.ParentObj as MpAvViewModelBase;
                }
                if (string.IsNullOrEmpty(to_sel_ann_guid) ||
                    Parent == null ||
                    Parent.CopyItemType != MpCopyItemType.Image ||
                    Parent.GetContentView() is not MpAvContentWebView wv) {
                    return;
                }
                while (!Parent.IsEditorLoaded) {
                    await Task.Delay(100);
                }
                var req = new MpQuillAnnotationSelectedMessage() {
                    annotationGuid = to_sel_ann_guid
                };
                wv.ExecuteJavascript($"annotationSelected_ext('{req.SerializeJsonObjectToBase64()}')");
            });

        public MpIAsyncCommand ShowMostRecentRuntimeTransactionCommand => new MpAsyncCommand(
            async () => {
                if (Parent == null || Parent.CopyItemType != MpCopyItemType.Image ||
                    SortedMessages.OfType<MpAvParameterRequestMessageViewModel>().FirstOrDefault() is not { } prmvm) {
                    return;
                }
                await ApplyTransactionAsync(prmvm.Parent, true);
            });

        public MpIAsyncCommand HideTransactionsCommand => new MpAsyncCommand(
            async () => {
                if (Parent == null ||
                    Parent.CopyItemType != MpCopyItemType.Image ||
                    Parent.GetContentView() is not MpAvContentWebView wv) {
                    return;
                }
                while (!Parent.IsEditorLoaded) {
                    await Task.Delay(100);
                }
                wv.ExecuteJavascript($"hideAnnotations_ext()");
            });
        #endregion
    }
}
