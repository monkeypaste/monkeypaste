using Avalonia.Threading;
using System;
using System.Collections.Generic;
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
        MpAvMultiSelectorViewModelBase<MpAvClipTileViewModel, MpITransactionNodeViewModel> {
        #region Private Variables

        #endregion

        #region Statics

        #endregion

        #region Properties

        #region View Models

        public IEnumerable<MpAvTransactionItemViewModelBase> Transactions =>
            Items.Where(x => x is MpAvTransactionItemViewModelBase).Cast<MpAvTransactionItemViewModelBase>();

        public IEnumerable<MpITransactionNodeViewModel> SortedItems =>
            Items
            .OrderByDescending(x => x.ComparableSortValue);
        //.OrderByDescending(x => x.SourcePriority)
        //.ThenByDescending(x=>x.SourceCreatedDateTime);

        public override MpITransactionNodeViewModel PrimaryItem => 
            Transactions.OrderBy(x => x.TransactionDateTimeUtc).FirstOrDefault();

        #region MpIContextMenuItemViewModel Implementation

        public MpMenuItemViewModel ContextMenuViewModel {
            get {
                //if (SelectedItem == null) {
                //    return new MpMenuItemViewModel();
                //}
                return new MpMenuItemViewModel() {
                    Header = "Transactions",
                    IconResourceKey = "EggImage",
                    SubItems = SortedItems.Select(x => x.ContextMenuItemViewModel).ToList()
                };
            }
        }

        public bool IsContextMenuOpen { get; set; }
        #endregion

        #region State

        public bool IsTransactionPaneOpen { get; set; } = false;
        public bool IsAnyBusy => IsBusy || Items.Any(x => x.IsBusy);

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

        #endregion

        #region Constructors

        public MpAvClipTileTransactionCollectionViewModel() : this(null) { }

        public MpAvClipTileTransactionCollectionViewModel(MpAvClipTileViewModel parent) : base(parent) {
            PropertyChanged += MpAvClipTileSourceCollectionViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods

        public async Task InitializeAsync(int copyItemId) {
            IsBusy = true;

            Items.Clear();

            var ci_transactions = await MpDataModelProvider.GetCopyItemTransactionsByCopyItemIdAsync(copyItemId);
            foreach (var cit in ci_transactions) {
                var cisvm = await CreateClipTileSourceViewModel(cit);
                Items.Add(cisvm);
            }

            while (Items.Any(x=>x.IsAnyBusy)) {
                await Task.Delay(100);
            }
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(PrimaryItem));
            OnPropertyChanged(nameof(SortedItems));

            IsBusy = false;
        }
        #endregion

        #region Protected Methods

        #region Db Op Overrides

        protected override void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if(e is MpCopyItemTransaction cit && cit.CopyItemId == CopyItemId) {
                Dispatcher.UIThread.Post(async () => {
                    var cisvm = await CreateClipTileSourceViewModel(cit);
                    Items.Add(cisvm);
                    OnPropertyChanged(nameof(SortedItems));
                });
            }
        }
        #endregion

        #endregion

        #region Private Methods

        private async Task<MpAvTransactionItemViewModelBase> CreateClipTileSourceViewModel(MpCopyItemTransaction cit) {
            var cisvm = new MpAvTransactionItemViewModelBase(this);
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
            }
        }

        #endregion

        #region Commands

        public ICommand FilterByPrimarySourceCommand => new MpCommand(
            () => {

            });

        #endregion
    }
}
