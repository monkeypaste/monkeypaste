using Avalonia.Controls;
using HtmlAgilityPack;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PropertyChanged;
using Avalonia;
using Avalonia.Data;
using Avalonia.Styling;
using Avalonia.Input;
using MonkeyPaste.Avalonia;
using MonkeyPaste;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvTextBox : 
        TextBox, 
        MpAvIDragSource,
        IStyleable {
        #region Private Variables

        #endregion

        #region Statics

        static MpAvTextBox() {
            TextProperty.Changed.AddClassHandler<MpAvTextBox>((x, y) => HandleTextChanged(x, y));
        }

        private static void HandleTextChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if (element is MpAvTextBox tb) {
                //RaisePropertyChanged(TextProperty, oldValue, value);
            }
        }


        #endregion

        #region Overrides
        Type IStyleable.StyleKey => typeof(TextBox);

        #endregion

        #region Properties


        #region MpAvIDragSource Implementation

        public bool WasDragCanceled { get; set; } = false;

        public PointerPressedEventArgs LastPointerPressedEventArgs { get; }

        public void NotifyModKeyStateChanged(bool ctrl, bool alt, bool shift, bool esc, bool meta) {
            throw new NotImplementedException();
        }

        public void NotifyDropComplete(DragDropEffects dropEffect) {
            throw new NotImplementedException();
        }

        public Task<MpAvDataObject> GetDataObjectAsync(bool forOle, string[] formats = null) {
            throw new NotImplementedException();
        }

        #endregion

        #endregion

        #region Constructors

        public MpAvTextBox() : base() {
        }

        #endregion

        #region Public Methods


        #endregion

        #region Private Methods


        #endregion
    }
}
