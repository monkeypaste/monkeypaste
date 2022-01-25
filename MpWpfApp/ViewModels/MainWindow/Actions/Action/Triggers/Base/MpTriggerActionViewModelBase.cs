using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public abstract class MpTriggerActionViewModelBase : MpActionViewModelBase {
        #region Properties

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

        public override void Enable() {
            switch (TriggerType) {
                case MpTriggerType.ContentItemAdded:
                    
                    break;
                case MpTriggerType.ContentItemAddedToTag:
                    var ttvm = MpTagTrayViewModel.Instance.TagTileViewModels.FirstOrDefault(x => x.TagId == ActionObjId);
                    if (ttvm != null) {
                        ttvm.RegisterTrigger(this);
                    }
                    break;
                case MpTriggerType.Shortcut:
                    var scvm = MpShortcutCollectionViewModel.Instance.Shortcuts.FirstOrDefault(x => x.ShortcutId == ActionObjId);
                    if (scvm != null) {
                        scvm.RegisterTrigger(this);
                    }
                    break;
                case MpTriggerType.FileSystemChange:
                    Task.Run(async () => {
                        var ci = await MpDb.GetItemAsync<MpCopyItem>(ActionObjId);
                        if (ci != null) {
                            if (ci.Source.App.UserDeviceId == MpPreferences.ThisUserDevice.Id) {
                                //only add filesystem watchers for this device
                                MpFileSystemWatcherViewModel.Instance.RegisterTrigger(this);
                            }
                        }
                    });
                    break;
                case MpTriggerType.ParentOutput:
                    var pmvm = Parent.Items.FirstOrDefault(x => x.ActionId == ParentActionId);
                    if (pmvm != null) {
                        ParentActionViewModel = pmvm;
                        ParentActionViewModel.RegisterTrigger(this);
                    }
                    break;
            }

            base.Enable();
        }


        #endregion
    }
}
