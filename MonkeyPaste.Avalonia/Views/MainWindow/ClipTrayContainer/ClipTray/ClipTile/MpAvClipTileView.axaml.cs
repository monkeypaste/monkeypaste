using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileView : MpAvUserControl<MpAvClipTileViewModel> {
        private bool _isOverDetail = false;
        public MpAvClipTileView() {
            InitializeComponent();
            DataContextChanged += MpAvClipTileView_DataContextChanged;
            this.PointerMoved += MpAvClipTileView_PointerMoved;
        }

        private void MpAvClipTileView_PointerMoved(object sender, PointerEventArgs e) {
            if (!BindingContext.IsHovering) {
                // dc mismatch
                Debugger.Break();
            }
        }

        private void MpAvClipTileView_DataContextChanged(object sender, EventArgs e) {
            if (BindingContext == null) {
                return;
            }
            BindingContext.PropertyChanged += BindingContext_PropertyChanged;
        }

        private void BindingContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            // BUG workaround for not being able to bind to row definition w/o getting binding null warning
            switch (e.PropertyName) {
                case nameof(BindingContext.IsHovering):
                    if (this.IsPointerOver != BindingContext.IsHovering) {
                        // dc mismatch
                        Debugger.Break();
                    }
                    break;
                case nameof(BindingContext.IsHeaderAndFooterVisible):
                case nameof(BindingContext.IsTitleVisible):
                case nameof(BindingContext.IsDetailVisible):
                    var tg = this.FindControl<Grid>("TileGrid");
                    if (tg == null) {
                        return;
                    }
                    string rd = "0.25*,*,20";
                    if (BindingContext.IsTitleVisible && BindingContext.IsDetailVisible) {

                    } else if (BindingContext.IsTitleVisible) {
                        rd = "0.25*,*,0";
                    } else if (BindingContext.IsDetailVisible) {
                        rd = "0,*,20";
                    } else {
                        rd = "0,*,0";
                    }
                    tg.RowDefinitions = new RowDefinitions(rd);
                    tg.RowDefinitions[0].MaxHeight = 40.0d;
                    break;
            }
        }

        private void InitializeComponent() {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
