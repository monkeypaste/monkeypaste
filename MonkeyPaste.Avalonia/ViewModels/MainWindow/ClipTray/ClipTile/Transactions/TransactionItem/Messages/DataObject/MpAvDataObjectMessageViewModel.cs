using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvDataObjectMessageViewModel  : MpAvTransactionMessageViewModelBase, MpITransactionNodeViewModel {

        #region Interfaces
        #endregion

        #region Properties

        public override object IconResourceObj => "ClipboardImage";

        public override string LabelText => "DataObject";
        #region View Models

        public ObservableCollection<MpAvDataObjectItemViewModel> Items { get; set; } = new ObservableCollection<MpAvDataObjectItemViewModel>();
        #endregion

        #region State

        #endregion

        #region Model

        public MpPortableDataObject DataObject { get; private set; }


        #endregion

        #endregion

        #region Constructors

        public MpAvDataObjectMessageViewModel(MpAvTransactionItemViewModelBase parent) : base(parent) { }

        #endregion

        #region Public Methods
        public override async Task InitializeAsync(object jsonOrParsedFragment, MpITransactionNodeViewModel parentAnnotation) {
            IsBusy = true;

            Json = jsonOrParsedFragment is string ? jsonOrParsedFragment.ToString() : string.Empty;
            if(Items != null) {
                Items.Clear();
            }
            ParentTreeItem = parentAnnotation;
            DataObject = MpPortableDataObject.Parse(Json);
            if(DataObject != null) {
                foreach(var kvp in DataObject.DataFormatLookup) {
                    MpAvDataObjectItemViewModel doivm = new MpAvDataObjectItemViewModel(Parent);
                    await doivm.InitializeAsync(kvp,this);
                    Items.Add(doivm);
                }
            }
            OnPropertyChanged(nameof(Children));
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(Body));
            IsBusy = false;
        }


        #endregion

        #region Commands


        #endregion
    }
}
