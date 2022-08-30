using Avalonia.Controls;
using HtmlAgilityPack;
using MonkeyPaste.Common;
using MonkeyPaste.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PropertyChanged;
using Avalonia;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvTextBox : TextBox, MpAvIContentView, MpAvIContentDocument {
        #region Statics

        static MpAvTextBox() {
            SelectionStartProperty.Changed.AddClassHandler<MpAvTextBox>((x, y) => HandleSelectionStartChanged(x, y));
            SelectionEndProperty.Changed.AddClassHandler<MpAvTextBox>((x, y) => HandleSelectionEndChanged(x, y));
        }

        private static void HandleSelectionStartChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if(element is MpAvTextBox tb && e.NewValue is int startIdx) {
                tb.UpdateSelection(startIdx, tb.SelectionEnd, true);
            }
        }
        private static void HandleSelectionEndChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if (element is MpAvTextBox tb && e.NewValue is int endIdx) {
                tb.UpdateSelection(tb.SelectionStart, endIdx, true);
            }
        }


        #endregion

        #region Properties

        //#region MpAvTextSelection Implementation

        //MpAvITextPointer MpAvITextRange.Start {
        //    get => new MpAvTextPointer(this, SelectionStart);
        //    set => SelectionStart = value == null ? 0 : value.Offset;
        //}
        //MpAvITextPointer MpAvITextRange.End {
        //    get => new MpAvTextPointer(this, SelectionEnd);
        //    set => SelectionEnd = value == null ? SelectionStart : value.Offset;
        //}

        //bool MpAvITextRange.IsEmpty => SelectionStart == SelectionEnd;

        //public void Select(MpAvITextPointer start, MpAvITextPointer end) {
        //    SelectionStart = start.Offset;
        //    SelectionEnd = end.Offset;
        //}

        //bool MpAvITextRange.IsPointInRange(MpPoint point) => (this as MpAvITextRange).IsPointInRange(point);

        //int IComparable.CompareTo(object obj) => (this as MpAvITextRange).CompareTo(obj);

        //bool IEquatable<MpAvITextRange>.Equals(MpAvITextRange other) => (this as MpAvITextRange).Equals(other);

        //#endregion

        public MpAvTextSelection Selection { get; private set; }


        #region MpAvIContentView Implementation
        IControl MpAvIContentDocument.Owner => this;
        public MpAvIContentDocument Document => this;

        void MpAvIContentView.SelectAll() {
            this.SelectAll();
        }

        #endregion

        #region MpAvIContentDocument Implementation

        public MpAvITextPointer ContentStart => new MpAvTextPointer(this, 0);
        public MpAvITextPointer ContentEnd => new MpAvTextPointer(this, Text == null ? 0 : Text.Length - 1);


        async Task<MpAvITextPointer> MpAvIContentDocument.GetPosisitionFromPointAsync(MpPoint point, bool snapToText) {
            await Task.Delay(1);
            var ft = this.ToFormattedText();
            if (snapToText) {
                point.Clamp(ft.Bounds.ToPortableRect());
            }
            var hitTestResult = ft.HitTestPoint(point.ToAvPoint());
            if (hitTestResult == null || hitTestResult.TextPosition < 0) {
                return null;
            }
            return new MpAvTextPointer(this, hitTestResult.TextPosition);
        }

        async Task<IEnumerable<MpAvITextRange>> MpAvIContentDocument.FindAllTextAsync(string matchText, bool isCaseSensitive, bool matchWholeWord, bool useRegex) {
            await Task.Delay(1);

            string pattern = useRegex ? matchText : matchText.Replace(Environment.NewLine, string.Empty);
            pattern = useRegex ? pattern : Regex.Escape(pattern);
            pattern = !useRegex && matchWholeWord ? $"\b{pattern}\b" : pattern;

            string input = Text;
            var mc = Regex.Matches(input, pattern, isCaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase);
            var matches = new List<MpAvITextRange>();

            foreach (Match m in mc) {
                foreach (Group mg in m.Groups) {
                    foreach (Capture c in mg.Captures) {
                        if (useRegex && matchWholeWord && !Regex.IsMatch(c.Value, $"\b{c.Value}\b")) {
                            continue;
                        }

                        matches.AddRange(
                            input.IndexListOfAll(c.Value)
                                    .Select(x=>
                                        new MpAvTextRange(
                                            new MpAvTextPointer(this,x),
                                            new MpAvTextPointer(this,x+c.Value.Length))));
                    }
                }
            }
            matches = matches.Distinct().ToList();

            return matches;
        }



        #endregion

        #endregion

        #region Public Methods

        public void UpdateSelection(int index, int length, bool isFromEditor) {
            var newStart = new MpAvTextPointer(Document, index);
            var newEnd = new MpAvTextPointer(Document, index + length);
            if (isFromEditor) {
                Selection.Start = newStart;
                Selection.End = newEnd;
            } else {
                Selection.Select(newStart, newEnd);
            }
        }



        #endregion
    }
}
