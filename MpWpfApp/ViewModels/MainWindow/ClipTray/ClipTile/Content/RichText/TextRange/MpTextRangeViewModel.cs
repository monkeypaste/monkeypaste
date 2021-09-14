using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace MpWpfApp {
    public class MpTextRangeViewModel {
        #region Properties
        public MpTextRangeViewModel Parent { get; set; }

        public int Index { get; set; }

        public int Length { get; set; }

        public Rect Bounds { get; set; }

        public string Text { get; set; }
        #endregion

        #region Public Methods

        #endregion
    }
}
