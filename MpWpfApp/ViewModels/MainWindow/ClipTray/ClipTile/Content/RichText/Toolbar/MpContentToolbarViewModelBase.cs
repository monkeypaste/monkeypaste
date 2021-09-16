using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public abstract class MpContentToolbarViewModelBase : MpUndoableViewModelBase<MpContentToolbarViewModelBase> {
        #region Properties

        #region View Models

        private MpContentContainerViewModel _containerViewModel;
        public MpContentContainerViewModel ContainerViewModel {
            get {
                return _containerViewModel;
            }
            set {
                if (_containerViewModel != value) {
                    _containerViewModel = value;
                    OnPropertyChanged(nameof(ContainerViewModel));
                    OnPropertyChanged(nameof(HostClipTileViewModel));
                    OnPropertyChanged(nameof(SubSelectedItemViewModel));
                }
            }
        }

        public MpClipTileViewModel HostClipTileViewModel {
            get {
                if (ContainerViewModel == null) {
                    return null;
                }

                return ContainerViewModel.HostClipTileViewModel;
            }
        }

        public MpContentItemViewModel SubSelectedItemViewModel {
            get {
                if (ContainerViewModel == null) {
                    return null;
                }

                return ContainerViewModel.PrimarySubSelectedClipItem;
            }
        }

        #endregion

        #region Layout 

        #endregion

        #endregion

        #region Public Methods

        public MpContentToolbarViewModelBase() : base() { }

        public MpContentToolbarViewModelBase(MpContentContainerViewModel ccvm) : this() {
            ContainerViewModel = ccvm;
        }
        #endregion
    }
}
