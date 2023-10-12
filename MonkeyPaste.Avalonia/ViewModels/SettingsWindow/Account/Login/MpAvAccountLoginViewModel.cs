using MonkeyPaste.Common;

namespace MonkeyPaste.Avalonia {
    public class MpAvAccountLoginViewModel : MpAvViewModelBase<MpAvAccountViewModel> {
        #region Private Variables
        #endregion

        #region Constants

        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region Appearance
        public string EmailValidationMessage { get; set; }
        public string PasswordValidationMessage { get; set; }
        public string LoginResultMessage =>
            WasLoginSuccessful.IsNull() ?
                string.Empty :
                WasLoginSuccessful.Value ?
                    UiStrings.AccountLoginSuccessfulText :
                    UiStrings.AccountLoginFailedText;
        #endregion

        #region State
        public bool CanAttempLogin =>
            !string.IsNullOrEmpty(Email) &&
            !string.IsNullOrEmpty(Password);

        public bool IsEmailValid =>
            string.IsNullOrEmpty(EmailValidationMessage);
        public bool IsPasswordValid =>
            string.IsNullOrEmpty(PasswordValidationMessage);
        public bool IsValid =>
            IsEmailValid && IsPasswordValid;

        public bool? WasLoginSuccessful { get; set; }

        #endregion

        #region Model
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Remember { get; set; }
        #endregion
        #endregion

        #region Constructors
        public MpAvAccountLoginViewModel(MpAvAccountViewModel parent) : base(parent) {
            PropertyChanged += MpAvAccountLoginViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        private void MpAvAccountLoginViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(WasLoginSuccessful):
                    OnPropertyChanged(nameof(LoginResultMessage));
                    break;
                case nameof(Email):
                case nameof(Password):
                    OnPropertyChanged(nameof(CanAttempLogin));
                    break;
            }
        }
        private bool Validate() {
            EmailValidationMessage = null;
            PasswordValidationMessage = null;
            if (!MpAvAccountTools.Instance.IsValidPassword(Password)) {
                PasswordValidationMessage = string.Format(UiStrings.AccountRegistrationPasswordValidationText, MpAvAccountTools.MIN_PASSWORD_LENGTH);
            }

            if (!MpAvAccountTools.Instance.IsValidEmail(Email)) {
                EmailValidationMessage = UiStrings.AccountRegistrationInvalidEmailText;
            }
            OnPropertyChanged(nameof(IsEmailValid));
            OnPropertyChanged(nameof(IsPasswordValid));
            return IsValid;
        }
        #endregion

        #region Commands

        public MpIAsyncCommand LoginCommand => new MpAsyncCommand(
            async () => {
                WasLoginSuccessful = null;
                if (!Validate()) {
                    return;
                }
                WasLoginSuccessful = await MpAvAccountTools.Instance.RegisterUserAsync(Email, Password, Remember, Parent.UserAccount);
                Parent.RefreshAccountPage();
            },
            () => {
                return CanAttempLogin;
            });
        #endregion
    }
}
