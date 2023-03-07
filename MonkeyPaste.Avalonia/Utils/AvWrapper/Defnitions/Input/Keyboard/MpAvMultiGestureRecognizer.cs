using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MonkeyPaste.Avalonia {
    public class MpAvMultiGestureRecognizer : InputGesture {
        public MpAvMultiGestureRecognizer() {
            Gestures = new InputGestureCollection();
        }

        public InputGestureCollection Gestures { get; private set; }

        private int _currentMatchIndex = 0;

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs) {
            if (_currentMatchIndex < Gestures.Count) {
                if (Gestures[_currentMatchIndex].Matches(targetElement, inputEventArgs)) {
                    _currentMatchIndex++;
                    return (_currentMatchIndex == Gestures.Count);
                }
            }
            _currentMatchIndex = 0;
            return false;
        }
    }
}
