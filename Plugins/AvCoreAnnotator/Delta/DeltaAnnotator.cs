﻿using HtmlAgilityPack;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace AvCoreAnnotator {
    public static class DeltaAnnotator {

        #region Private Variable
        #endregion

        #region Constants
        #endregion

        #region Properties
        #endregion

        #region Constructors
        #endregion

        #region Public Methods

        public static MpQuillDelta Annotate(string plain_text, IEnumerable<MpRegExType> formats) {
            MpQuillDelta delta = new MpQuillDelta() { ops = new List<Op>() };
            if (string.IsNullOrEmpty(plain_text)) {
                return delta;
            }

            foreach (MpRegExType ft in formats) {
                var annotation_type_results = AnnotateType(plain_text, ft);
                delta.ops.AddRange(annotation_type_results.ops);
            }
            delta = ProcessCollisions(delta);

            // order ops by index see (https://quilljs.com/guides/designing-the-delta-format/#pitfalls)
            delta.ops = delta.ops.OrderBy(x => x.format.index).ThenBy(x => x.format.length).ToList();
            delta.ops.ForEach(x => x.retain = 0);
            return delta;
        }
        private static MpQuillDelta AnnotateType(string pt, MpRegExType annotationRegExType) {
            MpQuillDelta delta = new MpQuillDelta() { ops = new List<Op>() };

            if (annotationRegExType == MpRegExType.None ||
                (int)annotationRegExType > MpRegEx.MAX_ANNOTATION_REGEX_TYPE) {
                return delta;
            }

            Regex regex = MpRegEx.RegExLookup[annotationRegExType];
            MatchCollection mc = regex.Matches(pt);
            foreach (Match m in mc) {
                //foreach (Group mg in m.Groups) {
                //    foreach (Capture c in mg.Captures) {
                MpConsole.WriteLine($"Annotation match: Type: {annotationRegExType} Value: {m.Value} Idx: {m.Index} Length: {m.Length}");

                Op op = new Op() {
                    format = new DeltaRange() { index = m.Index, length = m.Length },
                    attributes = GetLinkAttributes(annotationRegExType, m.Value)
                };

                delta.ops.Add(op);
                //    }
                //}
            }
            return delta;
        }

        private static Attributes GetLinkAttributes(MpRegExType annotationRegExType, string match) {
            var attr = new Attributes() {
                linkType = annotationRegExType.ToString().ToLower(),
                link = GetLinkHref(annotationRegExType, match)
            };
            if (annotationRegExType == MpRegExType.HexColor) {
                var color = new MpColor(match);
                attr.background = color.ToHex(true);
                attr.color = attr.background.IsHexStringBright() ? "#000000" : "#FFFFFF";
            }
            return attr;
        }
        private static string GetLinkHref(MpRegExType annotationRegExType, string match) {
            string href_value;
            switch (annotationRegExType) {
                case MpRegExType.HexColor:
                    href_value = "javascript:;";
                    //var c = new MpColor(match);
                    //href_value = $"https://www.htmlcsscolor.com/hex/{c.ToHex(true).ToUpper().Replace("#", string.Empty)}";
                    break;
                case MpRegExType.Email:
                    href_value = $"mailto:{match}";
                    break;
                case MpRegExType.FileOrFolder:
                    if (Uri.IsWellFormedUriString(match, UriKind.Absolute)) {
                        href_value = new Uri(match).AbsoluteUri;
                    } else if (match.ToFileSystemUriFromPath() is string path_uri &&
                        Uri.IsWellFormedUriString(path_uri, UriKind.Absolute)) {
                        href_value = path_uri;
                    } else {
                        href_value = string.Empty;
                    }
                    break;
                case MpRegExType.PhoneNumber:
                case MpRegExType.StreetAddress:
                case MpRegExType.Currency:
                    href_value = $"https://www.google.com/search?q={match}";
                    break;
                default:
                    href_value = match;
                    break;

            }
            return href_value;
        }

        private static MpQuillDelta ProcessCollisions(MpQuillDelta delta) {
            List<Op> ops_to_remove = new List<Op>();
            // order ops by desc length,
            // then remove any op that collides with it
            // (so longest match in any collision remains)
            foreach (var op in delta.ops.OrderByDescending(x => x.format.length)) {
                if (ops_to_remove.Contains(op)) {
                    continue;
                }
                foreach (var other_op in delta.ops.Where(x => x != op)) {
                    if (ops_to_remove.Contains(op)) {
                        continue;
                    }

                    if (op.format.IntersectsWith(other_op.format)) {
                        ops_to_remove.Add(other_op);
                    }
                }
            }

            ops_to_remove.ForEach(x => delta.ops.Remove(x));
            MpConsole.WriteLine($"{ops_to_remove.Count} colliding annotations removed.");
            return delta;
        }

        #endregion

        #region Protected Methods
        #endregion

        #region Private Methods
        #endregion

        #region Commands
        #endregion
    }

}