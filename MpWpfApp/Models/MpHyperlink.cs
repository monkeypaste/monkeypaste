using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace MpWpfApp  {
    public class MpHyperlink : Hyperlink {
        public MpHyperlink() : base() { }
        public MpHyperlink(TextPointer s, TextPointer e, string linkText) : base(s,e) {
            
        }

    }
    public class MpAddressHyperlink : MpHyperlink {

    }
}
