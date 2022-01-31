using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public interface MpIRectViewModel {
        double X { get; set; }
        double Y { get; set; }
        double Width { get; set; }
        double Height { get;set; }
    }
}
