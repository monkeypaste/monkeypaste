using Android.Content;
using AndroidX.Core.Content;
using AndroidX.Preference;
using Java.Interop;
using Java.Util;
using System.Collections.Generic;
using System.Linq;

namespace iosKeyboardTest.Android {

    public class MyPrefManager : ISharedPrefService {
        ISharedPreferences SharedPrefs { get; set; }
        public double VibrateDurMs { get; private set; }
        public float SoundVol { get; private set; }

        Dictionary<MyPrefKeys, object> _defValLookup;
        public Dictionary<MyPrefKeys, object> DefValLookup {
            get {
                if (_defValLookup == null) {
                    _defValLookup = new Dictionary<MyPrefKeys, object>() {
                        { MyPrefKeys.DO_FIRST_RUN, true },
                        { MyPrefKeys.DO_NUM_ROW, false },
                        { MyPrefKeys.DO_EMOJI_KEY, false },
                        { MyPrefKeys.DO_SOUND, true },
                        { MyPrefKeys.SOUND_LEVEL, 15 },
                        { MyPrefKeys.DO_VIBRATE, true },
                        { MyPrefKeys.VIBRATE_LEVEL, 15 },
                        { MyPrefKeys.DO_POPUP, true },
                        { MyPrefKeys.DO_LONG_POPUP, true },
                        { MyPrefKeys.LONG_POPUP_DELAY, 500 },
                        { MyPrefKeys.DO_NIGHT_MODE, false },
                        { MyPrefKeys.DO_KEY_BOARDERS, true },
                        { MyPrefKeys.BG_OPACITY, 255 },
                        { MyPrefKeys.FG_OPACITY, 255 },
                        { MyPrefKeys.DO_SUGGESTION_STRIP, true },
                        { MyPrefKeys.MAX_COMPLETION_COUNT, 8 },
                        { MyPrefKeys.DO_NEXT_WORD_COMPLETION, true },
                        { MyPrefKeys.DO_AUTO_CORRECT, true },
                        { MyPrefKeys.DO_BACKSPACE_UNDOS_LAST_AUTO_CORRECT, true },
                        { MyPrefKeys.DO_AUTO_CAPITALIZATION, true },
                        { MyPrefKeys.DO_DOUBLE_SPACE_PERIOD, true },
                        { MyPrefKeys.DO_CURSOR_CONTROL, true },
                        { MyPrefKeys.CURSOR_CONTROL_SENSITIVITY_X, 50 },
                        { MyPrefKeys.CURSOR_CONTROL_SENSITIVITY_Y, 50 },
                        { MyPrefKeys.DO_CASE_COMPLETION, true },
                    };
                }
                return _defValLookup;
            }
        }

        public MyPrefManager(Context context) {
            SharedPrefs = PreferenceManager.GetDefaultSharedPreferences(context);
            bool first_run = GetPrefValue<bool>(MyPrefKeys.DO_FIRST_RUN);

            if (first_run) {
                // initial startup
                RestoreDefaults();
                SetPrefValue(MyPrefKeys.DO_FIRST_RUN, false);
            }
        }
        public KeyboardFlags UpdateFlags(KeyboardFlags flags) {
            if (SharedPrefs == null) {
                return flags;
            }
            UpdateVibration();
            UpdateVolume();
            UpdateOpacity();

            if (GetPrefValue<bool>(MyPrefKeys.DO_NIGHT_MODE)) {
                flags &= ~KeyboardFlags.Light;
                flags |= KeyboardFlags.Dark;
            }

            return flags;
        }

        public T GetPrefValue<T>(MyPrefKeys prefKey) where T : struct {
            if (SharedPrefs == null) {
                return (T)(object)DefValLookup[prefKey];
            }
            string key = prefKey.ToString();
            object val = default;
            if (typeof(T) == typeof(bool)) {
                val = SharedPrefs.GetBoolean(key, (bool)DefValLookup[prefKey]);
            } else if (typeof(T) == typeof(int)) {
                val = SharedPrefs.GetInt(key, (int)DefValLookup[prefKey]);
            } else if (typeof(T) == typeof(float)) {
                val = SharedPrefs.GetFloat(key, (float)DefValLookup[prefKey]);
            }
            return (T)(object)val;
        }
        public void SetPrefValue<T>(MyPrefKeys prefKey, T newValue) where T : struct {
            if(SharedPrefs == null) {
                return;
            }
            string key = prefKey.ToString();
            var editor = SharedPrefs.Edit();
            if (typeof(T) == typeof(bool)) {
                editor.PutBoolean(key, (bool)(object)newValue);
            } else if (typeof(T) == typeof(int)) {
                editor.PutInt(key, (int)(object)newValue);
            } 
            editor.Apply();
        }

        void UpdateOpacity() {
            byte bg_op = (byte)GetPrefValue<int>(MyPrefKeys.BG_OPACITY);
            byte fg_op = (byte)GetPrefValue<int>(MyPrefKeys.FG_OPACITY);
            KeyboardPalette.SetTheme(bga: bg_op, fga: fg_op);
        }

        void UpdateVibration() {
            double dur = 0;
            if(GetPrefValue<bool>(MyPrefKeys.DO_VIBRATE)) {
                switch (GetPrefValue<int>(MyPrefKeys.VIBRATE_LEVEL)) {
                    case 1:
                        dur = 2;
                        break;
                    case 2:
                        dur = 20;
                        break;
                    case 3:
                        dur = 100;
                        break;
                    case 4:
                        dur = 500;
                        break;
                    case 5:
                        dur = 1000;
                        break;
                }
            }
            VibrateDurMs = dur;
        }
        void UpdateVolume() {
            float volume = 0;
            if(GetPrefValue<bool>(MyPrefKeys.DO_SOUND)) {
                volume = (float)GetPrefValue<int>(MyPrefKeys.SOUND_LEVEL);
            }
            SoundVol = volume;
        }

        void RestoreDefaults() {
            if(SharedPrefs == null) {
                return;
            }
            var editor = SharedPrefs.Edit();
            foreach(var def_kvp in DefValLookup) {
                string key = def_kvp.Key.ToString();
                object val = def_kvp.Value;
                if(val is bool boolVal) {
                    editor.PutBoolean(key, boolVal);
                } else if(val is int intVal) {
                    editor.PutInt(key, intVal);
                } else if(val is float floatVal) {
                    editor.PutFloat(key, floatVal);
                }
            }
            editor.Commit();
            editor.Apply();
        }

    }
}
