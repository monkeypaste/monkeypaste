using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpAnalyticItemCollectionViewModel : MpViewModelBase<MpContentItemViewModel> { //MpSingletonViewModel<MpAnalyticItemCollectionViewModel,object> { //
        #region Properties

        #region View Models

        public ObservableCollection<MpAnalyticItemViewModel> Items { get; private set; } = new ObservableCollection<MpAnalyticItemViewModel>();

        public MpAnalyticItemViewModel SelectedItem => Items.FirstOrDefault(x => x.IsSelected);

        #endregion

        #region Layout

        public double AnalyticTreeViewMaxWidth { get; set; } = MpMeasurements.Instance.ClipTileInnerBorderSize;

        #endregion

        #region State

        public bool IsLoaded => Items.Count > 0;

        #endregion

        #endregion

        #region Constructors

        public MpAnalyticItemCollectionViewModel() : base(null) { }

        public MpAnalyticItemCollectionViewModel(MpContentItemViewModel parent) : base(parent) {
            PropertyChanged += MpAnalyticItemCollectionViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods
        public async Task Init() {
            await InitDefaultItems();

            if (Items.Count > 0) {
                Items[0].IsSelected = true;
            }
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
