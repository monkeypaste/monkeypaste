using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpClassifyActionViewModel : MpActionViewModelBase {
        #region Private Variables

        #endregion

        #region Properties

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
                    OnPropertyChanged(nameof(TagId));
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

        protected override async Task PerformAction(MpCopyItem arg) {
            if(!IsEnabled) {
                return;
            }

            var ttvm = MpTagTrayViewModel.Instance.TagTileViewModels.FirstOrDefault(x => x.TagId == TagId);
            await ttvm.AddContentItem((arg as MpCopyItem).Id);

            await base.PerformAction(arg);
        }
        #endregion
    }
}
