using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvCopyItemSourceViewModel : MpAvTransactionSourceViewModel {

        #region Interfaces
        #endregion

        #region Properties

        #region View Models

        private MpAvClipTileViewModel _clipTileViewModel;
        public MpAvClipTileViewModel ClipTileViewModel {
            get {
                if (_clipTileViewModel == null ||
                    _clipTileViewModel.CopyItemId != CopyItemId) {
                    _clipTileViewModel = MpAvClipTrayViewModel.Instance.AllItems.FirstOrDefault(x => x.CopyItemId == CopyItemId);
                }
                return _clipTileViewModel;
            }
        }


        #endregion

        #region State
        #endregion

        #region Model

        public int CopyItemId { get; private set; }

        #endregion

        #endregion

        #region Constructors

        public MpAvCopyItemSourceViewModel(MpAvTransactionItemViewModel parent) : base(parent) { }

        #endregion

        #region Public Methods
        public override async Task InitializeAsync(MpTransactionSource ts) {
            IsBusy = true;
            await base.InitializeAsync(ts);

            CopyItemId = ts.SourceObjId;

            OnPropertyChanged(nameof(Children));
            OnPropertyChanged(nameof(ClipTileViewModel));
            OnPropertyChanged(nameof(Body));

            IsBusy = false;
        }

        #endregion

        #region Commands
        #endregion
    }
}
