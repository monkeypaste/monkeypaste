using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MpWpfApp {
    public class MpShortcutKeyViewModel : MpViewModelBase<object> {
        #region Properties
        private int _seqIdx = 0;
        public int SeqIdx {
            get {
                return _seqIdx;
            }
            set {
                if (_seqIdx != value) {
                    _seqIdx = value;
                    OnPropertyChanged_old(nameof(SeqIdx));
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
                    OnPropertyChanged_old(nameof(KeyStr));
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
                    OnPropertyChanged_old(nameof(ShowPlus));
                    OnPropertyChanged_old(nameof(PlusVisibility));
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
                    OnPropertyChanged_old(nameof(ShowComma));
                    OnPropertyChanged_old(nameof(CommaVisibility));
                }
            }
        }

        public Visibility PlusVisibility {
            get {
                return ShowPlus ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public Visibility CommaVisibility {
            get {
                return ShowComma ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        #endregion

        #region Public Methods
        public MpShortcutKeyViewModel() : base(null) { }

        public MpShortcutKeyViewModel(string keyStr,bool showPlus,bool showComma, int seqIdx) : this() {
            KeyStr = keyStr;
            ShowPlus = showPlus;
            ShowComma = showComma;
            SeqIdx = seqIdx;
        }
        #endregion
    }
}
