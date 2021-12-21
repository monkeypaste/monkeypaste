using System.Windows;

namespace MpWpfApp {
    public interface MpIChildWindow {
        Window Window { get; }
        bool Result { get;  }
        bool IsShowing { get; }
        bool IsDialog { get; }
    }
}
