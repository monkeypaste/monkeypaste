using Avalonia.Controls.Selection;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvMultiEnumerableParameterViewModel : MpAvEnumerableParameterViewModelBase {
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

        #endregion
        #endregion

        #region Constructors
        public MpAvMultiEnumerableParameterViewModel() : this(null) { }

        public MpAvMultiEnumerableParameterViewModel(MpAvViewModelBase parent) : base(parent) {
            Selection.SingleSelect = false;
        }
        #endregion

        #region Public Methods
        public override async Task InitializeAsync(MpParameterValue aipv) {
            IsBusy = true;
            await base.InitializeAsync(aipv);
            Items.Clear();

            List<string> selectedValues = new List<string>();

            if (!string.IsNullOrEmpty(ParameterValue.Value)) {
                selectedValues = ParameterValue.Value.ToListFromCsv(CsvProperties);
            } else {
                selectedValues = DefaultValues;
            }

            foreach (var paramVal in ParameterFormat.values) {
                var naipvvm = await CreateAnalyticItemParameterValueViewModel(Items.Count, paramVal.label, paramVal.value);
                Items.Add(naipvvm);
            }

            OnPropertyChanged(nameof(Items));

            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }
            Selection.BeginBatchUpdate();
            for (int i = 0; i < Items.Count; i++) {
                if (selectedValues.Contains(Items[i].Value)) {
                    Selection.Select(i);
                }
            }
            //if (Selection.Count == 0 && Items.Count > 0) {
            //    Selection.Select(0);
            //}
            Selection.EndBatchUpdate();

            OnPropertyChanged(nameof(CurrentValue));
            SetLastValue(CurrentValue);

            IsBusy = false;
        }
        #endregion

        #region Protected Methods
        protected override string GetCurrentValue() {
            return SelectedItems == null ? null : SelectedItems.Select(x => x.Value).ToList().ToCsv(CsvProperties);
        }
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
