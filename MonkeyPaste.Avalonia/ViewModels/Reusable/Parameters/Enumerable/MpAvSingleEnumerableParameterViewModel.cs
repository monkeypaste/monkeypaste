using Avalonia.Controls.Selection;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvSingleEnumerableParameterViewModel : MpAvEnumerableParameterViewModelBase {
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
        public MpAvEnumerableParameterValueViewModel SelectedItem =>
            Selection == null ? null : Selection.SelectedItem;

        #endregion

        #region State
        public override MpCsvFormatProperties CsvProperties =>
            MpCsvFormatProperties.Default;

        public int SelectedItemIdx {
            get => Selection.SelectedIndex;
            set => Selection.Select(value);
        }
        public string SelectedValue =>
            SelectedItem == null ? string.Empty : SelectedItem.Value;
        #endregion
        #region Model

        //public override string CurrentValue {
        //    get => SelectedItem == null ? null : SelectedItem.Value;
        //}
        #endregion
        #endregion

        #region Constructors
        public MpAvSingleEnumerableParameterViewModel() : this(null) { }

        public MpAvSingleEnumerableParameterViewModel(MpViewModelBase parent) : base(parent) {
            Selection.SingleSelect = true;
        }
        #endregion

        #region Public Methods
        public override async Task InitializeAsync(MpParameterValue aipv) {
            IsBusy = true;
            await base.InitializeAsync(aipv);
            Items.Clear();

            string selectedValue = null;
            if (!string.IsNullOrEmpty(PresetValueModel.Value)) {
                selectedValue = PresetValueModel.Value;
            } else {
                selectedValue = DefaultValues.FirstOrDefault();
            }

            foreach (var paramVal in ParameterFormat.values) {
                var naipvvm = await CreateAnalyticItemParameterValueViewModel(Items.Count, paramVal.label, paramVal.value);
                Items.Add(naipvvm);
            }
            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }
            if (Items.FirstOrDefault(x => x.Value == selectedValue) is MpAvEnumerableParameterValueViewModel spvvm) {
                Selection.SelectedItem = spvvm;
            }

            OnPropertyChanged(nameof(Items));
            SetLastValue(CurrentValue);

            IsBusy = false;
        }
        #endregion

        #region Protected Methods
        protected override void SelectionChanged(object sender, SelectionModelSelectionChangedEventArgs e) {
            base.SelectionChanged(sender, e);
            OnPropertyChanged(nameof(SelectedItem));
            OnPropertyChanged(nameof(CurrentValue));
            OnPropertyChanged(nameof(SelectedValue));
            OnPropertyChanged(nameof(SelectedItemIdx));
        }

        protected override string GetCurrentValue() {
            return SelectedItem == null ? null : SelectedItem.Value;
        }
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
