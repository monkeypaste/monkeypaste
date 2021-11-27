using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public interface MpIContentDragSource {
        object GetDragData();
        Task<object> PrepareForDrop();
        void StartDrag();
        void CancelDrag();
    }
}
