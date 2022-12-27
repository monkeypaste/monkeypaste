using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using MonkeyPaste.Common.Avalonia.Utils.Extensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public partial class MpAvClipTileView : MpAvUserControl<MpAvClipTileViewModel> {
        public MpAvClipTileView() {
            InitializeComponent();
            DataContextChanged += MpAvClipTileView_DataContextChanged;
            this.AttachedToVisualTree += MpAvCefNetContentWebView_AttachedToVisualTree;
        }
        private void MpAvCefNetContentWebView_AttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e) {
            //InitDrop();
        }

        #region Drop
        private MpPoint _last_wv_mp;

        private void InitDrop() {
            DragDrop.SetAllowDrop(this, true);
            this.AddHandler(DragDrop.DragEnterEvent, DragEnter);
            this.AddHandler(DragDrop.DragOverEvent, DragOver);
            this.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
            this.AddHandler(DragDrop.DropEvent, Drop);
        }

        private void DragEnter(object sender, DragEventArgs e) {
            RelayDropEventMessage("dragenter", e);
        }

        private void DragOver(object sender, DragEventArgs e) {
            RelayDropEventMessage("dragover", e);
        }
        private void DragLeave(object sender, RoutedEventArgs e) {
            RelayDropEventMessage("dragleave", e);
        }

        private void Drop(object sender, DragEventArgs e) {
            RelayDropEventMessage("drop", e);
        }

        private void RelayDropEventMessage(string eventType, RoutedEventArgs args) {
            var wv = this.GetVisualDescendant<MpAvCefNetWebView>();
            if (wv == null) {
                return;
            }
            MpPoint wv_mp;
            MpQuillHostDataItemsMessageFragment dt_frag = null;
            DragEventArgs drag_e = args as DragEventArgs;
            if (drag_e != null) {
                dt_frag = drag_e.Data.ToDataItemFragment(drag_e.DragEffects);
                wv_mp = drag_e.GetPosition(wv).ToPortablePoint();
                _last_wv_mp = wv_mp;
            } else {
                // for drag leave use last event (probably drag over) mp
                wv_mp = _last_wv_mp;
            }

            var msg = new MpQuillDragDropEventMessage() {
                eventType = eventType,
                screenX = wv_mp.X,
                screenY = wv_mp.Y,
                dataItemsFragment = dt_frag
            };
            if(drag_e != null) {
                msg = drag_e.KeyModifiers.SetJsModKeys(msg);
            }
            wv.ExecuteJavascript($"dragEventFromHost_ext('{msg.SerializeJsonObjectToBase64()}')");
        }
        #endregion
        private void MpAvClipTileView_DataContextChanged(object sender, EventArgs e) {
            if(BindingContext == null) {
                return;
            }
            BindingContext.PropertyChanged += BindingContext_PropertyChanged;
        }

        private void BindingContext_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
            // BUG workaround for not being able to bind to row definition w/o getting binding null warning
            switch(e.PropertyName) {
                case nameof(BindingContext.IsHeaderAndFooterVisible):
                case nameof(BindingContext.IsTitleVisible):
                case nameof(BindingContext.IsDetailVisible):
                    var tg = this.FindControl<Grid>("TileGrid");
                    if (tg == null) {
                        return;
                    }
                    string rd = "0.25*,*,20";
                    if(BindingContext.IsTitleVisible && BindingContext.IsDetailVisible) {
                        
                    } else if(BindingContext.IsTitleVisible) {
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
