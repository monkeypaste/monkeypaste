using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public abstract class MpToggleButtonBaseViewModel<P> : MpViewModelBase<P>, MpIToggleButtonViewModel 
        where P:MpViewModelBase {

        public abstract bool IsChecked { get; set; }

        public abstract Brush CheckedBorderBrush { get; }

        public abstract Brush CheckedBackgroundBrush { get; }

        public abstract Brush UncheckedBorderBrush { get; }

        public abstract Brush UncheckedBackgroundBrush { get; }

        public abstract bool IsHovering { get; set; }

        public abstract Brush HoverBorderBrush { get; }

        public abstract Brush HoverBackgroundBrush { get; }

        public abstract bool IsSelected { get; set; }

        public abstract Brush SelectedBorderBrush { get; }

        public abstract Brush SelectedBackgroundBrush { get; }

        public abstract Brush UnselectedBorderBrush { get; }

        public abstract Brush UnselectedBackgroundBrush { get; }

        public abstract bool IsEnabled { get; set; }

        public abstract Brush DisabledBorderBrush { get; }

        public abstract Brush DisabledBackgroundBrush { get; }

        public abstract ICommand Command { get; }

        public abstract object CommandParameter { get; }


        public MpToggleButtonBaseViewModel(P parent) : base(parent) { }
    }
}
