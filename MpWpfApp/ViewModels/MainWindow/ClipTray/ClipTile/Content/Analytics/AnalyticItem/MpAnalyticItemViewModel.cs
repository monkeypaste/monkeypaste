using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpAnalyticItemViewModel : MpViewModelBase<MpAnalyticItemCollectionViewModel> {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpAnalyticItemParameterViewModel> Parameters { get; set; } = new ObservableCollection<MpAnalyticItemParameterViewModel>();

        public MpAnalyticItemParameterViewModel SelectedParameter => Parameters.Where(x => x.IsSelected).FirstOrDefault();

        #endregion

        #region State

        public bool IsHovering { get; set; } = false;

        public bool IsSelected { get; set; } = false;

        #endregion

        #region Model

        public MpAnalyticItem AnalyticItem { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpAnalyticItemViewModel() : base(null) { }

        public MpAnalyticItemViewModel(MpAnalyticItemCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpAnalyticItemViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpAnalyticItem ai) {
            IsBusy = true;

            AnalyticItem = ai;
            Parameters = new ObservableCollection<MpAnalyticItemParameterViewModel>();
            foreach(var aip in AnalyticItem.Parameters.OrderBy(x=>x.SortOrderIdx)) {
                var naipvm = await CreateParameterViewModel(aip);
                Parameters.Add(naipvm);
            }

            OnPropertyChanged(nameof(Parameters));

            IsBusy = false;
        }

        public async Task<MpAnalyticItemParameterViewModel> CreateParameterViewModel(MpAnalyticItemParameter aip) {
            var naipvm = new MpAnalyticItemParameterViewModel(this);
            await naipvm.InitializeAsync(aip);
            return naipvm;
        }
        #endregion

        #region Private Methods

        private void MpAnalyticItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {

            }
        }
        #endregion
    }
}
