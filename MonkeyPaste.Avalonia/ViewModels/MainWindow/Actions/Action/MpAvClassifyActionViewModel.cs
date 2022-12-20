using MonkeyPaste;
using MonkeyPaste.Common.Plugin;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {

    public class MpAvClassifyActionViewModel : MpAvActionViewModelBase, MpIActionPluginComponent {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        public MpAvTagTileViewModel SelectedTag {
            get {
                if (MpAvMainWindowViewModel.Instance.IsMainWindowLoading) {
                    return null;
                }
                return MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);
            }
            set {
                if (SelectedTag != value) {
                    TagId = value.TagId;
                    OnPropertyChanged(nameof(SelectedTag));
                }
            }
        }

        #endregion

        #region Model

        public int TagId {
            get {
                if(Action == null) {
                    return 0;
                }
                return ActionObjId;
            }
            set {
                if(TagId != value) {
                    ActionObjId = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(TagId));
                    OnPropertyChanged(nameof(SelectedTag));
                }
            }
        }

        #endregion

        #endregion

        #region Constructors

        public MpAvClassifyActionViewModel(MpAvTriggerCollectionViewModel parent) : base(parent) {
        }

        #endregion

        #region Protected Overrides
        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpTag t && t.Id == TagId) {
                Task.Run(Validate);
            }
        }
        protected override async Task<bool> Validate() {
            await base.Validate();

            if (!IsValid) {
                return IsValid;
            }

            while(MpAvTagTrayViewModel.Instance.IsAnyBusy) {
                await Task.Delay(100);
            }

            var ttvm = MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);
            if (ttvm == null) {
                ValidationText = $"Tag for Classifier '{RootTriggerActionViewModel.Label}/{Label}' not found";

                await ShowValidationNotification();
            } else {
                ValidationText = string.Empty;
            }
            return IsValid;
        }

        public override async Task PerformActionAsync(object arg) {
            if (!CanPerformAction(arg)) {
                return;
            }

            var actionInput = GetInput(arg);

            var ttvm = MpAvTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);
            if(ttvm != null && actionInput != null && actionInput.CopyItem != null) {
                ttvm.LinkCopyItemCommand.Execute(actionInput.CopyItem.Id);


            }

            await base.PerformActionAsync(new MpAvClassifyOutput() {
                Previous = arg as MpAvActionOutput,
                CopyItem = actionInput.CopyItem,
                TagId = TagId
            });
        }

        #endregion
    }
}
