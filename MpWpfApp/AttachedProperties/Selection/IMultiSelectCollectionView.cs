using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls.Primitives;

namespace MpWpfApp
{
    public interface IMultiSelectCollectionView
    {
        void AddControl(Selector selector);
        void RemoveControl(Selector selector);
    }
}
