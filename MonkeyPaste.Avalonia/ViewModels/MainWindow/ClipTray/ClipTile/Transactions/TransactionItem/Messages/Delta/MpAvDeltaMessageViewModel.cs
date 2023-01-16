using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvDeltaMessageViewModel  : MpAvTransactionMessageViewModelBase, MpITransactionNodeViewModel {

        #region Interfaces
        #endregion

        #region Properties

        public object Body { get; }
        public override string LabelText => "Delta";
        #region View Models

        public ObservableCollection<MpAvDataObjectItemViewModel> Items { get; set; } = new ObservableCollection<MpAvDataObjectItemViewModel>();
        #endregion

        #region State

        #endregion

        #region Model

        public MpQuillDelta QuillDelta { get; set; }


        #endregion

        #endregion

        #region Constructors

        public MpAvDeltaMessageViewModel(MpAvTransactionItemViewModelBase parent) : base(parent) { }

        #endregion

        #region Public Methods
        public override async Task InitializeAsync(object jsonOrParsedFragment, MpITransactionNodeViewModel parentAnnotation) {
            IsBusy = true;
            await Task.Delay(1);

            Json = jsonOrParsedFragment is string ? jsonOrParsedFragment.ToString() : string.Empty;
            if(Items != null) {
                Items.Clear();
            }
            ParentTreeItem = parentAnnotation;
            QuillDelta = MpJsonObject.DeserializeObject<MpQuillDelta>(jsonOrParsedFragment);
            if(QuillDelta != null) {
                
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
