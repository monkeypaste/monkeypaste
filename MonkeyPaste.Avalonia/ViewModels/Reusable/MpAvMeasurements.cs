using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonkeyPaste;

namespace MonkeyPaste.Avalonia {
    public class MpAvMeasurementsViewModel : MpISingletonViewModel<MpAvMeasurementsViewModel> {
        #region Statics
        private static MpAvMeasurementsViewModel? _instance;
        public static MpAvMeasurementsViewModel Instance => _instance ??= new MpAvMeasurementsViewModel();
        #endregion

        #region Properties

        
        #endregion

        #region Public Methods

        public void Init() {
            
        }

        #endregion
    }
}
