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


        #endregion

        #region Model

        //public override string CurrentValue =>
        //    Items.Select(x => x.Value).ToList().ToCsv(CsvProperties);
        #endregion
        #endregion

        #region Constructors
        public MpAvEditableEnumerableParameterViewModel() : this(null) { }

        public MpAvEditableEnumerableParameterViewModel(MpViewModelBase parent) : base(parent) {
            Selection.SingleSelect = true;
        }
        #endregion

        #region Public Methods
        public override async Task InitializeAsync(MpParameterValue aipv) {
            IsBusy = true;
            await base.InitializeAsync(aipv);
            Items.Clear();

            List<string> selectedValues = new List<string>();

            if (!string.IsNullOrEmpty(PresetValueModel.Value)) {
                selectedValues = PresetValueModel.Value.ToListFromCsv(CsvProperties);
            } else {
                selectedValues = DefaultValues;
            }
            Selection.BeginBatchUpdate();
            foreach (var paramVal in ParameterFormat.values) {
                bool is_selected = selectedValues.Contains(paramVal.value);
                var naipvvm = await CreateAnalyticItemParameterValueViewModel(Items.Count, paramVal.label, paramVal.value);
                Items.Add(naipvvm);
                while (naipvvm.IsBusy) { await Task.Delay(100); }
                if (is_selected) {
                    // in case of multiple entries, remove first
                    selectedValues.RemoveAt(selectedValues.IndexOf(paramVal.value));
                    //Selection.Select(Items.Count - 1);
                }
            }

            // NOTE this secondary add is very editable lists where values wouldn't be found in parameter format..
            // reverse selected values to retain order (valueIdx increment maybe unnecessary, don't remember why its necessary but this retains order)
            selectedValues.Reverse();
            foreach (var selectValueStr in selectedValues) {
                // for new values add them to front of Items
                Items.ForEach(x => x.ValueIdx++);
                //for new values from preset add and select
                var nsaipvvm = await CreateAnalyticItemParameterValueViewModel(0, selectValueStr, selectValueStr);
                Items.Insert(0, nsaipvvm);
                while (nsaipvvm.IsBusy) { await Task.Delay(100); }
                //Selection.Select(0);
            }
            if (Selection.Count == 0 && Items.Count > 0) {
                Selection.Select(0);
            }
            Selection.EndBatchUpdate();

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
                case nameof(CurrentValue):
                    //SelectValueCommand.Execute(CurrentValue);
                    break;
                case nameof(HasModelChanged):
                case nameof(Items):
                    if (Parent is MpISaveOrCancelableViewModel socvm) {
                        socvm.OnPropertyChanged(nameof(socvm.CanSaveOrCancel));
                    }
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
             }, (args) => Items.Count > 0);
        #endregion
    }
}
