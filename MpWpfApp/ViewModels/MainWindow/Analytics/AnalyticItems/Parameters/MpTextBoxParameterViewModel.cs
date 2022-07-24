using MonkeyPaste;
using MonkeyPaste.Common.Plugin; using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;
using Newtonsoft.Json;
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
        MpPluginParameterViewModelBase,
        MpIMenuItemViewModel,
        MpITextSelectionRange,
        MpIContentQueryTextBoxViewModel {
        #region Private Variables
        
        private string _defaultValue;

        #endregion

        #region Properties

        #region View Models

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

        #region MpITextSelectionRangeViewModel Implementation 

        public int SelectionStart => MpTextBoxSelectionRangeExtension.GetSelectionStart(this);
        public int SelectionLength => MpTextBoxSelectionRangeExtension.GetSelectionLength(this);

        public string SelectedPlainText {
            get => MpTextBoxSelectionRangeExtension.GetSelectedPlainText(this);
            set => MpTextBoxSelectionRangeExtension.SetSelectionText(this, value);
        }


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

        public bool IsContentQuery => UnitType == MpPluginParameterValueUnitType.PlainTextContentQuery;

        #endregion

        #endregion

        #region Constructors

        public MpTextBoxParameterViewModel() : base(null) { }

        public MpTextBoxParameterViewModel(MpIPluginComponentViewModel parent) : base(parent) {
            PropertyChanged += MpTextBoxParameterViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpPluginPresetParameterValue aipv) {
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
            } else if (IllegalCharacters != null && CurrentValue
                        .Split(new string[] { string.Empty },StringSplitOptions.None)
                        .Any(x=> IllegalCharacters.Contains(x))) {
                ValidationMessage = $"{Label} cannot contain {ParameterFormat.illegalCharacters} characters";
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

                 var cm = MpContextMenuView.Instance;
                 cm.DataContext = ContextMenuItemViewModel;
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
                var cppt = (MpCopyItemPropertyPathType)args;
                if (cppt == MpCopyItemPropertyPathType.None) {
                    return;
                }

                string pathStr = string.Format(@"{{{0}}}", cppt.ToString());
                CurrentValue = CurrentValue.Remove(SelectionStart, SelectionLength).Insert(SelectionStart, pathStr);
            });

        #endregion
    }
}
