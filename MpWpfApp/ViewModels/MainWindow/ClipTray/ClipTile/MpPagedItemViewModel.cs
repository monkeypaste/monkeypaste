using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpPagedItemViewModel : MpViewModelBase<object> {
        public int ItemId { get; set; }
        public int ItemIdx { get; set; }
        public MpPagedItemViewModel() : base(null) {

        }
    }
}
