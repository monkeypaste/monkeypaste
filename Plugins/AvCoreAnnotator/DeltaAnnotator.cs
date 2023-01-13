using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Plugin;
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

        public static MpQuillDelta Annotate(string plain_text,IEnumerable<MpRegExType> formats) {
            MpQuillDelta delta = new MpQuillDelta() { ops = new List<Op>() };
            if(string.IsNullOrEmpty(plain_text)) {
                return delta;
            }

            foreach(MpRegExType ft in formats) {
                var annotation_type_results = DeltaAnnotator.AnnotateType(plain_text, ft);
                delta.ops.AddRange(annotation_type_results.ops);
            }
            List<Tuple<Op,Op>> collisions = new List<Tuple<Op, Op>>();
            foreach(var op in delta.ops) {
                foreach(var other_op in delta.ops) {
                    if(op == other_op) {
                        continue;
                    }
                    bool start_collision =
                        op.format.index >= other_op.format.index &&
                        op.format.index <= other_op.format.index + other_op.format.length;
                    
                    bool end_collision =
                        op.format.index + op.format.length >= other_op.format.index &&
                        op.format.index + op.format.length <= other_op.format.index + other_op.format.length;

                    if (start_collision || end_collision) {
                        if(collisions.All(x=>(x.Item1 != op && x.Item2 != other_op) || (x.Item1 != other_op && x.Item2 != op))) {
                            // ignore duplicates
                            collisions.Add(new Tuple<Op, Op>(op, other_op));
                        }
                    }
                }
            }

            if(collisions.Count > 0) {
                // TODO should sort regex's by priority to remove collisions
                Debugger.Break();
            }

            // order ops by index see (https://quilljs.com/guides/designing-the-delta-format/#pitfalls)
            delta.ops = delta.ops.OrderBy(x => x.format.index).ThenBy(x => x.format.length).ToList();

            return delta;
        }
        public static MpQuillDelta AnnotateType(string pt, MpRegExType annotationRegExType) {
            MpQuillDelta delta = new MpQuillDelta() { ops = new List<Op>() };

            if(annotationRegExType == MpRegExType.None ||
                (int)annotationRegExType > MpRegEx.MAX_ANNOTATION_REGEX_TYPE) {
                return delta;
            }
            // this text node continues 1 or more current pattern
            // ex 'Hey have you seen https://youtube.mw/blahblah yet? Its just like https://gumbo.com/clippers_fanatics


            int cur_idx = 0;
            Match m = MpRegEx.RegExLookup[annotationRegExType].Match(pt); 
            while (m.Success) {
                MpConsole.WriteLine($"Annotation match: Type: {annotationRegExType} Value: {m.Value}");
                // create annotation
                Op op = new Op() {
                    format = new DeltaRange() { index = m.Index, length = m.Length },
                    attributes = new Attributes() {
                        linkType = annotationRegExType.ToString().ToLower()
                    }
                };

                // set href to match value
                string href_value;
                switch (annotationRegExType) {
                    case MpRegExType.HexColor:
                        // add bg color to regex
                        var c = new MpColor(m.Value);
                        href_value = $"https://www.htmlcsscolor.com/hex/{c.ToHex(true).ToUpper().Replace("#", string.Empty)}";
                        break;
                    case MpRegExType.Email:
                        href_value = $"mailto:{m.Value}";
                        break;
                    case MpRegExType.FileOrFolder:
                        if (Uri.IsWellFormedUriString(m.Value, UriKind.Absolute)) {
                            href_value = new Uri(m.Value).AbsoluteUri;
                        } else {
                            href_value = string.Empty;
                        }
                        break;
                    case MpRegExType.PhoneNumber:
                    case MpRegExType.StreetAddress:
                    case MpRegExType.Currency:
                        href_value = $"https://www.google.com/search?q={m.Value}";
                        break;
                    default:
                        href_value = m.Value;
                        break;

                }
                op.attributes.link = href_value;


                int match_idx = m.Value.Substring(cur_idx).IndexOf(m.Value);
                cur_idx += match_idx + m.Value.Length;
                m = MpRegEx.RegExLookup[annotationRegExType].Match(pt.Substring(cur_idx));
            }
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
