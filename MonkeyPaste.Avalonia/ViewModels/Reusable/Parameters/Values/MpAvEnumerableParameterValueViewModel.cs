using Avalonia.Controls;
using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;


namespace MonkeyPaste.Avalonia {
    public class MpAvEnumerableParameterValueViewModel :
        MpAvViewModelBase<MpAvEnumerableParameterViewModelBase>,
        MpIPopupMenuViewModel,
        MpIContentQueryTextBoxViewModel {
        #region Private Variables

        #endregion

        #region Interfaces
        #region MpIContentQueryTextBoxViewModel Implementation
        ICommand MpIContentQueryTextBoxViewModel.OpenPopOutWindowCommand => null;
        bool MpIContentQueryTextBoxViewModel.IsWindowOpen => false;
        bool MpIContentQueryTextBoxViewModel.CanPopOut => false;
        bool MpIContentQueryTextBoxViewModel.IsSecure =>
            false;
        public bool IsFieldButtonVisible =>
            Parent == null ?
                false :
                Parent.UnitType == MpParameterValueUnitType.PlainTextContentQuery ||
                Parent.UnitType == MpParameterValueUnitType.RawDataContentQuery ||
                Parent.UnitType == MpParameterValueUnitType.DelimitedPlainTextContentQuery;

        public bool IsActionParameter { get; set; } = false;

        public string ContentQuery {
            get => Value;
            set => Value = value;
        }
        public string Watermark =>
            Label;


        public ICommand ClearQueryCommand => new MpCommand(() => {
            if (Parent is MpAvEditableEnumerableParameterViewModel epvm) {
                epvm.RemoveValueCommand.Execute(this);
            }
        });

        public ICommand ShowQueryMenuCommand => new MpCommand<object>(
            (args) => {
                MpAvMenuView.ShowMenu(
                    target: args as Control,
                    dc: PopupMenuViewModel);
            });

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

        public MpAvMenuItemViewModel PopupMenuViewModel =>
            MpAvContentQueryPropertyPathHelpers.GetContentPropertyRootMenu(
                AddContentPropertyPathCommand,
                IsActionParameter ? null : new[] { MpContentQueryPropertyPathType.LastOutput });


        public bool IsPopupMenuOpen { get; set; }
        #endregion

        #endregion

        #region Properties

        #region Appearance

        public string BackgroundBrush {
            get {
                if (IsSelected) {
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

        public bool IsDragAbove { get; set; }
        public bool IsDragBelow { get; set; }

        public bool CanDeleteOrMove =>
            Parent is MpAvEditableEnumerableParameterViewModel &&
            (Parent as MpAvEditableEnumerableParameterViewModel).CanDeleteOrMoveValue;

        public MpCsvFormatProperties CsvProperties =>
            Parent == null ? MpCsvFormatProperties.Default : Parent.CsvProperties;

        public bool IsHovering { get; set; } = false;

        public bool IsSelected =>
            Parent == null ? false : Parent.Selection.SelectedItems.Contains(this);

        public int ValueIdx { get; set; } = 0;

        #endregion

        #region Model

        public bool IsReadOnly {
            get {
                if (Parent == null) {
                    return false;
                }
                return Parent.ParameterFormat.controlType != MpParameterControlType.EditableList;
            }
        }

        private string _label;
        public string Label {
            get {
                if (string.IsNullOrEmpty(_label)) {
                    return Value;
                }
                return _label;
            }
            set {
                if (_label != value) {
                    _label = value;
                    OnPropertyChanged(nameof(Label));
                }
            }
        }

        public string Value { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpAvEnumerableParameterValueViewModel() : this(null) { }

        public MpAvEnumerableParameterValueViewModel(MpAvEnumerableParameterViewModelBase parent) : base(parent) {
            PropertyChanged += MpAnalyticItemParameterValueViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods

        public async Task InitializeAsync(int idx, string label, string value) {
            IsBusy = true;

            await Task.Delay(1);

            ValueIdx = idx;
            Label = label;
            Value = value;

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
                    if (IsBusy || Parent.IsBusy) {
                        return;
                    }
                    //if (Parent.ControlType == MpParameterControlType.ComboBox) {
                    //    // workaround since using selectedIdx cause of selection bug
                    //    break;
                    //}
                    if (IsSelected) {

                    } else {

                    }

                    //Parent.OnPropertyChanged(nameof(Parent.SelectedItem));
                    //Parent.OnPropertyChanged(nameof(Parent.SelectedItems));
                    //Parent.OnPropertyChanged(nameof(Parent.CurrentValue));
                    //Parent.CurrentValue = Parent.SelectedItems.Select(x => x.Value).ToList().ToCsv(CsvProperties);
                    Parent.OnPropertyChanged(nameof(Parent.CurrentValue));
                    break;
                case nameof(Value):
                    if (Parent == null) {
                        break;
                    }
                    Parent.OnPropertyChanged(nameof(Parent.CurrentValue));
                    //Parent.CurrentValue = Parent.SelectedItems.Select(x => x.Value).ToList().ToCsv(CsvProperties);
                    dynamic pp = Parent.Parent;
                    Parent.Parent.OnPropertyChanged(nameof(pp.IsAllValid));
                    break;
            }
        }
        #endregion

        #region Commands

        public ICommand AddContentPropertyPathCommand => new MpCommand<object>(
            (args) => {
                if (args == null) {
                    return;
                }
                var cppt = (MpContentQueryPropertyPathType)args;
                if (cppt == MpContentQueryPropertyPathType.None) {
                    if (args is int intArg) {
                        cppt = (MpContentQueryPropertyPathType)intArg;
                    }
                    if (cppt == MpContentQueryPropertyPathType.None) {
                        return;
                    }
                }

                string pathStr = string.Format(@"{{{0}}}", cppt.ToString());
                Value = Value.Remove(SelectionStart, SelectionLength).Insert(SelectionStart, pathStr);
            });

        public ICommand RemoveThisValueCommand => new MpCommand(
            () => {
                (Parent as MpAvEditableEnumerableParameterViewModel).RemoveValueCommand.Execute(this);
            },
            () => {
                return Parent is MpAvEditableEnumerableParameterViewModel eepvm && eepvm.CanDeleteOrMoveValue;
            });
        #endregion
    }
}
