﻿using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
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

            List<string> current_values = DefaultValues;
            if (!string.IsNullOrEmpty(ParameterValue.Value)) {
                // when not initial load use stored values
                current_values = ParameterValue.Value.ToListFromCsv(CsvProperties);
            }
            await SetCurrentValueAsync(current_values);
            SetLastValue(CurrentValue);

            IsBusy = false;
        }
        #endregion

        #region Protected Methods
        protected override void MpAnalyticItemParameterViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            base.MpAnalyticItemParameterViewModel_PropertyChanged(sender, e);

            switch (e.PropertyName) {
                case nameof(HasModelChanged):
                case nameof(Items):
                    if (SaveOrCancelableViewModel is MpISaveOrCancelableViewModel socvm) {
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
        protected override void RestoreLastValue() {
            if (!CanSetModelValue()) {
                return;
            }
            Dispatcher.UIThread.Post(async () => {
                var lvl = _lastValue.ToListFromCsv(CsvProperties);
                await SetCurrentValueAsync(lvl, _lastValue);
                if (SaveOrCancelableViewModel is MpISaveOrCancelableViewModel socvm) {
                    socvm.OnPropertyChanged(nameof(socvm.CanSaveOrCancel));
                }
            });
        }
        protected override string GetCurrentValue() {

            return Items == null ? null : Items.Select(x => x.Value).ToList().ToCsv(CsvProperties);
        }
        #endregion

        #region Private Methods
        private async Task SetCurrentValueAsync(List<string> newValue, string forced_last_value = default) {
            string new_last_value = forced_last_value ?? CurrentValue;

            Items.Clear();
            foreach (var paramVal in newValue) {
                var naipvvm = await CreateAnalyticItemParameterValueViewModel(Items.Count, string.Empty, paramVal);
                Items.Add(naipvvm);
            }
            if (Selection.Count == 0 && Items.Count > 0) {
                Selection.Select(0);
            }

            OnPropertyChanged(nameof(CurrentValue));
            SetLastValue(new_last_value);

            OnPropertyChanged(nameof(Items));
        }
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
