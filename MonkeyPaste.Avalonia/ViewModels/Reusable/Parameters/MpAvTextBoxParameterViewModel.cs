using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvTextBoxParameterViewModel :
        MpAvParameterViewModelBase,
        MpIPopupMenuViewModel,
        MpIContentQueryTextBoxViewModel {
        #region Private Variables

        //private string _defaultValue;

        #endregion

        #region Interfaces

        #region MpIPopupMenuViewModel Implementation

        public MpAvMenuItemViewModel PopupMenuViewModel =>
            MpAvContentQueryPropertyPathHelpers.GetContentPropertyRootMenu(
                AddContentPropertyPathCommand,
                IsActionParameter ? null : new[] { MpContentQueryPropertyPathType.LastOutput });

        public bool IsPopupMenuOpen { get; set; }

        #endregion

        #region MpIContentQueryTextBoxViewModel Implementation

        public bool IsFieldButtonVisible =>
            UnitType == MpParameterValueUnitType.PlainTextContentQuery ||
            UnitType == MpParameterValueUnitType.RawDataContentQuery ||
            UnitType == MpParameterValueUnitType.DelimitedPlainTextContentQuery;
        public string ContentQuery {
            get => CurrentValue;
            set => CurrentValue = value;
        }

        public ICommand ClearQueryCommand => new MpCommand(
            () => {
                CurrentValue = string.Empty;
            });

        public ICommand ShowQueryMenuCommand => new MpCommand<object>(
            (args) => {
                if (args is not Control c) {
                    return;
                }
                MpAvMenuExtension.ShowMenu(c, PopupMenuViewModel);
            });
        public string Watermark =>
            Placeholder;

        #region MpITextSelectionRange Implementation 
        public int SelectionStart { get; set; }
        public int SelectionEnd { get; set; }
        public string SelectedPlainText {
            get => Text.Substring(Math.Min(SelectionStart, SelectionEnd), SelectionLength);
            set {
                int actual_start_idx = Math.Min(SelectionStart, SelectionEnd);
                string pre_text = Text.Substring(actual_start_idx);
                string post_text = Text.Substring(SelectionLength);
                Text = $"{pre_text}{value}{post_text}";
                SelectionStart = actual_start_idx;
                SelectionEnd = actual_start_idx + value.Length;
            }
        }
        public int SelectionLength => Math.Max(SelectionStart, SelectionEnd) - Math.Min(SelectionStart, SelectionEnd);

        public string Text { get; set; }

        #endregion

        #endregion

        #endregion

        #region Properties

        #region View Models


        #endregion

        #region Appearance

        public string Placeholder {
            get {
                if (ParameterFormat == null) {
                    return string.Empty;
                }
                return ParameterFormat.placeholder;
            }
        }

        #endregion

        #region State


        #endregion

        #region Model

        public bool IsContentQuery => UnitType == MpParameterValueUnitType.PlainTextContentQuery;

        #endregion

        #endregion

        #region Constructors

        public MpAvTextBoxParameterViewModel() : base(null) { }

        public MpAvTextBoxParameterViewModel(MpAvViewModelBase parent) : base(parent) {
            PropertyChanged += MpTextBoxParameterViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpParameterValue aipv) {
            IsBusy = true;

            await base.InitializeAsync(aipv);

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
            if (CurrentValue.Length < MinLength) {
                ValidationMessage = $"{Label} must be at least {MinLength} characters";
            } else if (CurrentValue.Length > MaxLength) {
                ValidationMessage = $"{Label} can only be {MaxLength} characters";
            } else if (!string.IsNullOrEmpty(Pattern) && !Regex.IsMatch(CurrentValue, Pattern)) {
                ValidationMessage = $"{Label} is invalid: Conditions are: '{PatternInfo}'";
            } else {
                ValidationMessage = string.Empty;
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
                case nameof(CurrentValue):
                    (this as MpIContentQueryTextBoxViewModel).OnPropertyChanged(nameof(MpIContentQueryTextBoxViewModel.ContentQuery));
                    break;
            }
        }

        private void MpContentParameterViewModel_OnValidate(object sender, EventArgs e) {
            ValidationMessage = string.Empty;

            OnPropertyChanged(nameof(IsValid));
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
                CurrentValue = CurrentValue.Remove(SelectionStart, SelectionLength).Insert(SelectionStart, pathStr);
            });



        #endregion
    }
}
