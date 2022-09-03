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
using Avalonia.Data;
using Avalonia.Styling;

namespace MonkeyPaste.Avalonia {
    [DoNotNotify]
    public class MpAvTextBox : TextBox, MpAvIContentView, MpAvIContentDocument, IStyleable {
        #region Private Variables

        private bool _ignoreSelectionChanges = false;
        #endregion

        #region Statics

        static MpAvTextBox() {
            SelectionStartProperty.Changed.AddClassHandler<MpAvTextBox>((x, y) => HandleSelectionStartChanged(x, y));
            SelectionEndProperty.Changed.AddClassHandler<MpAvTextBox>((x, y) => HandleSelectionEndChanged(x, y));
            TextProperty.Changed.AddClassHandler<MpAvTextBox>((x, y) => HandleTextChanged(x, y));
        }

        private static void HandleSelectionStartChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if (element is MpAvTextBox tb && e.NewValue is int startIdx) {
                tb.UpdateSelection(startIdx, tb.SelectionEnd, true);
            }
        }
        private static void HandleSelectionEndChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if (element is MpAvTextBox tb && e.NewValue is int endIdx) {
                tb.UpdateSelection(tb.SelectionStart, endIdx, true);
            }
        }

        private static void HandleTextChanged(IAvaloniaObject element, AvaloniaPropertyChangedEventArgs e) {
            if (element is MpAvTextBox tb) {
                //RaisePropertyChanged(TextProperty, oldValue, value);
            }
        }


        #endregion

        #region Overrides
        Type IStyleable.StyleKey => typeof(TextBox);

        #endregion

        #region Properties

        private MpAvTextSelection _selection;
        public MpAvTextSelection Selection {
            get { return _selection; }
            set {
                value = CoerceSelection(value);
                SetAndRaise(SelectionProperty, ref _selection, value);
            }
        }

        public static readonly DirectProperty<MpAvTextBox, MpAvTextSelection> SelectionProperty =
            AvaloniaProperty.RegisterDirect<MpAvTextBox,MpAvTextSelection>(
                nameof(Selection),
                x => x.Selection,
                defaultBindingMode: BindingMode.TwoWay,
                enableDataValidation: true);

        #region MpAvIContentView Implementation
        public IControl Owner => this;
        public MpAvIContentDocument Document => this;

        #endregion

        #region MpAvIContentDocument Implementation

        public MpAvITextPointer ContentStart => new MpAvTextPointer(this, 0);
        public MpAvITextPointer ContentEnd => new MpAvTextPointer(this, Text == null ? 0 : Text.Length - 1);


        public async Task<MpAvITextPointer> GetPosisitionFromPointAsync(MpPoint point, bool snapToText) {
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

        public async Task<IEnumerable<MpAvITextRange>> FindAllTextAsync(string matchText, bool isCaseSensitive, bool matchWholeWord, bool useRegex) {
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
                                    .Select(x =>
                                        new MpAvTextRange(
                                            new MpAvTextPointer(this, x),
                                            new MpAvTextPointer(this, x + c.Value.Length))));
                    }
                }
            }
            matches = matches.Distinct().ToList();

            return matches;
        }



        #endregion

        #endregion

        #region Constructors

        public MpAvTextBox() : base() {
            Selection = new MpAvTextSelection(this);
        }
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


        protected override void OnDataContextChanged(EventArgs e) {
            base.OnDataContextChanged(e);
        }
        #endregion

        #region Private Methods

        private MpAvTextSelection CoerceSelection(MpAvTextSelection value) {
            if(value == null) {
                return ContentStart.ToTextRange() as MpAvTextSelection;
            }
            if(value.Start == null) {
                if(Text == null) {
                    Text = String.Empty;
                }
                value.Start = new MpAvTextPointer(this, 0);
            }
            if(value.End == null) {
                value.End = value.Start;
            }

            return value;

        }


        #endregion
    }
}
