using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public interface MpIButtonViewModel {
        bool IsHovering { get; set; }
        Brush HoverBorderBrush { get; }
        Brush HoverBackgroundBrush { get; }

        bool IsSelected { get; set; }
        Brush SelectedBorderBrush { get; }
        Brush SelectedBackgroundBrush { get; }

        Brush UnselectedBorderBrush { get; }
        Brush UnselectedBackgroundBrush { get; }

        bool IsEnabled { get; set; }
        Brush DisabledBorderBrush { get; }
        Brush DisabledBackgroundBrush { get; }        

        ICommand Command { get; }
        object CommandParameter { get; }
    }

    public interface MpIToggleButtonViewModel : MpIButtonViewModel {
        bool IsChecked { get; set; }
        Brush CheckedBorderBrush { get; }
        Brush CheckedBackgroundBrush { get; }

        Brush UncheckedBorderBrush { get; }
        Brush UncheckedBackgroundBrush { get; }
    }
}
