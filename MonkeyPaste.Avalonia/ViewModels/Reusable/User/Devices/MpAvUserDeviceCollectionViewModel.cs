using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public class MpAvUserDeviceCollectionViewModel : MpAvViewModelBase {
        #region Private Variable
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties

        #region View Models
        public ObservableCollection<MpAvUserDeviceViewModel> Items { get; private set; } = new ObservableCollection<MpAvUserDeviceViewModel>();

        public MpAvUserDeviceViewModel CurrentDeviceViewModel =>
            Items.FirstOrDefault(x => x.UserDeviceId == MpDefaultDataModelTools.ThisUserDeviceId);

        #endregion

        #region State


        #endregion

        #endregion

        #region Constructors
        public MpAvUserDeviceCollectionViewModel() : base(null) { }
        #endregion

        #region Public Methods
        public async Task InitializeAsync() {
            IsBusy = true;

            Items.Clear();

            var udl = await MpDataModelProvider.GetItemsAsync<MpUserDevice>();
            foreach (var ud in udl) {
                var udvm = await CreateUserDeviceViewModelAsync(ud);
                Items.Add(udvm);
            }
            while (Items.Any(x => x.IsBusy)) {
                await Task.Delay(100);
            }
            OnPropertyChanged(nameof(Items));
            OnPropertyChanged(nameof(CurrentDeviceViewModel));
            if (CurrentDeviceViewModel == null) {
                MpDebug.Break("error, user device not initialized/discovered");
            } else {

            }

            IsBusy = false;
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        private async Task<MpAvUserDeviceViewModel> CreateUserDeviceViewModelAsync(MpUserDevice ud) {
            var udvm = new MpAvUserDeviceViewModel(this);
            await udvm.InitializeAsync(ud);
            return udvm;
        }

        #endregion

        #region Commands
        #endregion
    }
}
