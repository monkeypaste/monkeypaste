using Avalonia.Controls;
using Avalonia.Interactivity;
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
        ContentPropertyPath,
        ApplicationCommand
    };

    public class MpAvComponentPickerParameterViewModel :
        MpAvParameterViewModelBase,
        MpAvIPopupSelectorMenuViewModel {
        #region Private Variables

        #endregion

        #region MpAvIPopupSelectorMenuViewModel Implementation

        public bool IsOpen { get; set; }
        public MpAvMenuItemViewModel PopupMenu {
            get {
                if (Selector == this) {
                    return ComponentPicker == null ? null : ComponentPicker.GetMenu(SelectComponentCommand, null, new int[] { ComponentId }, true);
                }
                if (Selector is MpAvMenuItemHostViewModel mihvm) {
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
                string.Format(UiStrings.ParamComponentPickerDefaultLabel, Label) :
                SelectedComponentMenuItemViewModel == null ?
                    UiStrings.ParamComponentPickerEmptyLabel :
                    SelectedComponentMenuItemViewModel.Header;
        #endregion

        #region Properties

        #region View Models

        public MpAvMenuItemViewModel SelectedComponentMenuItemViewModel {
            get {
                if (Selector == this) {
                    return SelectedComponentPicker == null ? null : SelectedComponentPicker.GetMenu(null, null, new List<int>() { }, false);
                }
                if (Selector is MpAvMenuItemHostViewModel mihvm) {
                    return mihvm.FindItemByIdentifier((MpContentQueryPropertyPathType)ComponentId, null);
                }
                return null;
            }
        }


        private MpAvIPopupSelectorMenuViewModel _selector;
        public MpAvIPopupSelectorMenuViewModel Selector {
            get {
                if (ComponentType == MpSelectableComponentType.ContentPropertyPath) {
                    if (_selector == null) {
                        _selector = new MpAvMenuItemHostViewModel(
                            MpAvContentQueryPropertyPathHelpers.GetContentPropertyRootMenu(
                                SelectComponentCommand,
                                IsActionParameterAllowLastOutput ? null : new[] { MpContentQueryPropertyPathType.LastOutput }),
                            (MpContentQueryPropertyPathType)ComponentId);
                    }
                    return _selector;
                }
                return this;
            }
        }

        #endregion

        #region Appearance
        public object DefaultIconResourceObj {
            get {
                switch (ComponentType) {
                    case MpSelectableComponentType.Collection:
                        return "TagImage";
                    case MpSelectableComponentType.Action:
                        return "BoltImage";
                    case MpSelectableComponentType.Analyzer:
                        return "BrainImage";
                    case MpSelectableComponentType.ContentPropertyPath:
                        return "GraphImage";
                    case MpSelectableComponentType.ApplicationCommand:
                        return "CommandImage";
                    default:
                        return "QuestionMarkImage";
                }
            }
        }
        #endregion

        #region State
        public MpIPopupMenuPicker SelectedComponentPicker {
            get {
                if (ComponentId == 0) {
                    return null;
                }
                switch (ComponentType) {
                    case MpSelectableComponentType.Collection:
                        return MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == ComponentId);
                    case MpSelectableComponentType.Action:
                        if (Parent is MpAvActionViewModelBase avm) {
                            return avm.RootTriggerActionViewModel.SelfAndAllDescendants.FirstOrDefault(x => x.ActionId == ComponentId);
                        }
                        return null;
                    case MpSelectableComponentType.Analyzer:
                        return MpAvAnalyticItemCollectionViewModel.Instance.AllPresets.FirstOrDefault(x => x.AnalyticItemPresetId == ComponentId);
                    case MpSelectableComponentType.ApplicationCommand:
                        return MpAvShortcutCollectionViewModel.Instance.Items.FirstOrDefault(x => x.ShortcutId == ComponentId);
                    default:
                        return null;
                }
            }
        }
        public MpIPopupMenuPicker ComponentPicker {
            get {
                switch (ComponentType) {
                    case MpSelectableComponentType.Collection:
                        return MpAvTagTrayViewModel.Instance;
                    case MpSelectableComponentType.Action:
                        if (Parent is MpAvActionViewModelBase avm) {
                            return avm.RootTriggerActionViewModel;
                        }
                        return null;
                    case MpSelectableComponentType.Analyzer:
                        return MpAvAnalyticItemCollectionViewModel.Instance;
                    case MpSelectableComponentType.ApplicationCommand:
                        return MpAvShortcutCollectionViewModel.Instance;
                    default:
                        return null;
                }
            }
        }

        public MpSelectableComponentType ComponentType {
            get {
                switch (UnitType) {
                    case MpParameterValueUnitType.CollectionComponentId:
                        return MpSelectableComponentType.Collection;
                    case MpParameterValueUnitType.ActionComponentId:
                        return MpSelectableComponentType.Action;
                    case MpParameterValueUnitType.AnalyzerComponentId:
                        return MpSelectableComponentType.Analyzer;
                    case MpParameterValueUnitType.ContentPropertyPathTypeComponentId:
                        return MpSelectableComponentType.ContentPropertyPath;
                    case MpParameterValueUnitType.ApplicationCommandComponentId:
                        return MpSelectableComponentType.ApplicationCommand;
                    default:
                        return MpSelectableComponentType.None;
                }
            }
        }


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

        public MpAvComponentPickerParameterViewModel(MpAvViewModelBase parent) : base(parent) { }

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

                    MpAvMenuView.CloseMenu();
                }
            });

        public ICommand ShowSelectorMenuCommand => new MpCommand<object>(
            (args) => {
                IsOpen = true;

                var cm = MpAvMenuView.ShowMenu(
                    target: args as Control,
                    dc: PopupMenu);

                void Cm_OnClosed(object sender, RoutedEventArgs e) {
                    cm.Closed -= Cm_OnClosed;
                    IsOpen = false;
                }
                cm.Closed += Cm_OnClosed;
            });
        #endregion
    }
}
