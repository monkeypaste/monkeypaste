using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MonkeyPaste {
    public interface MpIBindingContext<VM>
        where VM : class {

        //FE ThisFrameworkElement { get; }

        VM BindingContext { get; set; }

        VM GetBindingContext();

        void SetBindingContext(VM vm);
    }
}
