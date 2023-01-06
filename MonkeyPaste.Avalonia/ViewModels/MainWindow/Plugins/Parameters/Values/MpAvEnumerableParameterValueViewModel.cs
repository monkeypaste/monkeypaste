

using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using System.Windows.Input;


namespace MonkeyPaste.Avalonia {
    public class MpAvEnumerableParameterValueViewModel : 
        MpViewModelBase<MpAvEnumerableParameterViewModel>,
        MpIPopupMenuViewModel,
        MpIContentQueryTextBoxViewModel {
        #region Private Variables

        #endregion

        #region Properties

        #region MpIContentQueryTextBoxViewModel Implementation

        public bool IsActionParameter { get; set; } = false;

        public string ContentQuery {
            get => Value;
            set => Value = value;
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
                if (Parent == null) {
                    return null;
                }
                return Parent.RemoveValueCommand;
            }
        }
        bool MpIContentQueryTextBoxViewModel.IsPathSelectorPopupOpen { get; set; }
        #endregion

        #region MpITextSelectionRangeViewModel Implementation 

        public int SelectionStart { get; set; }
        public int SelectionEnd { get; set; }
        public string SelectedPlainText {
            get => Text.Substring(SelectionStart, SelectionLength);
            set {
                string pre_text = Text.Substring(SelectionStart);
                string post_text = Text.Substring(SelectionLength);
                Text = $"{pre_text}{value}{post_text}";
                SelectionEnd = SelectionStart + value.Length;
            }
        }
        public int SelectionLength => SelectionEnd - SelectionStart;

        public string Text { get; set; }


        #endregion

        #region MpIPopupMenuViewModel Implementation

        public MpMenuItemViewModel PopupMenuViewModel {
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

        public bool IsPopupMenuOpen { get; set; }
        #endregion



        #region Appearance

        public string BackgroundBrush {
            get {
                if(IsSelected) {
                    return MpSystemColors.blue1;
                }
                return MpSystemColors.Transparent;
            }
        }

        public string BorderBrush {
            get {
                if (IsHovering) {
                    return MpSystemColors.Yellow;
                }
                return MpSystemColors.Transparent;
            }
        }
        #endregion

        #region State

        public MpCsvFormatProperties CsvProperties =>
            Parent == null ? MpCsvFormatProperties.Default : Parent.CsvProperties;

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

        public MpAvEnumerableParameterValueViewModel() : this(null) { }

        public MpAvEnumerableParameterValueViewModel(MpAvEnumerableParameterViewModel parent) : base(parent) {
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
                    Parent.CurrentValue = Parent.SelectedItems.Select(x => x.Value).ToList().ToCsv(CsvProperties);
                    break;
                case nameof(Value):
                    Parent.CurrentValue = Parent.SelectedItems.Select(x => x.Value).ToList().ToCsv(CsvProperties);
                    dynamic pp = Parent.Parent;
                    Parent.Parent.OnPropertyChanged(nameof(pp.IsAllValid));
                    break;
            }
        }

        #endregion
    }
}
