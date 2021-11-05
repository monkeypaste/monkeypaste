using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpAnalyticItemCollectionViewModel : MpViewModelBase<MpContentItemViewModel> {

        #region Properties

        #region View Models

        public ObservableCollection<MpAnalyticItemViewModel> Items { get; private set; } = new ObservableCollection<MpAnalyticItemViewModel>();

        public MpAnalyticItemViewModel SelectedItem => Items.Where(x => x.IsSelected).FirstOrDefault();

        //public MpClipTileViewModel HostClipTileViewModel { get; set; }

        #endregion

        #region State

        public bool IsLoaded => Items.Count > 0;

        #endregion

        #endregion

        #region Constructors

        public MpAnalyticItemCollectionViewModel() : base(null) { }

        public MpAnalyticItemCollectionViewModel(MpContentItemViewModel parent) : base(parent) {        }

        #endregion

        #region Public Methods

        public async Task Init() {
            await InitDefaultItems();
        }

        public async Task<MpAnalyticItemViewModel> CreateAnalyticItemViewModel(MpAnalyticItem ai) {
            //var naivm = new MpAnalyticItemViewModel(this);
            //await naivm.InitializeAsync(ai);
            //return naivm;
            await Task.Delay(5);
            return null;
        }
        #endregion

        #region Private Methods

        private async Task InitDefaultItems() {
            IsBusy = true;

            Items.Clear();

            var translateVm = new MpTranslatorViewModel(this, 1);
            await translateVm.Initialize();
            Items.Add(translateVm);

            OnPropertyChanged(nameof(Items));

            IsBusy = false;
        }

        #endregion
    }
}
