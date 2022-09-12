using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Common;


namespace MonkeyPaste.Avalonia {
    public class MpAvFileItemViewModel : MpViewModelBase<MpAvClipTileViewModel>,
        MpISelectableViewModel,
        MpIHoverableViewModel{
        #region Properties

        #region MpISelectableViewModel Implementation

        public bool IsSelected { get; set; }
        public DateTime LastSelectedDateTime { get; set; }
        #endregion

        #region MpIHoverableViewModel Implementation
        public bool IsHovering { get; set; }
        #endregion

        #region Appearance

        public string FileItemBackgroundHexColor {
            get {
                if(IsHovering || IsSelected) {
                    return MpSystemColors.gainsboro;
                }
                return MpSystemColors.Transparent;
            }
        }

        public string FileItemBorderHexColor {
            get {
                if(IsSelected) {
                    return MpSystemColors.Red;
                }
                if (IsHovering) {
                    return MpSystemColors.black;
                }
                return MpSystemColors.Transparent;
            }
        }
        #endregion

        #region Model
        public string Path { get; set; }

        public int SourceDeviceId { get; private set; }

        public string IconBase64 { 
            get;
            private set;
        }
        #endregion
        #endregion
        public MpAvFileItemViewModel() :base (null) { }

        public MpAvFileItemViewModel(MpAvClipTileViewModel parent):base(parent) {

        }

        public async Task InitializeAsync(string path) {
            IsBusy = true;

            await Task.Delay(1);
            Path = path;

            // cache items source device and icon since they are constant (right?)

            var svm = MpAvSourceCollectionViewModel.Instance.Items.FirstOrDefault(x => x.SourceId == Parent.SourceId);
            if(svm == null) {
                Debugger.Break();
            }
            var avm = MpAvAppCollectionViewModel.Instance.Items.FirstOrDefault(x => x.AppId == svm.AppId);
            if(avm == null) {
                Debugger.Break();
            }
            SourceDeviceId = avm.UserDeviceId;

            var ivm = MpAvIconCollectionViewModel.Instance.IconViewModels.FirstOrDefault(x => x.IconId == avm.IconId);
            if(ivm == null) {
                Debugger.Break();
            }
            IconBase64 = ivm.IconBase64;

            IsBusy = false;
        }
    }
}
