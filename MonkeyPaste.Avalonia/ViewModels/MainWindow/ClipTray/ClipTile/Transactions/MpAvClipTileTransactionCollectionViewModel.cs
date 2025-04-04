﻿using Avalonia.Controls;
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
                        //.Where(x => !(x.SourceType == MpTransactionSourceType.App && x.SourceObjId == MpDefaultDataModelTools.ThisAppId) && x.ContextMenuItemViewModel != null)
                        .Where(x => x.ContextMenuItemViewModel != null)
                        .Select(x => x.SourceRef)
                        .OrderBy(x => SortedTransactions.IndexOf(SortedTransactions.FirstOrDefault(y => y.HasSource(x))))
                        .ToList();

                var source_ref_and_mi_tups = new List<(MpISourceRef, MpAvMenuItemViewModel)>();
                // first create flat list of sources and their default sub-items
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
                    source_ref_and_mi_tups.Add((source_ref, source_mi));
                }

                var grouped_tups = new List<(MpISourceRef, MpAvMenuItemViewModel)>();
                if (source_ref_and_mi_tups.Any()) {
                    // now group urls into apps and presets results into presets and presets into analyzers

                    foreach (var tup in source_ref_and_mi_tups) {
                        if (tup.Item1 is MpApp) {
                            // create base level apps
                            grouped_tups.Add(tup);
                        }
                        if (tup.Item1 is MpCopyItem) {
                            // NOTE hiding clip sources cause its confusing, they can be a source OR this is a source for them
                            // and the transaction log maybe messing up the transaction type for BOTH these cases
                            // so can't clearly know the relationship and as is it maybe more confusing than helpful to show.
                            // As long as analysis source is there thats the pertinent info really...

                            // create base level clips (clips can be a source or THIS clip is a source to the clip)
                            //if (AllSources.FirstOrDefault(x => x.SourceRef == tup.Item1) is { } ci_source_vm) {
                            //}
                            //grouped_tups.Add(tup);
                        }
                        if (tup.Item1 is MpPreset preset &&
                            MpAvAnalyticItemCollectionViewModel.Instance.AllPresets.FirstOrDefault(x => x.AnalyticItemPresetId == preset.Id) is { } aipvm) {

                            if (grouped_tups.FirstOrDefault(x => x.Item1.SourceType == MpTransactionSourceType.AnalyzerPreset && aipvm.Parent.Items.Any(y => y.AnalyticItemPresetId == preset.Id))
                                is { } analyzer_group_tup &&
                                analyzer_group_tup.Item2 is MpAvMenuItemViewModel preset_analyzer_mi &&
                                preset_analyzer_mi.SubItems.ToList() is { } analyzer_subitems) {
                                // analyzer group already exists

                                // add preset to analyzer
                                analyzer_subitems.Insert(0, tup.Item2);
                                preset_analyzer_mi.SubItems = analyzer_subitems;
                            } else {
                                // this presets analyzer not added yet

                                // add url to domain
                                List<MpAvMenuItemViewModel> analyzer_sub_items = [tup.Item2];

                                // create domain item w/ url sub item and reject after
                                var analyzer_mi = new MpAvMenuItemViewModel() {
                                    IconSourceObj = aipvm.Parent.IconId,
                                    Header = aipvm.Parent.Title,
                                    SubItems = analyzer_sub_items
                                };
                                grouped_tups.Add((preset, analyzer_mi));
                            }
                        }
                    }
                    foreach (var tup in source_ref_and_mi_tups) {
                        if (tup.Item1 is MpUrl url) {
                            if (grouped_tups.FirstOrDefault(x => url.AppId == x.Item1.SourceObjId && x.Item1.SourceType == MpTransactionSourceType.App)
                                    is { } url_app_tup) {
                                // found this url's app

                                if (url_app_tup.Item2.SubItems.FirstOrDefault(x => x.Header == url.UrlDomainPath) is MpAvMenuItemViewModel url_domain_mi &&
                                    url_domain_mi.SubItems.ToList() is { } url_domain_subitems) {
                                    // found this urls domain sub-item
                                    // add url to domain
                                    url_domain_subitems.Insert(0, tup.Item2);
                                    url_domain_mi.SubItems = url_domain_subitems;
                                } else {
                                    // this domain not added to app yet

                                    // add url to domain
                                    List<MpAvMenuItemViewModel> domain_sub_items = [tup.Item2];

                                    // add domain reject thing (if not top level)
                                    if (!MpUrlHelpers.IsUrlTopLevel(url.UrlPath) &&
                                        MpAvUrlCollectionViewModel.Instance.CanRejectUrls &&
                                        MpAvUrlCollectionViewModel.Instance.Items.FirstOrDefault(x => x.UrlId == url.Id) is { } uvm) {

                                        // TOGGLE REJECT SOURCE DOMAIN

                                        domain_sub_items.Add(
                                            new MpAvMenuItemViewModel() {
                                                HasLeadingSeparator = true,
                                                Header = $"{(url.IsDomainRejected ? UiStrings.SourceUnblockLabel : UiStrings.SourceBlockLabel)} {UiStrings.SourceDomainLabel} '{url.UrlDomainPath}'",
                                                AltNavIdx = 0,
                                                IconResourceKey = url.IsDomainRejected ? "AddGreenImage" : "NoEntryImage",
                                                Command = uvm.ToggleIsDomainRejectedCommand
                                            });
                                    }
                                    // create domain item w/ url sub item and reject after
                                    var domain_mi = new MpAvMenuItemViewModel() {
                                        IconSourceObj = url.IconId,
                                        Header = url.UrlDomainPath,
                                        SubItems = domain_sub_items
                                    };

                                    if (url_app_tup.Item2 is MpAvMenuItemViewModel url_app_mivm &&
                                        url_app_mivm.SubItems.ToList() is { } url_app_subitems) {
                                        // add domain to app 
                                        url_app_subitems.Insert(0, domain_mi);
                                        url_app_mivm.SubItems = url_app_subitems;
                                    }
                                }
                            }
                        }
                    }


                } else {
                    // no transactions to show, something must be wrong
                    // to avoid weird menu layout show stub item
                    grouped_tups.Add(
                        (null, new MpAvMenuItemViewModel() {
                            IconResourceKey = "QuestionMarkImage",
                            Header = UiStrings.ClipTileTransactionErrorText
                        }));
                }
                return new MpAvMenuItemViewModel() {
                    Header = UiStrings.ClipTileSourcesHeader,
                    HasLeadingSeparator = true,
                    IconResourceKey = "EggImage",
                    SubItems = grouped_tups.Select(x => x.Item2).ToList()
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
        int LastPopulatedHostId { get; set; }

        private bool _isPopulating;
        public bool IsPopulating {
            get {
                bool was_populating = _isPopulating;
                _isPopulating = CopyItemId != 0 && CopyItemId != LastPopulatedHostId;
                if (was_populating != _isPopulating) {
                    OnPropertyChanged(nameof(IsPopulating));
                }
                return _isPopulating;
            }
        }
        public bool IsPlainTextView { get; set; } = false;
        public bool DoShake { get; set; }
        public bool IsSortDescending { get; set; } = true;
        public bool IsTransactionPaneOpen { get; set; } = false;
        public bool IsTransactionPaneAnimating { get; set; }
        public bool IsAnyBusy =>
            IsBusy || Transactions.Any(x => x.IsBusy);

        public bool IsAnyAnalysisTransaction =>
            Transactions.Any(x => x.IsAnalysisTransaction);

        public int LastTabIdx { get; set; } = (int)MpTransactionTabType.Response;

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

            if (Parent != null &&
                Parent.CopyItemId > 0 &&
                Parent.CopyItemIconId == 0 &&
                CreateTransaction != null &&
                CreateTransaction.IconSourceObj is int ciid_icon_id) {
                // this is a fallback check for new/buggy passive ci source gathering, should figure out cases when this happens and fix/remove this at some point...
                MpConsole.WriteLine($"{Parent} used transaction fallback icon set to {ciid_icon_id}");
                Parent.CopyItemIconId = ciid_icon_id;

            }
            if (Transactions.Any()) {
                LastPopulatedHostId = copyItemId;
            } else {
                // must be a new tile and waiting for gather sources to report back
            }

            OnPropertyChanged(nameof(IsPopulating)); // triggers timer for sources to be gathered or fallback after timeout

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
        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            base.Instance_OnItemDeleted(sender, e);
            if (e is MpCopyItemTransaction cit &&
                cit.CopyItemId == CopyItemId &&
                Transactions.FirstOrDefault(x => x.TransactionId == cit.Id) is { } trvm) {
                Dispatcher.UIThread.Post(() => {
                    Transactions.Remove(trvm);
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
                case nameof(IsPopulating):
                    if (IsPopulating) {
                        Dispatcher.UIThread.Post(async () => {
                            var sw = Stopwatch.StartNew();
                            while (true) {
                                if (!IsPopulating) {
                                    return;
                                }
                                if (sw.Elapsed < TimeSpan.FromSeconds(7)) {
                                    await Task.Delay(100);
                                    continue;
                                }
                                break;
                            }
                            LastPopulatedHostId = CopyItemId;
                            OnPropertyChanged(nameof(IsPopulating));
                        });
                    }
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
            if (tivm.GetTransactionDelta() is not object updateObj) {
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
                await Task.Delay(1);

                IsTransactionPaneOpen = false;
                Parent.IsSubSelectionEnabled = false;
                SetTransactionViewGridLength(new GridLength(0, GridUnitType.Auto));
            }, () => {
                return Parent != null && IsTransactionPaneOpen;
            });

        public MpIAsyncCommand<object> OpenTransactionPaneCommand => new MpAsyncCommand<object>(
            async (args) => {
                Dispatcher.UIThread.VerifyAccess();
                if(MpAvThemeViewModel.Instance.IsMobileOrWindowed &&
                    MpAvMainWindowViewModel.Instance.IsVerticalOrientation) {
                    // TODO add vertical trans panel layout, just force horizontal for now
                    MpAvMainWindowViewModel.Instance.CycleOrientationCommand.Execute("CW");
                }

                await Task.Delay(1);
                IsTransactionPaneOpen = true;
                SetTransactionViewGridLength(new GridLength(0.6, GridUnitType.Star));
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
                    to_sel_ann_guid = guidArg;

                    if (!string.IsNullOrEmpty(to_sel_ann_guid) &&
                    isDblClick && !OpenTransactionPaneCommand.CanExecute(null)) {
                        // trans panel not open and user dbl clicked an ann
                        MpAvClipTrayViewModel.Instance.SelectClipTileTransactionNodeCommand.Execute(
                            new object[] { Parent.CopyItemId, to_sel_ann_guid });
                        return;
                    }
                }

                if (args is string argStr && argStr.IsStringGuid()) {
                    to_sel_ann_guid = argStr;
                } else if (args is MpAvITransactionNodeViewModel tnvm) {
                    to_select_tnvm = tnvm;
                } else {
                    //MpDebug.Assert(to_select_tnvm != null, $"Unhandled transaction.select child arg of type '{args?.GetType()}' What type is it??", silent: true);
                }


                if (to_select_tnvm == null) {
                    if (!string.IsNullOrEmpty(to_sel_ann_guid)) {
                        // this only comes from editor for ann atm
                        to_select_tnvm =
                            Messages
                                .OfType<MpAvAnnotationMessageViewModel>()
                                .SelectMany(x => x.RootAnnotationViewModel.SelfAndAllDescendants().Cast<MpAvAnnotationItemViewModel>())
                                .FirstOrDefault(x => x.AnnotationGuid == to_sel_ann_guid);
                    }

                    if (SelectedTransaction == null) {
                        // only fallback to default select if nothing already selected
                        to_select_tnvm = MostRecentTransaction;
                    }
                }
                if (to_select_tnvm == null) {
                    // ignore for whateber reason
                    MpConsole.WriteLine($"No child trans node to select with arg '{args}'");
                    return;
                }
                if (string.IsNullOrEmpty(to_sel_ann_guid)) {
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
                    //if (cur_to_select is MpISelectableViewModel svm) {
                    //    svm.IsSelected = true;

                    //    MpDebug.Assert(svm.IsSelected, $"{svm} can't be selected directly, fix property set/change handlers");
                    //} else {
                    //    MpDebug.Break("what kinda node is it?");
                    //}
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
                wv.SendMessage($"annotationSelected_ext('{req.SerializeObjectToBase64()}')");
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
                wv.SendMessage($"hideAnnotations_ext()");
            });
        #endregion
    }
}
