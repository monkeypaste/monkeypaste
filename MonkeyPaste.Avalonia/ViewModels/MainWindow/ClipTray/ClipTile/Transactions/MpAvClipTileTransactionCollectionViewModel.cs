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
        MpViewModelBase<MpAvClipTileViewModel>, MpIContextMenuViewModel {
        #region Private Variables

        #endregion

        #region Statics
        #endregion

        #region Interfaces

        #region MpIContextMenuItemViewModel Implementation

        public MpMenuItemViewModel ContextMenuViewModel {
            get {
                //if (SelectedItem == null) {
                //    return new MpMenuItemViewModel();
                //}
                return new MpMenuItemViewModel() {
                    Header = "Transactions",
                    IconResourceKey = "EggImage",
                    SubItems = SortedMessages.Select(x => x.ContextMenuItemViewModel).ToList()
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
        public MpAvTransactionItemViewModel SelectedTransaction { get; set; }

        public MpAvTransactionItemViewModel MostRecentTransaction =>
            Transactions.OrderByDescending(x => x.TransactionDateTime).FirstOrDefault();

        public IEnumerable<MpAvTransactionMessageViewModelBase> Messages =>
            Transactions.SelectMany(x => x.Messages).ToList();

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
        public double BoundWidth { get; set; }
        public double BoundHeight { get; set; }

        public double ObservedWidth { get; set; }
        public double ObservedHeight { get; set; }

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
        public bool IsAnyBusy => IsBusy || Transactions.Any(x => x.IsBusy);

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
                    var cisvm = await CreateTransactionItemViewModelAsync(cit);
                    while (cisvm.IsAnyBusy) {
                        await Task.Delay(100);
                    }

                    DoShake = true;

                    ApplyTransactionAsync(cisvm).FireAndForgetSafeAsync(this);

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

        private async Task ApplyTransactionAsync(MpAvTransactionItemViewModel tivm) {
            if (tivm == null) {
                return;
            }

            bool is_new = false;
            if (Transactions.All(x => x.TransactionId != tivm.TransactionId)) {
                Transactions.Add(tivm);
                is_new = true;
            }
            Transactions.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));

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

            while (!Parent.IsEditorLoaded) {
                await Task.Delay(100);
            }

            if (Parent.GetContentView() is MpIContentView cv) {
                MpJsonObject updateObj = null;
                updateObj = tivm.GetTransactionDelta();
                if (updateObj == null) {
                    updateObj = tivm.GetTransactionAnnotation();
                }
                if (updateObj == null) {
                    return;
                }
                bool success = await cv.UpdateContentAsync(updateObj);
                if (success) {
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
        }

        private void Transactions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(Transactions));
            OnPropertyChanged(nameof(SortedTransactions));
            OnPropertyChanged(nameof(IsAnyAnalysisTransaction));
        }
        private void SetTransactionViewGridLength(GridLength gl) {
            if (Parent.GetContentView() is Control Control &&
                    Control.GetVisualAncestor<MpAvClipTileView>() is MpAvClipTileView ctv &&
                    ctv.FindControl<Grid>("TileGrid") is Grid tileGrid) {
                // setting all column 1 view to IsVisible=false doesn't decrease 
                // the column's grid length so all other tileGrid views (column 0) 
                // are <transaction width> less than total width, so this reset column 1 width
                var trans_gl = tileGrid.ColumnDefinitions[1];
                trans_gl.Width = gl;// new GridLength(0, GridUnitType.Auto);
            }
        }

        #endregion

        #region Commands

        public ICommand FilterByPrimarySourceCommand => new MpCommand(
            () => {

            });
        public MpIAsyncCommand<object> OpenTransactionPaneCommand => new MpAsyncCommand<object>(
            async (args) => {
                Dispatcher.UIThread.VerifyAccess();

                IsTransactionPaneOpen = true;

                OnPropertyChanged(nameof(MaxWidth));
                //BoundWidth = DefaultTransactionPanelLength;
                //BoundHeight = Parent.BoundHeight;
                SetTransactionViewGridLength(new GridLength(DefaultTransactionPanelWidth, GridUnitType.Auto));

                IsTransactionPaneAnimating = true;
                double nw = Parent.BoundWidth + DefaultTransactionPanelWidth;
                double nh = Parent.BoundHeight;

                MpAvResizeExtension.ResizeAnimated(
                        Parent.GetContentView() as Control,
                        nw, nh,
                        () => {
                            IsTransactionPaneAnimating = false;
                        });

                while (IsTransactionPaneAnimating) {
                    await Task.Delay(100);
                }

                // only wait for executeAsync calls
                return;
            }, (args) => {
                return Parent != null && !IsTransactionPaneOpen;
            });

        public MpIAsyncCommand CloseTransactionPaneCommand => new MpAsyncCommand(
            async () => {
                Dispatcher.UIThread.VerifyAccess();
                IsTransactionPaneOpen = false;

                SetTransactionViewGridLength(new GridLength(0, GridUnitType.Auto));
                IsTransactionPaneAnimating = true;
                double nw = Parent.Parent.DefaultQueryItemWidth;
                double nh = Parent.Parent.DefaultQueryItemHeight;

                MpAvResizeExtension.ResizeAnimated(
                        Parent.GetContentView() as Control,
                        nw, nh,
                        () => {
                            IsTransactionPaneAnimating = false;
                        });
                while (IsTransactionPaneAnimating) {
                    await Task.Delay(100);
                }
                // only wait if executed async for timing
                return;
            }, () => {
                return Parent != null && IsTransactionPaneOpen;
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

        public MpIAsyncCommand<object> SelectChildCommand => new MpAsyncCommand<object>(
            async (args) => {
                MpAvITransactionNodeViewModel to_select_tnvm = null;

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
                    MpDebug.Assert(args == null, $"Unhandled transaction.select child arg of type '{args?.GetType()}' What type is it??");
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

                await OpenTransactionPaneCommand.ExecuteAsync(null);

                if (to_select_tnvm is MpViewModelBase to_select_vmb) {
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
                        cur_to_select = cur_to_select.ParentObj as MpViewModelBase;
                    }
                }

            }) {

        };

        #endregion
    }
}
