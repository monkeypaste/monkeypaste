using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MpWpfApp {
    public class MpOverrideCursor : IDisposable {
        static Stack<Cursor> s_Stack = new Stack<Cursor>();

        public MpOverrideCursor(Cursor changeToCursor) {
            s_Stack.Push(changeToCursor);

            if (Mouse.OverrideCursor != changeToCursor)
                Mouse.OverrideCursor = changeToCursor;
        }

        public void Dispose() {
            s_Stack.Pop();

            Cursor cursor = s_Stack.Count > 0 ? s_Stack.Peek() : null;

            if (cursor != Mouse.OverrideCursor)
                Mouse.OverrideCursor = cursor;
        }

    }
}
