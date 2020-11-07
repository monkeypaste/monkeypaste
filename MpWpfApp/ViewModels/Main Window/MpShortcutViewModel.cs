using GalaSoft.MvvmLight.CommandWpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MpWpfApp {
    public class MpShortcutViewModel : MpViewModelBase {
        private string _keyList = string.Empty;
        public string KeyList {
            get {
                return _keyList;
            }
            set {
                if(_keyList != value) {
                    _keyList = value;
                    Shortcut.KeyList = _keyList;
                    OnPropertyChanged(nameof(KeyList));
                }
            }
        }
        private string _shortcutName = string.Empty;
        public string ShortcutName {
            get {
                return _shortcutName;
            }
            set {
                if (_shortcutName != value) {
                    _shortcutName = value;
                    Shortcut.ShortcutName = _shortcutName;
                    OnPropertyChanged(nameof(ShortcutName));
                }
            }
        }

        private bool _isGlobal = false;
        public bool IsGlobal {
            get {
                return _isGlobal;
            }
            set {
                if (_isGlobal != value) {
                    _isGlobal = value;
                    OnPropertyChanged(nameof(IsGlobal));
                }
            }
        }

        private string _shortcutTypeName = string.Empty;
        public string ShortcutTypeName {
            get {
                return _shortcutTypeName;
            }
            set {
                if (_shortcutTypeName != value) {
                    _shortcutTypeName = value;
                    OnPropertyChanged(nameof(ShortcutTypeName));
                }
            }
        }

        private Visibility _deleteButtonVisibility;
        public Visibility DeleteButtonVisibility {
            get {
                return _deleteButtonVisibility;
            }
            set {
                if (_deleteButtonVisibility != value) {
                    _deleteButtonVisibility = value;
                    OnPropertyChanged(nameof(DeleteButtonVisibility));
                }
            }
        }

        private Visibility _resetButtonVisibility;
        public Visibility ResetButtonVisibility {
            get {
                return _resetButtonVisibility;
            }
            set {
                if (_resetButtonVisibility != value) {
                    _resetButtonVisibility = value;
                    OnPropertyChanged(nameof(ResetButtonVisibility));
                }
            }
        }

        public MpShortcut Shortcut { get; set; }

        public MpShortcutViewModel(MpShortcut shortcut) {
            if(shortcut == null) {
                throw new Exception("ShortcutViewModel error, shortcut cannot be null");
            }
            Shortcut = shortcut;
            KeyList = Shortcut.KeyList;
            ShortcutName = Shortcut.ShortcutName;
            IsGlobal = Shortcut.IsGlobal;
            if(Shortcut.IsCustom()) {
                if(Shortcut.CopyItemId > 0) {
                    ShortcutTypeName = "Clip";
                } else {
                    ShortcutTypeName = "Tag";
                }
            } else {
                ShortcutTypeName = "Application";
            }
            if(Shortcut.TagId <= 0 && Shortcut.CopyItemId <= 0) {
                ResetButtonVisibility = Visibility.Visible;
                DeleteButtonVisibility = Visibility.Collapsed;
            } else {
                ResetButtonVisibility = Visibility.Collapsed;
                DeleteButtonVisibility = Visibility.Visible;
            }
        }
    }
}
