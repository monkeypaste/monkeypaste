using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MpWinFormsApp {
    public static class MpTypeHelper {
        public static object GetPropertyValue(object obj,string name) {
            return obj == null ? null : obj.GetType()
                                           .GetProperty(name)
                                           .GetValue(obj,null);
        }
    }
}
