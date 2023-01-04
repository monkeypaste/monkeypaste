using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;

namespace MonkeyPaste.Avalonia {
    public class MpAvContentAddTriggerViewModel : 
        MpAvTriggerActionViewModelBase {
        #region MpIPluginHost Overrides

        private MpActionPluginFormat _actionComponentFormat;
        public override MpActionPluginFormat ActionComponentFormat {
            get {
                if (_actionComponentFormat == null) {
                    _actionComponentFormat = new MpActionPluginFormat() {
                        parameters = new List<MpPluginParameterFormat>() {
                            new MpPluginParameterFormat() {
                                label = "Trigger",
                                controlType = MpPluginParameterControlType.ComboBox,
                                unitType = MpPluginParameterValueUnitType.PlainText,
                                isRequired = true,
                                paramId = "4",
                                description = "Content (not meeting rejection criteria) of this type will trigger this action.",
                                values = new List<MpPluginParameterValueFormat>() {
                                    new MpPluginParameterValueFormat() {
                                        isDefault = true,
                                        label = "All",
                                        value = MpCopyItemType.None.ToString()
                                    },
                                    new MpPluginParameterValueFormat() {
                                        label = "Text",
                                        value = MpCopyItemType.Text.ToString()
                                    },
                                    new MpPluginParameterValueFormat() {
                                        label = "Image",
                                        value = MpCopyItemType.Image.ToString()
                                    },
                                    new MpPluginParameterValueFormat() {
                                        label = "Files",
                                        value = MpCopyItemType.FileList.ToString()
                                    },
                                }
                            }
                        }
                    };
                }
                return _actionComponentFormat;
            }
        }

        #endregion

        #region Properties

        #region Model

        public MpCopyItemType AddedContentType {
            get {
                //if (Action == null || string.IsNullOrEmpty(Arg4)) {
                //    return MpCopyItemType.None;
                //}
                //return Arg4.ToEnum<MpCopyItemType>();
                if(ArgLookup.ContainsKey("4") && ArgLookup["4"].CurrentValue is string curVal) {
                    return curVal.ToEnum<MpCopyItemType>();
                }
                return MpCopyItemType.None;
            }
            set {
                if (AddedContentType != value) {
                    if (!ArgLookup.ContainsKey("4")) {
                        Debugger.Break();
                        return;
                    }
                    ArgLookup["4"].CurrentValue =   value.ToString();
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(AddedContentType));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpAvContentAddTriggerViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) {

        }
        #endregion

        #region Protected Methods

        protected override async Task ValidateActionAsync() {
            // is always valid
            await Task.Delay(1);
        }
        protected override void EnableTrigger() {
            MpAvClipTrayViewModel.Instance.RegisterActionComponent(this);
        }

        protected override void DisableTrigger() {
            MpAvClipTrayViewModel.Instance.UnregisterActionComponent(this);
        }

        protected override bool CanPerformAction(object arg) {
            if(!base.CanPerformAction(arg)) {
                return false;
            }
            if(AddedContentType == MpCopyItemType.None) {
                // NOTE Default is treated as all types
                return true;
            }
            if(arg is MpCopyItem ci && ci.ItemType != AddedContentType) {
                return false;
            }
            return true;
        }

        #endregion
    }
}
