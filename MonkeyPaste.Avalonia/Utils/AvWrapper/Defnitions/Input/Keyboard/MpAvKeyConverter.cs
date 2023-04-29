using Avalonia.Input;
using MonkeyPaste.Common;
using SharpHook.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyPaste.Avalonia {
    public class MpAvKeyConverter : MpIKeyConverterHub {
        #region Private Variables
        private MpIKeyConverter<KeyCode> _globalConverter = new MpGlobalKeyConverter();
        private MpIKeyConverter<Key> _internalConverter = new MpAvInternalKeyConverter();
        #endregion

        #region Constants
        #endregion

        #region Statics
        #endregion

        #region Interfaces
        #endregion

        #region Properties
        #endregion

        #region Constructors
        #endregion

        #region Public Methods
        public string ConvertKeySequenceToString<T>(IEnumerable<IEnumerable<T>> keyList) {
            var ordered_key_list =
                keyList
                .Where(x => x.Any())
                .Select(x => x.OrderBy(y => GetPriority(y)));

            var sb = new StringBuilder();
            foreach (var (combo, comboIdx) in ordered_key_list.WithIndex()) {
                if (comboIdx > 0) {
                    sb.Append(MpInputConstants.SEQUENCE_SEPARATOR);
                }
                foreach (var (k, kIdx) in combo.OrderBy(x => GetPriority(x)).WithIndex()) {
                    sb.Append(GetLiteral(k));
                    if (kIdx < combo.Count() - 1) {
                        sb.Append(MpInputConstants.COMBO_SEPARATOR);
                    }
                }
            }
            return sb.ToString();
        }
        public IReadOnlyList<IReadOnlyList<T>> ConvertStringToKeySequence<T>(string keyStr) where T : Enum {
            var keyList = new List<List<T>>();
            if (string.IsNullOrEmpty(keyStr)) {
                return keyList;
            }

            var combos = keyStr.SplitNoEmpty(MpInputConstants.SEQUENCE_SEPARATOR);
            foreach (var c in combos) {
                var kl = c.SplitNoEmpty(MpInputConstants.COMBO_SEPARATOR);
                keyList.Add(new List<T>());
                foreach (var k in kl) {
                    T t_key = default(T);
                    if (typeof(T) == typeof(KeyCode) &&
                        _globalConverter.ConvertStringToKey(k) is KeyCode gk &&
                        gk != KeyCode.CharUndefined && gk != KeyCode.VcUndefined) {
                        t_key = (T)Convert.ChangeType(gk, Enum.GetUnderlyingType(typeof(T)));
                    } else if (typeof(T) == typeof(Key) &&
                                _internalConverter.ConvertStringToKey(k) is Key ik &&
                                ik != Key.None) {
                        t_key = (T)Convert.ChangeType(ik, Enum.GetUnderlyingType(typeof(T)));
                    }
                    keyList[keyList.Count - 1].Add(t_key);
                }
            }
            return keyList;
        }

        public IReadOnlyList<IReadOnlyList<string>> ConvertStringToKeyLiteralSequence(string keyStr) {
            // NOTE arbitrarily using avalonia keys as intermediary here

            var kseq = ConvertStringToKeySequence<Key>(keyStr);
            var lseq = new List<List<string>>();
            foreach (var kcombo in kseq) {
                var lcombo = new List<string>();
                foreach (var k in kcombo) {
                    lcombo.Add(_internalConverter.GetKeyLiteral(k));
                }
                lseq.Add(lcombo);
            }
            return lseq;
        }
        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods

        private int GetPriority<T>(T key) {
            if (key is KeyCode kc) {
                return _globalConverter.GetKeyPriority(kc);
            }
            if (key is Key k) {
                return _internalConverter.GetKeyPriority(k);
            }
            throw new NotImplementedException($"Unknown key type '{typeof(T)}'");
        }

        private string GetLiteral<T>(T key) {
            if (key is KeyCode kc) {
                return _globalConverter.GetKeyLiteral(kc);
            }
            if (key is Key k) {
                return _internalConverter.GetKeyLiteral(k);
            }
            throw new NotImplementedException($"Unknown key type '{typeof(T)}'");
        }
        #endregion

        #region Commands
        #endregion



    }
}
