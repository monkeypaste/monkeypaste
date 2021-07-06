using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MpWpfApp {
    public abstract class MpRestfulApi  {
        private string _apiName = string.Empty;

        protected MpRestfulApi(string apiName) {
            _apiName = apiName;
        }

        protected abstract int GetMaxCallCount();
        protected abstract int GetCurCallCount();
        protected abstract void IncrementCallCount();
        protected abstract void ClearCount();

        private void RefreshCount() {
            var diff = DateTime.Today - Properties.Settings.Default.RestfulBillingDate;
            if (diff.TotalDays >= 30) {
                // TODO refresh billing date probably need to use window store api
                ClearCount();
            }
        }

        protected Nullable<bool> CheckRestfulApiStatus() {
            if(!MpHelpers.Instance.IsConnectedToInternet()) {
                MessageBox.Show("Please connect to internet to use "+_apiName, "No Internet Connection", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            RefreshCount();

            if(GetCurCallCount() < GetMaxCallCount()) {
                return true;
            }

            MessageBox.Show("You have reached your usage limit for " + _apiName, "Limit Reached", MessageBoxButton.OK,MessageBoxImage.Error);

            return false;
        }

        protected void ShowError() {
            MessageBox.Show("There was an error using " + _apiName, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
