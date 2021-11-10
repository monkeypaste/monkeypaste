using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpAnalyticItemCollectionViewModel : MpSingletonViewModel<MpAnalyticItemCollectionViewModel,object> { //MpViewModelBase<MpContentItemViewModel> { 

        #region Properties

        #region View Models

        public ObservableCollection<MpAnalyticItemViewModel> Items { get; private set; } = new ObservableCollection<MpAnalyticItemViewModel>();

        public MpAnalyticItemViewModel SelectedItem => Items.FirstOrDefault(x => x.IsSelected);

        public MpClipTileViewModel HostClipTileViewModel { get; set; }

        #endregion

        #region Layout

        public double UnexpandedHeight { get; set; } = 10;

        public double ExpandedHeight { get; set; } = 33;//MpMeasurements.Instance.AnalyzerMenuHeight;

        public double ToolbarHeight {
            get {
                return IsExpanded ? ExpandedHeight : UnexpandedHeight;
            }
        }
        #endregion

        #region State

        public bool IsLoaded => Items.Count > 0;

        public bool IsExpanded { get; set; } = false;

        #endregion

        #endregion

        #region Constructors

        public async Task Init() {
            PropertyChanged += MpAnalyticItemCollectionViewModel_PropertyChanged;

            await InitDefaultItems();

            if(Items.Count > 0) {
                Items[0].IsSelected = true;
            }
        }

        //public MpAnalyticItemCollectionViewModel() : base(null) {
        //   // PropertyChanged += MpAnalyticItemCollectionViewModel_PropertyChanged;
        //}

        //public MpAnalyticItemCollectionViewModel(MpContentItemViewModel parent) : base(parent) {
        //    PropertyChanged += MpAnalyticItemCollectionViewModel_PropertyChanged;
        //}

        #endregion

        #region Public Methods

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

            var openAiVm = new MpOpenAiViewModel(this, 2);
            await openAiVm.Initialize();
            Items.Add(openAiVm);

            OnPropertyChanged(nameof(Items));

            IsBusy = false;
        }

        private void MpAnalyticItemCollectionViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                //case nameof(HostClipTileViewModel):
                //    HostClipTileViewModel.DoCommandSelection();
                //    break;
                
            }
        }
        #endregion
    }
}
