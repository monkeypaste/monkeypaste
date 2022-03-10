using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MonkeyPaste;
using MonkeyPaste.Plugin;

namespace MpWpfApp {
    public class MpListBoxParameterViewModel : MpComboBoxParameterViewModel {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models
        public virtual IList<MpEnumerableParameterValueViewModel> SelectedItems {
            get {
                if (IsMultiSelect) {
                    return Items.Where(x => x.IsSelected).ToList();
                } else if (IsSingleSelect) {
                    return new List<MpEnumerableParameterValueViewModel>() {
                        Items.FirstOrDefault(x => x.IsSelected)
                    };
                }
                return Items;
            }
            set {
                if (SelectedItems != value) { 
                    if(value == null) {
                        Items.ForEach(x => x.IsSelected = false);
                    } else {
                        Items.ForEach(x => x.IsSelected = value.Contains(x));
                    }
                    OnPropertyChanged(nameof(CurrentValue));
                }
            }
        }
        
        #endregion
        
        #region State

        #endregion

        #region Model

        public override string CurrentValue {
            get => string.Join(",", SelectedItems.Select(x => x.Value));
            set {
                if(CurrentValue != value) {
                    Items.ForEach(x => x.IsSelected = false);
                    if (value != null) {
                        var ncvvm = Items.FirstOrDefault(x => x.Value == value);
                        if (ncvvm == null) {
                            throw new Exception("Cannot set combobox to: " + value);
                        }
                        ncvvm.IsSelected = true;
                    }
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(CurrentValue));
                    OnPropertyChanged(nameof(SelectedItem));
                }
            }
        }

        public override string DefaultValue => Items.FirstOrDefault(x => x.IsDefault)?.Value;

        public bool IsMultiSelect {
            get {
                if(Parameter == null) {
                    return false;
                }
                return Parameter.isMultiSelect;
            }
        }

        public bool IsSingleSelect {
            get {
                if (Parameter == null) {
                    return false;
                }
                return Parameter.isSingleSelect;
            }
        }

        public bool IsAllSelect => !IsSingleSelect && !IsMultiSelect;

        public bool CanAddValues {
            get {
                if (Parameter == null) {
                    return false;
                }
                return Parameter.canAddValues;
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpListBoxParameterViewModel() : base () { }

        public MpListBoxParameterViewModel(MpAnalyticItemPresetViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpAnalyticItemParameterFormat aipf, MpAnalyticItemPresetParameterValue aipv) {
            IsBusy = true;

            Parameter = aipf;
            ParameterValue = aipv;
            
            Items.Clear();
            if (!string.IsNullOrEmpty(ParameterValue.Value)) {
                Parameter.values.ForEach(x => x.isDefault = false);

                var presetValParts = ParameterValue.Value.Split(new string[] { "," }, StringSplitOptions.None).ToList();
                for (int i = 0; i < presetValParts.Count; i++) {
                    string presetValStr = presetValParts[i];
                    var paramVal = Parameter.values.FirstOrDefault(x => x.value == presetValStr);
                    if (paramVal == null) {
                        paramVal = new MpAnalyticItemParameterValue() {
                            isDefault = true,
                            label = presetValStr,
                            value = presetValStr
                        };
                        if (i >= Parameter.values.Count) {
                            Parameter.values.Add(paramVal);
                        }
                    } else {
                        paramVal.isDefault = true;
                    }                    
                }
            }

            foreach (var paramVal in Parameter.values) {
                var naipvvm = await CreateAnalyticItemParameterValueViewModel(Items.Count, paramVal);
                if(!IsAllSelect) {
                    naipvvm.IsSelected = aipv.Value.Contains(paramVal.value);
                }
                Items.Add(naipvvm);
            }

            if (!IsAllSelect) {
                foreach (var spv in Items.Where(x => x.IsDefault)) {
                    SelectedItems.Add(spv);
                }
            }
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(CurrentValue));
            OnPropertyChanged(nameof(DefaultValue));

            OnPropertyChanged(nameof(SelectedItems));


            IsBusy = false;


        }

        #endregion

        #region Protected Methods
        #endregion

        #region Commands

        public ICommand AddValueCommand => new MpAsyncCommand(
            async() => {
                IsBusy = true;

                var paramVal = new MpAnalyticItemParameterValue() {
                    isDefault = true
                };
                var naipvvm = await CreateAnalyticItemParameterValueViewModel(Items.Count, paramVal);
                Items.Add(naipvvm);

                if (IsMultiSelect) {
                    naipvvm.IsSelected = true;
                } else if(IsSingleSelect) {
                    Items.ForEach(x => x.IsSelected = false);
                    naipvvm.IsSelected = true;
                }

                OnPropertyChanged(nameof(Items));
                OnPropertyChanged(nameof(SelectedItems));
                OnPropertyChanged(nameof(SelectedItem));
                HasModelChanged = true;

                IsBusy = false;
            });

        public ICommand RemoveValueCommand => new MpCommand<object>(
             (args) => {
                IsBusy = true;
                var epvvm = args as MpEnumerableParameterValueViewModel;
                
                int idxToRemove = Items.IndexOf(epvvm);
                if(idxToRemove >= 0) {
                    if(Items.Count == 1) {
                         Items[0].Value = string.Empty;
                     } else {
                         Items.RemoveAt(idxToRemove);
                     }
                }

                OnPropertyChanged(nameof(Items));
                OnPropertyChanged(nameof(SelectedItems));
                OnPropertyChanged(nameof(SelectedItem));

                HasModelChanged = true;
                IsBusy = false;
            },(args)=>Items.Count > 0);

        #endregion
    }
}
