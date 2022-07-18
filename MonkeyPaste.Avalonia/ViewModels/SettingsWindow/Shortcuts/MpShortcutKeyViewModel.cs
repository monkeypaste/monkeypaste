using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MonkeyPaste;
namespace MonkeyPaste.Avalonia {
    public class MpShortcutKeyViewModel : MpViewModelBase<MpShortcutViewModel> {
        #region Properties

        private int _seqIdx = 0;
        public int SeqIdx {
            get {
                return _seqIdx;
            }
            set {
                if (_seqIdx != value) {
                    _seqIdx = value;
                    OnPropertyChanged(nameof(SeqIdx));
                }
            }
        }

        private string _keyStr = string.Empty;
        public string KeyStr {
            get {
                return _keyStr;
            }
            set {
                if(_keyStr != value) {
                    _keyStr = value;
                    OnPropertyChanged(nameof(KeyStr));
                }
            }
        }

        private bool _showPlus = false;
        public bool ShowPlus {
            get {
                return _showPlus;
            }
            set {
                if(_showPlus != value) {
                    _showPlus = value;
                    OnPropertyChanged(nameof(ShowPlus));
                    OnPropertyChanged(nameof(IsPlusVisible));
                }
            }
        }

        private bool _showComma = false;
        public bool ShowComma {
            get {
                return _showComma;
            }
            set {
                if (_showComma != value) {
                    _showComma = value;
                    OnPropertyChanged(nameof(ShowComma));
                    OnPropertyChanged(nameof(IsCommaVisible));
                }
            }
        }

        public bool IsPlusVisible {
            get {
                return ShowPlus ? true : false;
            }
        }

        public bool IsCommaVisible {
            get {
                return ShowComma ? true : false;
            }
        }
        #endregion

        #region Constructors
        public MpShortcutKeyViewModel() : base(null) { }

        public MpShortcutKeyViewModel(string keyStr,bool showPlus,bool showComma, int seqIdx) : this() {
            KeyStr = keyStr;
            ShowPlus = showPlus;
            ShowComma = showComma;
            SeqIdx = seqIdx;
        }

        public MpShortcutKeyViewModel(MpShortcutViewModel parent, string keyStr, bool showPlus, bool showComma, int seqIdx) : base(parent) {
            KeyStr = keyStr;
            ShowPlus = showPlus;
            ShowComma = showComma;
            SeqIdx = seqIdx;
        }
        #endregion
    }
}
