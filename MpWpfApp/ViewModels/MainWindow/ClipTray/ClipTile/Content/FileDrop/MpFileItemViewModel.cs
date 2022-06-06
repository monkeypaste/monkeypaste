using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Wpf;

namespace MpWpfApp {
    public class MpFileItemViewModel : MpViewModelBase<MpClipTileViewModel>,
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

        #endregion
        #endregion
        public MpFileItemViewModel() :base (null) { }

        public MpFileItemViewModel(MpClipTileViewModel parent):base(parent) {

        }

        public async Task InitializeAsync(string path) {
            IsBusy = true;

            Path = path;

            IsBusy = false;
        }
    }
}
