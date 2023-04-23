using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {

    public enum MpSelectableComponentType {
        None = 0,
        Collection,
        Action,
        Analyzer,
        ContentPropertyPath
    };

    public class MpAvComponentPickerParameterViewModel :
        MpAvParameterViewModelBase,
        MpIPopupSelectorMenuViewModel {
        #region Private Variables

        #endregion

        #region MpIPopupSelectorMenuViewModel Implementation

        public bool IsOpen { get; set; }
        public MpMenuItemViewModel PopupMenu {
            get {
                if (Selector == this) {
                    return ComponentPicker == null ? null : ComponentPicker.GetMenu(SelectComponentCommand, null, new int[] { ComponentId }, true);
                }
                if (Selector is MpMenuItemHostViewModel mihvm) {
                    return mihvm.PopupMenu;
                }
                return null;
            }
        }

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

        public MpMenuItemViewModel SelectedComponentMenuItemViewModel {
            get {
                if (Selector == this) {
                    return SelectedComponentPicker == null ? null : SelectedComponentPicker.GetMenu(null, null, new List<int>() { }, false);
                }
                if (Selector is MpMenuItemHostViewModel mihvm) {
                    return mihvm.FindItemByIdentifier((MpContentQueryPropertyPathType)ComponentId, null);
                }
                return null;
            }
        }


        private MpIPopupSelectorMenuViewModel _selector;
        public MpIPopupSelectorMenuViewModel Selector {
            get {
                if (ComponentType == MpSelectableComponentType.ContentPropertyPath) {
                    if (_selector == null) {
                        _selector = new MpMenuItemHostViewModel(
                            MpContentQueryPropertyPathHelpers.GetContentPropertyRootMenu(
                                SelectComponentCommand,
                                IsActionParameter ? null : new[] { MpContentQueryPropertyPathType.LastOutput }),
                            (MpContentQueryPropertyPathType)ComponentId);
                    }
                    return _selector;
                }
                return this;
            }
        }

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
                MpSelectableComponentType.ContentPropertyPath =>
                    "GraphImage",
                _ =>
                    "QuestionMarkImage",
            };
        #endregion

        #region State

        public MpIPopupMenuPicker SelectedComponentPicker =>
            ComponentId == 0 ? null :
            ComponentType switch {
                MpSelectableComponentType.Collection =>
                    MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == ComponentId),
                MpSelectableComponentType.Action =>
                    (Parent as MpAvActionViewModelBase).RootTriggerActionViewModel.SelfAndAllDescendants.FirstOrDefault(x => x.ActionId == ComponentId),
                MpSelectableComponentType.Analyzer =>
                    MpAvAnalyticItemCollectionViewModel.Instance.AllPresets.FirstOrDefault(x => x.AnalyticItemPresetId == ComponentId),
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
               MpParameterValueUnitType.ContentPropertyPathTypeComponentId =>
                   MpSelectableComponentType.ContentPropertyPath,
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

        public MpAvComponentPickerParameterViewModel(MpViewModelBase parent) : base(parent) { }

        public override async Task InitializeAsync(MpParameterValue aipv) {
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

                    Mp.Services.ContextMenuCloser.CloseMenu();
                }
            });


        #endregion
    }
}
