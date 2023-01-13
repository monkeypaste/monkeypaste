using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public abstract class MpAvAnnotationViewModelBase : MpAvTransactionItemViewModelBase {
        public MpAvAnnotationViewModelBase(MpAvClipTileTransactionCollectionViewModel parent) : base(parent) {
        }
    }
}
