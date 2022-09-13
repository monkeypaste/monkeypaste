using Avalonia.Input;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public interface MpAvIDragHost {
        bool CanDrag { get; }
        bool IsDragValid(MpPoint host_mp);
        Task<IDataObject> GetDragDataObjectAsync(bool fillTemplates);
        void DragBegin();
        void DragEnd();
    }

    public interface MpAvIDropHost  {
        bool IsDropEnabled { get; }
        bool IsDropValid(IDataObject avdo, MpPoint host_mp, DragDropEffects dragEffects);
        //void DragEnter();
        void DragOver(MpPoint host_mp, IDataObject avdo, DragDropEffects dragEffects);
        void DragLeave();
        Task<DragDropEffects> DropDataObjectAsync(IDataObject avdo, MpPoint host_mp, DragDropEffects dragEffects);
    }
}
