using MonkeyPaste;
using MonkeyPaste.Common.Plugin; 
using MonkeyPaste.Common; using MonkeyPaste.Common.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpClassifyOutput : MpActionOutput {
        public override object OutputData => TagId;
        public int TagId { get; set; }
        public override string ActionDescription => $"CopyItem({CopyItem.Id},{CopyItem.Title}) Classified to Tag({TagId})";
    }

    public class MpClassifyActionViewModel : MpActionViewModelBase, MpIActionPluginComponent {
        #region Private Variables

        #endregion

        #region Properties

        #region View Models

        public MpTagTileViewModel SelectedTag {
            get {
                if (MpMainWindowViewModel.Instance.IsMainWindowLoading) {
                    return null;
                }
                return MpTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);
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

        public MpClassifyActionViewModel(MpActionCollectionViewModel parent) : base(parent) {
        }

        #endregion

        #region Protected Overrides
        protected override void Instance_OnItemDeleted(object sender, MpDbModelBase e) {
            if (e is MpTag t && t.Id == TagId) {
                Task.Run(ValidateAsync);
            }
        }
        protected override async Task<bool> ValidateAsync() {
            await base.ValidateAsync();

            if (!IsValid) {
                return IsValid;
            }

            while(MpTagTrayViewModel.Instance.IsAnyBusy) {
                await Task.Delay(100);
            }

            var ttvm = MpTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);
            if (ttvm == null) {
                ValidationText = $"Tag for Classifier '{RootTriggerActionViewModel.Label}/{Label}' not found";

                await ShowValidationNotificationAsync();
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

            var ttvm = MpTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);
            await ttvm.AddContentItem(actionInput.CopyItem.Id);

            await base.PerformActionAsync(new MpClassifyOutput() {
                Previous = arg as MpActionOutput,
                CopyItem = actionInput.CopyItem,
                TagId = TagId
            });
        }

        #endregion
    }
}
