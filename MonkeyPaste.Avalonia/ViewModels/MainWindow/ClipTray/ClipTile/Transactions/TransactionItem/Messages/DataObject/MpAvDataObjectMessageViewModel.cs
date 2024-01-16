using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Plugin;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvDataObjectMessageViewModel : MpAvTransactionMessageViewModelBase, MpAvITransactionNodeViewModel {

        #region Interfaces
        #endregion

        #region Properties

        public override object IconResourceObj => "ClipboardImage";

        public override string LabelText => "DataObject";
        #region View Models

        public ObservableCollection<MpAvTransactionMessageViewModelBase> Items { get; set; } = new ObservableCollection<MpAvTransactionMessageViewModelBase>();
        #endregion

        #region State

        #endregion

        #region Model

        public MpAvDataObject DataObject { get; private set; }


        #endregion

        #endregion

        #region Constructors

        public MpAvDataObjectMessageViewModel(MpAvTransactionItemViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods
        public override async Task InitializeAsync(object jsonOrParsedFragment, MpAvITransactionNodeViewModel parentAnnotation) {
            IsBusy = true;

            Json = jsonOrParsedFragment is string ? jsonOrParsedFragment.ToString() : string.Empty;
            if (Items != null) {
                Items.Clear();
            }
            ParentTreeItem = parentAnnotation;
            DataObject = MpAvDataObject.Parse(Json);
            if (DataObject != null) {
                foreach (var kvp in DataObject.DataFormatLookup) {
                    var doivm = await CreateDataObjectItemViewModel(kvp.Key, kvp.Value);
                    Items.Add(doivm);
                }
            }
            OnPropertyChanged(nameof(Children));
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(Body));
            IsBusy = false;
        }


        #endregion

        #region Private Methods

        private async Task<MpAvTransactionMessageViewModelBase> CreateDataObjectItemViewModel(string format, object data) {
            MpAvTransactionMessageViewModelBase doivm;
            switch (format) {
                case MpPortableDataFormats.INTERNAL_CONTENT_ANNOTATION_FORMAT:
                    doivm = new MpAvAnnotationMessageViewModel(Parent);
                    break;
                default:
                    doivm = new MpAvGenericDataObjectItemViewModel(Parent);
                    break;
            }
            await doivm.InitializeAsync(data, this);
            return doivm;
        }
        #endregion

        #region Commands


        #endregion
    }
}
