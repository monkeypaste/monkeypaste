using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpSolidColorBrushComparer  {
        public bool Equals(SolidColorBrush x, SolidColorBrush y) {
            return x.Color.R == y.Color.R && x.Color.G == y.Color.G && x.Color.B == y.Color.B;

        }
    }
}
