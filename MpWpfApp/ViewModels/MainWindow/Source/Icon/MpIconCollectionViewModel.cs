using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpIconCollectionViewModel : MpViewModelBase, MpISingletonViewModel<MpIconCollectionViewModel> {
        #region Properties

        #region View Models

        public ObservableCollection<MpIconViewModel> IconViewModels { get; set; } = new ObservableCollection<MpIconViewModel>();
        #endregion

        #endregion

        #region Constructors

        private static MpIconCollectionViewModel _instance;
        public static MpIconCollectionViewModel Instance => _instance ?? (_instance = new MpIconCollectionViewModel());


        public MpIconCollectionViewModel() : base(null) {
            MpHelpers.RunOnMainThreadAsync(Init);
        }

        public async Task Init() {
            IsBusy = true;

            IconViewModels.Clear();
            var il = await MpDb.GetItemsAsync<MpIcon>();
            foreach(var i in il) {
                var ivm = await CreateIconViewModel(i);
                IconViewModels.Add(ivm);
            }
            OnPropertyChanged(nameof(IconViewModels));

            IsBusy = false;
        }

        #endregion

        #region Public Methods

        public async Task<MpIconViewModel> CreateIconViewModel(MpIcon i) {
            var ivm = new MpIconViewModel(this);
            await ivm.InitializeAsync(i);
            return ivm;
        }

        #endregion

        #region Protected Methods

        protected override async void Instance_OnItemAdded(object sender, MpDbModelBase e) {
            if(e is MpIcon i) {
                var ivm = await CreateIconViewModel(i);
                IconViewModels.Add(ivm);
            }
        }

        protected override async void Instance_OnItemUpdated(object sender, MpDbModelBase e) {
            if (e is MpIcon i) {
                var ivm = IconViewModels.FirstOrDefault(x => x.IconId == i.Id);
                await ivm.InitializeAsync(i);
            }
        }

        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpIcon i) {
                var ivm = IconViewModels.FirstOrDefault(x => x.IconId == i.Id);
                IconViewModels.Remove(ivm);
                OnPropertyChanged(nameof(IconViewModels));
            }
        }

        #endregion

        #region Private Methods


        #endregion
    }
}
