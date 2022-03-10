using GalaSoft.MvvmLight.CommandWpf;
using MonkeyPaste;
using MonkeyPaste.Plugin;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpEnumerableParameterValueViewModel : 
        MpViewModelBase<MpAnalyticItemParameterViewModel>,
        MpIMenuItemViewModel,
        MpITextSelectionRangeViewModel,
        MpIContentQueryTextBoxViewModel {
        #region Private Variables

        #endregion

        #region Properties

        #region MpITextSelectionRangeViewModel Implementation

        public int SelectionStart { get; set; } = 0;
        public int SelectionLength { get; set; } = 0;

        #endregion

        #region MpIMenuItemViewModel Implementation

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

        #region MpIContentQueryTextBoxViewModel Implementation

        public bool IsActionParameter { get; set; } = false;

        public string ContentQuery {
            get => Value;
            set => Value = value;
        }

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

        private void ContextMenu_Closed(object sender, RoutedEventArgs e) {
            var cm = sender as ContextMenu;
            IsActionParameter = false;
            cm.Closed -= ContextMenu_Closed;
        }

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
                Value = Value.Remove(SelectionStart, SelectionLength).Insert(SelectionStart, pathStr);
            });



        public ICommand ClearQueryCommand {
            get {
                if(Parent is MpListBoxParameterViewModel lbpvm) {
                    return lbpvm.RemoveValueCommand;
                }
                return null;
            }
        }

        #endregion

        #region Appearance

        public Brush BackgroundBrush {
            get {
                if(IsSelected) {
                    return Brushes.Blue;
                }
                return Brushes.Transparent;
            }
        }

        public Brush BorderBrush {
            get {
                if (IsHovering) {
                    return Brushes.Yellow;
                }
                return Brushes.Transparent;
            }
        }
        #endregion

        #region State

        public bool IsHovering { get; set; } = false;

        public bool IsSelected { get; set; } = false;

        public int ValueIdx { get; set; } = 0;               

        #endregion

        #region Model
        
        public bool IsReadOnly {
            get {
                if(Parent == null) {
                    return false;
                }
                return Parent.IsReadOnly;
            }
        }

        public bool IsDefault {
            get {
                if(AnalyticItemParameterValue == null) {
                    return false;
                }
                return AnalyticItemParameterValue.isDefault;
            }
        }

        public bool IsMaximum {
            get {
                if (AnalyticItemParameterValue == null) {
                    return false;
                }
                return AnalyticItemParameterValue.isMaximum;
            }
        }

        public bool IsMinimum {
            get {
                if (AnalyticItemParameterValue == null) {
                    return false;
                }
                return AnalyticItemParameterValue.isMinimum;
            }
        }

        public string Label {
            get {
                if (AnalyticItemParameterValue == null) {
                    return string.Empty;
                }
                return AnalyticItemParameterValue.label;
            }
        }

        public string Value {
            get {
                if (AnalyticItemParameterValue == null) {
                    return null;
                }
                return AnalyticItemParameterValue.value;
            }
            set {
                if (Value != value) {
                    //HasModelChanged = AnalyticItemParameterValue.value != value;
                    HasModelChanged = true;
                    AnalyticItemParameterValue.value = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        public MpAnalyticItemParameterValue AnalyticItemParameterValue { get; set; }
        #endregion

        #endregion

        #region Constructors

        public MpEnumerableParameterValueViewModel() : base(null) { }

        public MpEnumerableParameterValueViewModel(MpAnalyticItemParameterViewModel parent) : base(parent) {
            PropertyChanged += MpAnalyticItemParameterValueViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(int idx, MpAnalyticItemParameterValue valueSeed) {
            IsBusy = true;

            await Task.Delay(1);

            ValueIdx = idx;
            AnalyticItemParameterValue = valueSeed;

            OnPropertyChanged(nameof(IsReadOnly));

            IsBusy = false;
        }

        public override string ToString() {
            return Value;
        }

        #region Equals Override

        public bool Equals(MpEnumerableParameterValueViewModel other) {
            if (other == null)
                return false;

            if (this.Value == other.Value)
                return true;
            else
                return false;
        }

        public override bool Equals(Object obj) {
            if (obj == null)
                return false;

            MpEnumerableParameterValueViewModel personObj = obj as MpEnumerableParameterValueViewModel;
            if (personObj == null)
                return false;
            else
                return Equals(personObj);
        }

        public override int GetHashCode() {
            return this.Value.GetHashCode();
        }

        public static bool operator ==(MpEnumerableParameterValueViewModel person1, MpEnumerableParameterValueViewModel person2) {
            if (((object)person1) == null || ((object)person2) == null)
                return Object.Equals(person1, person2);

            return person1.Equals(person2);
        }

        public static bool operator !=(MpEnumerableParameterValueViewModel person1, MpEnumerableParameterValueViewModel person2) {
            if (((object)person1) == null || ((object)person2) == null)
                return !Object.Equals(person1, person2);

            return !(person1.Equals(person2));
        }

        #endregion

        #endregion

        #region Private Methods

        private void MpAnalyticItemParameterValueViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if(IsBusy || Parent.IsBusy) {
                        return;
                    } 
                    if(Parent is MpListBoxParameterViewModel mscbpvm) {
                        mscbpvm.OnPropertyChanged(nameof(mscbpvm.SelectedItems));
                    } else if(Parent is MpComboBoxParameterViewModel cbpvm) {
                        cbpvm.OnPropertyChanged(nameof(cbpvm.SelectedItem));
                    }
                    Parent.OnPropertyChanged(nameof(Parent.CurrentValue));
                    HasModelChanged = true;
                    break;
                case nameof(HasModelChanged):
                    if (IsBusy || Parent.IsBusy) {
                        //workaround for initializing where value falls back to parameter default and not preset value
                        return;
                    }
                    if (HasModelChanged) {
                        Parent.HasModelChanged = true;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.CurrentValue));
                    break;
            }
            Parent.Parent.OnPropertyChanged(nameof(Parent.Parent.IsAllValid));
            //(Parent.Parent.ExecuteAnalysisCommand as RelayCommand).RaiseCanExecuteChanged();
        }

        #endregion
    }
}
