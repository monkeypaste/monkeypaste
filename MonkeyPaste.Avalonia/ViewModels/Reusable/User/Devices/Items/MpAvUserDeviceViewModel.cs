

using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvUserDeviceViewModel : MpAvViewModelBase<MpAvUserDeviceCollectionViewModel> {
        #region Private Variable
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region Model

        public int UserDeviceId {
            get {
                if (UserDevice == null) {
                    return 0;
                }
                return UserDeviceId;
            }
        }

        public MpUserDevice UserDevice { get; set; }
        #endregion

        #endregion

        #region Constructors
        public MpAvUserDeviceViewModel() : this(null) { }
        public MpAvUserDeviceViewModel(MpAvUserDeviceCollectionViewModel parent) : base(parent) {
        }
        #endregion

        #region Public Methods
        public async Task InitializeAsync(MpUserDevice ud) {
            IsBusy = true;

            await Task.Delay(1);
            UserDevice = ud;

            IsBusy = false;
        }

        public override string ToString() {
            return UserDevice == null ? base.ToString() : UserDevice.ToString();
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
