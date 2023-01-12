using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvDataObjectMessageViewModel  : MpAvClipTileTransactionItemMessageViewModelBase, MpITransactionNodeViewModel {

        #region Interfaces
        #endregion

        #region Properties

        public object Body { get; }
        public override string LabelText => "Clipboard";
        #region View Models
        #endregion

        #region State

        #endregion

        #region Model

        public MpPortableDataObject DataObject { get; private set; }


        #endregion

        #endregion

        #region Constructors

        public MpAvDataObjectMessageViewModel(MpAvClipTileTransactionItemViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods
        public override async Task InitializeAsync(object jsonOrParsedFragment, MpITransactionNodeViewModel parentAnnotation) {
            IsBusy = true;

            if(Items != null) {
                Items.Clear();
            }
            ParentTreeItem = parentAnnotation;
            DataObject = MpJsonObject.DeserializeObject<MpPortableDataObject>(jsonOrParsedFragment);
            if(DataObject != null) {
                foreach(var kvp in DataObject.DataFormatLookup) {
                    MpAvDataObjectItemViewModel doivm = new MpAvDataObjectItemViewModel(Parent);
                    await doivm.InitializeAsync(kvp,this);
                    Items.Add(doivm);
                }
            }
            OnPropertyChanged(nameof(Children));
            OnPropertyChanged(nameof(Items));
            IsBusy = false;
        }


        #endregion

        #region Commands


        #endregion
    }
}
