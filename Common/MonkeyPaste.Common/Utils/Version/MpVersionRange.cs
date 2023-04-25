using System;
using System.Collections.Generic;
using System.Text;

namespace MonkeyPaste.Common {
    public class MpVersionRange {
        #region Private Variables
        #endregion

        #region Constants
        #endregion

        #region Statics
        public static MpVersionRange None =>
            new MpVersionRange();

        public static MpVersionRange Parse(string text) {
            if (!IsValidVersionText(text)) {
                return None;
            }
            return new MpVersionRange(text);
        }

        public static bool IsValidVersionText(string text) {
            if (string.IsNullOrWhiteSpace(text)) {
                return false;
            }
            return true;
        }
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        private string _rawText;
        public string RawText =>
            _rawText;
        #endregion

        #region Constructors
        public MpVersionRange() : this(null) { }
        public MpVersionRange(string info_text) {
            _rawText = info_text;
        }
        #endregion

        #region Public Methods

        public override string ToString() {
            return RawText;
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }
}
