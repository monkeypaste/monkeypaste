using Avalonia.Input;
using MonkeyPaste.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonkeyPaste.Avalonia {
    public interface MpAvIDragDataHost {
        bool IsDragValid(MpPoint host_mp);
        Task<IDataObject> GetDragDataObjectAsync();
        void DragBegin();
        void DragEnd();
    }

    public interface MpAvIDropHost {
        bool IsDropValid(IDataObject avdo);
        void DragEnter();
        void DragOver(MpPoint host_mp, IDataObject avdo, DragDropEffects dragEffects);
        void DragLeave();
        Task<DragDropEffects> DropDataObjectAsync(IDataObject avdo, DragDropEffects dragEffects);
    }
}
