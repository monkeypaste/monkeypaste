using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MpWpfApp {
    public abstract class MpTriggerActionViewModelBase : 
        MpActionViewModelBase,
        MpIResizableViewModel,
        MpISidebarItemViewModel,
        MpIActionDesignerCollectionViewModel {
        #region Properties

        #region View Models

        public ObservableCollection<MpActionViewModelBase> AllChildren => new ObservableCollection<MpActionViewModelBase>(this.FindAllChildren());

        #endregion

        #region MpISidebarItemViewModel Implementation

        public double DefaultSidebarWidth => MpMeasurements.Instance.DefaultDesignerWidth;
        public double SidebarWidth { get; set; } = MpMeasurements.Instance.DefaultDesignerWidth;

        public bool IsSidebarVisible { get; set; }
        public MpISidebarItemViewModel NextSidebarItem => null;
        public MpISidebarItemViewModel PreviousSidebarItem => Parent;

        #endregion


        #region MpIResizableViewModel Implementation

        public bool IsResizing { get; set; }
        public bool CanResize { get; set; }

        #endregion

        #region MpIActionDesignerCollectionViewModel Implementation

        public double CameraZoomFactor { get; set; } = 1.0;

        public double DesignerWidth { get; set; } = 300;
        public double DesignerHeight { get; set; } = 250;

        public double CameraX { get; set; } = 150;
        public double CameraY { get; set; } = 150;

        #endregion

        #region Appearance

        #endregion

        #region State

        public bool IsDesignerVisible { get; set; } = false;

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
                    HasModelChanged = true;
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

        public override void Validate() {
            if(TriggerType != MpTriggerType.ParentOutput && string.IsNullOrWhiteSpace(Label)) {
                ValidationText = "Trigger Action Must Have Name";
            }
        }
        #endregion
    }
}
