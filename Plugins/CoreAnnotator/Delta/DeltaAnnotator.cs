using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CoreAnnotator {
    public static class DeltaAnnotator {
        #region Private Variables

        static Dictionary<TextAnnotationType, Regex> _annRegExLookup;
        static Dictionary<TextAnnotationType, Regex> AnnRegExLookup {
            get {
                if (_annRegExLookup == null) {
                    var regex_strs = new Dictionary<TextAnnotationType, string>(){
                        { TextAnnotationType.Url,@"(https?://|www|https?://www|file://).\S+"},
                        { TextAnnotationType.Email,@"([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})"},
                        { TextAnnotationType.PhoneNumber,@"(\+?\d{1,3}?[ -.]?)?\(?(\d{3})\)?[ -.]?(\d{3})[ -.]?(\d{4})"},
                        { TextAnnotationType.Currency,@"[$£€¥][\d|\.]([0-9]{0,3},([0-9]{3},)*[0-9]{3}|[0-9]+)?(\.\d{0,2})?"},
                        { TextAnnotationType.HexColor,@"#([0-9]|[a-fA-F]){8}|#([0-9]|[a-fA-F]){6}"}
                    };
                    _annRegExLookup = regex_strs.ToDictionary(x => x.Key, x => new Regex(x.Value, RegexOptions.Compiled));
                }
                return _annRegExLookup;
            }
        }
        #endregion

        #region Constants
        #endregion

        #region Properties
        #endregion

        #region Constructors
        #endregion

        #region Public Methods

        public static MpQuillDelta Annotate(string plain_text, IEnumerable<TextAnnotationType> formats) {
            MpQuillDelta delta = new MpQuillDelta() { ops = new List<MpQuillOp>() };

            foreach (TextAnnotationType ft in formats) {
                var annotation_type_results = AnnotateType(plain_text, ft);
                delta.ops.AddRange(annotation_type_results.ops);
            }
            delta = ProcessCollisions(delta);

            // order ops by index see (https://quilljs.com/guides/designing-the-delta-format/#pitfalls)
            delta.ops = delta.ops.OrderBy(x => x.format.index).ThenBy(x => x.format.length).ToList();
            delta.ops.ForEach(x => x.retain = 0);
            return delta;
        }
        private static MpQuillDelta AnnotateType(string pt, TextAnnotationType annotationRegExType) {
            MpQuillDelta delta = new MpQuillDelta() { ops = new List<MpQuillOp>() };
            Regex regex = AnnRegExLookup[annotationRegExType];
            MatchCollection mc = regex.Matches(pt);
            foreach (Match m in mc) {
                MpConsole.WriteLine($"Annotation match: Type: {annotationRegExType} Value: {m.Value} Idx: {m.Index} Length: {m.Length}");

                var attr = GetLinkAttributes(annotationRegExType, m.Value);
                if (attr == null) {
                    // bad match
                    continue;
                }
                MpQuillOp op = new MpQuillOp() {
                    format = new MpQuillDeltaRange() { index = m.Index, length = m.Length },
                    attributes = attr
                };

                delta.ops.Add(op);
            }
            return delta;
        }

        private static MpQuillAttributes GetLinkAttributes(TextAnnotationType annotationRegExType, string match) {
            string href = GetLinkHref(annotationRegExType, match);
            if (string.IsNullOrEmpty(href)) {
                return null;
            }
            var attr = new MpQuillAttributes() {
                linkType = annotationRegExType.ToString().ToLowerInvariant(),
                link = href
            };
            if (annotationRegExType == TextAnnotationType.HexColor) {
                var color = new MpColor(match);
                attr.background = color.ToHex(true);
                attr.color = attr.background.IsHexStringBright() ? "#000000" : "#FFFFFF";
            }
            return attr;
        }
        private static string GetLinkHref(TextAnnotationType annotationRegExType, string match) {
            string href_value;
            switch (annotationRegExType) {
                case TextAnnotationType.HexColor:
                    href_value = "javascript:;";
                    //var c = new MpColor(match);
                    //href_value = $"https://www.htmlcsscolor.com/hex/{c.ToHex(true).ToUpper().Replace("#", string.Empty)}";
                    break;
                case TextAnnotationType.Email:
                    href_value = $"mailto:{match}";
                    break;
                case TextAnnotationType.PhoneNumber:
                case TextAnnotationType.Currency:
                    href_value = $"https://www.google.com/search?q={match}";
                    break;
                default:
                    href_value = match;
                    break;

            }
            return href_value;
        }

        private static MpQuillDelta ProcessCollisions(MpQuillDelta delta) {
            List<MpQuillOp> ops_to_remove = new List<MpQuillOp>();
            // order ops by desc length,
            // then remove any op that collides with it
            // so longest match in any collision remains
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
