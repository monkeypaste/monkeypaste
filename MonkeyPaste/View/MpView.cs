using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MonkeyPaste {
    public interface MpIView {
        string ViewType { get; set; }
        string ViewName { get; set; }
        int ViewId { get; set; }
        object ViewData { get; set; }
    }
    //public abstract class MpView {
    //    public static int ViewCount { get; set; } = 0;
    //    public string ViewType { get; set; } = string.Empty;

    //    public string ViewName { get; set; } = string.Empty;

    //    public int ViewId { get; set; }

    //    protected MpView Parent { get; set; }

    //    public MpView(MpView p,int cid = -1) {
    //        ++ViewCount;
    //        Parent = p;
    //        ViewType = GetType().ToString();
    //        ViewId = cid;
    //        ViewName = ViewType + (ViewId == -1 ? ViewCount : ViewId);
    //    }
    //}

    //public abstract class MpContainerView : MpView<ContainerControl> {
    //    public MpContainerView(MpView<ContainerControl> p,int vid = -1) : base(p,vid) {
    //    }
    //}
    //public abstract class MpControlView : MpView<Control> {
    //    public MpControlView(MpView<Control> p,int vid = -1) : base(p,vid) {
    //    }
    //}
    //public abstract class MpComponentView : MpView<Component> {
    //    public MpComponentView(MpView<Component> p,int vid = -1) : base(p,vid) {
    //    }
    //}
    //public abstract class MpTextBoxView:MpView<TextBox> {
    //    public MpTextBoxView(MpView<TextBox> p,int vid = -1) : base(p,vid) {
    //    }
    //}
}
