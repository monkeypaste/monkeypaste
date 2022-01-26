using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MpWpfApp {
    public abstract class MpTriggerActionViewModelBase : 
        MpActionViewModelBase,
        MpIMenuItemViewModel {
        #region Properties

        #region View Models

        #endregion

        #region MpIMenuItemViewModel Implementation

        public override MpMenuItemViewModel MenuItemViewModel {
            get {
                var amivml = new List<MpMenuItemViewModel>();
                var triggerLabels = typeof(MpActionType).EnumToLabels();
                for (int i = 0; i < triggerLabels.Length; i++) {
                    string resourceKey = string.Empty;
                    switch ((MpActionType)i) {
                        case MpActionType.Analyze:
                            resourceKey = "BrainIcon";
                            break;
                        case MpActionType.Classify:
                            resourceKey = "PinToCollectionIcon";
                            break;
                        case MpActionType.Compare:
                            resourceKey = "ScalesIcon";
                            break;
                        case MpActionType.Macro:
                            resourceKey = "HotkeyIcon";
                            break;
                        case MpActionType.Timer:
                            resourceKey = "AlarmClockIcon";
                            break;
                    }
                    amivml.Add(new MpMenuItemViewModel() {
                        IconResourceKey = Application.Current.Resources[resourceKey] as string,
                        Header = triggerLabels[i],
                        Command = Parent.AddActionCommand,
                        CommandParameter = (MpActionType)i,
                        IsVisible = (MpActionType)i != MpActionType.None && (MpActionType)i != MpActionType.Trigger
                    });
                }
                return new MpMenuItemViewModel() {
                    SubItems = amivml
                };
            }
        }

        #endregion

        #region Appearance

        //public string TriggerTypeLabel => TriggerType.EnumToLabel();

        #endregion

        #region State


        #endregion

        #region Model

        public MpTriggerType TriggerType {
            get {
                if (Action == null) {
                    return MpTriggerType.None;
                }
                return (MpTriggerType)Action.ActionObjId;
            }
            set {
                if (TriggerType != value) {
                    Action.ActionObjId = (int)value;
                    OnPropertyChanged(nameof(TriggerType));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpTriggerActionViewModelBase(MpActionCollectionViewModel parent) : base(parent) {

        }
        #endregion

        #region Public Mehthods


        #endregion
    }
}
