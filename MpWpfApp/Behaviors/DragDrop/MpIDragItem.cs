using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public interface MpIDragItem {
        event EventHandler<MouseButtonEventArgs> MouseLeftButtonDown;
        event EventHandler<MouseButtonEventArgs> MouseLeftButtonUp;
        event EventHandler<MouseEventArgs> MouseMove;

        Transform RenderTransform { get; set; }
    }

    public interface MpIDropContainer {
        Orientation Orientation { get; }

        AdornerLayer AdornerLayer { get; }

        Transform RenderTransform { get; }

        ScrollViewer ScrollViewer { get; }


    }
}
