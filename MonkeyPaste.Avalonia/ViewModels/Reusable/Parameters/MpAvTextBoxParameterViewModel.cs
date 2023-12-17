using Avalonia.Controls;
using Avalonia.Media;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvTextBoxParameterViewModel :
        MpAvParameterViewModelBase,
        MpIWantsTopmostWindowViewModel,
        MpIWindowStateViewModel,
        MpICloseWindowViewModel,
        MpIPopupMenuViewModel,
        MpIContentQueryTextBoxViewModel {
        #region Private Variables
        #endregion

        #region Interfaces

        #region MpIPopupMenuViewModel Implementation

        public MpAvMenuItemViewModel PopupMenuViewModel =>
            MpAvContentQueryPropertyPathHelpers.GetContentPropertyRootMenu(
                AddContentPropertyPathCommand,
                IsActionParameter ? null : new[] { MpContentQueryPropertyPathType.LastOutput });

        public bool IsPopupMenuOpen { get; set; }

        #endregion

        #region MpIWindowViewModel Implementation

        public WindowState WindowState { get; set; }
        public MpWindowType WindowType =>
            MpWindowType.PopOut;

        public bool WantsTopmost =>
            true;
        public bool IsWindowOpen { get; set; }
        #endregion

        #region MpIContentQueryTextBoxViewModel Implementation

        public bool CanPopOut { get; set; } = true;
        bool MpIContentQueryTextBoxViewModel.IsSecure =>
            ControlType == MpParameterControlType.PasswordBox;

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
                MpAvMenuView.ShowMenu(
                    target: args as Control,
                    dc: PopupMenuViewModel);
            });
        public string Watermark =>
            Placeholder;

        #region MpITextSelectionRange Implementation 
        public int SelectionStart { get; set; }
        public int SelectionEnd { get; set; }
        public string SelectedPlainText {
            get {
                if (string.IsNullOrEmpty(Text)) {
                    return string.Empty;
                }
                try {
                    return Text.Substring(Math.Min(SelectionStart, SelectionEnd), SelectionLength);
                }
                catch (Exception ex) {
                    MpConsole.WriteTraceLine($"SelectedText error.", ex);
                    return string.Empty;
                }
            }
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

        public bool IsContentQuery =>
            UnitType == MpParameterValueUnitType.PlainTextContentQuery;

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

            IsBusy = false;
        }

        protected override void MpAnalyticItemParameterViewModel_OnValidate(object sender, EventArgs e) {
            base.MpAnalyticItemParameterViewModel_OnValidate(sender, e);

            if (CurrentValue != null && CurrentValue.Length < MinLength) {
                ValidationMessage = string.Format(UiStrings.ParameterInvalidLengthCaption, Label, MinLength);
            } else if (CurrentValue != null && CurrentValue.Length > MaxLength) {
                ValidationMessage = string.Format(UiStrings.ParameterInvalidLengthCaption2, Label, MinLength);
            } else if (!string.IsNullOrEmpty(Pattern) && !Regex.IsMatch(CurrentValue, Pattern)) {
                ValidationMessage = PatternInfo;
                if (string.IsNullOrWhiteSpace(ValidationMessage)) {
                    ValidationMessage = string.Format(UiStrings.ParameterInvalidDefaultPatternInfoCaption, Label);
                }
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
                    //MpConsole.WriteLine($"Start: {SelectionStart} Length: {SelectionLength}");
                    break;
                case nameof(CurrentValue):
                    (this as MpIContentQueryTextBoxViewModel).OnPropertyChanged(nameof(MpIContentQueryTextBoxViewModel.ContentQuery));
                    break;
                case nameof(ControlType):
                    if (ControlType == MpParameterControlType.PasswordBox) {
                        CanPopOut = false;
                    }
                    break;
            }
        }

        private MpAvWindow CreatePopoutWindow() {
            var pow = new MpAvWindow() {
                DataContext = this,
                ShowInTaskbar = true,
                Width = 300,
                Height = 300,
                Title = Label.ToWindowTitleText(),
                Icon = MpAvIconSourceObjToBitmapConverter.Instance.Convert("JigsawImage", null, null, null) as WindowIcon,
                Content = new MpAvContentQueryTextBoxView(),
                Background = Mp.Services.PlatformResource.GetResource<IBrush>(MpThemeResourceKey.ThemeInteractiveBgColor.ToString())
            };
            return pow;
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

                CurrentValue = CurrentValue.Remove(SelectionStart, SelectionLength).Insert(SelectionStart, cppt.ToQueryFragmentString());
            });
        public ICommand ShowContentQueryPopupCommand => new MpCommand<object>(
            (args) => {

                MpAvMenuView.ShowMenu(
                    target: args as Control,
                    dc: PopupMenuViewModel);
            });


        public ICommand OpenPopOutWindowCommand => new MpCommand(
            () => {
                if (IsWindowOpen) {
                    WindowState = WindowState.Normal;
                    return;
                }
                CreatePopoutWindow().Show();
            });

        #endregion
    }
}
