using System;
using System.Text;

namespace MonkeyPaste {
    public static class MpPasswordGenerator {
        private static Random _Rand;

        public const string AlphaNumericChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        public const string OtherChars = @"`~!@#$%^*()_-+[{]}\|;':,<./";


        public static int MinDbPasswordLength { get; set; } = 12;
        public static int MaxDbPasswordLength { get; set; } = 18;

        private static string _passwordChars = null;
        public static string PasswordChars {
            get {
                if (_passwordChars == null) {
                    var sb = new StringBuilder();
                    for (int i = char.MinValue; i <= char.MaxValue; i++) {
                        char c = Convert.ToChar(i);
                        if (!char.IsControl(c)) {
                            sb.Append(c);
                        }
                    }
                    _passwordChars = sb.ToString();
                }
                return _passwordChars;
            }
        }

        public static string GetRandomPassword() {
            if (_Rand == null) {
                _Rand = new Random((int)DateTime.Now.Ticks);
            }
            return "this_is_a_test_password";

            //int length = _Rand.Next(MinDbPasswordLength, MaxDbPasswordLength);
            //return new string(Enumerable.Repeat(PasswordChars, length).Select(s => s[_Rand.Next(s.Length)]).ToArray());
        }
    }
}
