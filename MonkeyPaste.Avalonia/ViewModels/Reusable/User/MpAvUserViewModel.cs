using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvUserViewModel : MpViewModelBase {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        #region State
        public bool HasAccount =>
            !string.IsNullOrEmpty(Email);

        public bool IsLoggedIn { get; set; }

        #endregion

        #region Model

        public string Email {
            get {
                if (User == null) {
                    return string.Empty;
                }
                return User.Email;
            }
            set {
                if (Email != value) {
                    User.Email = value;
                    HasModelChanged = true;
                    OnPropertyChanged(nameof(Email));
                }
            }
        }
        public MpUser User { get; set; }
        #endregion
        #endregion

        #region Constructors
        #endregion

        #region Public Methods

        public async Task InitializeAsync() {
            IsBusy = true;
            await Task.Delay(1);
            User = new MpUser();
            IsBusy = false;
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
