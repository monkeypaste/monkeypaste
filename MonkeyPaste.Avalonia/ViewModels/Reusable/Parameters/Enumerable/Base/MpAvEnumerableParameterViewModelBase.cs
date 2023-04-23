using Avalonia.Controls.Selection;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {

    public abstract class MpAvEnumerableParameterViewModelBase : MpAvParameterViewModelBase {
        #region Private Variables
        //private List<MpAvEnumerableParameterValueViewModel> _lastSelectedValues;
        #endregion

        #region Properties

        #region View Models

        public virtual ObservableCollection<MpAvEnumerableParameterValueViewModel> Items { get; set; } = new ObservableCollection<MpAvEnumerableParameterValueViewModel>();

        public SelectionModel<MpAvEnumerableParameterValueViewModel> Selection { get; }

        #endregion

        #region State

        public bool IsParameterDropDownOpen { get; set; }

        public abstract MpCsvFormatProperties CsvProperties { get; }

        #endregion

        #region Model

        #region Db
        #endregion

        #endregion

        #endregion

        #region Constructors

        public MpAvEnumerableParameterViewModelBase() : this(null) { }

        public MpAvEnumerableParameterViewModelBase(MpViewModelBase parent) : base(parent) {
            // the `ListBox` when bound.
            Selection = new SelectionModel<MpAvEnumerableParameterValueViewModel>(Items);
            Selection.SelectionChanged += SelectionChanged;
        }

        #endregion

        #region Public Methods
        public async Task<MpAvEnumerableParameterValueViewModel> CreateAnalyticItemParameterValueViewModel(
            int idx, string label, string value) {
            var naipvvm = new MpAvEnumerableParameterValueViewModel(this);
            naipvvm.PropertyChanged += MpAnalyticItemParameterValueViewModel_PropertyChanged;
            await naipvvm.InitializeAsync(idx, label, value);
            return naipvvm;
        }

        #endregion

        #region Protected Methods
        protected override void MpAnalyticItemParameterViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            base.MpAnalyticItemParameterViewModel_PropertyChanged(sender, e);
            switch (e.PropertyName) {
                //case nameof(CurrentValue):
                //    //SelectValueCommand.Execute(CurrentValue);
                //    break;
                //case nameof(HasModelChanged):
                //case nameof(Items):
                //    if (Parent is MpISaveOrCancelableViewModel socvm) {
                //        socvm.OnPropertyChanged(nameof(socvm.CanSaveOrCancel));
                //    }
                //    break;
                case nameof(IsParameterDropDownOpen):
                    MpAvMainWindowViewModel.Instance.IsAnyDropDownOpen = IsParameterDropDownOpen;
                    break;
            }
        }
        protected virtual void SelectionChanged(object sender, SelectionModelSelectionChangedEventArgs e) {
            Items.ForEach(x => x.OnPropertyChanged(nameof(x.IsSelected)));
            OnPropertyChanged(nameof(Selection));
            CurrentValue = GetCurrentValue();
        }

        //protected override void SetLastValue(object value) {
        //    if (value is string val_str) {
        //        // for single selectables
        //        var epvvm = new MpAvEnumerableParameterValueViewModel() {
        //            Value = val_str
        //        };
        //        SetLastValue(new[] { epvvm });
        //    } else if (value is IEnumerable<MpAvEnumerableParameterValueViewModel> val_vml) {
        //        _lastSelectedValues = val_vml.ToList();
        //    } else {
        //        _lastSelectedValues = new List<MpAvEnumerableParameterValueViewModel>();
        //    }
        //}

        protected override void RestoreLastValue() {
            //if (_lastSelectedValues is IEnumerable<MpAvEnumerableParameterValueViewModel> val_vml) {
            //    Selection.BeginBatchUpdate();
            //    foreach (var (pvvm, idx) in Items.WithIndex()) {
            //        if (val_vml.Contains(pvvm)) {
            //            Selection.Select(idx);
            //        } else {
            //            Selection.Deselect(idx);
            //        }
            //    }
            //    val_vml.ForEach((x, idx) => Selection.Select(idx));
            //    Selection.EndBatchUpdate();
            //} else {
            //    Selection.Clear();
            //}
            var lvl = _lastValue.ToListFromCsv(CsvProperties);
            Selection.BeginBatchUpdate();
            foreach (var (pvvm, idx) in Items.WithIndex()) {
                if (lvl.Contains(pvvm.Value)) {
                    Selection.Select(idx);
                } else {
                    Selection.Deselect(idx);
                }
            }
            Selection.EndBatchUpdate();
        }

        protected abstract string GetCurrentValue();
        #endregion

        #region Private Methods

        #endregion

        #region Commands



        #endregion
    }
}
