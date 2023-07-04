using System;
using System.Collections;
using System.Collections.Generic;

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

        public static int[] GetUniqueRandomInts(int min, int max, int count) {
            List<int> vals = new List<int>();
            while (vals.Count < count) {
                int val = Rand.Next(min, max);
                while (vals.Contains(val)) {
                    val = Rand.Next(min, max);
                }
                vals.Add(val);
            }
            return vals.ToArray();
        }
    }
}
