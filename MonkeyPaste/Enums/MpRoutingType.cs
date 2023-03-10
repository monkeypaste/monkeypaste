namespace MonkeyPaste {
    public enum MpRoutingType {
        None = 0,
        Internal, //1
        //Direct, //2 basic global active app will receive before/after based on startup time (i think)
        Bubble, //3 sendkey before
        Tunnel,  //4 sendkey after
        Override
    }

}
