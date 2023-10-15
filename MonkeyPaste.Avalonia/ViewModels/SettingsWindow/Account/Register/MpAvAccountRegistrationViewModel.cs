using MonkeyPaste.Common;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvAccountRegistrationViewModel : MpAvViewModelBase<MpAvAccountViewModel> {
        #region Private Variables
        #endregion

        #region Constants
        const int MIN_PASSWORD_LENGTH = 6;

        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region Appearance
        public string EmailValidationMessage { get; set; }
        public string PasswordValidationMessage { get; set; }
        public string RegistrationMessage =>
            WasRegistrationSuccessful.IsNull() ?
                string.Empty :
                WasRegistrationSuccessful.Value ?
                    UiStrings.AccountRegistrationSuccessText :
                    UiStrings.AccountRegistrationRegisterFailedText;
        #endregion

        #region State
        public bool CanAttempRegistration =>
            Parent != null &&
            !Parent.IsRegistered &&
            !string.IsNullOrEmpty(Email) &&
            !string.IsNullOrEmpty(Password) &&
            !string.IsNullOrEmpty(ConfirmPassword);

        public bool IsEmailValid =>
            string.IsNullOrEmpty(EmailValidationMessage);
        public bool IsPasswordValid =>
            string.IsNullOrEmpty(PasswordValidationMessage);
        public bool IsValid =>
            IsEmailValid && IsPasswordValid;

        public bool? WasRegistrationSuccessful { get; set; }

        #endregion

        #region Model
        public bool Remember { get; set; } = true;
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        #endregion
        #endregion

        #region Constructors
        public MpAvAccountRegistrationViewModel(MpAvAccountViewModel parent) : base(parent) {
            PropertyChanged += MpAvAccountRegistrationViewModel_PropertyChanged;
        }

        #endregion

        #region Public Methods
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        private void MpAvAccountRegistrationViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(WasRegistrationSuccessful):
                    OnPropertyChanged(nameof(RegistrationMessage));
                    break;
                case nameof(Email):
                case nameof(Password):
                case nameof(ConfirmPassword):
                    OnPropertyChanged(nameof(CanAttempRegistration));
                    break;
            }
        }
        private bool Validate() {
            EmailValidationMessage = null;
            PasswordValidationMessage = null;
            if (!MpAvAccountTools.Instance.IsValidPassword(Password)) {
                PasswordValidationMessage = string.Format(UiStrings.AccountRegistrationPasswordValidationText, MIN_PASSWORD_LENGTH);
            } else if (Password != ConfirmPassword) {
                PasswordValidationMessage = UiStrings.AccountRegistrationPasswordMismatchValidationText;
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

        public ICommand RegisterAccountCommand => new MpCommand(
            () => {
                WasRegistrationSuccessful = null;
                if (!Validate()) {
                    return;
                }
                //WasRegistrationSuccessful = await MpAvAccountTools.Instance.RegisterUserAsync(
                //    Email, Password, Remember, Parent.UserAccount);
                Parent.RefreshAccountPage();
            },
            () => {
                return CanAttempRegistration;
            });
        #endregion
    }
}
