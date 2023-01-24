using MonkeyPaste;
using MonkeyPaste.Common.Plugin; 
using MonkeyPaste.Common; 
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;

namespace MonkeyPaste.Avalonia {

    public enum MpSelectableComponentType {
        None = 0,
        Collection,
        Action,
        Analyzer
    };

    public class MpAvComponentPickerParameterViewModel : 
        MpAvParameterViewModelBase,
        MpIPopupSelectorMenu {
        #region Private Variables

        #endregion

        #region MpIPopupSelectorMenu Implementation

        public bool IsOpen { get; set; }
        public MpMenuItemViewModel PopupMenu =>
            ComponentPicker == null ? null : ComponentPicker.GetMenu(SelectComponentCommand, null,new int[] { ComponentId }, true);

        public object SelectedIconResourceObj =>
            ComponentId == 0 ?
                DefaultIconResourceObj :
                SelectedComponentMenuItemViewModel == null ?
                    "WarningImage" :
                    SelectedComponentMenuItemViewModel.IconSourceObj;
        public string SelectedLabel =>
            ComponentId == 0 ?
                $"Select {ComponentType}..." :
                SelectedComponentMenuItemViewModel == null ?
                    "Not found..." :
                    SelectedComponentMenuItemViewModel.Header;
        #endregion

        #region Properties

        #region View Models

        public MpMenuItemViewModel SelectedComponentMenuItemViewModel =>
            SelectedComponentPicker == null ? null : SelectedComponentPicker.GetMenu(null, null, new List<int>() { }, false);

        #endregion

        #region Appearance

        public object DefaultIconResourceObj =>
            ComponentType switch {
                MpSelectableComponentType.Collection =>
                    "PinToCollectionImage",
                MpSelectableComponentType.Action =>
                    "BoltImage",
                MpSelectableComponentType.Analyzer =>
                    "BrainImage",
                _ =>
                    "QuestionMarkImage",
            };
        #endregion

        #region State

        public MpIPopupMenuPicker SelectedComponentPicker =>
            ComponentId == 0 ? null :
            ComponentType switch {
                MpSelectableComponentType.Collection =>
                    MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x=>x.TagId == ComponentId),
                MpSelectableComponentType.Action =>
                    (Parent as MpAvActionViewModelBase).RootTriggerActionViewModel.SelfAndAllDescendants.FirstOrDefault(x=>x.ActionId == ComponentId),
                MpSelectableComponentType.Analyzer =>
                    MpAvAnalyticItemCollectionViewModel.Instance.AllPresets.FirstOrDefault(x=>x.AnalyticItemPresetId == ComponentId),
                _ => null
            };
        
        public MpIPopupMenuPicker ComponentPicker =>
            ComponentType switch {
                MpSelectableComponentType.Collection =>
                    MpAvTagTrayViewModel.Instance.AllTagViewModel,
                MpSelectableComponentType.Action =>
                    (Parent as MpAvActionViewModelBase).RootTriggerActionViewModel,
                MpSelectableComponentType.Analyzer =>
                    MpAvAnalyticItemCollectionViewModel.Instance,
                _ => null
            };

        public MpSelectableComponentType ComponentType =>
           UnitType switch {
               MpParameterValueUnitType.CollectionComponentId => 
                        MpSelectableComponentType.Collection,
                    MpParameterValueUnitType.ActionComponentId => 
                        MpSelectableComponentType.Action,
                    MpParameterValueUnitType.AnalyzerComponentId => 
                        MpSelectableComponentType.Analyzer,
                    _ => MpSelectableComponentType.None
           };


        #endregion

        #region Model

        public int ComponentId {
            get {
                return IntValue;
            }
            set {
                if (ComponentId != value) {
                    IntValue = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(ComponentId));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpAvComponentPickerParameterViewModel() : base(null) { }

        public MpAvComponentPickerParameterViewModel(MpIParameterHostViewModel parent) : base(parent) { }

        public override async Task InitializeAsync(MpPluginPresetParameterValue aipv) {
            IsBusy = true;

            await base.InitializeAsync(aipv);

            IsBusy = false;
        }

        #endregion

        #region Commands

        public ICommand SelectComponentCommand => new MpCommand<object>(
            (args) => {
                if (args is int componentId) {
                    if (ComponentId == componentId) {
                        ComponentId = 0;
                    } else {
                        ComponentId = componentId;
                    }
                    OnPropertyChanged(nameof(SelectedComponentMenuItemViewModel));
                    OnPropertyChanged(nameof(SelectedComponentPicker));
                    OnPropertyChanged(nameof(SelectedLabel));
                    OnPropertyChanged(nameof(SelectedIconResourceObj));

                    MpPlatformWrapper.Services.ContextMenuCloser.CloseMenu();
                }
            });


        #endregion
    }
}
