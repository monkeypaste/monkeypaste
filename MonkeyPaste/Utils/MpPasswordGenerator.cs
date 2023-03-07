using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonkeyPaste {
    public static class MpPasswordGenerator {

        public const string AlphaNumericChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        public const string OtherChars = @"`~!@#$%^*()_-+[{]}\|;':,<./";

        private static string _passwordChars;
        public static string PasswordChars {
            get {
                if (_passwordChars == null) {
                    _passwordChars = AlphaNumericChars + OtherChars;
                }
                return _passwordChars;
            }
        }
        //private static string _passwordChars = null;
        //public static string PasswordChars {
        //    get {
        //        if (_passwordChars == null) {
        //            var sb = new StringBuilder();
        //            for (int i = char.MinValue; i <= char.MaxValue; i++) {
        //                char c = Convert.ToChar(i);
        //                if (!char.IsControl(c)) {
        //                    sb.Append(c);
        //                }
        //            }
        //            _passwordChars = sb.ToString();
        //        }
        //        return _passwordChars;
        //    }
        //}

        public static string GetRandomPassword(int minLength = 6, int maxLength = 12, string valid_chars = null) {
            valid_chars = valid_chars ?? PasswordChars;
            int length = MpRandom.Rand.Next(minLength, maxLength);
            var sb = new StringBuilder();
            for (int i = 0; i < length; i++) {
                int idx = MpRandom.Rand.Next(valid_chars.Length);
                sb.Append(valid_chars[idx]);
            }
            return sb.ToString();
        }
    }
}
