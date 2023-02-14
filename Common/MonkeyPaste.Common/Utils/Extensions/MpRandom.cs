using System;

namespace MonkeyPaste.Common {
    public static class MpRandom {
        private static Random _Rand;
        public static Random Rand {
            get {
                if (_Rand == null) {
                    _Rand = new Random((int)DateTime.Now.Ticks);
                }
                return _Rand;
            }
        }
    }
}
