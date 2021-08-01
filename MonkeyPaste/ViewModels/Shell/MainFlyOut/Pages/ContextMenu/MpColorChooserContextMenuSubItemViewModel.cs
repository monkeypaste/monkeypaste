using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace MonkeyPaste {
    public class MpColorChooserContextMenuSubItemViewModel : MpViewModelBase {
        #region Private Variables

        #endregion

        #region Properties
        public bool IsSelected { get; set; } = false;

        public double BorderSize {
            get {
                return 1;
            }
        }

        public Brush BorderBrush {
            get {
                if(IsSelected) {
                    return Brush.White;
                }
                return Brush.Transparent;
            }
        }

        public double SubItemSize {
            get {
                return 15;
            }
        }
        #endregion

        #region Public Methods
        public MpColorChooserContextMenuSubItemViewModel() : base() { }
        #endregion

        #region Private Methods

        #endregion

        #region Commands

        #endregion
    }
}
