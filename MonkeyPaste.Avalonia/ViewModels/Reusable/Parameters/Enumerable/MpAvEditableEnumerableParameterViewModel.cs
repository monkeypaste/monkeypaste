using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvEditableEnumerableParameterViewModel : MpAvEnumerableParameterViewModelBase {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region View Models
        public IEnumerable<MpAvEnumerableParameterValueViewModel> SelectedItems =>
            Selection == null ? null : Selection.SelectedItems;

        #endregion

        #region State
        public override MpCsvFormatProperties CsvProperties =>
            MpCsvFormatProperties.DefaultBase64Value;

        public bool CanDeleteOrMoveValue =>
            Items.Count > 1;

        #endregion

        #region Model

        //public override string CurrentValue =>
        //    Items.Select(x => x.Value).ToList().ToCsv(CsvProperties);
        #endregion
        #endregion

        #region Constructors
        public MpAvEditableEnumerableParameterViewModel() : this(null) { }

        public MpAvEditableEnumerableParameterViewModel(MpAvViewModelBase parent) : base(parent) {
            Selection.SingleSelect = true;
        }
        #endregion

        #region Public Methods
        public override async Task InitializeAsync(MpParameterValue aipv) {
            IsBusy = true;
            await base.InitializeAsync(aipv);
            Items.Clear();

            if (Parent is MpAvSettingsFrameViewModel) {

            }

            List<string> current_values = DefaultValues;

            if (!string.IsNullOrEmpty(PresetValueModel.Value)) {
                // when not initial load use stored values
                current_values = PresetValueModel.Value.ToListFromCsv(CsvProperties);
            }

            foreach (var paramVal in current_values) {
                var naipvvm = await CreateAnalyticItemParameterValueViewModel(Items.Count, string.Empty, paramVal);
                Items.Add(naipvvm);
            }
            if (Selection.Count == 0 && Items.Count > 0) {
                Selection.Select(0);
            }


            OnPropertyChanged(nameof(CurrentValue));
            SetLastValue(CurrentValue);

            OnPropertyChanged(nameof(Items));

            IsBusy = false;
        }
        #endregion

        #region Protected Methods
        protected override void MpAnalyticItemParameterViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            base.MpAnalyticItemParameterViewModel_PropertyChanged(sender, e);

            switch (e.PropertyName) {
                case nameof(HasModelChanged):
                case nameof(Items):
                    if (Parent is MpISaveOrCancelableViewModel socvm) {
                        socvm.OnPropertyChanged(nameof(socvm.CanSaveOrCancel));
                    }
                    break;
                case nameof(CanDeleteOrMoveValue):
                    Items.ForEach(x => x.OnPropertyChanged(nameof(x.CanDeleteOrMove)));
                    break;
            }
        }

        protected override void MpAnalyticItemParameterValueViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            base.MpAnalyticItemParameterValueViewModel_PropertyChanged(sender, e);
            switch (e.PropertyName) {
                case nameof(MpAvEnumerableParameterValueViewModel.Value):
                    CurrentValue = GetCurrentValue();
                    break;
            }
        }
        protected override string GetCurrentValue() {

            return Items == null ? null : Items.Select(x => x.Value).ToList().ToCsv(CsvProperties);
        }
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        public ICommand AddValueCommand => new MpAsyncCommand(
            async () => {
                IsBusy = true;

                var naipvvm = await CreateAnalyticItemParameterValueViewModel(Items.Count, string.Empty, string.Empty);
                Items.Add(naipvvm);
                OnPropertyChanged(nameof(Items));
                OnPropertyChanged(nameof(CurrentValue));
                HasModelChanged = true;

                IsBusy = false;
            });

        public ICommand RemoveValueCommand => new MpCommand<object>(
             (args) => {
                 IsBusy = true;
                 var epvvm = args as MpAvEnumerableParameterValueViewModel;

                 int idxToRemove = Items.IndexOf(epvvm);
                 if (idxToRemove >= 0) {
                     if (Items.Count == 1) {
                         Items[0].Value = string.Empty;
                     } else {
                         Items.RemoveAt(idxToRemove);
                     }
                 }

                 OnPropertyChanged(nameof(Items));

                 //if (Items.Count > 0) {
                 //    idxToRemove = Math.Max(0, idxToRemove - 1);
                 //    Items.ForEach(x => x.IsSelected = Items.IndexOf(x) == idxToRemove);
                 //}
                 OnPropertyChanged(nameof(CurrentValue));

                 HasModelChanged = true;
                 IsBusy = false;
             }, (args) => {
                 return Items.Count > 0 || args is not MpAvEnumerableParameterValueViewModel;
             });
        #endregion
    }
}
