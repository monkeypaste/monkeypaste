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
        MpActionViewModelBase {
        #region Properties

        #region View Models

        #endregion

        #region Appearance

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
