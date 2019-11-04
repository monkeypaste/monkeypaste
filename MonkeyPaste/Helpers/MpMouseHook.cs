using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;

namespace MonkeyPaste {    
    public enum MpMouseEvent {
        None,
        Wheel,
        ClickL,
        ClickR,
        ClickM,
        Click1,
        Click2,
        DClickL,
        DClickR,
        DClickM,
        DClick1,
        DClick2,
        DownL,
        DownR,
        DownM,
        Down1,
        Down2,
        UpR,
        UpL,
        UpM,
        Up1,
        Up2,
        Move,
        SDragL,
        SDragR,
        SDragM,
        SDrag1,
        SDrag2,
        EDragL,
        EDragR,
        EDragM,
        EDrag1,
        EDrag2,
        HitBox,
    }
    public class MpMouseHook : IDisposable {
        private IKeyboardMouseEvents _mouseHook = null;
        private MpMouseEvent _me = MpMouseEvent.None;
        public event EventHandler<MouseEventExtArgs> MouseEvent;

        public Rectangle HitBox { get; set; } = Rectangle.Empty;
        public bool IsMouseInHitBox { get; set; } = false;
        public bool IsMouseEnterHitBox { get; set; } = false;
        public bool IsMouseLeaveHitBox { get; set; } = false;
        private bool _eventRaised = false;

        public MpMouseHook() {
            MouseEvent += delegate (object sender,MouseEventExtArgs args) {
                if(MouseEvent != null && _eventRaised) {
                    _eventRaised = false;
                    MouseEvent(sender,args);
                }
            };
        }
        public void RegisterMouseEvent(MpMouseEvent me,object args = null) {
            _mouseHook = Hook.GlobalEvents();
            _me = me;
            switch(me) {
                case MpMouseEvent.Wheel:
                    _mouseHook.MouseWheelExt += _mouseHook_MouseEventExt;
                    break;
                case MpMouseEvent.HitBox:
                    _mouseHook.MouseMoveExt += _mouseHook_MouseEventExt;
                    if(args == null || args.GetType() != typeof(Rectangle)) {
                        Console.WriteLine("Warning MpMouseHook hitbox error no hitbox arg");
                    }
                    else {
                        HitBox = (Rectangle)args;
                    }
                    break;
                default:
                    Console.WriteLine("Error MpMouseHook cannot register for event with MpMouseEvent: " + Enum.GetName(typeof(MpMouseEvent),me));
                break;
            }
        }
        public void UnregisterMouseEvent() {
            IsMouseEnterHitBox = false;
            IsMouseInHitBox = false;
            IsMouseLeaveHitBox = false;
            HitBox = Rectangle.Empty;
            _me = MpMouseEvent.None;
            _mouseHook.Dispose();
        }
        private void _mouseHook_MouseEventExt(object sender,MouseEventExtArgs e) {
            if(_eventRaised) {
                return;
            }
            switch(_me) {
                case MpMouseEvent.Wheel:
                    Console.WriteLine("Mouse Wheel delta: " + e.Delta);
                    break;
                case MpMouseEvent.HitBox:
                    bool eh = false, lh = false;
                    bool isHit = HitBox.Contains(new Point(e.X,e.Y));
                    if(isHit) {
                        //a new hit
                        if(!IsMouseInHitBox) {
                            eh = true;
                            lh = false;
                        } else {
                            eh = false;
                            lh = false;
                        }
                    } else {
                        //prev hit
                        if(IsMouseInHitBox) {
                            eh = false;
                            lh = true;
                        } else {
                            eh = false;
                            lh = false;
                        }
                    }
                    bool raiseEvent = isHit != IsMouseInHitBox || eh != IsMouseEnterHitBox || lh != IsMouseLeaveHitBox;
                    IsMouseInHitBox = isHit; IsMouseEnterHitBox = eh; IsMouseLeaveHitBox = lh;
                    if(raiseEvent) {
                        if(MouseEvent != null) {
                            _eventRaised = true;
                            MouseEvent(this,e);
                        }
                    }
                    break;
                default: 
                    Console.WriteLine("MpMouseHook error in class: '"+sender.GetType().ToString()+"' cannot respond to event with MpMouseEvent: " + Enum.GetName(typeof(MpMouseEvent),_me));
                    break;
            }
        }        
        public void Dispose() {
            UnregisterMouseEvent();
        }
    }
}
