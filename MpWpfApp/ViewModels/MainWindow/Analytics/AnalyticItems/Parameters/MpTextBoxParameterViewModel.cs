using MonkeyPaste;
using MonkeyPaste.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Windows.Foundation.Collections;

namespace MpWpfApp {
    public class MpTextBoxParameterViewModel : 
        MpAnalyticItemParameterViewModel,
        MpIMenuItemViewModel,
        MpITextSelectionRangeViewModel,
        MpIContentQueryTextBoxViewModel {
        #region Private Variables
        
        private string _defaultValue;

        #endregion

        #region Properties

        #region View Models

        public MpMenuItemViewModel MenuItemViewModel {
            get {
                var tmivml = new List<MpMenuItemViewModel>();
                var propertyPathLabels = typeof(MpComparePropertyPathType).EnumToLabels();
                for (int i = 0; i < propertyPathLabels.Length; i++) {
                    var ppt = (MpComparePropertyPathType)i;
                    var mivm = new MpMenuItemViewModel() {
                        Header = propertyPathLabels[i],
                        Command = AddContentPropertyPathCommand,
                        CommandParameter = ppt
                    };
                    if (ppt == MpComparePropertyPathType.None || (ppt == MpComparePropertyPathType.LastOutput && !IsActionParameter)) {
                        mivm.IsVisible = false;
                    }
                    tmivml.Add(mivm);
                }
                return new MpMenuItemViewModel() {
                    SubItems = tmivml
                };
            }
        }

        #endregion

        #region MpITextSelectionRangeViewModel Implementation

        public int SelectionStart { get; set; } = 0;
        public int SelectionLength { get; set; } = 0;

        #endregion

        #region MpIContentQueryTextBoxViewModel Implementation

        string MpIContentQueryTextBoxViewModel.ContentQuery {
            get => CurrentValue;
            set => CurrentValue = value;
        }

        ICommand MpIContentQueryTextBoxViewModel.ClearQueryCommand => new MpCommand(
            () => {
                CurrentValue = string.Empty;
            },
            () => !string.IsNullOrEmpty(CurrentValue));

        #endregion

        #region State

        public bool IsActionParameter { get; set; } = false;

        public int CaretIndex { get; set; } = 0;

        #endregion

        #region Model

        public bool IsContentQuery {
            get {
                if(Parameter == null) {
                    return false;
                }
                return Parameter.parameterValueType == MpAnalyticItemParameterValueUnitType.ContentQuery;
            }
        }

        public override string CurrentValue { get; set; }

        public override string DefaultValue => _defaultValue;

        #endregion

        #endregion

        #region Constructors

        public MpTextBoxParameterViewModel() : base(null) { }

        public MpTextBoxParameterViewModel(MpAnalyticItemPresetViewModel parent) : base(parent) {
            PropertyChanged += MpTextBoxParameterViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpAnalyticItemParameterFormat aipf, MpAnalyticItemPresetParameterValue aipv) {
            IsBusy = true;

            Parameter = aipf;
            ParameterValue = aipv;

            CurrentValue = _defaultValue = aipv.Value;

            OnPropertyChanged(nameof(DefaultValue));
            OnPropertyChanged(nameof(CurrentValue));

            OnValidate += MpTextBoxParameterViewModel_OnValidate;
            await Task.Delay(1);

            IsBusy = false;
        }

        private void MpTextBoxParameterViewModel_OnValidate(object sender, EventArgs e) {
            //if (!IsRequired) {
            //    return true;
            //}
            //if (Parameter == null || CurrentValue == null) {
            //    return false;
            //}

            var minCond = Parameter.values.FirstOrDefault(x => x.isMinimum);
            if (minCond != null) {
                int minLength = 0;
                try {
                    minLength = Convert.ToInt32(minCond.value);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"Minimum val: {minCond.value} could not conver to int, exception: {ex}");
                }
                if (CurrentValue.Length < minLength) {
                    ValidationMessage = $"{Label} must be at least {minLength} characters";
                } else {
                    ValidationMessage = string.Empty;
                }
            }
            if(IsValid) {
                var maxCond = Parameter.values.FirstOrDefault(x => x.isMaximum);
                if (maxCond != null) {
                    // TODO should cap all input string but especially here
                    int maxLength = int.MaxValue;
                    try {
                        maxLength = Convert.ToInt32(maxCond.value);
                    }
                    catch (Exception ex) {
                        MpConsole.WriteTraceLine($"Maximum val: {minCond.value} could not conver to int, exception: {ex}");
                    }
                    if (CurrentValue.Length > maxLength) {
                        ValidationMessage = $"{Label} can be no more than {maxLength} characters";
                    } else {
                        ValidationMessage = string.Empty;
                    }
                }
            }

            if(IsValid) {
                if (!string.IsNullOrEmpty(FormatInfo)) {
                    if (CurrentValue.IndexOfAny(FormatInfo.ToCharArray()) != -1) {
                        ValidationMessage = $"{Label} cannot contain '{FormatInfo}' characters";
                    }
                }
            }

            OnPropertyChanged(nameof(IsValid));
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        private void MpTextBoxParameterViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(SelectionLength):
                case nameof(SelectionStart):
                    MpConsole.WriteLine($"Start: {SelectionStart} Length: {SelectionLength}");
                    break;
            }
        }

        private void MpContentParameterViewModel_OnValidate(object sender, EventArgs e) {
            ValidationMessage = string.Empty;

            OnPropertyChanged(nameof(IsValid));
        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e) {
            var cm = sender as ContextMenu;
            IsActionParameter = false;
            cm.Closed -= ContextMenu_Closed;
        }

        #endregion

        #region Commands

        public ICommand ShowContentPathSelectorMenuCommand => new MpCommand<object>(
             (args) => {
                 var fe = args as FrameworkElement;

                 IsActionParameter = fe.GetVisualAncestor<MpTriggerActionChooserView>() != null;

                 var cm = new MpContextMenuView();
                 cm.DataContext = MenuItemViewModel;
                 fe.ContextMenu = cm;
                 fe.ContextMenu.PlacementTarget = fe;
                 fe.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;
                 fe.ContextMenu.IsOpen = true;
                 fe.ContextMenu.Closed += ContextMenu_Closed;
             });


        public ICommand AddContentPropertyPathCommand => new MpCommand<object>(
            (args) => {
                if (args == null) {
                    return;
                }
                var cppt = (MpComparePropertyPathType)args;
                if (cppt == MpComparePropertyPathType.None) {
                    return;
                }

                string pathStr = string.Format(@"{{{0}}}", cppt.ToString());
                CurrentValue = CurrentValue.Remove(SelectionStart, SelectionLength).Insert(SelectionStart, pathStr);
            });

        #endregion
    }
}
