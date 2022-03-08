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
using Windows.UI.Xaml.Controls.Primitives;

namespace MpWpfApp {
    public class MpContentParameterViewModel : 
        MpAnalyticItemParameterViewModel, 
        MpIMenuItemViewModel,
        MpITextSelectionRangeViewModel {
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

        #region State

        public bool IsActionParameter { get; set; } = false;

        public int CaretIndex { get; set; } = 0;

        #endregion

        #region Model

        public override string CurrentValue { get; set; }

        public override string DefaultValue => _defaultValue;

        #endregion

        #endregion

        #region Constructors

        public MpContentParameterViewModel() : base(null) { }

        public MpContentParameterViewModel(MpAnalyticItemPresetViewModel parent) : base(parent) {
            PropertyChanged += MpContentParameterViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods

        public override async Task InitializeAsync(MpAnalyticItemParameterFormat aipf, MpAnalyticItemPresetParameterValue aipv) {
            IsBusy = true;

            Parameter = aipf;
            ParameterValue = aipv;

            _defaultValue = aipv.Value;
            CurrentValue = DefaultValue;

            OnPropertyChanged(nameof(DefaultValue));

            OnValidate += MpContentParameterViewModel_OnValidate;
            await Task.Delay(1);

            IsBusy = false;
        }

        #endregion

        #region Protected Methods

        #endregion

        #region Private Methods

        private void MpContentParameterViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch(e.PropertyName) {
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
