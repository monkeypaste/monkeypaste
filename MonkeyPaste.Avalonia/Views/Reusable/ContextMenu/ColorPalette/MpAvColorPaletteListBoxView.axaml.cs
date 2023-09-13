using Avalonia.Controls;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvColorPaletteListBoxView : MpAvUserControl<MpAvMenuItemViewModel> {
        public MpAvColorPaletteListBoxView() {
            InitializeComponent();
        }

        private void Btn_EffectiveViewportChanged(object sender, global::Avalonia.Layout.EffectiveViewportChangedEventArgs e) {
            if (sender is not Button btn ||
                btn.Content is not Border brdr ||
                btn.Parent is not ListBoxItem lbi ||
                lbi.DataContext is not MpAvMenuItemViewModel color_item_mivm ||
                lbi.GetVisualAncestor<MenuItem>() is not MenuItem mi) {
                return;
            }

            int row = (int)Math.Floor((double)color_item_mivm.SortOrderIdx / (double)MpSystemColors.COLOR_PALETTE_COLS);
            int col = color_item_mivm.SortOrderIdx % MpSystemColors.COLOR_PALETTE_COLS;
            Grid.SetRow(lbi, row);
            Grid.SetColumn(lbi, col);

            double pad = mi.Bounds.Width / 2;
            double color_item_size = 16;
            //(mi.Bounds.Width - pad) / MpSystemColors.COLOR_PALETTE_COLS;
            lbi.Width = color_item_size;
            lbi.Height = color_item_size;

            btn.Width = color_item_size;
            btn.Height = color_item_size;

            brdr.Width = color_item_size;
            brdr.Height = color_item_size;
        }
    }
}
