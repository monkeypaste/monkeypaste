using Avalonia.Threading;
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

        public ObservableCollection<MpAvTransactionItemViewModelBase> Transactions { get; set; } = new ObservableCollection<MpAvTransactionItemViewModelBase>();

        public ObservableCollection<MpITransactionNodeViewModel> SelectedItems { get; set; } = new ObservableCollection<MpITransactionNodeViewModel>();
        public IEnumerable<MpAvTransactionMessageViewModelBase> Messages =>
            Transactions.SelectMany(x => x.Items);
            
        //public IEnumerable<MpAvTransactionItemViewModelBase> Transactions =>
        //    Transactions.Where(x => x is MpAvTransactionItemViewModelBase).Cast<MpAvTransactionItemViewModelBase>();

        public IEnumerable<MpAvTransactionMessageViewModelBase> SortedMessages =>
            Messages
            .OrderByDescending(x => x.ComparableSortValue);
        //.OrderByDescending(x => x.SourcePriority)
        //.ThenByDescending(x=>x.SourceCreatedDateTime);

        public MpITransactionNodeViewModel PrimaryItem => 
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
                    SubItems = SortedMessages.Select(x => x.ContextMenuItemViewModel).ToList()
                };
            }
        }

        public bool IsContextMenuOpen { get; set; }
        #endregion

        #region Layout

        public double DefaultTransactionPanelLength => 150;
        public double BoundWidth { get; set; }
        public double BoundHeight { get; set; }
        
        public double ObservedWidth { get; set; }
        public double ObservedHeight { get; set; }
        #endregion

        #region State

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

            Transactions.Clear();

            var ci_transactions = await MpDataModelProvider.GetCopyItemTransactionsByCopyItemIdAsync(copyItemId);
            foreach (var cit in ci_transactions) {
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
                    OnPropertyChanged(nameof(SortedMessages));
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
        public ICommand OpenTransactionPaneCommand => new MpCommand(
            () => {
                IsTransactionPaneOpen = true;
                BoundWidth = DefaultTransactionPanelLength;
                BoundHeight = Parent.BoundHeight;

                //double nw = Parent.BoundWidth + DefaultTransactionPanelLength;
                //double nh = Parent.BoundHeight;
                //Dispatcher.UIThread.Post(() => {
                //    MpAvResizeExtension.ResizeAnimated(
                //        Parent.GetDragSource() as MpAvCefNetWebView, nw, nh);
                //});
            }, () => {
                return Parent != null && !IsTransactionPaneOpen;
            });

        public ICommand CloseTransactionPaneCommand => new MpCommand(
            () => {
                
                IsTransactionPaneOpen = false;
                Parent.BoundWidth -= ObservedWidth;
                BoundWidth = 0;
                //double nw = Parent.BoundWidth - BoundWidth;
                //double nh = Parent.BoundHeight;
                //Dispatcher.UIThread.Post(() => {
                //    MpAvResizeExtension.ResizeAnimated(
                //        Parent.GetDragSource() as MpAvCefNetWebView, nw, nh);
                //});
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
        #endregion
    }
}
