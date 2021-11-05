using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using System.Net;
using System.Windows.Media;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using System.Windows;

namespace MpWpfApp {
    public abstract class MpAnalyticItemViewModel : MpViewModelBase<MpAnalyticItemCollectionViewModel> {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        public ObservableCollection<MpAnalyticItemParameterViewModel> Parameters { get; set; } = new ObservableCollection<MpAnalyticItemParameterViewModel>();

        public MpAnalyticItemParameterViewModel SelectedParameter => Parameters.Where(x => x.IsSelected).FirstOrDefault();

        #endregion

        #region Appearance

        public string ItemIconSourcePath { get; set; }

        public Brush ItemBackgroundBrush {
            get {
                if (IsHovering && !IsSelected) {
                    return Brushes.Yellow;
                }
                return Brushes.White;
            }
        }
        #endregion

        #region State

        public bool IsHovering { get; set; } = false;

        public bool IsSelected { get; set; } = false;

        public bool IsExpanded { get; set; } = false;

        public string UnformattedResponse { get; set; } = string.Empty;

        public HttpStatusCode ResponseCode { get; set; }

        #endregion

        #region Model

        public string Title {
            get {
                if (AnalyticItem == null) {
                    return string.Empty;
                }
                return AnalyticItem.Title;
            }
            set {
                if (Title != value) {
                    Title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        public string Description {
            get {
                if (AnalyticItem == null) {
                    return string.Empty;
                }
                return AnalyticItem.Description;
            }
        }

        public int AnalyticItemId {
            get {
                if (AnalyticItem == null) {
                    return 0;
                }
                return AnalyticItem.Id;
            }
        }

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

        public async virtual Task Initialize() { await Task.Delay(1); }

        public async Task InitializeAsync(MpAnalyticItem ai) {
            IsBusy = true;
            await Task.Delay(1);
            AnalyticItem = ai;

            IsBusy = false;
        }

        public async Task LoadChildren() {
            if (AnalyticItem == null) {
                return;
            }
            IsBusy = true;

            Parameters = new ObservableCollection<MpAnalyticItemParameterViewModel>();
            foreach (var aip in AnalyticItem.Parameters.OrderBy(x => x.SortOrderIdx)) {
                var naipvm = await CreateParameterViewModel(aip);
                Parameters.Add(naipvm);
            }

            if (!Parameters.Any(x => x.Parameter.ParameterType == MpAnalyticParameterType.Execute)) {
                var eaip = new MpAnalyticItemParameter() {
                    Id = Parameters.Count + 1,
                    AnalyticItemParameterGuid = Guid.NewGuid(),
                    AnalyticItemId = AnalyticItemId,
                    AnalyticItem = this.AnalyticItem,
                    ParameterType = MpAnalyticParameterType.Execute,
                    Key = "Execute",
                    ValueCsv = null
                };
                var eaipvm = await CreateParameterViewModel(eaip);
                Parameters.Add(eaipvm);
            }

            if (!Parameters.Any(x => x.Parameter.ParameterType == MpAnalyticParameterType.Result)) {
                var raip = new MpAnalyticItemParameter() {
                    Id = Parameters.Count + 1,
                    AnalyticItemParameterGuid = Guid.NewGuid(),
                    AnalyticItemId = AnalyticItemId,
                    AnalyticItem = this.AnalyticItem,
                    ParameterType = MpAnalyticParameterType.Result,
                    Key = "Result",
                    ValueCsv = string.Empty
                };
                var raipvm = await CreateParameterViewModel(raip);
                Parameters.Add(raipvm);
            }

            OnPropertyChanged(nameof(Parameters));

            IsBusy = false;
        }

        public async Task<MpAnalyticItemParameterViewModel> CreateParameterViewModel(MpAnalyticItemParameter aip) {
            MpAnalyticItemParameterViewModel naipvm = null;

            switch (aip.ParameterType) {
                case MpAnalyticParameterType.ComboBox:
                    naipvm = new MpComboBoxParameterViewModel(this);
                    break;
                case MpAnalyticParameterType.Text:
                    naipvm = new MpTextInputParameterViewModel(this);
                    break;
                case MpAnalyticParameterType.CheckBox:
                    naipvm = new MpCheckBoxParameterViewModel(this);
                    break;
                case MpAnalyticParameterType.Slider:
                    naipvm = new MpSliderParameterViewModel(this);
                    break;
                case MpAnalyticParameterType.Execute:
                    naipvm = new MpExecuteParameterViewModel(this);
                    break;
                case MpAnalyticParameterType.Result:
                    naipvm = new MpResultParameterViewModel(this);
                    break;
            }

            await naipvm.InitializeAsync(aip);
            return naipvm;
        }

        protected virtual async Task ExecuteAnalysis() {
            await Task.Delay(1);
            MpConsole.WriteLine("Base execute, no implementation");
        }
        #endregion

        #region Private Methods

        private void MpAnalyticItemViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
            }
        }
        #endregion

        #region Commands

        public ICommand ExecuteAnalysisCommand => new RelayCommand(
            async () => {
                await ExecuteAnalysis();
            });
        #endregion
    }
}
