using MonkeyPaste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MpWpfApp {
    public class MpCompareActionViewModel : MpActionViewModelBase {
        #region Private Variables

        private Regex _regEx = null;

        #endregion

        #region Properties

        #region Model

        public string SourcePropertyPath {
            get {
                if (Action == null) {
                    return null;
                }
                return Action.Arg1;
            }
            set {
                if (SourcePropertyPath != value) {
                    Action.Arg1 = value;
                    OnPropertyChanged(nameof(SourcePropertyPath));
                }
            }
        }

        public string CompareData {
            get {
                if (Action == null) {
                    return null;
                }
                return Action.Arg2;
            }
            set {
                if (CompareData != value) {
                    Action.Arg2 = value;
                    OnPropertyChanged(nameof(CompareData));
                }
            }
        }

        public MpCompareType CompareType {
            get {
                if (Action == null) {
                    return MpCompareType.None;
                }
                return (MpCompareType)Action.ActionObjId;
            }
            set {
                if (CompareType != value) {
                    Action.ActionObjId = (int)value;
                    OnPropertyChanged(nameof(CompareType));
                }
            }
        }

        #endregion

        #endregion


        #region Constructors

        public MpCompareActionViewModel(MpActionCollectionViewModel parent) : base(parent) {

        }


        public override async Task InitializeAsync(MpAction m) {
            await base.InitializeAsync(m);

            if (CompareType == MpCompareType.Regex) {
                _regEx = new Regex(
                    CompareData,
                    RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture);
            }
        }

        #endregion

        #region Protected Overrides

        protected override async Task PerformAction(MpCopyItem arg) {
            object matchVal = arg.GetPropertyValue(SourcePropertyPath);
            string compareStr = string.Empty;
            if (matchVal != null) {
                compareStr = matchVal.ToString();
            }

            if (IsMatch(compareStr)) {
                await base.PerformAction(arg);
            }
        }

        #endregion

        #region Private Methods

        private bool IsMatch(string compareStr) {
            switch (CompareType) {
                case MpCompareType.Contains:
                    if (compareStr.ToLower().Contains(CompareData.ToLower())) {
                        return true;
                    }
                    break;
                case MpCompareType.Exact:
                    if (compareStr.ToLower().Equals(CompareData.ToLower())) {
                        return true;
                    }
                    break;
                case MpCompareType.BeginsWith:
                    if (compareStr.ToLower().StartsWith(CompareData.ToLower())) {
                        return true;
                    }
                    break;
                case MpCompareType.EndsWith:
                    if (compareStr.ToLower().EndsWith(CompareData.ToLower())) {
                        return true;
                    }
                    break;
                case MpCompareType.Regex:
                    if (_regEx != null && _regEx.IsMatch(compareStr)) {
                        return true;
                    }
                    break;
            }
            return false;
        }

        #endregion
    }
}
