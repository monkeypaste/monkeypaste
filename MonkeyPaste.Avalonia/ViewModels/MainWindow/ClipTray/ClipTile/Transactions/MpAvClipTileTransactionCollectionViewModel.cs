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
        MpILabelTextViewModel, 
        MpISortableViewModel, 
        MpIHasIconSourceObjViewModel,
        MpIMenuItemViewModel,
        MpIAsyncCollectionObject {
        object TransactionModel { get; }
    }

    public class MpAvClipTileTransactionCollectionViewModel : 
        MpAvMultiSelectorViewModelBase<MpAvClipTileViewModel, MpITransactionNodeViewModel> {
        #region Private Variables

        #endregion

        #region Statics

        #endregion

        #region Properties

        #region View Models

        public IEnumerable<MpITransactionNodeViewModel> SortedItems =>
            Items
            .OrderByDescending(x => x.ComparableSortValue);
            //.OrderByDescending(x => x.SourcePriority)
            //.ThenByDescending(x=>x.SourceCreatedDateTime);

        public override MpITransactionNodeViewModel PrimaryItem => SortedItems.FirstOrDefault();

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

        public bool IsAnyBusy => IsBusy || Items.Any(x => x.IsBusy);

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
            var ci_sources = await MpDataModelProvider.GetCopyItemSources(copyItemId);
            foreach(var cis in ci_sources) {
                var source_ref = await GetSourceRefAsync(cis);
                var cisvm = await CreateClipTileSourceViewModel(source_ref);
                Items.Add(cisvm);
            }

            var ci_transactions = await MpDataModelProvider.GetCopyItemTransactionsByCopyItemIdAsync(copyItemId);
            foreach (var cit in ci_transactions) {
                var source_ref = await GetSourceRefAsync(cit);
                var cisvm = await CreateClipTileSourceViewModel(source_ref, cit);
                Items.Add(cisvm);
            }

            while (Items.Any(x=>x.IsBusy)) {
                await Task.Delay(100);
            }
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(PrimaryItem));

            IsBusy = false;
        }
        #endregion

        #region Private Methods

        private async Task<MpAvClipTileSourceViewModel> CreateClipTileSourceViewModel(MpISourceRef sourceRef, MpCopyItemTransaction cit = null) {
            var cisvm = new MpAvClipTileSourceViewModel(this);
            await cisvm.InitializeAsync(sourceRef,cit);
            return cisvm;
        }
        
        private async Task<MpISourceRef> GetSourceRefAsync(MpDbModelBase dbmb) {
            if(dbmb is MpCopyItemSource cis) {
                switch (cis.CopyItemSourceType) {
                    case MpCopyItemSourceType.App:
                        var avm = MpAvAppCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AppId == cis.SourceObjId);
                        if (avm == null) {
                            // where/what is it?
                            Debugger.Break();
                            break;
                        }
                        return avm.App;
                    case MpCopyItemSourceType.Url:
                        var uvm = MpAvUrlCollectionViewModel.Instance.Items.FirstOrDefault(x => x.UrlId == cis.SourceObjId);
                        if (uvm == null) {
                            // where/what is it?
                            Debugger.Break();
                            break;
                        }
                        return uvm.Url;
                    case MpCopyItemSourceType.CopyItem:
                        var civm = MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.CopyItemId == cis.SourceObjId);
                        if (civm != null) {
                            return civm.CopyItem;
                        }
                        break;
                }
            }
            if(dbmb is MpCopyItemTransaction cit) {
                var source_ref = await MpDataModelProvider.GetSourceRefByCopyItemTransactionIdAsync(cit.Id);
                return source_ref;
            }
            return null;
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
