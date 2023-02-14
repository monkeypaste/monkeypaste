using System;
using System.Threading.Tasks;
//using Xamarin.Essentials;

namespace MonkeyPaste {
    public class MpContactViewModel : MpViewModelBase<MpContactCollectionViewModel>,
        MpISelectableViewModel {
        #region Properties

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }

        public DateTime LastSelectedDateTime { get; set; }

        #endregion

        #region Model

        public string DisplayValue {
            get {
                if (Contact == null) {
                    return string.Empty;
                }
                if (string.IsNullOrWhiteSpace(Contact.FullName)) {
                    if (string.IsNullOrWhiteSpace(Contact.Email)) {
                        return "No Display Data";
                    }
                    return Contact.Email;
                }
                return Contact.FullName;
            }
        }

        public MpContact Contact { get; set; }

        #endregion

        #endregion

        #region Constructors

        public MpContactViewModel() : base(null) { }

        public MpContactViewModel(MpContactCollectionViewModel parent) : base(parent) {
            PropertyChanged += MpContactViewModel_PropertyChanged;
        }


        #endregion

        #region Public Methods

        public async Task InitializeAsync(MpContact contact) {
            IsBusy = true;

            await Task.Delay(1);
            Contact = contact;

            IsBusy = false;
        }

        #endregion

        #region Private Methods

        private void MpContactViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(IsSelected):
                    if (IsSelected) {
                        LastSelectedDateTime = DateTime.Now;
                    }
                    break;
            }
        }

        #endregion
    }
}
