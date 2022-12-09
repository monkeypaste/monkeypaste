using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvClipTileSourceCollectionViewModel : 
        MpAvSelectorViewModelBase<MpAvClipTileViewModel, MpAvClipTileSourceViewModel>,
        MpIContextMenuViewModel {
        #region Private Variables

        #endregion

        #region Statics

        #endregion

        #region Properties

        #region View Models

        public IEnumerable<MpAvClipTileSourceViewModel> SortedItems => Items.OrderBy(x => x.SourceCreatedDateTime);

        public MpAvClipTileSourceViewModel PrimaryItem => SortedItems.FirstOrDefault();

        #region MpIContextMenuItemViewModel Implementation

        public MpMenuItemViewModel ContextMenuViewModel {
            get {
                if (SelectedItem == null) {
                    return new MpMenuItemViewModel();
                }
                return new MpMenuItemViewModel() {
                    Header = "Sources",
                    IconResourceKey = "EggImage",
                    SubItems = SortedItems.Select(x => x.ContextMenuViewModel).ToList()
                };
            }
        }

        #endregion

        #region State

        public bool IsAnyBusy => IsBusy || Items.Any(x => x.IsBusy);

        #endregion

        #endregion

        #endregion

        #region Constructors

        public MpAvClipTileSourceCollectionViewModel() : this(null) { }

        public MpAvClipTileSourceCollectionViewModel(MpAvClipTileViewModel parent) : base(parent) {
            PropertyChanged += MpAvClipTileSourceCollectionViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods

        public async Task InitializeAsync(int copyItemId) {
            IsBusy = true;

            Items.Clear();
            var cisl = await MpDataModelProvider.GetCopyItemSources(copyItemId);
            foreach(var cis in cisl) {
                var cisvm = await CreateClipTileSourceViewModel(cis);
                Items.Add(cisvm);
            }
            while(Items.Any(x=>x.IsBusy)) {
                await Task.Delay(100);
            }
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(PrimaryItem));

            IsBusy = false;
        }
        #endregion

        #region Private Methods

        private async Task<MpAvClipTileSourceViewModel> CreateClipTileSourceViewModel(MpCopyItemSource cis) {
            var cisvm = new MpAvClipTileSourceViewModel(this);
            await cisvm.InitializeAsync(cis);
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
