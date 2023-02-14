using SharpHook.Native;
using System.Collections.Generic;
using System.Linq;

namespace MonkeyPaste.Common.Avalonia {
    public class MpSharpHookMouseButtonMap {


        #region Statics
        private static Dictionary<MouseButton, MpPortablePointerButtonType> _DefaultButtonMap {
            get {
                return new Dictionary<MouseButton, MpPortablePointerButtonType>() {
                    {
                        MouseButton.NoButton, MpPortablePointerButtonType.None
                    },
                    {
                        MouseButton.Button1, MpPortablePointerButtonType.Left
                    },
                    {
                        MouseButton.Button2, MpPortablePointerButtonType.Right
                    },
                    // NOTE rest are unverified
                    {
                        MouseButton.Button3, MpPortablePointerButtonType.Back
                    },
                    {
                        MouseButton.Button4, MpPortablePointerButtonType.Forward
                    },
                    {
                        MouseButton.Button5, MpPortablePointerButtonType.ScrollButton
                    }
                };
            }
        }

        public static MpSharpHookMouseButtonMap Default => new MpSharpHookMouseButtonMap();

        public static MpSharpHookMouseButtonMap Current { get; private set; } = Default;

        public static void SetCurrent(MpSharpHookMouseButtonMap newCurrent) {
            Current = newCurrent;
        }
        #endregion

        #region Properties

        public Dictionary<MouseButton, MpPortablePointerButtonType> ButtonMap { get; set; }

        #endregion

        public MpSharpHookMouseButtonMap() {
            ButtonMap = _DefaultButtonMap;
        }

        public MpSharpHookMouseButtonMap(
            MpPortablePointerButtonType b1,
            MpPortablePointerButtonType b2,
            MpPortablePointerButtonType b3,
            MpPortablePointerButtonType b4,
            MpPortablePointerButtonType b5) {
            ButtonMap = new Dictionary<MouseButton, MpPortablePointerButtonType>() {
                    {
                        MouseButton.NoButton, MpPortablePointerButtonType.None
                    },
                    {
                        MouseButton.Button1, b1
                    },
                    {
                        MouseButton.Button2, b2
                    },
                    {
                        MouseButton.Button3, b3
                    },
                    {
                        MouseButton.Button4, b4
                    },
                    {
                        MouseButton.Button5, b5
                    }
                };
        }

    }

    public static class MpSharpHookMouseButtonExtensions {
        public static MpPortablePointerButtonType ToPortableButton(this MouseButton mb) {
            return MpSharpHookMouseButtonMap.Current.ButtonMap[mb];
        }
        public static MouseButton ToSharpHookButton(this MpPortablePointerButtonType pmb) {
            var kvp = MpSharpHookMouseButtonMap.Current.ButtonMap.FirstOrDefault(x => x.Value == pmb);
            return kvp.Key;
        }
    }
}
