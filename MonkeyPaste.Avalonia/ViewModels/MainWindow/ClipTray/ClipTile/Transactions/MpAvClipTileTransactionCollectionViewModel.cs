using Avalonia.Controls;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonoMac.OpenGL;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public interface MpITransactionNodeViewModel : 
        MpITreeItemViewModel, 
        MpISelectableViewModel, 
        MpIHoverableViewModel,
        MpILabelTextViewModel, 
        MpISortableViewModel, 
        MpIHasIconSourceObjViewModel,
        MpIMenuItemViewModel,
        MpIAsyncCollectionObject {
        object TransactionModel { get; }
        object Body { get; }
    }

    public class MpAvClipTileTransactionCollectionViewModel : 
        MpViewModelBase<MpAvClipTileViewModel> {
        #region Private Variables

        #endregion

        #region Statics
        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpAvTransactionItemViewModel> Transactions { get; set; } = new ObservableCollection<MpAvTransactionItemViewModel>();
        public IEnumerable<MpAvTransactionItemViewModel> SortedTransactions =>
            IsSortDescending ?
                Transactions.OrderByDescending(x => x.TransactionDateTime) :
                Transactions.OrderBy(x => x.TransactionDateTime);
        public MpAvTransactionItemViewModel SelectedTransaction { get; set; }

        public MpAvTransactionItemViewModel MostRecentTransaction =>
            Transactions.OrderByDescending(x => x.TransactionDateTime).FirstOrDefault();

        public IEnumerable<MpAvTransactionMessageViewModelBase> Messages =>
            Transactions.SelectMany(x => x.Items);

        public IEnumerable<MpAvTransactionMessageViewModelBase> SortedMessages =>
            Messages
            .OrderByDescending(x => x.ComparableSortValue);

        public MpITransactionNodeViewModel PrimaryItem => 
            Transactions.OrderBy(x => x.TransactionDateTime).FirstOrDefault();

        #endregion

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

        #region Layout

        public double DefaultTransactionPanelWidth {
            get {
                return MpAvClipTrayViewModel.Instance.DefaultItemWidth * 0.5;
            }
        }
        public double DefaultTransactionPanelHeight {
            get {
                return MpAvClipTrayViewModel.Instance.DefaultItemHeight * 0.5;
            }
        }
        public double BoundWidth { get; set; }
        public double BoundHeight { get; set; }
        
        public double ObservedWidth { get; set; }
        public double ObservedHeight { get; set; }

        public double MaxWidth {
            get {
                if(Parent == null) {
                    return 0;
                }
                if(!IsTransactionPaneOpen) {
                    return 0;
                }
                return Parent.BoundWidth * 0.5;
            }
        }

        #endregion

        #region State

        public bool IsViewByTransaction { get; set; } = false;
        public bool IsSortDescending { get; set; } = true;
        public bool IsTransactionPaneOpen { get; set; } = false;
        public bool IsAnyBusy => IsBusy || Transactions.Any(x => x.IsBusy);

        #endregion

        #region Model

        public int CopyItemId {
            get {
                if(Parent == null) {
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
            foreach (var cit in ci_transactions.OrderByDescending(x=>x.TransactionDateTime)) {
                var cisvm = await CreateClipTileSourceViewModel(cit);
                Transactions.Add(cisvm);
            }

            while (Transactions.Any(x=>x.IsAnyBusy)) {
                await Task.Delay(100);
            }
            OnPropertyChanged(nameof(Transactions));
            OnPropertyChanged(nameof(PrimaryItem));
            OnPropertyChanged(nameof(SortedMessages));

            IsBusy = false;
        }
        #endregion

        #region Protected Methods

        #region Db Op Overrides

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if(e is MpCopyItemTransaction cit && cit.CopyItemId == CopyItemId) {
                Dispatcher.UIThread.Post(async () => {
                    var cisvm = await CreateClipTileSourceViewModel(cit);
                    Transactions.Add(cisvm);
                    if(cit.TransactionLabel == "Edit") {
                        // since source is editor content doesn't need up 
                        // TODO this should only be temporary, need a better way to 
                        // react/interpret transactions
                        return;
                    }
                    while(cisvm.IsAnyBusy) {
                        await Task.Delay(100);
                    }
                    OpenTransactionPaneCommand.Execute(null);
                    SelectedTransaction = cisvm;
                });
            }
        }
        #endregion

        #endregion

        #region Private Methods

        private async Task<MpAvTransactionItemViewModel> CreateClipTileSourceViewModel(MpCopyItemTransaction cit) {
            var cisvm = new MpAvTransactionItemViewModel(this);
            await cisvm.InitializeAsync(cit);
            return cisvm;
        }


        private void MpAvClipTileSourceCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsBusy):
                    if(Parent == null) {
                        break;
                    }
                    OnPropertyChanged(nameof(IsAnyBusy));
                    Parent.OnPropertyChanged(nameof(Parent.IsAnyBusy));
                    break;
                case nameof(IsTransactionPaneOpen):
                    if(Parent == null) {
                        break;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.IsTitleVisible));
                    break;
                case nameof(PrimaryItem):
                    if(Parent == null) {
                        break;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.IconResourceObj));
                    break;
                case nameof(SelectedTransaction):
                    ApplyTransactionAsync(SelectedTransaction).FireAndForgetSafeAsync(this);
                    break;
            }
        }

        private async Task ApplyTransactionAsync(MpAvTransactionItemViewModel tivm) {
            if(tivm == null || tivm.TransactionLabel == "Edit" || tivm.TransactionLabel == "Drop") {
                return;
            }

            if (Parent.GetDragSource() is MpAvCefNetWebView wv) {
                MpJsonObject updateObj = null;
                updateObj = tivm.GetTransactionDelta();
                if(updateObj == null) {
                    updateObj = tivm.GetTransactionAnnotation();
                }
                if(updateObj == null) {
                    return;
                }
                await wv.PerformUpdateContentRequestAsync(updateObj);
            }
        }

        private void Transactions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {
            OnPropertyChanged(nameof(Transactions));
            OnPropertyChanged(nameof(SortedTransactions));
        }
        private void SetTransactionViewGridLength(GridLength gl) {
            if (Parent.GetDragSource() is MpAvCefNetWebView wv &&
                    wv.GetVisualAncestor<MpAvClipTileView>() is MpAvClipTileView ctv &&
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
        public ICommand OpenTransactionPaneCommand => new MpCommand(
            () => {
                IsTransactionPaneOpen = true;
                OnPropertyChanged(nameof(MaxWidth));
                //BoundWidth = DefaultTransactionPanelLength;
                //BoundHeight = Parent.BoundHeight;
                SetTransactionViewGridLength(new GridLength(DefaultTransactionPanelWidth, GridUnitType.Auto));

                double nw = Parent.BoundWidth + DefaultTransactionPanelWidth;
                double nh = Parent.BoundHeight;
                Dispatcher.UIThread.Post(() => {
                    MpAvResizeExtension.ResizeAnimated(
                        Parent.GetDragSource() as MpAvCefNetWebView, 
                        nw, nh, 
                        () => {
                            if(Parent.IsPinned) {
                                return;
                            }
                            Parent.Parent.RefreshQueryTrayLayout();
                        });
                });
            }, () => {
                return Parent != null && !IsTransactionPaneOpen;
            });

        public ICommand CloseTransactionPaneCommand => new MpCommand(
            () => {
                IsTransactionPaneOpen = false;

                SetTransactionViewGridLength(new GridLength(0, GridUnitType.Auto));

                double nw = Parent.Parent.DefaultItemWidth;
                double nh = Parent.Parent.DefaultItemHeight;
                Dispatcher.UIThread.Post(() => {
                    MpAvResizeExtension.ResizeAnimated(
                        Parent.GetDragSource() as MpAvCefNetWebView, 
                        nw, nh,
                        () => {
                            if (Parent.IsPinned) {
                                return;
                            }
                            Parent.Parent.RefreshQueryTrayLayout();
                        });
                });
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
                if(args is MpAvTransactionItemViewModel) {
                    tivm = args as MpAvTransactionItemViewModel;
                }

                if(tivm == null) {
                    return;
                }
                int tivm_to_select_idx = -1;
                if(tivm == SelectedTransaction) {
                    tivm_to_select_idx = SortedTransactions.IndexOf(tivm);
                }
                Transactions.Remove(tivm);
                tivm.Transaction.DeleteFromDatabaseAsync().FireAndForgetSafeAsync(this);
                if(tivm_to_select_idx >= 0 && tivm_to_select_idx < Transactions.Count) {
                    SelectedTransaction = SortedTransactions.ElementAt(tivm_to_select_idx);
                }
            });

        public ICommand RemoveMostRecentTransactionCommand => new MpCommand(
            () => {
                RemoveTransactionCommand.Execute(MostRecentTransaction);
            });
        #endregion
    }
}
