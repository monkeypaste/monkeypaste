using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpClassifyOutput : MpActionOutput {
        public int TagId { get; set; }
    }

    public class MpClassifyActionViewModel : MpActionViewModelBase {
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

        public override async Task PerformAction(object arg) {
            MpCopyItem ci = null;
            if (arg is MpCopyItem) {
                ci = arg as MpCopyItem;
            } else if (arg is MpCompareOutput co) {
                ci = co.CopyItem;
            } else if (arg is MpAnalyzeOutput ao) {
                ci = ao.CopyItem;
            } else if (arg is MpClassifyOutput clo) {
                ci = clo.CopyItem;
            }

            if (!IsEnabled) {
                return;
            }

            var ttvm = MpTagTrayViewModel.Instance.Items.FirstOrDefault(x => x.TagId == TagId);
            await ttvm.AddContentItem(ci.Id);

            await base.PerformAction(new MpClassifyOutput() {
                Previous = arg as MpActionOutput,
                CopyItem = ci,
                TagId = TagId
            });
        }
        #endregion
    }
}
