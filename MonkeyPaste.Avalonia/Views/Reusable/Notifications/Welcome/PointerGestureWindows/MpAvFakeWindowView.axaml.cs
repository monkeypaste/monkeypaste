using Avalonia.Input;
using MonkeyPaste.Common;
using PropertyChanged;
using System;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public partial class MpAvFakeWindowView : MpAvUserControl<MpAvFakeWindowViewModel> {
        public const string DRAG_TEXT = "Mmm, freshly dragged bananas";
        public MpAvFakeWindowView() : base() {
            InitializeComponent();
        }


        #region Dnd
        protected override void OnDataContextChanged(EventArgs e) {
            base.OnDataContextChanged(e);
            if (BindingContext == null) {
                return;
            }
            InitDnd();
        }
        private void InitDnd() {
            // NOTE drop is enabled in createFakeWindow (only used in drag-to-open)
            DragDrop.SetAllowDrop(this, BindingContext.IsDndEnabled);

            if (BindingContext.IsDndEnabled) {
                //this.AddHandler(DragDrop.DragEnterEvent, DragEnter);
                this.AddHandler(DragDrop.DragOverEvent, DragOver);
                this.AddHandler(DragDrop.DragLeaveEvent, DragLeave);
                this.AddHandler(DragDrop.DropEvent, Drop);
            } else {
                //this.RemoveHandler(DragDrop.DragEnterEvent, DragEnter);
                this.RemoveHandler(DragDrop.DragOverEvent, DragOver);
                this.RemoveHandler(DragDrop.DragLeaveEvent, DragLeave);
                this.RemoveHandler(DragDrop.DropEvent, Drop);
            }

        }


        //private void DragEnter(object sender, DragEventArgs e) { }
        private void DragOver(object sender, DragEventArgs e) {
            e.DragEffects = GetDropEffect(e);
            if (e.DragEffects == DragDropEffects.None) {
                return;
            }
            BindingContext.IsDragOver = true;
        }
        private void DragLeave(object sender, DragEventArgs e) {
            BindingContext.IsDragOver = false;
        }
        private void Drop(object sender, DragEventArgs e) {
            e.DragEffects = GetDropEffect(e);
            if (e.DragEffects == DragDropEffects.None) {
                return;
            }
            //BindingContext.HasDropped = true;

        }

        private DragDropEffects GetDropEffect(DragEventArgs e) {
            if (BindingContext == null ||
                !BindingContext.IsDndEnabled) {
                return DragDropEffects.None;
            }

            if (e.Data.Get(MpPortableDataFormats.Text) is string drag_text &&
                drag_text == DRAG_TEXT) {
                return DragDropEffects.Copy;
            }
            return DragDropEffects.None;
        }
        #endregion
    }
}
