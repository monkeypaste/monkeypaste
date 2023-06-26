using MonkeyPaste.Common;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MonkeyPaste {
    public static class MpCustomDbFunctions {

        public static void AddCustomFunctions(sqlite3 handle) {
            raw.sqlite3_create_function(handle, "REGEXP", 2, null, MatchRegex);
            raw.sqlite3_create_function(handle, "PIXEL_COUNT", 2, null, PixelColorCount);
            raw.sqlite3_create_function(handle, "HEX_MATCH", 2, null, HexColorMatch);

            raw.sqlite3_create_function(handle, "FILE_NAME_FILTER", 1, null, FilterFileName);
            raw.sqlite3_create_function(handle, "FILE_EXT_FILTER", 1, null, FilterFileExt);
        }

        private static void FilterFileName(sqlite3_context ctx, object user_data, sqlite3_value[] args) {
            string filter_val = string.Empty;
            // unix uses forward slashes but backslash is escape
            string pattern = raw.sqlite3_value_text(args[0]).utf8_to_string();
            if (pattern.Contains("/")) {
                // presume its unix, windows won't have forward slash
                var pattern_parts = pattern.SplitNoEmpty("/");
                if (pattern_parts.Length > 1) {
                    // remove escape char
                    filter_val = pattern_parts[pattern_parts.Length - 1].Replace(@"\", string.Empty);
                }
            } else if (pattern.Contains(@"\")) {
                //presume its windows
                var pattern_parts = pattern.SplitNoEmpty(@"\");
                if (pattern_parts.Length > 1) {
                    // remove escape double quote
                    filter_val = pattern_parts[pattern_parts.Length - 1].Replace("\"", string.Empty);
                }
            }

            raw.sqlite3_result_text(ctx, filter_val);
        }
        private static void FilterFileExt(sqlite3_context ctx, object user_data, sqlite3_value[] args) {
            string filter_val = string.Empty;

            string pattern = raw.sqlite3_value_text(args[0]).utf8_to_string();
            var pattern_parts = pattern.SplitNoEmpty(".");
            if (pattern_parts.Any()) {
                // remove double quotes in case its an escaped windows path
                filter_val = pattern_parts.Last().Replace("\"", string.Empty);
            }
            raw.sqlite3_result_text(ctx, filter_val);
        }
        private static void MatchRegex(sqlite3_context ctx, object user_data, sqlite3_value[] args) {
            string pattern = raw.sqlite3_value_text(args[0]).utf8_to_string();
            pattern = pattern == null ? string.Empty : pattern;

            string input = raw.sqlite3_value_text(args[1]).utf8_to_string();
            input = input == null ? string.Empty : input;

            if (args.Length > 2) {
                string test = raw.sqlite3_value_text(args[2]).utf8_to_string();
                test = test == null ? string.Empty : test;
                MpDebug.Break();
            }
            bool isMatched = false;
            try {
                isMatched = Regex.IsMatch(input, pattern);

            }
            catch (Exception ex) {
                MpConsole.WriteTraceLine($"Regex exception using pattern '{pattern}'", ex);
                isMatched = false;
            }

            if (isMatched) {
                raw.sqlite3_result_int(ctx, 1);
            } else {
                raw.sqlite3_result_int(ctx, 0);
            }
        }

        private static void PixelColorCount(sqlite3_context ctx, object user_data, sqlite3_value[] args) {
            int count = 0;
            string color_str = raw.sqlite3_value_text(args[0]).utf8_to_string();
            if (string.IsNullOrEmpty(color_str)) {
                raw.sqlite3_result_int(ctx, count);
                return;
            }

            string base64Str = raw.sqlite3_value_text(args[1]).utf8_to_string();
            if (string.IsNullOrEmpty(base64Str)) {
                raw.sqlite3_result_int(ctx, count);
                return;
            }
            var color_dist_tup = ParseColorAndDistance(color_str);
            color_str = color_dist_tup.Item1;
            double max_dist = color_dist_tup.Item2;
            count = Mp.Services.ColorQueryTools.ColorPixelCount(base64Str, color_str, max_dist);
            raw.sqlite3_result_int(ctx, count);
        }

        private static void HexColorMatch(sqlite3_context ctx, object user_data, sqlite3_value[] args) {
            string color_str = raw.sqlite3_value_text(args[0]).utf8_to_string();

            string hexColorFieldStr = raw.sqlite3_value_text(args[1]).utf8_to_string();
            if (string.IsNullOrEmpty(hexColorFieldStr)) {
                // when no color db, its false
                raw.sqlite3_result_int(ctx, 0);
                return;
            }
            if (string.IsNullOrEmpty(color_str)) {
                // when match color provided its true (implying db color exists)
                raw.sqlite3_result_int(ctx, 1);
                return;
            }

            var color_dist_tup = ParseColorAndDistance(color_str);
            color_str = color_dist_tup.Item1;
            double max_dist = color_dist_tup.Item2;
            bool is_match = Mp.Services.ColorQueryTools.IsHexColorMatch(hexColorFieldStr, color_str, max_dist);
            raw.sqlite3_result_int(ctx, is_match ? 1 : 0);
        }
        private static Tuple<string, double> ParseColorAndDistance(string color_str) {
            double max_dist = 0;
            if (color_str.Contains(",")) {
                if (color_str.StartsWith("(")) {
                    // channel str
                    if (color_str.EndsWith(")")) {
                        // no distance 
                    } else if (color_str.Contains("),")) {
                        try {
                            var channel_dist_parts = color_str.SplitNoEmpty("),");
                            color_str = color_str.Substring(0, channel_dist_parts[0].Length + 1);
                            max_dist = double.Parse(channel_dist_parts[1]);
                        }
                        catch (Exception ex) {
                            MpConsole.WriteTraceLine($"Error parsing color distance from color str '{color_str}'", ex);
                        }
                    } else {
                        MpDebug.Break($"Whats this color str? '{color_str}'");
                    }
                } else {
                    // hex or named color
                    try {
                        var hex_parts = color_str.SplitNoEmpty(",");
                        color_str = hex_parts[0];
                        max_dist = double.Parse(hex_parts[1]);
                    }
                    catch (Exception ex) {
                        MpConsole.WriteTraceLine($"Error parsing color distance from color str '{color_str}'", ex);
                    }
                }
            }
            return new Tuple<string, double>(color_str, max_dist);
        }
    }
}
