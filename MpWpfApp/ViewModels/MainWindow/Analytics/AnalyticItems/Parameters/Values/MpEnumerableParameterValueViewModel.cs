using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Office.Interop.Outlook;
using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpEnumerableParameterValueViewModel : 
        MpViewModelBase<MpEnumerableParameterViewModel>,
        MpIMenuItemViewModel,
        MpITextSelectionRange,
        MpIContentQueryTextBoxViewModel {
        #region Private Variables

        #endregion

        #region Properties

        #region MpITextSelectionRangeViewModel Implementation 

        public int SelectionStart => MpTextBoxSelectionRangeExtension.GetSelectionStart(this);
        public int SelectionLength => MpTextBoxSelectionRangeExtension.GetSelectionLength(this);

        public string SelectedPlainText {
            get => MpTextBoxSelectionRangeExtension.GetSelectedPlainText(this);
            set => MpTextBoxSelectionRangeExtension.SetSelectionText(this, value);
        }


        #endregion

        #region MpIMenuItemViewModel Implementation

        public MpMenuItemViewModel ContextMenuItemViewModel {
            get {
                var tmivml = new List<MpMenuItemViewModel>();
                var propertyPathLabels = typeof(MpCopyItemPropertyPathType).EnumToLabels();
                for (int i = 0; i < propertyPathLabels.Length; i++) {
                    var ppt = (MpCopyItemPropertyPathType)i;
                    var mivm = new MpMenuItemViewModel() {
                        Header = propertyPathLabels[i],
                        Command = AddContentPropertyPathCommand,
                        CommandParameter = ppt
                    };
                    if (ppt == MpCopyItemPropertyPathType.None || (ppt == MpCopyItemPropertyPathType.LastOutput && !IsActionParameter)) {
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

                 var cm = MpContextMenuView.Instance;
                 cm.DataContext = ContextMenuItemViewModel;
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
                var cppt = (MpCopyItemPropertyPathType)args;
                if (cppt == MpCopyItemPropertyPathType.None) {
                    return;
                }

                string pathStr = string.Format(@"{{{0}}}", cppt.ToString());
                Value = Value.Remove(SelectionStart, SelectionLength).Insert(SelectionStart, pathStr);
            });

        public ICommand ClearQueryCommand {
            get {
                if(Parent == null) {
                    return null;
                }
                return Parent.RemoveValueCommand;
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
                return Parent.ParameterFormat.controlType != MpPluginParameterControlType.EditableList;
            }
        }

        public string Label { get; set; }

        public string Value { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpEnumerableParameterValueViewModel() : base(null) { }

        public MpEnumerableParameterValueViewModel(MpEnumerableParameterViewModel parent) : base(parent) {
            PropertyChanged += MpAnalyticItemParameterValueViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(int idx, string label, string value, bool isSelected) {
            IsBusy = true;

            await Task.Delay(1);

            ValueIdx = idx;
            Label = label;
            Value = value;
            IsSelected = isSelected;           

            IsBusy = false;
        }

        public override string ToString() {
            return Value;
        }


        #endregion

        #region Private Methods

        private void MpAnalyticItemParameterValueViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if(IsBusy || Parent.IsBusy) {
                        return;
                    }

                    Parent.OnPropertyChanged(nameof(Parent.SelectedItem));
                    Parent.OnPropertyChanged(nameof(Parent.SelectedItems));
                    Parent.OnPropertyChanged(nameof(Parent.CurrentValue));
                    Parent.CurrentValue = Parent.SelectedItems.Select(x => x.Value).ToList().ToCsv();
                    break;
                case nameof(Value):
                    Parent.CurrentValue = Parent.SelectedItems.Select(x => x.Value).ToList().ToCsv();
                    dynamic pp = Parent.Parent;
                    Parent.Parent.OnPropertyChanged(nameof(pp.IsAllValid));
                    break;
            }
        }

        #endregion
    }
}
