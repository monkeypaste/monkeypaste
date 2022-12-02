using Avalonia;
using Avalonia.Data;
using Avalonia.Threading;
using MonkeyPaste.Common;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvCefNetPlainHtmlConverterWebView : MpAvCefNetWebView {
        #region Private Variables

        #endregion

        #region Statics
        public static string HTML_CONVERTER_PARAMS => "converter=true";

        #endregion

        #region Properties
        public override string ContentUrl => base.ContentUrl+$"?{HTML_CONVERTER_PARAMS}";
        #endregion

        #region Constructors
        #endregion

        #region Public Methods

        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
