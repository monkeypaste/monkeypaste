using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MpWpfApp {
    public class MpAnalyticItemPresetViewModel : MpViewModelBase<MpAnalyticItemViewModel>, ICloneable {
        #region Properties

        #region View Models

        #endregion

        #region State
        public bool IsSelected { get; set; }

        public bool IsEditing { get; set; }

        #endregion

        #region Model 

        public string Label {
            get {
                if (Preset == null) {
                    return string.Empty;
                }
                if (string.IsNullOrEmpty(Preset.Label)) {
                    return Preset.Label;
                }
                return Preset.Label;
            }
        }

        public string Description {
            get {
                if (Preset == null) {
                    return string.Empty;
                }
                if (string.IsNullOrEmpty(Preset.Description)) {
                    return Preset.Description;
                }
                return Preset.Description;
            }
        }

        public int SortOrderIdx {
            get {
                if (Preset == null) {
                    return 0;
                }
                return Preset.SortOrderIdx;
            }
        }

        public bool IsReadOnly {
            get {
                if (Preset == null) {
                    return false;
                }
                return Preset.IsReadOnly;
            }
        }

        public bool IsQuickAction {
            get {
                if (Preset == null) {
                    return true;
                }
                return Preset.IsQuickAction;
            }
            set {
                if(IsQuickAction != value) {
                    Preset.IsQuickAction = value;
                    OnPropertyChanged(nameof(IsQuickAction));
                }
            }
        }

        public MpAnalyticItemPreset Preset { get; protected set; }
        #endregion

        #endregion

        #region Constructors

        public MpAnalyticItemPresetViewModel() : base (null) { }

        public MpAnalyticItemPresetViewModel(MpAnalyticItemViewModel parent) : base(parent) {
            PropertyChanged += MpPresetParameterViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpAnalyticItemPreset aip) {
            IsBusy = true;

            Preset = aip;

            await Task.Delay(1);

            IsBusy = false;
        }

        public object Clone() {
            var caipvm = new MpAnalyticItemPresetViewModel(Parent);
            caipvm.Preset = Preset.Clone() as MpAnalyticItemPreset;
            return caipvm;
        }

        #endregion

        #region Private Methods

        private void MpPresetParameterViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
                case nameof(IsSelected):
                    foreach(var presetParam in Preset.PresetParameterValues) {
                        var paramVm = 
                            Parent.ParameterViewModels.FirstOrDefault(x => 
                                x.Parameter.EnumId == presetParam.ParameterEnumId);
                        if(paramVm == null) {
                            continue;
                        }
                        paramVm.CurrentValueViewModel.Value = presetParam.Value;
                    }
                    break;
            } 
        }

        #endregion
    }
}
